# Sharper
R package which allowing the use of .Net Core and .Net Framework from R as well as embedding R into .Net runtime

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

You can load a .Net core application which has been published with self-contained without this step.

## Compile sources

You can build the package by running the `Build.cmd` script file.

or by executing this 2 following steps:
```
rscript -e devtools::document()
r CMD INSTALL . --build --clean
```

### Prerequist

* **R** installed on your machine with the `R_HOME` environment variable defined which targets your current R version.
* **dotnet** installed. It's the CLI for .Net environments.

### Compile with Visual Studio

Before the first compilation you need to run this script: `src\prepare-vs-build.cmd` 
and be sure that the variable at the begin of the script are valid with your machine setup.

```
SET R_HOME=%R_HOME%
SET RTOOLS=C:\Program Files\R\Rtools
SET VS_VC_BUILD="C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\VC\Auxiliary\Build"
```


