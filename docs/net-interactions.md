# Interact with .Net objects

The package allows you to create and manage .Net objects from R. A .Net object is stored in an R `externalptr` which represents the address in the CLR. 

To create and manage .Net objects from R you can use the following R functions:

* [`netNew`](#netNew): Create an object from its .Net type name
* [`netCall`](#netCall): Call a member method for a given external pointer of a .Net object.
* [`netGet`](#netGet): Gets property value from a given external pointer of .Net object.
* [`netSet`](#netSet): Gets property value from a given external pointer of .Net object.

## netNew

Instantiate a .Net object for a given type name.

### Parameters

- `typeName`: The .Net full name type
- `...`: .Net Constructor arguments.

### Return

Returns a converted .Net instance if a converter is defined, an external pointer otherwise.

### Details

The `typeName` should respect the full type name convention: `Namespace.TypeName`. Ellipses `...` has to keep the .net constructor arguments order, the named arguments are not supported yet. If there is many constructors defined for the given .Net type, a score selection is computed from your arguments orders and types to choose the best one. We consider as a higher priority single value compare to collection of values.

### Examples

```R
# Load the package and start the dotnet CLR
library(sharper)

# Load a dotnet assembly into the CLR
package_folder <- path.package("sharper")
netLoadAssembly(file.path(package_folder, "tests", "RDotNet.AssemblyTest.dll"))

# Instantiate a .Net object
x <- netNew("RDotNet.AssemblyTest.OneCtorData", 21L)
netCall(x, "ToString")
```



## netCall

Call a .Net method member for a given a .Net object.

### Parameters

- `x`: a .Net object, which can be an `externalptr` or a `NetObject`.
- `methodName`: Method name to call
- `...`: Method arguments
- `wrap`: Specify if you want to wrap `externalptr` .Net object results into `NetObject` `R6` object. `FALSE` by default.
- `out_env`: In case of a .Net method with `out` or `ref` argument is called, specify on which `environment` you want to out put this arguments. By default it's the caller `environment` i.e. `parent.frame()`.

### Return

Returns the .Net result. If a converter has been defined between the .Net type and a `R` type, the `R` type will be returned. Otherwise an `externalptr` or a `NetObject` if `wrap` is set to `TRUE`.

### Details

Call a method member for a given .Net object. Ellipses has to keep the .Net arguments method order, the named arguments are not yet supported. If there is conflicts with a method name (many definition in .Net), a score is computed from your argument's order and type. We consider a higher score single value comparing to collection of values.

If you decide to set `wrap` to `TRUE`, the function returns a `NetObject` instead of a raw `externalptr`. To remind an `externalptr` is returned only if no one native converter has been found. The `NetObject R6` object wrapper can be an inherited `R6` class. For more details about inherited `NetObject` class please see `netGenerateR6` function. 

The `out_env` is useful when the callee .Net method has some `out` or `ref` argument. Because in .Net this argument set the given variable in the caller scope. We reflect this mechanism in R. By default the given variable is modify in the parent `R environment` which means the caller or `parent.frame()`. You can decide where to redirect the output value by specifying another `environment`. Of course be sure that the variable name exists in this targeted `environment`.

### Examples

```R
# Load the package and start the dotnet CLR
library(sharper)

# Load a dotnet assembly into the CLR
package_folder <- path.package("sharper")
netLoadAssembly(file.path(package_folder, "tests", "RDotNet.AssemblyTest.dll"))

x <- netNew("AssemblyForTests.OneCtorData", 21L)
netCall(x, "ToString")

# wrap result
x <- NetObject$new(ptr = x)
clone <- netCall(x, "Clone", wrap = TRUE)

# out a variable
x <- netNew("AssemblyForTests.DefaultCtorData")
out_variable = 0
netCall(x, "TryGetValue", out_variable)
```



## netGet

Gets a property value from a .Net object

### Parameters

- `x`: a .Net object, which can be an `externalptr` or a `NetObject`.
- `propertyName`: Property name to get value
- `wrap`: Specify if you want to wrap `externalptr` .Net object into `NetObject` `R6` object. `FALSE` by default.

### Return

Returns the .Net property value. If a converter has been defined between the .Net type and a `R` type, the `R` type will be returned. Otherwise an `externalptr` or a `NetObject` if `wrap` is set to `TRUE`.

### Details

Allows you to get a property value for a .Net object. The result will be converted if the type mapping is defined. All native C# types are mapped to R types but you can define custom converters in C# for that see the C# `RDotNetConverter` class.

If you decide to set `wrap` to `TRUE` this function supports the `NetObject R6` class and all inherited. The function result if no converter has been found will return a `NetObject` of an inherited best type instead of a raw `externalptr`. For more details about inherited `NetObject` class please see `netGenerateR6` function.

### Examples

```R
# Load the package and start the dotnet CLR
library(sharper)

# Load a dotnet assembly into the CLR
package_folder <- path.package("sharper")
netLoadAssembly(file.path(package_folder, "tests", "RDotNet.AssemblyTest.dll"))

x <- netNew("AssemblyForTests.DefaultCtorData")
netSet(x, "Name", "foo")
x_name <- netGet(x, "Name")

netSet(x, "Integers", c(12L, 23L))
x_integers <- netGet(x, "Integers")
```



## netSet

Sets a property value from a .Net object

### Parameters

- `x`: a .Net object, which can be an `externalptr` or a `NetObject`.
- `propertyName`: Property name to get value
- `value`: Value to set.

### Details

Allows you to set a property value of a .Net object. The input value will be converted from R type to a .Net type if a converter exists. If the property value isn't a native C# type or a mapped conversion type you have to use a wrapped `NetObject` or whichever inherited class. If none of this type cannot be provide you have to provide an external pointer on .Net object. 

You can define custom converters in C# for that see [`RDotNetConverter`](https://github.com/fdieulle/sharper/blob/master/docs/custom-converters.md) class.

### Examples

```R
# Load the package and start the dotnet CLR
library(sharper)

# Load a dotnet assembly into the CLR
package_folder <- path.package("sharper")
netLoadAssembly(file.path(package_folder, "tests", "RDotNet.AssemblyTest.dll"))

x <- netNew("AssemblyForTests.DefaultCtorData")
netSet(x, "Name", "foo")
x_name <- netGet(x, "Name")

netSet(x, "Integers", c(12L, 23L))
x_integers <- netGet(x, "Integers")
```





