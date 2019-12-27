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

# Copy the c++ dll into package dir
files <- Sys.glob(paste0("*", SHLIB_EXT))
dest <- file.path(R_PACKAGE_DIR, paste0('libs', R_ARCH))
print(sprintf("Copy the compiled C++ %s in %s", SHLIB_EXT, dest))
dir.create(dest, recursive = TRUE, showWarnings = FALSE)
file.copy(files, dest, overwrite = TRUE)

arch = R_ARCH
if(arch == "i386") {
  arch = "x86"
} else {
  arch = "x64"
}

# Check if dotnet is installed and intall it if not found
command = "dotnet"
has_to_install_dotnet = FALSE
if (WINDOWS) {
  if (system2("where", "dotnet") != 0L) {
    has_to_install_dotnet = TRUE
  }
} else {
  if (system2("command", c("-v", "dotnet")) != 0L) {
    has_to_install_dotnet = TRUE
  }
}

if (has_to_install_dotnet) {
  source(file.path(R_PACKAGE_SOURCE, "R", "install_dotnet_core.R"))
  dotnet_install_folder <- file.path(R_PACKAGE_DIR, "bin", "dotnet")
  install_dotnet_core(installDir = dotnet_install_folder, architecture = arch)
  command = file.path(dotnet_install_folder, arch, "dotnet.exe")
  
  print(paste0(
    "install done and exists: ", 
    file.exists(command),
    ", command",
    command))
  print(list.files(dotnet_install_folder, recursive = TRUE))
}


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