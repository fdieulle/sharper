#' NetObject class
#'
#' @description
#' NetObject R6 class to wrap an `externalptr``which represents a .Net object.
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
    
    #' @field Ptr `externalptr` of the wrapped .Net object
    Ptr = function(value) {
      if (missing(value))
        return(private$ptr)
      else
        invisible(private$ptr <- value)
    }
  ),
  public = list(
    
    #' @description 
    #' Create a new NetObject.
    #' 
    #' @param typeName .Net full type name.
    #' @param ptr `externalptr` of the .Net object.
    #' @param ... Property setters
    #' @return A new wrapped .Net object.
    #' 
    #' @details 
    #' You can use 2 ways to instanciate a .NetObject.
    #' If you specify the `externalptr` through the `ptr` parameter, 
    #' this pointer will be wrapped and stored into the `Ptr` active binding.
    #' Otherwise if your .Net class has a default constructor you can specify the `typeName`
    #' as `netNew(typeName)` does.
    #' 
    #' The ellipsis parameter can be use to setup the .Net properties. This feature
    #' works with both building ways.
    #' You can use it as follow `o <- NetObject$new(typeName, Name = "My name", Id = 1L)`
    initialize = function(typeName = NULL, ptr = NULL, ...) {
      
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
    
    #' @description 
    #' Gets a property value
    #' 
    #' @param propertyName Property name
    #' @param wrap Specify if you want to wrap a `externalptr` .Net object into a `NetObject` object. `TRUE`` by default.
    #' @return Returns the .Net property value. 
    #' If a converter has been defined between the .Net type and a `R` type, the `R` type will be returned.
    #' Otherwise an `externalptr` or a `NetObject` if `wrap` is set to `TRUE`.
    #' 
    #' @details
    #' Allows you to get a property value for a .Net object.
    #' The result will be converted if the type mapping is defined. All native C# types are mapped to R types
    #' but you can define custom converters in C# for that see the C# `RDotNetConverter` class.
    #' 
    #' By default `wrap` is set to `TRUE`. If there is no native convertion found between .Net type and R type, 
    #' the result is wrapped into `NetObject` instance.
    #' A best `R6` type can be chosen if this type exists. This `R6` class has to inherit from `NetObject` 
    #' and has to have the same name than the C# class. You can generate automatically this inherited class.
    #' For more details about this feature please see the `netGenerateR6` function. 
    #' If you prefer get a raw `externalptr` to the .Net object,
    #' 
    #' This function is aquivalent to call `netGet(o$Ptr, propertyName)`
    get = function (propertyName, wrap = TRUE) {
      return(netGet(private$ptr, propertyName, wrap = wrap))
    },
    
    #' @description 
    #' Sets a property value
    #' 
    #' @param propertyName Property name.
    #' @param value value to set
    #' 
    #' @details
    #' Allows you to set a property value of a .Net object.
    #' The input value will be converted from R type to a .Net type. 
    #' 
    #' If the property value isn't a native C# type or a mapped conversion type,
    #' you have to use an `externalptr` on .Net object or a `NetObject R6` instance.
    #' 
    #' You can define custom converters in C# for that see `RDotNetConverter` C# class.
    #' 
    #' This function is aquivalent to call `netSet(o$Ptr, propertyName, value)`.
    set = function (propertyName, value) {
      invisible(netSet(private$ptr, propertyName, value))
    },
    
    #' @description
    #' Call a .Net method member.
    #' 
    #' @param methodName Method name
    #' @param ... Method arguments
    #' @param wrap Specify if you want to wrap `externalptr` .Net object into `NetObject` `R6` object. `TRUE`` by default.
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
    #' This function is aquivalent to call `netCall(o$Ptr, methodName)`.
    call = function(methodName, ..., wrap = TRUE, out_env = parent.frame()) {
      return(netCall(private$ptr, methodName, ..., wrap = wrap, out_env = out_env))
    },
    
    #' @description 
    #' Cast the current R6 class to another.
    #' 
    #' @param className `R6` class name to cast.
    #' @return a new `R6` instance of type `className`.
    #'  
    #' @details
    #' If the `R6` `className` already exists the wrapped `externalptr` 
    #' which represents a .Net object will be transfer to a new `R6` 
    #' instance of type `className`.
    as = function(className) {
      return(get(className)$new(ptr = private$ptr))
    },
    
    #' @description 
    #' Gets `NetType` description of wrapped .Net object
    getType = function() {
      return(private$type)
    },
    
    #' @description 
    #' Print the object
    #' 
    #' @param ... .
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
#' print(sprintf("Name=%s, Namespace=%s, FullName=%s", 
#'   netType$Name, netType$Namespace, netType$FullName))
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
    #' @field Name .Net type name.
    Name = function(value) {
      if (missing(value)) {
        return(private$name)
      }
    },
    #' @field Namespace namespace of .Net type.
    Namespace = function(value) {
      if (missing(value)) {
        return(private$namespace)
      }
    },
    #' @field FullName .Net type name
    FullName = function(value) {
      if (missing(value)) {
        return(private$fullName)
      }
    }
  ),
  public = list(
    #' @description
    #' Create a new NetType object.
    #' @param namespace Namespace namespace of .Net type.
    #' @param name .Net type name.
    initialize = function(namespace, name) {
      private$namespace = namespace
      private$name = name
      private$fullName = paste(namespace, name, sep = ".")
    },
    #' @description 
    #' Create a new .Net object and wrap it.
    #' @param ... Ctor arguments of the .Net type
    createObject = function(...) {
      return (netWrap(netNew(private$fullName, ...)))
    }
  )
)
