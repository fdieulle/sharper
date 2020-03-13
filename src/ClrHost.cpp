#include "ClrHost.h"

ClrHost::ClrHost()
{
}


ClrHost::~ClrHost()
{
}

void ClrHost::rloadAssembly(char** filePath)
{
	if (!loadAssembly(filePath[0]))
		Rf_error(getLastError());
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
	
	bool isOk = callStaticMethod(typeName, methodName, args, argsSize, &results, &resultsSize);
	
	delete[] args;

	if (!isOk)
	{
		Rf_error(getLastError());
		return R_NilValue;
	}
	
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
	
	if(!getStaticProperty(typeName, propertyName, &result))
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

	if (!setStaticProperty(typeName, propertyName, value))
		Rf_error(getLastError());

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
	bool isOk = createObject(typeName, args, argsSize, &result);

	delete[] args;

	if(!isOk)
	{
		Rf_error(getLastError());
		return R_NilValue;
	}

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
	
	bool isOk = callMethod(objectPtr, methodName, args, argsSize, &results, &resultsSize);

	delete[] args;

	if (!isOk)
	{
		Rf_error(getLastError());
		return R_NilValue;
	}

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
	if (!getProperty(objectPtr, propertyName, &result))
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

	if (!setProperty(objectPtr, propertyName, value))
		Rf_error(getLastError());
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

void path_combine(std::string& path, const char* path2, std::string& path_combined) {
	path_combined.append(path);
	path_combined.append("\\");
	path_combined.append(path2);
}

bool path_expand(const char* path, std::string& absolute_path) {
	if (path == NULL) return false;
	
#if WINDOWS
	char real_path[MAX_PATH];
	if (GetFullPathNameA(path, MAX_PATH, real_path, NULL) > 0) {
		absolute_path.assign(real_path);
		return true;
	}
#elif LINUX
	char real_path[MAX_PATH];
	if (realpath(path, real_path) != nullptr && real_path[0] != '\0') {
		absolute_path.assign(real_path);
		return true;
	}
#endif

	return false;
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