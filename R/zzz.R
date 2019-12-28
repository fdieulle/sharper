# sharper .onLoad.
# 
# Function called when the sharper package is loading. 
# 
# @param pkgsDir package folder where the library is stored and loaded.
# @param pkgname package name.
# @rdname dotOnLoad
# @name dotOnLoad
.onLoad <- function(pkgsDir='~/R', pkgname = 'sharper') {
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
# @param pkgsDir package folder where the library is stored and loaded.
# @param pkgname package name.
# @rdname dotOnUnload
# @name dotOnUnload
.onUnload <- function(pkgsDir='~/R', pkgname = 'sharper') {

	.C("rShutdownClr", PACKAGE = pkgname)

	pkgDir <- system.file(package = pkgname)
	libsPath <- file.path(pkgDir, "libs")
	
	libName <- paste0(pkgname, .Platform$dynlib.ext)
	nativeLibPath <- file.path(libsPath, Sys.getenv('R_ARCH'), libName)
	
	if (file.exists(nativeLibPath)) {
		dyn.unload(nativeLibPath)
	}
}