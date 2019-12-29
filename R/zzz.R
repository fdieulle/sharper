# sharper .onLoad.
# 
# Function called when the sharper package is loading. 
# 
# @param libname a character string giving the library directory where the package defining the namespace was found.
# @param pkgname a character string giving the name of the package.
# @rdname dotOnLoad
# @name dotOnLoad
.onLoad <- function(libname = '~/R', pkgname = 'sharper') {
	pkgDir <- system.file(package = pkgname)
	libsPath <- file.path(pkgDir, "libs")
	
	libName <- paste0(pkgname, .Platform$dynlib.ext)
	nativeLibPath <- file.path(libsPath, Sys.getenv('R_ARCH'), libName)
	
	if (file.exists(nativeLibPath)) 
	{
		dyn.load(nativeLibPath)
		start_dotnet_core_clr()
	}
}

# sharper .onLoad.
# 
# Function called when the sharper package is loading. 
# 
# @param libname a character string giving the library directory where the package defining the namespace was found.
# @param pkgname a character string giving the name of the package.
# @rdname dotOnUnload
# @name dotOnUnload
.onUnload <- function(libname='~/R', pkgname = 'sharper') {

	.C("rShutdownClr", PACKAGE = pkgname)

	pkgDir <- system.file(package = pkgname)
	libsPath <- file.path(pkgDir, "libs")
	
	libName <- paste0(pkgname, .Platform$dynlib.ext)
	nativeLibPath <- file.path(libsPath, Sys.getenv('R_ARCH'), libName)
	
	if (file.exists(nativeLibPath)) {
		dyn.unload(nativeLibPath)
	}
}