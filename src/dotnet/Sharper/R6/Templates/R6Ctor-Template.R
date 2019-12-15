initialize = function ({{parameters}}{{comma}}ptr = NULL, ...) {
  if (!is.null(ptr) && inherits(ptr, "externalptr")) {
    super$initialize(ptr = ptr, ...)
  } else if({{CanBeInstantiated}}) {
    ptr = netNew("{{FullTypeName}}"{{comma}}{{parameters}})
    super$initialize(ptr = ptr, ...)
  } else {
    stop("Impossible to instanciate a .Net object of type {{FullTypeName}}.\nBecause it's an abstract class or an interface.")
  }
}