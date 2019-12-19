#' @title 
#' Call method
#'
#' @description
#' Call a .Net method member for a given a .Net object.
#'
#' @param x a .Net object, which can be an `externalptr` or a `NetObject`.
#' @param methodName Method name to call
#' @param ... Method arguments
#' @param wrap Specify if you want to wrap `externalptr` .Net object results into `NetObject` `R6` object. `FALSE`` by default.
#' @param out_env In case of a .Net method with `out` or `ref` argument is called, 
#' specify on which `environment` you want to out put this arguments. 
#' By default it's the caller `environment` i.e. `parent.frame()`.
#' @return Returns the .Net result. 
#' If a converter has been defined between the .Net type and a `R` type, the `R` type will be returned.
#' Otherwise an `externalptr` or a `NetObject` if `wrap` is set to `TRUE`.
#' 
#' @details
#' Call a method member for a given .Net object.
#' Ellipses has to keep the .Net arguments method order, the named arguments are not yet supported.
#' If there is conflicts with a method name (many definition in .Net), a score is computed from your argument's 
#' order and type. We consider a higher score single value comparing to collection of values.
#' 
#' If you decide to set `wrap` to `TRUE`, the function returns a `NetObject` instead of a raw `externalptr`. 
#' To remind an `externalptr` is returned only if no one native converter has been found.
#' The `NetObject R6` object wrapper can be an inherited `R6` class. For more details about 
#' inherited `NetObject` class please see `netGenerateR6` function. 
#' 
#' The `out_env` is usefull when the callee .Net method has some `out` or `ref` argument.
#' Because in .Net this argument set the given variable in the caller scope. We reflect this
#' mechanism in R. By default the given varable is modify in the parent `R environment` which means
#' the caller or `parent.frame()`. You can decide where to redirect the outputed value 
#' by specifying another `environment`. Of course be sure that the variable name exists in this 
#' targetd `environment`.
#'
#' @export
#' @examples
#' library(sharper)
#'
#' package_folder <- path.package("sharper")
#' netLoadAssembly(file.path(package_folder, "tests", "AssemblyForTests.dll"))
#' 
#' x <- netNew("AssemblyForTests.OneCtorData", 21L)
#' netCall(x, "ToString")
#' 
#' # wrap result
#' x <- NetObject$new(ptr = x)
#' clone <- netCall(x, "Clone", wrap = TRUE)
#' 
#' # out a variable
#' x <- netNew("AssemblyForTests.DefaultCtorData")
#' out_variable = 0
#' netCall(x, "TryGetValue", out_variable)
netCall <- function(x, methodName, ..., wrap = FALSE, out_env = parent.frame()) {
  
  if (any(as.logical(lapply(list(...), function(x) inherits(x, "NetObject"))))) {
    exp = substitute(list(...))
    if (length(exp) > 1) {
      src <- paste0(".External('rCallMethod', netUnwrap(x), '", methodName, "'")
      for (i in 2:length(exp)) {
        wrappedArg <- substitute(netUnwrap(x), list(x = exp[[i]]))
        src <- paste(sep = ", ", src, deparse(wrappedArg))
      }
      src <- paste(sep = ", ", src, "PACKAGE = 'sharper')")
      # print(src)
      results <- eval(parse(text = src), envir = parent.frame())
    } else {
      results <- .External("rCallMethod", netUnwrap(x), methodName, ..., PACKAGE = 'sharper')
    }
  } else {
    results <- .External("rCallMethod", netUnwrap(x), methodName, ..., PACKAGE = 'sharper')
  }
  
  if (wrap) results <- netWrap(results)
  
	if (length(results) > 1) {
	  args <- lapply(eval(substitute(alist(...))), deparse)
		for (i in seq_along(args)) {
			assign(args[[i]], results[[i + 1]], env = out_env)
		}
	} 
	
	return (results[[1]])
}
