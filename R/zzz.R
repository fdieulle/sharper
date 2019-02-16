#' sharper .onLoad.
#' 
#' Function called when the sharper package is loading. 
#' 
#' @param pkgsDir package folder where the library is stored and loaded.
#' @param pkgname package name.
#' @rdname dotOnLoad
#' @name dotOnLoad
.onLoad <- function(pkgsDir='~/R', pkgname = 'sharper') {
	pkgDir <- system.file(package = pkgname)
	libsPath <- file.path(pkgDir, "libs")
	
	libName <- paste0(pkgname, .Platform$dynlib.ext)
	arch <- Sys.getenv('R_ARCH')
	nativeLibPath <- file.path(libsPath, arch, libName)
	
	if (file.exists(nativeLibPath)) {
		dyn.load(nativeLibPath)
	
		dotNetCorePath <- file.path(pkgDir, "bin", "dotnet-core")
		if (arch == "/i386") {
			dotNetCorePath <- file.path(dotNetCorePath, "x86")
		} else if (arch == "/x64") {
			dotNetCorePath <- file.path(dotNetCorePath, "x64")
		} else return()

		dotNetCorePath <- file.path(dotNetCorePath, "shared", "Microsoft.NETCore.App")

		versions <- sort(list.dirs(dotNetCorePath, full.names = FALSE, recursive = FALSE), decreasing = FALSE)
		if (length(versions) == 0) return()
		
		binPath <- file.path(pkgDir, "bin")

		.C("rStartClr", file.path(pkgDir, "bin"), file.path(dotNetCorePath, versions[1]), PACKAGE = pkgname)
	}
}

#' sharper .onLoad.
#' 
#' Function called when the sharper package is loading. 
#' 
#' @param pkgsDir package folder where the library is stored and loaded.
#' @param pkgname package name.
#' @rdname dotOnUnload
#' @name dotOnUnload
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