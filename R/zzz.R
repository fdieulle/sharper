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
	arch <- Sys.getenv('R_ARCH')
	nativeLibPath <- file.path(libsPath, arch, libName)
	
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
	arch <- Sys.getenv('R_ARCH')
	if (arch == "/i386" || arch == "i386")  {
	  arch = "x86"
	} else {
	  arch = "x64"
	} 
	
	nativeLibPath <- file.path(libsPath, arch, libName)
	
	if (file.exists(nativeLibPath)) {
		dyn.unload(nativeLibPath)
	}
}