#' @title 
#' Instantiate a .Net object
#' 
#' @description
#' Instantiate a .Net object from its type name.
#'
#' @param typeName The .Net full name type
#' @param ... .Net Constructor arguments.
#' @return Returns a converted .Net instance if a converter is defined, an external pointer otherwise.
#'
#' @details
#' The `typeName` should respect the full type name convention: `Namespace.TypeName`
#' Ellipses `...` has to keep the .net constructor arguments order, the named arguments are not supported yet.
#' If there is many constructors defined for the given .Net type, a score selection is computed from your arguments orders and types to choose the best one. 
#' We consider as a higher priority single value compare to collection of values.
#' 
#' @md
#' @export
#' @examples
#' \dontrun{
#' library(sharper)
#'
#' package_folder <- path.package("sharper")
#' netLoadAssembly(file.path(package_folder, "tests", "RDotNet.AssemblyTest.dll"))
#' 
#' x <- netNew("RDotNet.AssemblyTest.OneCtorData", 21L)
#' netCall(x, "ToString")
#' }
netNew <- function(typeName, ...) {
  result <- .External("rCreateObject", typeName, ..., PACKAGE = 'sharper')
  return (result)
}