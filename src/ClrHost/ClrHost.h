#ifndef __CLR_HOST_H__
#define __CLR_HOST_H__

#include <string>

// Todo: Find a good way to isolate the target system preprocessor value than project properties. For now it's WINDOWS
#if WINDOWS
#include <Windows.h>
#define FS_SEPERATOR "\\"
#define PATH_DELIMITER ";"
#elif LINUX
#include <dirent.h>
#include <dlfcn.h>
#include <limits.h>
#define FS_SEPERATOR "/"
#define PATH_DELIMITER ":"
#endif

#include <R.h>
#include <Rinternals.h>

class ClrHost
{
public:
	ClrHost();
	~ClrHost();

	uint32_t getDomainId() { return _domainId; }

	virtual void start(const char* appBaseDir, const char* dotnetcorePath) = 0;
	virtual void shutdown() = 0;

	void rloadAssembly(char** filePath);

	SEXP rCallStaticMethod(SEXP p);
	SEXP rGetStaticProperty(SEXP p);
	SEXP rSetStaticProperty(SEXP p);
	SEXP rCreateObject(SEXP p);
	SEXP rCall(SEXP p);
	SEXP rGet(SEXP p);
	SEXP rSet(SEXP p);

protected:
	uint32_t _domainId;

	virtual void loadAssembly(const char* filePath) = 0;
	virtual void callStaticMethod(const char* typeName, const char* methodName, uint64_t* args, int32_t argsSize, uint64_t** results, int32_t* resultsSize) = 0;
	virtual uint64_t getStaticProperty(const char* typeName, const char* propertyName) = 0;
	virtual void setStaticProperty(const char* typeName, const char* propertyName, uint64_t value) = 0;
	virtual void releaseObject(int64_t ptr) = 0;
private:

	char* readStringFromSexp(SEXP p);
	uint64_t* readParametersFromSexp(SEXP p, int32_t& length);
	SEXP WrapResults(uint64_t* results, uint32_t length);
	SEXP WrapResult(uint64_t result);
	void clrObjectFinalizer(SEXP p);
};

#endif // !__CLR_HOST_H__