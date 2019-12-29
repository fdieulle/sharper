#' @title 
#' R6 class generator
#'
#' @description
#' Generate R6 classes from .Net types
#'
#' @param typeNames a list of .Net type names
#' @param file File path where generated types will be stored
#' @param appendFile If the specified file already exists we append it.
#' @param withInheritedTypes Defines if you want to generate all inherited types loaded in the clr from your specified types.
#' 
#' @details
#' It can be usefull to use this function to generate R6 classes mapped on .Net types before to build your package.
#' Like that you automatize R6 classes generation and reduce your work to keep consistency between R classes and .Net types.
#' R6 generator generate also the Roxygen2 documentation which will be include in your package to navigate easly in your
#' R6 class graph hierarchy and dependencies. The generator supports type dependencies and type hierarchy. 
#' It can also generate R6 classes for interface, but be carefull because of R6 doesn't support yet multi heritage or interfaces implementation.
#' All generated R6 classes inherits from NetObject class which provides helpers to interact with .Net object instances.
#' 
#' @seealso NetObject
#' @export
#' @examples
#' \dontrun{
#' library(sharper)
#'
#' package_folder <- path.package("sharper")
#' netLoadAssembly(file.path(package_folder, "tests", "AssemblyForTests.dll"))
#' 
#' netGenerateR6("AssemblyForTests.OneCtorData", "AutoGenerate-Simple-R6.R")
#' source("AutoGenerate-Simple-R6.R")
#' o1 <- OneCtorData$new(10L)
#' print(o1$Id)
#' 
#' netGenerateR6("AssemblyForTests.IData", "AutoGenerate-IData-R6.R", withInheritedTypes = TRUE)
#' source("AutoGenerate-IData-R6.R")
#' o2 <- DefaultCtorData$new(Name = "Test")
#' print(o2$Name)
#' o2$Name = "Updated property"
#' print(o2$Name)
#' 
#' o3 <- ManyCtorData$new(Name = "MyName", Id = 1.23)
#' print(o3$Name)
#' }
netGenerateR6 <- function(typeNames, file, appendFile = FALSE, withInheritedTypes = FALSE) {
  invisible (netCallStatic("Sharper.ClrProxy", "GenerateR6Classes", typeNames, file, appendFile, withInheritedTypes))
}