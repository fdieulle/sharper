#' @title 
#' Gets a static property value
#' 
#' @description
#' Gets a static property value from a .Net type name
#'
#' @param typeName Full .Net type name 
#' @param propertyName Property name to get value
#' @return Returns a converted .Net instance if a converter is defined, an external pointer otherwise.
#' 
#' @details
#' Allows you to get a static property value from a .Net type name.
#' The result will be converted if the type mapping is defined. All native C# types are mapped to R types
#' but you can define custom converters in C# for that see the C# `RDotNetConverter` class.
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
netGetStatic <- function(typeName, propertyName) {
  result <- .External("rGetStaticProperty", typeName, propertyName, PACKAGE = 'sharper')
  return (result)
}