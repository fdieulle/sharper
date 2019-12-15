#' @description 
#' {{Description}}
#' {{Parameters_doc}}
#' @return {{Return_doc}}
initialize = function ({{Parameters}}{{Comma}}ptr = NULL, ...) {
  if (!is.null(ptr) && inherits(ptr, "externalptr")) {
    super$initialize(ptr = ptr, ...)
  } else if({{CanBeInstantiated}}) {
    ptr = netNew("{{FullTypeName}}"{{Comma}}{{Parameters}})
    super$initialize(ptr = ptr, ...)
  } else {
    stop("Impossible to instanciate a .Net object of type {{FullTypeName}}.\nBecause it's an abstract class or an interface.")
  }
}