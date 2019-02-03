#include "ClrHost.h"

ClrHost::ClrHost()
{
}


ClrHost::~ClrHost()
{
}

void ClrHost::rloadAssembly(SEXP p)
{
	// 1 - Get data from SEXP
	p = CDR(p); // Skip the first parameter because of function name
	const char* filePath = readStringFromSexp(p);

	loadAssembly(filePath);
}

SEXP ClrHost::rCallStaticMethod(SEXP p)
{
	// 1 - Get data from SEXP
	p = CDR(p); // Skip the first parameter because of function name
	const char* typeName = readStringFromSexp(p); p = CDR(p);
	const char* methodName = readStringFromSexp(p); p = CDR(p);
	int32_t length = 0;
	int64_t* parameters = readParametersFromSexp(p, length);

	// 2 - Call delegate on clr runtime
	int64_t result = callStaticMethod(typeName, methodName, parameters, length);

	delete[] parameters;
	
	// 3 - Convert and return the result
	return convertToSEXP(result);
}

SEXP ClrHost::rGetStatic(SEXP p)
{
	return SEXP();
}

SEXP ClrHost::rSetStatic(SEXP p)
{
	return SEXP();
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

long long * ClrHost::readParametersFromSexp(SEXP p, int32_t& length)
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

SEXP ClrHost::convertToSEXP(int64_t ptr)
{
	auto result = (SEXP)ptr;
	if (TYPEOF(result) == EXTPTRSXP)
	{
		//R_RegisterCFinalizerEx(result, this->clrObjectFinalize, (Rboolean)1);
	}
	
	return result;
}

void ClrHost::clrObjectFinalizer(SEXP p)
{
	auto address = (long long)p;

	releaseObject(address);
}
