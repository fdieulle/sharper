# How to interact with static methods and properties

Once your assemblies are loaded in your process you can start to interact with them. A simple entry point is to use static call. The package provides you 3 functions for that

* [`netCallStatic(typeName, methodName, ...)`](#netCallStatic): Call a static method for a given .Net type name
* [`netGetStatic(typeName, propertyName)`](#netGetStatic): Gets a static property value from a .Net type name
* [`netSetStatic(typeName, propertyName, value)`](#netSetStatic): Sets a static property value from a .Net type name

## netCallStatic

Call a static method .Net method for a given .Net type name.

### Parameters

- `typeName`: Full .Net type name 
- `methodName`: Method name to call
- `...`: Method arguments
- `wrap`: Specify if you want to wrap `externalptr` .Net object into a `R6` `NetObject`. `FALSE` by default.
- `out_env`: In case of the .Net method defines an `out` or `ref` argument, specify on which `R` `environment` you want to out put the value. By default it's the caller  `environment` i.e. `parent.frame()`.

### Return

Returns the .Net result. If a converter has been defined between the .Net type and a `R` type, the `R` type will be returned. Otherwise an `externalptr` or a `NetObject` if `wrap` is set to `TRUE`.

### Details

Call a static method for a given .Net type name. 

Ellipses has to keep the .net arguments method order, the named arguments are not supported yet. If there is conflicts with a method name (many definition in .Net), a score is computed from your argument's order and type. We consider a higher score single value comparing to collection of values.

If you decide to set `wrap` to `TRUE`, the function returns a `NetObject` instead of a raw `externalptr`. To remind an `externalptr` is returned only if no one native converter has been found. The `NetObject R6` object wrapper can be an inherited `R6` class. For more details about inherited `NetObject` class please see `netGenerateR6` function. 

The `out_env` is useful when the callee .Net method has some `out` or `ref` argument. Because in .Net this argument set the given variable in the caller scope. We reflect this mechanism in R. By default the given variable is modify in the parent `R environment` which means the caller or `parent.frame()`. You can decide where to redirect the output value by specifying another `environment`. Of course be sure that the variable name exists in this targeted `environment`.

### Examples

````R
# Load the package and start the dotnet CLR
library(sharper)

# Load a dotnet assembly into the CLR
pkgPath <- path.package("sharper")
f <- file.path(pkgPath, "tests", "AssemblyForTests.dll")
netLoadAssembly(f)

# Call some static dotnet methods
type <- "AssemblyForTests.StaticClass"
netCallStatic(type, "CallWithInteger", 2L)
netCallStatic(type, "CallWithIntegerVector", c(2L, 3L))

# Method selection single value vs vector values
netCallStatic(type, "SameMethodName", 1.23)
netCallStatic(type, "SameMethodName", c(1.24, 1.25))
netCallStatic(type, "SameMethodName", c(1.24, 1.25), 12L)

# wrap result
x <- NetObject$new(ptr = netNew("AssemblyForTests.DefaultCtorData"))
clone <- netCallStatic(type, "Clone", x, wrap = TRUE)

# out a variable
out_variable = 0
netCallStatic(type, "TryGetValue", out_variable)
````



## netGetStatic

Gets a static property value for a given .Net type name.

### Parameters

- `typeName`: Full .Net type name 
- `propertyName`: Property name to get value
- `wrap`: Specify if you want to wrap `externalptr` .Net object into a `R6` `NetObject`. `FALSE` by default.

### Return

Returns the .Net result. If a converter has been defined between the .Net type and a `R` type, the `R` type will be returned. Otherwise an `externalptr` or a `NetObject` if `wrap` is set to `TRUE`.

### Details

Allows you to get a static property value from a .Net type name. The result will be converted if the type mapping is defined. All native C# types are mapped to R types but you can define custom converters in C# for that see the C# `RDotNetConverter` class.

If you decide to set `wrap` to `TRUE` this function supports the `NetObject R6` class and all inherited. The function result if no converter has been found will return a `NetObject` of an inherited best type instead of a raw `externalptr`. For more details about inherited `NetObject` class please see `netGenerateR6` function. 

### Examples

````R
# Load the package and start the dotnet CLR
library(sharper)

# Load a dotnet assembly into the CLR
pkgPath <- path.package("sharper")
f <- file.path(pkgPath, "tests", "AssemblyForTests.dll")
netLoadAssembly(f)

# Get some scalar property value
type <- "AssemblyForTests.StaticClass"
x <- netGetStatic(type, "DoubleProperty")
y <- netGetStatic(type, "Int32Property")

# Get some vector property values
xx <- netGetStatic(type, "DoubleArrayProperty")
yy <- netGetStatic(type, "Int32ArrayProperty")
````



## netSetStatic

Sets a static property value for a given .Net type name.

### Parameters

- `typeName`: Full .Net type name 
- `propertyName`: Property name to set value
- `value`: Value to set

### Details

Allows you to set a property value from .Net type name. The input value will be converted from R type to a .Net type if a converter exists. If the property value isn't a native C# type or a mapped conversion type you have to use a wrapped `NetObject` or whichever inherited class. If none of this type cannot be provide you have to provide an external pointer on .Net object. 

You can define custom converters in C# for that see [`RDotNetConverter`](https://github.com/fdieulle/sharper/docs/custom-converters.md) class.

### Examples

```R
# Load the package and start the dotnet CLR
library(sharper)

# Load a dotnet assembly into the CLR
pkgPath <- path.package("sharper")
f <- file.path(pkgPath, "tests", "AssemblyForTests.dll")
netLoadAssembly(f)

# Set some scalar property value
type <- "AssemblyForTests.StaticClass"
netSetStatic(type, "DoubleProperty", 1.23)
netGetStatic(type, "DoubleProperty")

netSetStatic(type, "Int32Property", 123L)
netGetStatic(type, "Int32Property")

# Set some vector property values
netSetStatic(type, "DoubleArrayProperty", c(1.23, 1.24, 1.25))
netGetStatic(type, "DoubleArrayProperty")

netSetStatic(type, "Int32ArrayProperty", c(123L, 124L, 125L))
netGetStatic(type, "Int32ArrayProperty")
```

