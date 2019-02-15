#include "CoreClrHost.h"

CoreClrHost::CoreClrHost()
{
}


CoreClrHost::~CoreClrHost()
{
}


void CoreClrHost::start(const char* appBaseDir, const char* dotnetcoreInstallPath)
{
	// dotnet core default install folder
	// Windows: C:\Program Files\dotnet\shared\Microsoft.NETCore.App
	// Ubuntu or Alpine: /usr/share/dotnet/shared/Microsoft.NETCore.App/
	// MacOS: /usr/local/share/dotnet/shared/Microsoft.NETCore.App/

	if (dotnetcoreInstallPath == NULL)
	{
#if WINDOWS
		dotnetcoreInstallPath = "C:/Program Files/dotnet/shared/Microsoft.NETCore.App";
#elif LINUX
#if OSX
		dotnetcoreInstallPath = "/usr/share/dotnet/shared/Microsoft.NETCore.App/";
#else
		dotnetcoreInstallPath = "/usr/share/dotnet/shared/Microsoft.NETCore.App/";
#endif
#endif
	}

	// Todo: Check if we found coreclr.dll other wise found the last dotnet core version 
	// if it's not specified by user

	if (_coreClr != NULL || _hostHandle != NULL)
		shutdown();

	char packagePath[MAX_PATH];
	char dotnetCorePath[MAX_PATH];
#if WINDOWS
	GetFullPathNameA(appBaseDir, MAX_PATH, packagePath, NULL);
	GetFullPathNameA(dotnetcoreInstallPath, MAX_PATH, dotnetCorePath, NULL);
#elif LINUX
	realpath(appBaseDir, packagePath);
	realpath(dotnetcoreInstallPath, dotnetCorePath);
#endif

	Rprintf("AppBaseDir %s \n DotNetCorePath %s \n", packagePath, dotnetCorePath);

	// Construct the CoreCLR path
	// Todo: it may be necessary to probe for coreclr.dll/libcoreclr.so
	std::string coreClrPath(dotnetCorePath);
	coreClrPath.append(FS_SEPERATOR);
	coreClrPath.append(CORECLR_FILE_NAME);

	// 1. Load CoreCLR (coreclr.dll/libcoreclr.so)
#if WINDOWS
	_coreClr = LoadLibraryExA(coreClrPath.c_str(), NULL, 0);
#elif LINUX
	_coreClr = dlopen(coreClrPath.c_str(), RTLD_NOW | RTLD_LOCAL);
#endif

	if (_coreClr == NULL)
	{
		Rf_error("Failed to load CoreCLR from %s\n", coreClrPath.c_str());
		return;
	}
	else
	{
		Rprintf("Loaded CoreCLR from %s\n", coreClrPath.c_str());
	}

	// 2. Get CoreCLR hosting functions
#if WINDOWS
	_initializeCoreClr = (coreclr_initialize_ptr)GetProcAddress(_coreClr, "coreclr_initialize");
	_createManagedDelegate = (coreclr_create_delegate_ptr)GetProcAddress(_coreClr, "coreclr_create_delegate");
	_shutdownCoreClr = (coreclr_shutdown_ptr)GetProcAddress(_coreClr, "coreclr_shutdown");
#elif LINUX
	_initializeCoreClr = (coreclr_initialize_ptr)dlsym(_coreClr, "coreclr_initialize");
	_createManagedDelegate = (coreclr_create_delegate_ptr)dlsym(_coreClr, "coreclr_create_delegate");
	_shutdownCoreClr = (coreclr_shutdown_ptr)dlsym(_coreClr, "coreclr_shutdown");
#endif

	if (_initializeCoreClr == NULL)
	{
		Rf_error("coreclr_initialize not found");
		return;
	}

	if (_createManagedDelegate == NULL)
	{
		Rf_error("coreclr_create_delegate not found");
		return;
	}

	if (_shutdownCoreClr == NULL)
	{
		Rf_error("coreclr_shutdown not found");
		return;
	}

	// 3. Construct properties used when starting the runtime

	// Construct the trusted platform assemblies (TPA) list
	// This is the list of assemblies that .NET Core can load as trusted system assemblies.
	std::string tpaList;
	BuildTpaList(dotnetCorePath, ".dll", tpaList);
	BuildTpaList(packagePath, ".dll", tpaList);

	// Define CoreCLR properties
	// Other properties related to assembly loading are common here
	const char* propertyKeys[] = {
		"TRUSTED_PLATFORM_ASSEMBLIES"      // Trusted assemblies
	};

	const char* propertyValues[] = {
		tpaList.c_str()
	};

	// 4. Start the CoreCLR runtime and create the default (and only) AppDomain
	auto hr = _initializeCoreClr(
		packagePath,        // App base path
		"CoreClrHost",       // AppDomain friendly name
		sizeof(propertyKeys) / sizeof(char*),   // Property count
		propertyKeys,       // Property names
		propertyValues,     // Property values
		&_hostHandle,        // Host handle
		&_domainId); // AppDomain ID

	if (hr >= 0)
		Rprintf("CoreCLR started\n");
	else
	{
		Rf_error("coreclr_initialize failed - status: 0x%08x\n", hr);
		return;
	}

	// 5. Create delegates to managed code to be able to invoke them
	createManagedDelegate("LoadAssembly", (void**)&_loadAssemblyFunc);
	createManagedDelegate("CallStaticMethod", (void**)&_callStaticMethodFunc);
	createManagedDelegate("GetStaticProperty", (void**)&_getStaticPropertyFunc);
	createManagedDelegate("SetStaticProperty", (void**)&_setStaticPropertyFunc);
}

void CoreClrHost::shutdown()
{
	auto hr = _shutdownCoreClr(_hostHandle, _domainId);

	if (hr >= 0)
	{
		Rprintf("CoreCLR successfully shutdown\n");
		_hostHandle = NULL;
		_domainId = 0;
	}
	else Rf_error("coreclr_shutdown failed - status: 0x%08x\n", hr);
	
	// Unload CoreCLR
#if WINDOWS
	if (!FreeLibrary(_coreClr))
		Rf_error("Failed to free coreclr.dll\n");
#elif LINUX
	if (dlclose(coreClr))
		Rf_error("Failed to free libcoreclr.so\n");
#endif

	_coreClr = NULL;
}

void CoreClrHost::loadAssembly(const char * filePath)
{
	if (_coreClr == NULL && _hostHandle == NULL)
	{
		Rf_error("CoreCLR isn't started.");
		return;
	}

	_loadAssemblyFunc(filePath);
}

void CoreClrHost::callStaticMethod(const char* typeName, const char* methodName, int64_t* args, int32_t argsSize, int64_t** results, int32_t* resultsSize)
{
	if (_coreClr == NULL && _hostHandle == NULL)
	{
		Rf_error("CoreCLR isn't started.");
		return;
	}

	_callStaticMethodFunc(typeName, methodName, args, argsSize, results, resultsSize);
}

int64_t CoreClrHost::getStaticProperty(const char* typeName, const char* propertyName)
{
	if (_coreClr == NULL && _hostHandle == NULL)
	{
		Rf_error("CoreCLR isn't started.");
		return 0;
	}

	return _getStaticPropertyFunc(typeName, propertyName);
}

void CoreClrHost::setStaticProperty(const char* typeName, const char* propertyName, int64_t value)
{
	if (_coreClr == NULL && _hostHandle == NULL)
	{
		Rf_error("CoreCLR isn't started.");
		return;
	}

	_setStaticPropertyFunc(typeName, propertyName, value);
}

void CoreClrHost::releaseObject(int64_t ptr)
{
}

void CoreClrHost::BuildTpaList(const char* directory, const char* extension, std::string& tpaList)
{
#if WINDOWS
	// Win32 directory search for .dll files

	// This will add all files with a .dll extension to the TPA list. 
	// This will include unmanaged assemblies (coreclr.dll, for example) that don't
	// belong on the TPA list. In a real host, only managed assemblies that the host
	// expects to load should be included. Having extra unmanaged assemblies doesn't
	// cause anything to fail, though, so this function just enumerates all dll's in
	// order to keep this sample concise.
	std::string searchPath(directory);
	searchPath.append(FS_SEPERATOR);
	searchPath.append("*");
	searchPath.append(extension);

	WIN32_FIND_DATAA findData;
	HANDLE fileHandle = FindFirstFileA(searchPath.c_str(), &findData);

	if (fileHandle != INVALID_HANDLE_VALUE)
	{
		do
		{
			// Append the assembly to the list
			tpaList.append(directory);
			tpaList.append(FS_SEPERATOR);
			tpaList.append(findData.cFileName);
			tpaList.append(PATH_DELIMITER);

			// Note that the CLR does not guarantee which assembly will be loaded if an assembly
			// is in the TPA list multiple times (perhaps from different paths or perhaps with different NI/NI.dll
			// extensions. Therefore, a real host should probably add items to the list in priority order and only
			// add a file if it's not already present on the list.
			//
			// For this simple sample, though, and because we're only loading TPA assemblies from a single path,
			// and have no native images, we can ignore that complication.
		} while (FindNextFileA(fileHandle, &findData));
		FindClose(fileHandle);
	}
#elif LINUX
	// POSIX directory search for .dll files
	DIR* dir = opendir(directory);
	struct dirent* entry;
	int extLength = strlen(extension);

	while ((entry = readdir(dir)) != NULL)
	{
		// This simple sample doesn't check for symlinks
		std::string filename(entry->d_name);

		// Check if the file has the right extension
		int extPos = filename.length() - extLength;
		if (extPos <= 0 || filename.compare(extPos, extLength, extension) != 0)
		{
			continue;
		}

		// Append the assembly to the list
		tpaList.append(directory);
		tpaList.append(FS_SEPERATOR);
		tpaList.append(filename);
		tpaList.append(PATH_DELIMITER);

		// Note that the CLR does not guarantee which assembly will be loaded if an assembly
		// is in the TPA list multiple times (perhaps from different paths or perhaps with different NI/NI.dll
		// extensions. Therefore, a real host should probably add items to the list in priority order and only
		// add a file if it's not already present on the list.
		//
		// For this simple sample, though, and because we're only loading TPA assemblies from a single path,
		// and have no native images, we can ignore that complication.
	}
#endif
}

void CoreClrHost::createManagedDelegate(const char* entryPointMethodName, void** delegate)
{
	auto hr = _createManagedDelegate(
		_hostHandle,
		_domainId,
		"Sharper",
		"Sharper.ClrProxy",
		entryPointMethodName,
		delegate);

	if (hr >= 0)
		Rprintf("Managed delegate created for %s function\n", entryPointMethodName);
	else
		Rf_error("coreclr_create_delegate for method %s failed - status: %d\n", entryPointMethodName, hr);
}