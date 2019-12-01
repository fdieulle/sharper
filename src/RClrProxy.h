#ifndef __R_CLR_PROXY_H__
#define __R_CLR_PROXY_H__

#include "ClrHost.h"
#include "CoreClrHost.h"

static CoreClrHost mainHost = CoreClrHost();

#ifdef __cplusplus
extern "C" {
#endif
	// ClrEnvironment methods
	void rStartClr(char** app_base_dir, char** package_bin_path, char** dotnet_core_path);
	void rShutdownClr();

	void rLoadAssembly(char** fileName);

	// Proxy methods
	SEXP rCallStaticMethod(SEXP p);
	SEXP rGetStaticProperty(SEXP p);
	SEXP rSetStaticProperty(SEXP p);
	SEXP rCreateObject(SEXP p);
	SEXP rCallMethod(SEXP p);
	SEXP rGetProperty(SEXP p);
	SEXP rSetProperty(SEXP p);

#ifdef __cplusplus
} // end of extern "C" block
#endif

#endif // !__R_CLR_PROXY_H__