#' @title
#' Load assembly.
#'
#' @description
#' Loads an assembly in the Clr (Common Language Runtime). 
#'
#' @param filePath Assembly file. It can be the full file path of the assembly, or a qualified assembly name.
#'
#' @export
#' @examples
#' \dontrun{
#' library(sharper)
#'
#' pkgPath <- path.package("sharper")
#' f <- file.path(pkgPath, "tests", "AssemblyForTests.dll")
#' netLoadAssembly(f)
#' }
netLoadAssembly <- function(filePath) {
  .C("rLoadAssembly", filePath, PACKAGE = 'sharper')
}