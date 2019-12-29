#' @title 
#' Wrap `externalptr` into `NetObject`
#'
#' @description
#' Wrap .Net object `externalptr` into a `R6` `NetObject` instance.
#'
#' @param ptr a .Net object `externalptr` or a `list`.
#' @return Returns the wrapped .Net `externalptr` into `R6 NetObject` instance.
#' 
#' @details
#' todo
#' 
#' @export
#' @examples
#' \dontrun{
#' library(sharper)
#'
#' package_folder <- path.package("sharper")
#' netLoadAssembly(file.path(package_folder, "tests", "AssemblyForTests.dll"))
#' 
#' x <- netNew("AssemblyForTests.OneCtorData", 21L)
#' object <- netWrap(x)
#' 
#' l <- list(
#'   x, 
#'   c(12, 13), 
#'   NetObject$new(ptr = netNew("AssemblyForTests.DefaultCtorData")),
#'   netNew("AssemblyForTests.DefaultCtorData"))
#' objects <- netWrap(l)
#' }
netWrap = function(ptr)
{
  if (inherits(ptr, "externalptr")) {
    r6_type_names <- netCallStatic("Sharper.ClrProxy", "GetHierarchyTypeNames", ptr)
    for (r6_type_name in r6_type_names) {
      if (!is.null(r6_type_name) && exists(r6_type_name)) {
        r6_class <- get(r6_type_name)
        if (R6::is.R6Class(r6_class)) {
          return(r6_class$new(ptr = ptr))
        }
      }
    }
    
    return(NetObject$new(ptr = ptr))  
  } else if(is.list(ptr) && length(ptr) > 0) {
    for (i in seq_along(ptr)) {
      if (!is.null(ptr[[i]])) {
        ptr[[i]] <- netWrap(ptr[[i]])
      }
    }
  }
  
  return(ptr)
}
