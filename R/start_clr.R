# @title 
# Gets the dotnet core installation folder
# 
# @details
# Gets the dotnet core installation folder.
# We try first to load the installation folder from the settings file.
# This settings file is updated by the install_dotnet_core function.
# If we can't found this settings we try to retreived the dotnet installation folder from the default system installation.
#
get_dotnet_core_install_folder <- function() {
	
	# Load settings
	settings <- load_settings()
	arch <- Sys.getenv('R_ARCH')
	
	# Targets the good arch
	if (arch == "/i386")  {
	  arch = "x86"
	} else if (arch == "/x64") {
	  arch = "x64"
	} 
	install_folder <- settings[[arch]]
	
	# Try the default system install folder
	if(is.null(install_folder) || !file.exists(install_folder)) 
	{
		install_folder<- file.path(Sys.getenv("LocalAppData"), "Microsoft", "dotnet")
	  
		if(file.exists(install_folder))
		{
			if (arch == "/i386") {
				install_folder <- file.path(install_folder, "x86")
			} else if (arch == "/x64") {
				install_folder <- file.path(install_folder, "x64")
			} else install_folder <- NULL
		} 
		else 
		{
			os <- Sys.info()["sysname"]
			if (os == "windows") {
				install_folder <- "C:/Program Files/dotnet"
			} else if (os == "macOS") {
				install_folder <- "/usr/local/share/dotnet"
			} else install_folder <- "/usr/share/dotnet"
		}
	}
	
	# Unable to find the install folder so process an installation
	if(is.null(install_folder) || !file.exists(install_folder)) {
		
	  install_folder <- file.path(path.package("sharper"), "bin", "dotnet")
	  print(install_folder)
	  install_dotnet_core(installDir = install_folder, architecture = arch)
	  install_folder <- file.path(install_folder, arch)
	  print(install_folder)
	} 
	
	return(path.expand(install_folder))
}

# @title 
# Gets the dotnet core runtime installation folder
# 
# @details
# Gets the dotnet core runtime installation folder.
# The runtime and the version can be specifed. For the version we apply the following heuristic:
# If the value 'latest' or NULL is defined we gets the latest version installed for the given runtime.
# Otherwise we try to match the version in this order: 
#	* Perfect match
#	* Higher Major.Minor version found
#  * Higher Major version found. This laste try cannot gurantee some breaking changes or other border effects.
#
# @param runtime Select the shared runtime. The possible values are:
# 		* `dotnet` - the `Microsoft.NETCore.App` shared runtime (default).
# 		* `aspnetcore` - the `Microsoft.AspNetCore.App` shared runtime.
# @param version Select the shared runtime version. `latest` by default. For more details see details section
# 
get_dotnet_core_runtime_folder <- function(runtime = "dotnet", version = "latest") {
  
	install_folder = get_dotnet_core_install_folder()
	if (is.null(install_folder))
		return(NULL)

	# Build the install folder with the selected runtime
	install_folder <- file.path(install_folder, "shared")
	if (runtime == "dotnet" || runtime == "Microsoft.NETCore.App") {
		install_folder <- file.path(install_folder, "Microsoft.NETCore.App")
	} else if (runtime  == "aspnetcore" || "Microsoft.AspNetCore.App") {
		install_folder <- file.path(install_folder, "Microsoft.AspNetCore.App")
	} else if (runtime == "Microsoft.AspNetCore.All") {
		install_folder <- file.path(install_folder, "Microsoft.AspNetCore.App")
	} else stop("The runtime value isn't supported: ", runtime)

	# Load all installed versions and sort them
	version_folders <- list.dirs(install_folder, full.names = FALSE, recursive = FALSE)
	if (length(version_folders) <= 0)
		return(NULL)

	filter_versions <- version_folders[grepl("^(\\d+\\.)?(\\d+\\.)?(\\d+\\.)?(\\*|\\d+)$", version_folders)]
	sorted_versions <- sort(numeric_version(filter_versions))

	# If the latest version was choosen
	if (is.null(version) || version == "latest") 
	{
		latest <- sorted_versions[[length(sorted_versions)]]
		version <- as.character(latest)
	} 
	else # Try to find the closest compatible version 
	{
		# First try with a perfect match
		candidate_version <- version_folders[version_folders == version]
		
		# Second try with the higher major.minor matched version.
		if (is.null(candidate_version))
		{
			target_version <- package_version(version)
			versions <- package_version(sorted_versions)
			matched_versions <- versions[versions$major == target_version$major & versions$minor == target_version$minor]
			
			if (length(matched_versions) > 0)
				candidate_version <- matched_versions[length(matched_versions)]
			else # Third try with the higher major matched version (can have breaking changes)
			{
				matched_versions <- versions[versions$major == target_version$major]
				if (length(matched_versions) > 0)
				{
					candidate_version <- matched_versions[length(matched_versions)]
					print("Only a major version can match your version.\nSome breaking changes can occurs.\nVersion requested: %s\nVersion used     : %s", version, as.character(candidate_version))
				}
				else print("No version matched for runtime: ", runtime, " and version: ", version)
			}
		}
		
		version <- as.character(candidate_version)
	}

	install_folder <- file.path(install_folder, version)

	if (!file.exists(install_folder))
		return(NULL)
	
	return(install_folder)
}

#' @title 
#' Start dotnet core runtime from an application base directory.
#' 
#' @details
#' Starts a dotnet core runtime from an application base directory.
#' If the application base directory isn't self contained, the runtime and the version can be specifed.
#' For the version we apply the following heuristic:
#' If the value 'latest' or NULL is defined we gets the latest version installed for the given runtime.
#' Otherwise we try to match the version in this order: 
#'	* Perfect match
#'	* Higher Major.Minor version found
#'  * Higher Major version found. This laste try cannot gurantee some breaking changes or other border effects.
#'
#' @param app_base_dir defines the application base directory where the dlls are stored
#' @param runtime Select the shared runtime. The possible values are:
#' 		* `dotnet` - the `Microsoft.NETCore.App` shared runtime (default).
#' 		* `aspnetcore` - the `Microsoft.AspNetCore.App` shared runtime.
#' @param version Select the shared runtime version. `latest` by default. For more details see details section
#' 
#' @export
start_dotnet_core_clr <- function(app_base_dir = NULL, runtime = "dotnet", version = "latest") {
	
	package_name <- "sharper"
	package_folder <- system.file(package = package_name)

	if (is.null(app_base_dir)) 
		app_base_dir = package_folder
		
	package_bin_folder <- file.path(package_folder, "bin")
	dotnet_core_folder <- as.character(get_dotnet_core_runtime_folder(runtime, version))
  
	invisible(.C("rStartClr", app_base_dir, package_bin_folder, dotnet_core_folder, PACKAGE = package_name))
}