#ifndef __R_CLR_PROXY_H__
#define __R_CLR_PROXY_H__

#include "ClrHost.h"
#include "CoreClrHost.h"

CoreClrHost mainHost = CoreClrHost();

#ifdef __cplusplus
extern "C" {
#endif
	// ClrEnvironment methods
	void rStartClr(char** appBaseDir, char** dotnetcorePath);
	void rShutdownClr();

	void rLoadAssembly(SEXP fileName);

	// Proxy methods
	SEXP rCallStaticMethod(SEXP p);
	//SEXP rGetStatic(SEXP p);
	//SEXP rSetStatic(SEXP p);
	//SEXP rCreateObject(SEXP p);
	//SEXP rCall(SEXP p);
	//SEXP rGet(SEXP p);
	//SEXP rSet(SEXP p);

#ifdef __cplusplus
} // end of extern "C" block
#endif

#endif // !__R_CLR_PROXY_H__