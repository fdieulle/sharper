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

	void rloadAssembly(SEXP p);

	SEXP rCallStaticMethod(SEXP p);
	SEXP rGetStatic(SEXP p);
	SEXP rSetStatic(SEXP p);
	SEXP rCreateObject(SEXP p);
	SEXP rCall(SEXP p);
	SEXP rGet(SEXP p);
	SEXP rSet(SEXP p);

protected:
	uint32_t _domainId;

	virtual void loadAssembly(const char* filePath) = 0;
	virtual long long callStaticMethod(const char* typeName, const char* methodName, int64_t argsPtr[], int32_t size) = 0;
	virtual void releaseObject(int64_t ptr) = 0;
private:

	char* readStringFromSexp(SEXP p);
	int64_t* readParametersFromSexp(SEXP p, int32_t& length);
	SEXP convertToSEXP(int64_t ptr);
	void clrObjectFinalizer(SEXP p);
};

#endif // !__CLR_HOST_H__