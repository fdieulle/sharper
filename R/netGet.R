#' @title 
#' Gets property value
#' 
#' @description
#' Gets a property value from a .Net object
#'
#' @param x .Net object
#' @param propertyName Property name to get value
#' @return Returns a converted .Net instance if a converter is defined, an external pointer otherwise.
#' 
#' @details
#' Allows you to get a property value for a .Net object.
#' The result will be converted if the type mapping is defined. All native C# types are mapped to R types
#' but you can define custom converters in C# for that see the C# `RDotNetConverter` class.
#' 
#' @export
#' @examples
#' \dontrun{
#' library(sharper)
#' 
#' package_folder = path.package("sharper")
#, netLoadAssembly(file.path(package_folder, "tests", "AssemblyForTests.dll"))
#' 
#' x <- netNew("AssemblyForTests.DefaultCtorData")
#' netSet(x, "Name", "foo")
#' x_name <- netGet(x, "Name")
#' 
#' netSet(x, "Integers", c(12L, 23L))
#' x_integers <- netGet(x, "Integers")
#'
#' }
netGet <- function(x, propertyName) {
  result <- .External("rGetProperty", x, propertyName, PACKAGE = 'sharper')
  return (result)
}