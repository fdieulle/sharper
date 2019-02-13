#include "ClrHost.h"

ClrHost::ClrHost()
{
}


ClrHost::~ClrHost()
{
}

void ClrHost::rloadAssembly(char** filePath)
{
	// 1 - Get data from SEXP
	//p = CDR(p); // Skip the first parameter because of function name
	//const char* filePath = readStringFromSexp(p);

	Rprintf("setp 2 %s\n", filePath[0]);
	loadAssembly(filePath[0]);
}

SEXP ClrHost::rCallStaticMethod(SEXP p)
{
	// 1 - Get data from SEXP
	p = CDR(p); // Skip the first parameter because of function name
	const char* typeName = readStringFromSexp(p); p = CDR(p);
	const char* methodName = readStringFromSexp(p); p = CDR(p);
	int32_t argsSize = 0;
	uint64_t* args = readParametersFromSexp(p, argsSize);

	// 2 - Call delegate on clr runtime
	uint64_t* results;
	int32_t resultsSize;
	callStaticMethod(typeName, methodName, args, argsSize, &results, &resultsSize);

	delete[] args;
	
	// 3 - Convert and return the result
	return WrapResults(results, resultsSize);
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

uint64_t* ClrHost::readParametersFromSexp(SEXP p, int32_t& length)
{
	length = Rf_length(p);
	if (length == 0) {
		return NULL;
	}

	uint64_t* result = new uint64_t[length];

	int32_t i;
	SEXP el;
	for (i = 0; i < length && p != R_NilValue; i++, p = CDR(p)) {
		el = CAR(p);
		result[i] = (uint64_t)el;
	}

	return result;
}

SEXP ClrHost::WrapResults(uint64_t* results, uint32_t length)
{
	auto list = Rf_allocVector(VECSXP, length);
	for (auto i = 0; i < length; i++)
	{
		auto sexp = results[i] == 0 ? R_NilValue : (SEXP)results[i];
		
		//if (TYPEOF(sexp) == EXTPTRSXP)
		//	R_RegisterCFinalizerEx(sexp, this->clrObjectFinalizer, (Rboolean)1);

		SET_VECTOR_ELT(list, i, sexp);
	}

	return list;
}

void ClrHost::clrObjectFinalizer(SEXP p)
{
	releaseObject((uint64_t)p);
}