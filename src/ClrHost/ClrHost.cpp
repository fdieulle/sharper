#include "ClrHost.h"

ClrHost::ClrHost()
{
}


ClrHost::~ClrHost()
{
}

void ClrHost::rloadAssembly(char** filePath)
{
	loadAssembly(filePath[0]);
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
	callStaticMethod(typeName, methodName, args, argsSize, &results, &resultsSize);

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

	auto result = getStaticProperty(typeName, propertyName);

	return WrapResult(result);
}

SEXP ClrHost::rSetStaticProperty(SEXP p)
{
	// 1 - Get data from SEXP
	p = CDR(p); // Skip the first parameter because of function name
	const char* typeName = readStringFromSexp(p); p = CDR(p);
	const char* propertyName = readStringFromSexp(p); p = CDR(p);

	int32_t argsSize = 0;
	int64_t* args = readParametersFromSexp(p, argsSize);

	if (argsSize < 1)
	{
		Rf_error("Property value is missing\n");
		return R_NilValue;
	}

	int64_t result;
	setStaticProperty(typeName, propertyName, args[0]);

	delete[] args;

	return WrapResult(result);
}

SEXP ClrHost::rCreateObject(SEXP p)
{
	return SEXP();
}

SEXP ClrHost::rCall(SEXP p)
{
	return SEXP();
}

SEXP ClrHost::rGet(SEXP p)
{
	return SEXP();
}

SEXP ClrHost::rSet(SEXP p)
{
	return SEXP();
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

	auto result = new int64_t[length];

	int32_t i;
	SEXP el;
	for (i = 0; i < length && p != R_NilValue; i++, p = CDR(p)) {
		el = CAR(p);
		result[i] = (int64_t)el;
	}

	return result;
}

SEXP ClrHost::WrapResults(int64_t* results, uint32_t length)
{
	auto list = Rf_allocVector(VECSXP, length);

	for (auto i = 0; i < length; i++)
		SET_VECTOR_ELT(list, i, WrapResult(results[i]));

	return list;
}

SEXP ClrHost::WrapResult(int64_t result)
{
	auto sexp = result == 0 ? R_NilValue : (SEXP)result;

	//if (TYPEOF(sexp) == EXTPTRSXP)
	//	R_RegisterCFinalizerEx(sexp, this->clrObjectFinalizer, (Rboolean)1);

	return sexp;
}

void ClrHost::clrObjectFinalizer(SEXP p)
{
	releaseObject((uint64_t)p);
}
