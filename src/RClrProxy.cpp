#include "RClrProxy.h"

void rStartClr(char** appBaseDir, char** dotnetcorePath)
{
	Rprintf("Step 1");
	mainHost.start(appBaseDir[0], dotnetcorePath[0]);
}

void rShutdownClr()
{
	mainHost.shutdown();
}

void rLoadAssembly(char** filePath)
{
	mainHost.rloadAssembly(filePath);
}

SEXP rCallStaticMethod(SEXP p)
{
	return mainHost.rCallStaticMethod(p);
}

SEXP rGetStaticProperty(SEXP p)
{
	return mainHost.rGetStaticProperty(p);
}

void rSetStaticProperty(SEXP p)
{
	mainHost.rSetStaticProperty(p);
}
