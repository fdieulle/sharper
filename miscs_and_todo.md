## Todo list:

- [ ] Review the logger into ClrProxy because it logs in a file for now.
- [ ] Test if the GC moves .Net objects which are shared through ExternalPtr.
- [ ] Find a better test for disposing external ptr.
- [ ] Check if RClrProxy.def is useless and should use only the sharper-win.def only.
- [x] Turn publish dotnet in Release mode
- [ ] Assemblies lazy loading. To avoid calling `netLoadAssemblies` The `start_dotnet_clr` function should be able to define the application base directory and for .Net core can define different folder through `tpa_list` 
- [ ] Check if this CLR supports the .Net framework. Otherwise import the cpp code from `r.net` package. and implements a FrameworkClrHost.cpp
- [ ] Test to run a self contained application
- [ ] Check the package on Linux and Mac systems
- [ ] Create more details documentations through vignettes
  - [ ] .Net interactions
  - [ ] install/start/stop CLR 
  - [ ] data conversions
  - [ ] How to create custom packages base on .Net assemblies
- [ ] Add `netEvaluate` function in R package which compiles and run C# code.
- [ ] Add `netRun` function which allows run a .Net application.

#### Technical consideration

With the CLR loaded a .Net AppDomain is also loaded, so all constraints linked to it are kept. The main constraint is that an AppDomain can't unload an assembly. It can't also load an assembly a second time. So if you want to update an assembly loaded you have to restart your R process and reload all assemblies.

You can load a .Net core application which has been published with self-contained without this step.

## How compile sources

You can build the package by running the `Build.cmd` script file from windows.

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

