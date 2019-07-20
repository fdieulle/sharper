#include "RClrProxy.h"

void rStartClr(char** app_base_dir, char** package_bin_path, char** dotnet_core_path)
{
	mainHost.start(first_or_default(app_base_dir), first_or_default(package_bin_path), first_or_default(dotnet_core_path));
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
