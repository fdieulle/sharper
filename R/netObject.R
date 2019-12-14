#' @title
#' NetObject class
#'
#' @description
#' NetObject class to wrap a .Net external pointer
#'
#' @section Properties:
#'
#' * `Ptr`: External pointer of the wrapped .Net object
#'
#' @section Methods:
#'
#' * `get(propertyName)`: Gets a property value
#' * `set(propertyName, value)`: Sets a property value
#' * `call(methodName, ...)`: Call a method
#' * `unwrap(value)`: Unwrap any NetObject or collection of NetObjects to external pointers
#' * `as(className)`: Cast the current R6 class to another by keeping the same .Net pointer
#' * `getType()`: Gets `NetType` description of wrapped .Net object
#'
#' @md
#' @export
#' @examples
#' \dontrun{
#' library(sharper)
#' package_folder <- path.package("sharper")
#' netLoadAssembly(file.path(package_folder, "tests", "AssemblyForTests.dll"))
#'
#' x <- netNew("AssemblyForTests.DefaultCtorData")
#' object <- NetObject$new(ptr = x)
#' object$set("Name", "My Name")
#' object$get("Name")
#' }
NetObject <- R6Class(
  "NetObject",
  private = list(
    ptr = NULL,
    type = NULL
  ),
  active = list(
    Ptr = function(value) {
      if (missing(value))
        return(private$ptr)
      else
        invisible(private$ptr <- value)
    }
  ),
  public = list(
    initialize = function(ptr = NULL, typeName = NULL, ...) {
      
      if (!is.null(ptr) && inherits(ptr, "externalptr")) {
        private$ptr <- ptr
      } else if(!is.null(typeName)) {
        private$ptr <- netNew(typeName)
      } else {
        stop("Give at least a ptr or a typeName parameter")
      }
      
      if (!is.null(private$ptr)) {
        type <- netCall(private$ptr, "GetType")
        private$type <- NetType$new(netGet(type, "Namespace"), netGet(type, "Name"))

        items <- list(...)
        for (name in names(items)) {
          self$set(name, items[[name]])
        }
      }
    },
    get = function (propertyName) {
      return(netGet(private$ptr, propertyName, wrap = TRUE))
    },
    set = function (propertyName, value) {
      invisible(netSet(private$ptr, propertyName, value))
    },
    call = function(methodName, ...) {
      return(netCall(private$ptr, methodName, ..., wrap = TRUE, out_env = parent.frame()))
    },
    as = function(className) {
      return(get(className)$new(ptr = private$ptr))
    },
    getType = function() {
      return(private$type)
    },
    print = function(...) {
      classes <- class(self)
      for (i in seq_along(tail(classes, -1))) {
        cat(classes[[i]], ": \n")
        propertyNames <- names(get(classes[[i]])$active)
        for(name in propertyNames) {
          value <- eval(parse(text = paste0("self$", name)))
          if (inherits(value, "externalptr"))
            value <- "externalptr"
          cat("  ", name, ": ", value, "\n")  
        }
      }
    }
  )
)

#' @title
#' NetType class
#'
#' @description
#' NetType provide the name and namespace of a .Net type
#'
#' @section Properties:
#'
#' * `Name`: .Net type
#' * `Namespace`: .Net type namespace
#' * `FullName`: Full .Net type
#'
#' @md
#' @export
#' @examples
#' \dontrun{
#' library(sharper)
#' package_folder <- path.package("sharper")
#' netLoadAssembly(file.path(package_folder, "tests", "AssemblyForTests.dll"))
#'
#' netType <- NetType$new("AssemblyForTests", "ManyCtorData")
#' print(sprintf("Name=%s, Namespace=%s, FullName=%s", netType$Name, netType$Namespace, netType$FullName))
#'
#' object <- netType$createObject(21L)
#' object$get("Id")
#' }
NetType <- R6Class(
  "NetType",
  private = list(
    name = NULL,
    namespace = NULL,
    fullName = NULL
  ),
  active = list(
    Name = function(value) {
      if (missing(value)) {
        return(private$name)
      }
    },
    Namespace = function(value) {
      if (missing(value)) {
        return(private$namespace)
      }
    },
    FullName = function(value) {
      if (missing(value)) {
        return(private$fullName)
      }
    }
  ),
  public = list(
    initialize = function(namespace, name) {
      private$namespace = namespace
      private$name = name
      private$fullName = paste(namespace, name, sep = ".")
    },
    createObject = function(...) {
      return (NetObject$new(ptr = netNew(private$fullName, ...)))
    }
  )
)
