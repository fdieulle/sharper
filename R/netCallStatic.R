#' @title 
#' Call a static method
#' 
#' @description
#' Call a static method for a given .Net type name
#'
#' @param typeName Full .Net type name 
#' @param methodName Method name to call
#' @param ... Method arguments
#' @return Returns a converted .Net instance if a converter is defined, an external pointer otherwise.
#'
#'@details
#' Call a static method for a given .Net type name.
#' Ellipses has to keep the .net arguments method order, the named arguments are not supported yet.
#' If there is conflicts with a method name (many definition in .Net), the best matched one will be chose.
#' A score is computed from your arguments orders and types. We consider as higher priority single value compare to collection of values.
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
#' netCallStatic(type, "CallWithInteger", 2L)
#' netCallStatic(type, "CallWithIntegerVector", c(2L, 3L))
#'
#' # Method selection single value vs vector values
#' netCallStatic(type, "SameMethodName", 1.23)
#' netCallStatic(type, "SameMethodName", c(1.24, 1.25))
#' netCallStatic(type, "SameMethodName", c(1.24, 1.25), 12L)
#' }
netCallStatic <- function(typeName, methodName, ...) {
	results <- .External("rCallStaticMethod", typeName, methodName, ..., PACKAGE = 'sharper')
	if (length(results) > 1) {
		args <- lapply(eval(substitute(alist(...))), deparse)
		for (i in seq_along(length(args))) {
			assign(args[[i]], results[[i + 1]], env = parent.frame())
		}
	} 
	
	return (results[[1]])
}