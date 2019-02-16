# Sharper
R package which allowing the use of .Net Core and .Net Framework from R as well as embedding R into .Net runtime

## Installation

You can install the latest version of sharper from CRAN:

``` R
install.packages("sharper") # to be tested
```

or the development version from GitHub using devtools:

``` R
devtools::install_github("fdieulle/sharper") # to be tested
```

then load the package

``` R
library(sharper)
```

### .Net core

You can install the latest .Net Core version for both environments `x86` and `x64` as follow:

``` R
install_dotnetCore()
```

You can load a .Net core application which has been published with self-contained without this step.