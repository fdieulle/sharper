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


files <- Sys.glob(paste0("*", SHLIB_EXT))
dest <- file.path(R_PACKAGE_DIR, paste0('libs', R_ARCH))
dir.create(dest, recursive = TRUE, showWarnings = FALSE)
file.copy(files, dest, overwrite = TRUE)