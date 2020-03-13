#ifndef __CLR_HOST_H__
#define __CLR_HOST_H__

#include <string>
#include <stdint.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <vector>
#include <algorithm>

// Check if its windows
#ifdef _WIN32
#define WINDOWS 1
#define LINUX 0
#define OSX 0
#endif // _WIN32

#ifdef _WIN64
#define WINDOWS 1
#define LINUX 0
#define OSX 0
#endif // _WIN64

// Check if its MacOS
#ifdef __APPLE__
#define OSX 1
#define WINDOWS 0
#endif // __APPLE__

#if !WINDOWS
#define LINUX 1
#endif



// Todo: Find a good way to isolate the target system preprocessor value than project properties. For now it's WINDOWS
#if WINDOWS
#include <Windows.h>
#define FS_SEPERATOR "\\"
#define PATH_DELIMITER ";"
#ifdef ERROR
#undef ERROR
#endif

#elif LINUX
#include <dirent.h>
#include <dlfcn.h>
#include <limits.h>
#define FS_SEPERATOR "/"
#define PATH_DELIMITER ":"
#define MAX_PATH PATH_MAX
#endif

#include <R.h>
#include <Rinternals.h>

class ClrHost
{
public:
	ClrHost();
	~ClrHost();

	unsigned int getDomainId() { return _domainId; }

	virtual void start(const char* app_base_dir, const char* package_bin_folder, const char* dotnet_install_path) = 0;
	virtual void shutdown() = 0;

	void rloadAssembly(char** filePath);

	SEXP rCallStaticMethod(SEXP p);
	SEXP rGetStaticProperty(SEXP p);
	void rSetStaticProperty(SEXP p);
	SEXP rCreateObject(SEXP p);
	SEXP rCallMethod(SEXP p);
	SEXP rGetProperty(SEXP p);
	void rSetProperty(SEXP p);

protected:
	unsigned int _domainId;

	virtual const char* getLastError() = 0;
	virtual bool loadAssembly(const char* filePath) = 0;
	virtual bool callStaticMethod(const char* typeName, const char* methodName, int64_t* args, int32_t argsSize, int64_t** results, int32_t* resultsSize) = 0;
	virtual bool getStaticProperty(const char* typeName, const char* propertyName, int64_t* value) = 0;
	virtual bool setStaticProperty(const char* typeName, const char* propertyName, int64_t value) = 0;
	
	virtual bool createObject(const char* typeName, int64_t* args, int32_t argsSize, int64_t* value) = 0;
	virtual void registerFinalizer(SEXP sexp) = 0;
	virtual bool callMethod(int64_t objectPtr, const char* methodName, int64_t* args, int32_t argsSize, int64_t** results, int32_t* resultsSize) = 0;
	virtual bool getProperty(int64_t objectPtr, const char* propertyName, int64_t* value) = 0;
	virtual bool setProperty(int64_t objectPtr, const char* propertyName, int64_t value) = 0;
private:

	char* readStringFromSexp(SEXP p);
	int64_t* readParametersFromSexp(SEXP p, int32_t& length);
	SEXP WrapResults(int64_t* results, int32_t length);
	SEXP WrapResult(int64_t result);
	int64_t readObjectPtrFromSexp(SEXP p);
};

bool file_exists(const char* path);

bool is_directory(const char* path);

void path_combine(std::string& path, const char* path2, std::string& path_combined);

bool path_expand(const char* path, std::string& absolute_path);

const char* path_get_parent(const char* path);

const char* first_or_default(char** value);

#endif // !__CLR_HOST_H__