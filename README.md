# .Net GitHub Actions Toolkit

This is a port to .Net of the [GitHub Actions Toolkit](https://github.com/actions/toolkit).  It makes it more convenient to author GitHub Actions in .Net based languages.

To use this in your project get the [latest version from NuGet.org](https://www.nuget.org/packages/DotnetActionsToolkit/)

TODO: Link to dotnet-action-template repo for guidance on how to build dotnet actions

# Porting Status

Not all of the packages/code in the Actions Toolkit has been or will be ported. Some of the functionality in the original toolkit exists in the .Net Framework. This project focuses on porting the functionality that is specifically related to the GitHub Actions system.

### :heavy_check_mark: [core](https://github.com/actions/toolkit/tree/main/packages/core)
- exportVariable - PORTED
- setSecret - PORTED
- addPath - PORTED
- getInput - PORTED
- setOutput - PORTED
- setCommandEcho - PORTED
- setFailed - PORTED
- isDebug - PORTED
- debug - PORTED
- error - PORTED
- warning - PORTED
- info - PORTED
- startGroup - PORTED
- endGroup - PORTED
- group - PORTED
- saveState - PORTED
- getState - PORTED

### :runner: [exec](https://github.com/actions/toolkit/blob/main/packages/exec) - WILL NOT BE PORTED

### :ice_cream: [glob](https://github.com/actions/toolkit/blob/main/packages/glob) - WILL NOT BE PORTED

### :pencil2: [io](https://github.com/actions/toolkit/blob/main/packages/io) - WILL NOT BE PORTED

### :hammer: [tool-cache](https://github.com/actions/toolkit/tree/main/packages/tool-cache)
- downloadTool - WILL NOT BE PORTED
- downloadToolAttempt - WILL NOT BE PORTED
- extract7z - WILL NOT BE PORTED
- extractTar - WILL NOT BE PORTED
- extractXar - WILL NOT BE PORTED
- extractZip - WILL NOT BE PORTED
- cacheDir - PORTED
- cacheFile - PORTED
- find - PORTED
- findAllVersions - PORTED
- getManifestFromRepo - NOT PORTED YET
- findFromManifest - NOT PORTED YET

### :octocat: [github](https://github.com/actions/toolkit/blob/main/packages/github) - NOT PORTED YET

### :floppy_disk: [artifact](https://github.com/actions/toolkit/blob/main/packages/artifact) - NOT PORTED YET

### :dart: [cache](https://github.com/actions/toolkit/blob/main/packages/cache) - NOT PORTED YET
