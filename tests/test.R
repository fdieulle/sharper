dotnetCorePath <- "C:/Program Files/dotnet/shared/Microsoft.NETCore.App/2.1.6"
packageFolder <- "C:/OtherDrive/Workspace/Git/fdieulle/sharper"
lib <- file.path(packageFolder, "inst", "libs")
nativeLib <- file.path(lib, "x64", "ClrHost.dll")

dyn.load(nativeLib)
.C("rStartClr", lib, dotnetCorePath)

dll <- file.path(packageFolder, "inst", "tests", "AssemblyForTests.dll")
.C("rLoadAssembly", dll)

typeName = "AssemblyForTests.StaticClass"
methodName = "SameMethodName"
result <- .External("rCallStaticMethod", typeName, methodName)

.C("rShutdownClr")
dyn.unload(nativeLib)
