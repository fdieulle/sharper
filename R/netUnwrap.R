#' @title 
#' Unwrap `NetObject` from `externalptr`
#'
#' @description
#' Unwrap a `R6` `NetObject` instance to a .Net object `externalptr`.
#'
#' @param object `NetObject` instance or a `list`.
#' @return Returns the unwrapped `R6 NetObject` instance to .Net `externalptr`.
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
#' object <- NetObject$new(ptr = x)
#' ptr <- netUnwrap(object)
#' 
#' l <- list(
#'   object, 
#'   c(12, 13), 
#'   NetObject$new(typeName = "AssemblyForTests.DefaultCtorData"),
#'   netNew("AssemblyForTests.DefaultCtorData"))
#' ptrs <- netUnwrap(l)
#' }
netUnwrap <- function(object) {
  if (inherits(object, "NetObject")) {
    return(object$Ptr)
  } else if (is.list(object) && length(object) > 0) {
    for (i in seq_along(object)) {
      object[[i]] <- netUnwrap(object[[i]])
    }
  }
  return(object)
}
