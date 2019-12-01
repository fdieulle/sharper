#' @title 
#' Sets property value
#' 
#' @description
#' Sets a property value from a .Net object
#'
#' @param x .Net object
#' @param propertyName Property name to set value
#' @param value value to set.
#' 
#' @details
#' Allows you to set a property value for a .Net object.
#' The input value will be converted from R type to a .Net type. 
#' If the property value isn't a native C# type or a mapped conversion type you have to use an external pointer on .Net object.
#' You can define custom converters in C# for that see `RDotNetConverter` class.
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
netSet <- function(x, propertyName, value) {
  invisible(.External("rSetProperty", x, propertyName, value, PACKAGE = 'sharper'))
}