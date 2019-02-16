dotnetCorePath <- "C:/Program Files/dotnet/shared/Microsoft.NETCore.App/2.1.6"
packageFolder <- "C:/OtherDrive/Workspace/Git/fdieulle/sharper"
lib <- file.path(packageFolder, "inst", "bin")
nativeLib <- file.path(lib, "x64", "ClrHost.dll")
nativeLib <- file.path(packageFolder, "src", "sharper.dll")

dyn.load(nativeLib)
.C("rStartClr", lib, dotnetCorePath, PACKAGE = "sharper.dll")

dll <- file.path(packageFolder, "inst", "tests", "AssemblyForTests.dll")
.C("rLoadAssembly", dll)

typeName = "AssemblyForTests.StaticClass"
methodName = "SameMethodName"
result <- .External("rCallStaticMethod", typeName, methodName)

result <- .External("rCallStaticMethod", typeName, methodName)

result <- .External("rCallStaticMethod", typeName, methodName, 1L)
result <- .External("rCallStaticMethod", typeName, methodName, 1.23)
result <- .External("rCallStaticMethod", typeName, methodName, c(2L, 3L))
result <- .External("rCallStaticMethod", typeName, methodName, c(1.24, 1.25))

result <- .External("rCallStaticMethod", typeName, methodName, 2.13, 1L)
result <- .External("rCallStaticMethod", typeName, methodName, 2.14, c(2L, 3L))
result <- .External("rCallStaticMethod", typeName, methodName, c(1.24, 1.25), 14L)
result <- .External("rCallStaticMethod", typeName, methodName, c(1.24, 1.25), c(14L, 15L))


.C("rShutdownClr")
dyn.unload(nativeLib)
