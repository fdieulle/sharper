#' @title
#' Install .Net core.
#'
#' @description
#' Install the dotnet core runtime on your machine. 
#' For more informations on this method see [dotnet-install-script](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-install-script)
#'
#' @param channel Specifies the source channel for the installation. The possible values are:
#'		* `Current` - Most current release.
#'      * `LTS` - Long-Term Support channel (most current supported release).
#'		* Two-part version in X.Y format representing a specific release (for example, `2.0` or `1.0`).
#'		* Branch name. For example, `release/2.0.0`, `release/2.0.0-preview2`, or `master` (for nightly releases).
#'	The default value is `LTS`. For more information on [.NET support channels](https://www.microsoft.com/net/platform/support-policy#dotnet-core), see the .NET Support Policy page.
#' @param version Represents a specific build version. The possible values are:
#' 		* `latest` - Latest build on the channel (used with the `channel` option).
#' 		* `coherent` - Latest coherent build on the channel; uses the latest stable package combination (used with Branch name `channel` options).
#' 		* Three-part version in X.Y.Z format representing a specific build version; supersedes the -Channel option. For example: 2.0.0-preview2-006120.
#'	If not specified, version defaults to latest.
#' @param installDir Specifies the installation path. The directory is created if it doesn't exist. The default value is %LocalAppData%\Microsoft\dotnet. 
#'	Binaries are placed directly in this directory.
#' @param architecture Architecture of the .NET Core binaries to install. Possible values are `<auto>`, `amd64`, `x64`, `x86`, `arm64`, and `arm`. The default value is `x86` and `x64`.
#' @param runtime Installs just the shared runtime, not the entire SDK. The possible values are:
#' 		* `dotnet` - the `Microsoft.NETCore.App` shared runtime.
#' 		* `aspnetcore` - the `Microsoft.AspNetCore.App` shared runtime.
#'
#' @export
#' @examples
#' \dontrun{
#' library(sharper)
#'
#' install_dotnetCore()
#' install_dotnetCore(installDir = "./", runtime = "aspnetcore")
#' }
install_dotnetCore <- function(channel = "LTS", version = "latest", installDir = NULL, architecture = NULL, runtime = "dotnet") {
	pkgFolder <- system.file(package = "sharper")
	
	binFolder <- file.path(pkgFolder, "bin")
	if (!file.exists(binFolder)) {
		dir.create(binFolder, recursive = TRUE, showWarnings = FALSE)
	}
	if (is.null(installDir)) {
		installDir <- file.path(binFolder, "dotnet-core")
	}
	
	arguments <- paste("-Channel", channel, "-Version", version, "-Runtime", runtime, "-NoPath", sep = ' ')
	argumentsList <- list()
	if (is.null(architecture)) {
		argumentsList[["x86"]] = paste(arguments, "-Architecture", "x86", sep = ' ')
		argumentsList[["x64"]] = paste(arguments, "-Architecture", "x64", sep = ' ')
	} else {
		argumentsList[[architecture]] = paste(arguments, "-Architecture", architecture, sep = ' ')
	}
		
	sysinf <- Sys.info()
	if (sysinf["sysname"] == "Windows") {
			
		commandLine <- "[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; &([scriptblock]::Create((Invoke-WebRequest -useb 'https://dot.net/v1/dotnet-install.ps1')))"
		for (arch in names(argumentsList)) {
			arguments <- paste(argumentsList[[arch]], "-InstallDir", file.path(installDir, arch), sep = ' ')
			system2("powershell", 
				args = c(
				"-NoProfile",
				"-ExecutionPolicy", "unrestricted",
				"-Command", 
				paste(commandLine, arguments, sep = ' ')))
		}
	} else {
	
		commandLine <- "https://dot.net/v1/dotnet-install.sh | bash /dev/stdin"
		for (arch in names(argumentsList)) {
			arguments <- paste(argumentsList[[arch]], "-InstallDir", file.path(installDir, arch), sep = ' ')
			system2("curl", 
				args = c(
				"-sSL",
				paste(commandLine, arguments, sep = ' ')))
		}
	}
}
