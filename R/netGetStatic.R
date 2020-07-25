#' @title 
#' Get static property value
#' 
#' @description
#' Gets a static property value from a .Net type name
#'
#' @param typeName Full .Net type name 
#' @param propertyName Property name to get value
#' @param wrap Specify if you want to wrap `externalptr` .Net object into `NetObject` `R6` object. `FALSE` by default.
#' @return Returns the .Net result. 
#' If a converter has been defined between the .Net type and a `R` type, the `R` type will be returned.
#' Otherwise an `externalptr` or a `NetObject` if `wrap` is set to `TRUE`.
#'  
#' @details
#' Allows you to get a static property value from a .Net type name.
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
#' pkgPath <- path.package("sharper")
#' f <- file.path(pkgPath, "tests", "AssemblyForTests.dll")
#' netLoadAssembly(f)
#' 
#' type <- "AssemblyForTests.StaticClass"
#' x <- netGetStatic(type, "DoubleProperty")
#' y <- netGetStatic(type, "Int32Property")
#'
#' xx <- netGetStatic(type, "DoubleArrayProperty")
#' yy <- netGetStatic(type, "Int32ArrayProperty")
#' }
netGetStatic <- function(typeName, propertyName, wrap = FALSE) {
  result <- .External("rGetStaticProperty", typeName, propertyName, PACKAGE = 'sharper')
  if (wrap) result <- netWrap(result)
  return (result)
}