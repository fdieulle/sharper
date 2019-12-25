# Sharper
[![Build Status](https://api.travis-ci.com/fdieulle/sharper.svg?branch=master)](https://travis-ci.com/fdieulle/sharper)

Sharper is a R package which allows to use .Net assemblies from your R environment.

The goal of this package was driven by the idea to use production and industrial code implemented in .Net into a R environment, gets results in R to process statistics exploration, analysis or research.

The package provides you tools to interact with your .Net assemblies. A large basic data conversions between R and .Net are included and the package allows you to enrich or override data converters. A part of those conversions are based on [R.Net](https://github.com/rdotnet/rdotnet) project. Thank you to the creator Jean-Michel Perraud and all contributors for this repository. 

Because of .Net is a POO language and you need to manipulate objects in R, the package provides also a R6 class generator from .Net classes.

## Installation

You can install the latest version of sharper from CRAN:

``` R
install.packages("sharper") # Not available yet
```

or the development version from GitHub using devtools:

``` R
devtools::install_github("fdieulle/sharper")
```

then load the package

``` R
library(sharper)
```

### .Net Core

You can install the latest .Net Core version for both environments `x86` and `x64` as follow:

``` R
install_dotnetCore()
```

Once this installation is proceed the settings choose will be save and your chosen .Net environment will be automatically loaded when the package will with `library(sharper)`

## Getting started

### Load an assembly

The function `netLoadAssembly(filePath)` loads a .Net assembly in the CLR (Common Language Runtime).

If your custom library contains other assemblies as dependencies, I advise you to use this following code to load all of them.

```R
library(sharper)
path <- "Your path where dlls are stored"
assembiles <- list.files(path = path, pattern = "*.dll$|*.exe$", full.names = TRUE)
lapply(assemblies, netLoadAssembly)
```

When the `sharper` package is loaded it contains by default all the .Net framework and access on your GAC stored assemblies. So you don't need to load them manually.

### How to interact with static methods and properties

Once your assemblies are loaded in your process you can start to interact with them. A simple entry point is to use static call. The package provides you 3 functions for that

* `netCallStatic(typeName, methodName, ...)`: Call a static method for a given .Net type name
* `netGetStatic(typeName, propertyName)`: Gets a static property value from a .Net type name
* `netSetStatic(typeName, propertyName, value)`: Sets a static property value from a .Net type name

For more details about the static interactions [see](https://github.com/fdieulle/sharper/docs/net-interactions.md)

### How to interact with .Net objects

The package allows you to create and manage .Net objects from R. A .Net object is stored in an R `externalptr` which represents the address in the CLR. 

To create and manage .Net objects from R you can use the following R functions:

* `netNew`: Create an object from its .Net type name
* `netCall`: Call a member method for a given external pointer of a .Net object.
* `netGet`: Gets property value from a given external pointer of .Net object.
* `netSet`: Gets property value from a given external pointer of .Net object.

For more details about the static interactions [see](https://github.com/fdieulle/sharper/docs/net-interactions.md)

To easily manipulate this .Net objects you can wrap them into a R6 class named `NetObject`. This class provides you some function as follow:

* `get`: Gets a property value
* `set`: Sets a property value
* `call`: Call a member method
* `as`: Cast the current `R6Class` to another `R6Class` which has to inherit from `NetObject` by keeping the same `externalptr`.

For more details about the static interactions [see](https://github.com/fdieulle/sharper/docs/net-interactions.md)

### How to generate R6 classes from .Net types

Because .Net is an OOP language you can have a lot of classes to use in your R scripts. The package provides you a function which generates R6 classes from .Net types.

* `netGenerateR6(typeNames, file)`: Generate R6 classes from .Net types

This generator respects the class hierarchy and also generate an `roxygen2` syntax for your custom package documentations. `roxygen2` supports R6 class documentation since the version 7.

### How to debug

During the development step of your projects it's always helpful to debug your code. the .Net code can be easily debugged with your Visual Studio or another IDE. For R I like to use the [`restorepoint`](https://github.com/skranz/restorepoint).

#### Debug from R

#### Debug from C#