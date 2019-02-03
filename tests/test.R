

dotnetCorePath <- "C:/Program Files/dotnet/shared/Microsoft.NETCore.App/2.1.6"

packageFolder <- "C:/OtherDrive/Workspace/Git/fdieulle/sharper"
lib <- file.path(packageFolder, "inst", "libs")

nativeLib <- file.path(lib, "x64", "ClrHost.dll")


dyn.load(nativeLib)

.C("rStartClr", lib, dotnetCorePath)
#.External("loadAssembly", "NULL")

#.C("rShutdownClr")

dyn.unload(nativeLib)
