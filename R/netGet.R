#' @title 
#' Get property value
#' 
#' @description
#' Gets a property value from a .Net object
#'
#' @param x .Net object
#' @param propertyName Property name to get value
#' @param wrap Specify if you want to wrap `externalptr` .Net object into `NetObject` `R6` object. `FALSE` by default.
#' @return Returns the .Net property value. 
#' If a converter has been defined between the .Net type and a `R` type, the `R` type will be returned.
#' Otherwise an `externalptr` or a `NetObject` if `wrap` is set to `TRUE`.
#' 
#' @details
#' Allows you to get a property value for a .Net object.
#' The result will be converted if the type mapping is defined. All native C# types are mapped to R types
#' but you can define custom converters in C# for that see the C# `RDotNetConverter` class.
#' 
#' If you decide to set `wrap` to `TRUE` this function supports the `NetObject R6` class and all inherited.
#' The function result if no converter has been found will return a `NetObject` of an inherited best type 
#' instead of a raw `externalptr`. For more details about inherited `NetObject` class please see `netGenerateR6` function. 
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
netGet <- function(x, propertyName, wrap = FALSE) {
  result <- .External("rGetProperty", netUnwrap(x), propertyName, PACKAGE = 'sharper')
  if (wrap) result <- netWrap(result)
  return (result)
}
