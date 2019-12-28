# If a package wants to install other binaries (for example executable programs),
# it should provide an R script src/install.libs.R which will be run as part of the installation
# in the src build directory instead of copying the shared objects/DLLs.
# The script is run in a separate R environment containing the following variables:
#
# R_PACKAGE_NAME: the name of the package
# R_PACKAGE_SOURCE: the path to the source directory of the package
# R_PACKAGE_DIR: the path of the target installation directory of the package
# R_ARCH: the arch-dependent part of the path, often empty
# SHLIB_EXT: the extension of shared objects
# WINDOWS: TRUE on Windows, FALSE elsewhere

print("Run install.libs.R script ...")
print(paste0("R_PACKAGE_NAME: ", R_PACKAGE_NAME))
print(paste0("R_PACKAGE_SOURCE: ", R_PACKAGE_SOURCE))
print(paste0("R_PACKAGE_DIR: ", R_PACKAGE_DIR))
print(paste0("R_ARCH: ", R_ARCH))
print(paste0("SHLIB_EXT: ", SHLIB_EXT))
print(paste0("WINDOWS: ", WINDOWS))

arch = R_ARCH
if(arch == "i386") {
  arch = "x86"
} else {
  arch = "x64"
}

# Copy the c++ dll into package dir
files <- Sys.glob(paste0("*", SHLIB_EXT))
dest <- file.path(R_PACKAGE_DIR, paste0('libs', arch))
print(sprintf("Copy the compiled C++ %s in %s", SHLIB_EXT, dest))
dir.create(dest, recursive = TRUE, showWarnings = FALSE)
file.copy(files, dest, overwrite = TRUE)

# install the dotnet sdk
dotnet_install_folder <- file.path(R_PACKAGE_DIR, "bin", "dotnet", arch)
if (WINDOWS) {
  commandLine <- "[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; &([scriptblock]::Create((Invoke-WebRequest -useb 'https://dot.net/v1/dotnet-install.ps1')))"
  arguments <- paste("-InstallDir", dotnet_install_folder, sep = ' ')
  system2("powershell", 
    args = c(
      "-NoProfile",
      "-ExecutionPolicy", "unrestricted",
      "-Command", 
      paste(commandLine, arguments, sep = ' ')))
} else {
  commandLine <- "https://dot.net/v1/dotnet-install.sh | bash /dev/stdin"
  arguments <- paste("-InstallDir", dotnet_install_folder, sep = ' ')
  system2("curl", 
    args = c(
      "-sSL",
      paste(commandLine, arguments, sep = ' ')))
}

# Build csharp projects
command = file.path(dotnet_install_folder, "dotnet")
configuration = "Release"

print("Publish the Sharper dotnet project")
publish_args <- c(
  "publish",
  file.path(R_PACKAGE_SOURCE, "src", "dotnet", "Sharper", "Sharper.csproj"),
  "-o", file.path(R_PACKAGE_SOURCE, "inst", "bin"),
  "-c", configuration)
system2(command, publish_args)

print("Publish the dotnet test assembly for unit tests")
publish_args <- c(
  "publish",
  file.path(R_PACKAGE_SOURCE, "tests", "dotnet", "AssemblyForTests", "AssemblyForTests.csproj"),
  "-o", file.path(R_PACKAGE_SOURCE, "inst", "tests"),
  "-c", configuration, 
  "--no-dependencies")
system2(command, publish_args)