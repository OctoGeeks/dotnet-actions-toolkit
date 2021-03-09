# dotnet-actions-toolkit

This is a port to .Net of the [GitHub Actions Toolkit](https://github.com/actions/toolkit).  It makes it more convenient to author GitHub Actions in .Net based languages.

TODO: Show samples of .Net based actions, and the tradeoffs for the 3 approaches

Not all of the packages/code in the Actions Toolkit has been or will be ported. Some of the functionality in the original toolkit exists in the .Net Framework. This project focuses on porting the functionality that is specifically related to the GitHub Actions system.

## [core](https://github.com/actions/toolkit/tree/main/packages/core)
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

## [exec](https://github.com/actions/toolkit/blob/main/packages/exec) - WILL NOT BE PORTED

## [glob](https://github.com/actions/toolkit/blob/main/packages/glob) - WILL NOT BE PORTED

## [io](https://github.com/actions/toolkit/blob/main/packages/io) - WILL NOT BE PORTED

## [tool-cache](https://github.com/actions/toolkit/tree/main/packages/tool-cache)
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

## [github](https://github.com/actions/toolkit/blob/main/packages/github) - NOT PORTED YET

## [cache](https://github.com/actions/toolkit/blob/main/packages/cache) - NOT PORTED YET
