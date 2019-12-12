#include "ClrHost.h"

ClrHost::ClrHost()
{
}


ClrHost::~ClrHost()
{
}

void ClrHost::rloadAssembly(char** filePath)
{
	try
	{
		loadAssembly(filePath[0]);
	}
	catch (...)
	{
		Rf_error(getLastError());
	}
}

SEXP ClrHost::rCallStaticMethod(SEXP p)
{
	// 1 - Get data from SEXP
	p = CDR(p); // Skip the first parameter because of function name
	const char* typeName = readStringFromSexp(p); p = CDR(p);
	const char* methodName = readStringFromSexp(p); p = CDR(p);
	int32_t argsSize = 0;
	int64_t* args = readParametersFromSexp(p, argsSize);

	// 2 - Call delegate on clr runtime
	int64_t* results;
	int32_t resultsSize;
	try
	{
		callStaticMethod(typeName, methodName, args, argsSize, &results, &resultsSize);
	}
	catch (...)
	{
		Rf_error(getLastError());
		return R_NilValue;
	}

	delete[] args;
	
	// 3 - Convert and return the result
	return WrapResults(results, resultsSize);
}

SEXP ClrHost::rGetStaticProperty(SEXP p)
{
	// 1 - Get data from SEXP
	p = CDR(p); // Skip the first parameter because of function name
	const char* typeName = readStringFromSexp(p); p = CDR(p);
	const char* propertyName = readStringFromSexp(p); p = CDR(p);

	int64_t result;
	try 
	{
		result = getStaticProperty(typeName, propertyName);
	}
	catch (...)
	{
		Rf_error(getLastError());
		return R_NilValue;
	}

	return WrapResult(result);
}

void ClrHost::rSetStaticProperty(SEXP p)
{
	// 1 - Get data from SEXP
	p = CDR(p); // Skip the first parameter because of function name
	const char* typeName = readStringFromSexp(p); p = CDR(p);
	const char* propertyName = readStringFromSexp(p); p = CDR(p);
	int64_t value = (int64_t)CAR(p);

	try 
	{
		setStaticProperty(typeName, propertyName, value);
	}
	catch (...) 
	{
		Rf_error(getLastError());
	}
}

SEXP ClrHost::rCreateObject(SEXP p)
{
	// 1 - Get data from SEXP
	p = CDR(p); // Skip the first parameter because of function name
	const char* typeName = readStringFromSexp(p); p = CDR(p);
	
	// 2 - Prepare arguments to call proxy
	int32_t argsSize = 0;
	int64_t* args = readParametersFromSexp(p, argsSize);

	int64_t result;
	try 
	{
		result = createObject(typeName, args, argsSize);
	}
	catch (...) 
	{
		Rf_error(getLastError());
		return R_NilValue;
	}

	delete[] args;

	return WrapResult(result);
}

SEXP ClrHost::rCallMethod(SEXP p)
{
	// 1 - Get data from SEXP
	p = CDR(p); // Skip the first parameter because of function name
	int64_t objectPtr = readObjectPtrFromSexp(p); p = CDR(p);
	const char* methodName = readStringFromSexp(p); p = CDR(p);

	// 2 - Prepare arguments to call proxy
	int32_t argsSize = 0;
	int64_t* args = readParametersFromSexp(p, argsSize);

	// 3 - Call delegate on clr runtime
	int64_t* results;
	int32_t resultsSize;
	try 
	{
		callMethod(objectPtr, methodName, args, argsSize, &results, &resultsSize);
	}
	catch (...) 
	{
		Rf_error(getLastError());
		return R_NilValue;
	}

	delete[] args;

	// 4 - Convert and return the result
	return WrapResults(results, resultsSize);
}

SEXP ClrHost::rGetProperty(SEXP p)
{
	// 1 - Get data from SEXP
	p = CDR(p); // Skip the first parameter because of function name
	int64_t objectPtr = readObjectPtrFromSexp(p); p = CDR(p);
	const char* propertyName = readStringFromSexp(p); p = CDR(p);

	int64_t result;
	try
	{
		result = getProperty(objectPtr, propertyName);
	}
	catch (...)
	{
		Rf_error(getLastError());
		return R_NilValue;
	}

	return WrapResult(result);
}

void ClrHost::rSetProperty(SEXP p)
{
	// 1 - Get data from SEXP
	p = CDR(p); // Skip the first parameter because of function name
	int64_t objectPtr = readObjectPtrFromSexp(p); p = CDR(p);
	const char* propertyName = readStringFromSexp(p); p = CDR(p);
	int64_t value = (int64_t)CAR(p);

	try
	{
		setProperty(objectPtr, propertyName, value);
	}
	catch (...)
	{
		Rf_error(getLastError());
	}
}

char * ClrHost::readStringFromSexp(SEXP p)
{
	SEXP e = CAR(p);
	if (TYPEOF(e) != STRSXP || LENGTH(e) != 1)
		error("[ERROR] ReadStringFromSexp: cannot parse string from SEXP: need a STRSXP of length 1\n");

	return (char*)CHAR(STRING_ELT(e, 0));
}

int64_t* ClrHost::readParametersFromSexp(SEXP p, int32_t& length)
{
	length = Rf_length(p);
	if (length == 0) {
		return NULL;
	}

	int64_t* result = new int64_t[length];

	int32_t i;
	SEXP el;
	for (i = 0; i < length && p != R_NilValue; i++, p = CDR(p)) {
		el = CAR(p);
		result[i] = (int64_t)el;
	}

	return result;
}

int64_t ClrHost::readObjectPtrFromSexp(SEXP p) {
	SEXP e = CAR(p);

	if (e == R_NilValue)
	{
		error("Can't get .net object pointer from a NULL parameter\n");
		return 0;
	}

	return (int64_t)e;
}

SEXP ClrHost::WrapResults(int64_t* results, int32_t length)
{
	SEXP list = Rf_allocVector(VECSXP, length);

	for (int32_t i = 0; i < length; i++)
		SET_VECTOR_ELT(list, i, WrapResult(results[i]));

	return list;
}

SEXP ClrHost::WrapResult(int64_t result)
{
	SEXP sexp = result == 0 ? R_NilValue : (SEXP)result;

	if (TYPEOF(sexp) == EXTPTRSXP)
		registerFinalizer(sexp);

	return sexp;
}

bool file_exists(const char* path) {
	if (path == NULL) return false;

	FILE* f = std::fopen(path, "r");
	if (NULL == f)
		return false;

	std::fclose(f);
	return true;
}

bool is_directory(const char* path) {
	if (path == NULL) return false;

	struct stat info;

	if (stat(path, &info) != 0)
		return false; // Can't access to the path
	if (info.st_mode & S_IFDIR)
		return true;
	return false;
}

const char* path_combine(const char* path, const char* path2) {
	std::string combined(path);
	combined = combined.append(FS_SEPERATOR);
	combined = combined.append(path2);

	char* result = new char[combined.size()];
	std::strcpy(result, combined.c_str());
	return result;
}

const char* path_expand(const char* path) {
	if (path == NULL) return NULL;
	
	char expanded_path[MAX_PATH];
#if WINDOWS
	int size = GetFullPathNameA(path, MAX_PATH, expanded_path, NULL);
#elif LINUX
	int size = realpath(path, expanded_path);
#endif

	char* result = new char[size];
	std::strcpy(result, expanded_path);
	return result;
}

const char* path_get_parent(const char* path) {
	std::string full_path(path);
	std::size_t found = full_path.find_last_of("/\\");
	if (found <= 0) return "..";

	return full_path.substr(0, found).c_str();
}

const char* first_or_default(char** value) {
	if (value == NULL) return NULL;
	return value[0];
}