#ifndef __CORECLR_HOST_H__
#define __CORECLR_HOST_H__

#include "ClrHost.h"


#if defined(_WIN32) && defined(_M_IX86)
#define CORECLR_CALLING_CONVENTION __stdcall
#else
#define CORECLR_CALLING_CONVENTION
#endif

// For each hosting API, we define a function prototype and a function pointer
// The prototype is useful for implicit linking against the dynamic coreclr
// library and the pointer for explicit dynamic loading (dlopen, LoadLibrary)
#define CORECLR_HOSTING_API(function, ...) \
	extern "C" int CORECLR_CALLING_CONVENTION function(__VA_ARGS__); \
	typedef int (CORECLR_CALLING_CONVENTION *function##_ptr)(__VA_ARGS__)

CORECLR_HOSTING_API(coreclr_initialize,
	const char* exePath,
	const char* appDomainFriendlyName,
	int propertyCount,
	const char** propertyKeys,
	const char** propertyValues,
	void** hostHandle,
	unsigned int* domainId);

CORECLR_HOSTING_API(coreclr_shutdown,
	void* hostHandle,
	unsigned int domainId);

CORECLR_HOSTING_API(coreclr_shutdown_2,
	void* hostHandle,
	unsigned int domainId,
	int* latchedExitCode);

CORECLR_HOSTING_API(coreclr_create_delegate,
	void* hostHandle,
	unsigned int domainId,
	const char* entryPointAssemblyName,
	const char* entryPointTypeName,
	const char* entryPointMethodName,
	void** delegate);

CORECLR_HOSTING_API(coreclr_execute_assembly,
	void* hostHandle,
	unsigned int domainId,
	int argc,
	const char** argv,
	const char* managedAssemblyPath,
	unsigned int* exitCode);

#undef CORECLR_HOSTING_API

#if WINDOWS
#define CORECLR_FILE_NAME "coreclr.dll"
#elif OSX
#define CORECLR_FILE_NAME "libcoreclr.dylib"
#else 
#define CORECLR_FILE_NAME "libcoreclr.so"
#endif

// Function pointer types for the managed call and callbacks
typedef const char*(CORECLR_CALLING_CONVENTION *getLastError_ptr)();
typedef bool (CORECLR_CALLING_CONVENTION *loadAssembly_ptr)(const char* pathOrAssemblyName);
typedef bool (CORECLR_CALLING_CONVENTION *callStaticMethod_ptr)(const char* typeName, const char* methodName, int64_t* argsPtr, int32_t size, int64_t** results, int32_t* resultsSize);
typedef bool (CORECLR_CALLING_CONVENTION *getStaticProperty_ptr)(const char* typeName, const char* propertyName, int64_t* value);
typedef bool (CORECLR_CALLING_CONVENTION *setStaticProperty_ptr)(const char* typeName, const char* propertyName, int64_t value);
typedef bool (CORECLR_CALLING_CONVENTION *createObject_ptr)(const char* typeName, int64_t argPtr[], int32_t size, int64_t* value);
typedef bool (CORECLR_CALLING_CONVENTION *releaseObject_ptr)(int64_t objPtr);
typedef bool (CORECLR_CALLING_CONVENTION *callMethod_ptr)(int64_t objPtr, const char* methodName, int64_t* argsPtr, int32_t argsSize, int64_t** results, int32_t* resultsSize);
typedef bool (CORECLR_CALLING_CONVENTION *getProperty_ptr)(int64_t objPtr, const char* methodName, int64_t* value);
typedef bool (CORECLR_CALLING_CONVENTION *setProperty_ptr)(int64_t objPtr, const char* methodName, int64_t argPtr);

class CoreClrHost : public ClrHost
{
public:
	CoreClrHost();
	~CoreClrHost();

	virtual void start(const char* app_base_dir, const char* package_bin_folder, const char* dotnet_install_path);
	virtual void shutdown();

protected:
	virtual const char* getLastError();
	virtual bool loadAssembly(const char* filePath);
	virtual bool callStaticMethod(const char* typeName, const char* methodName, int64_t* args, int32_t argsSize, int64_t** results, int32_t* resultsSize);
	virtual bool getStaticProperty(const char* typeName, const char* propertyName, int64_t* value);
	virtual bool setStaticProperty(const char* typeName, const char* propertyName, int64_t value);

	virtual bool createObject(const char* typeName, int64_t* args, int32_t argsSize, int64_t* value);
	virtual void registerFinalizer(SEXP sexp);
	virtual bool callMethod(int64_t objectPtr, const char* methodName, int64_t* args, int32_t argsSize, int64_t** results, int32_t* resultsSize);
	virtual bool getProperty(int64_t objectPtr, const char* propertyName, int64_t* value);
	virtual bool setProperty(int64_t objectPtr, const char* propertyName, int64_t value);

private:
#if WINDOWS
	HMODULE _coreClr;
#elif LINUX
	void* _coreClr;
#endif
	void* _hostHandle;

	coreclr_initialize_ptr _initializeCoreClr;
	coreclr_create_delegate_ptr _createManagedDelegate;
	coreclr_shutdown_ptr _shutdownCoreClr;

	getLastError_ptr _getLastErrorFunc;
	loadAssembly_ptr _loadAssemblyFunc;
	callStaticMethod_ptr _callStaticMethodFunc;
	getStaticProperty_ptr _getStaticPropertyFunc;
	setStaticProperty_ptr _setStaticPropertyFunc;
	createObject_ptr _createObjectFunc;
	static releaseObject_ptr releaseObjectFunc;
	callMethod_ptr _callFunc;
	getProperty_ptr _getFunc;
	setProperty_ptr _setFunc;

	void createManagedDelegate(const char* entryPointMethodName, void** delegate);
	
	static bool get_core_clr_with_tpa_list(const char* app_base_dir, const char* package_bin_folder, const char* dotnet_install_path, std::string& core_clr, std::string& tpa_list);
	static void build_tpa_list(const char* directory, const char* extension, std::string& tpaList);
};

#endif // !__CORECLR_HOST_H__

