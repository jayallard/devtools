# DevTools

This code is in active development. It is usable, but not polished. Parts are hacky.

## Prerequisites

- dotnet sdk >= 6.0.100

## Installation

Due to the current state of this project, which is just dev/experimentation, the nuget package isn't deployed to nuget. You must manually install it.

```bash
# get the code (if necessary)
git clone https://github.com/jayallard/devtools.git

# get the latest version (if necessary)
git pull
cd devtools\DevTools.Application.Cli

# build the tool
dotnet pack

# uninstall the tool (just incase), then install
dotnet tool uninstall devtools.application.cli  -g
dotnet tool install --version 0.0.0-dev --global --add-source ./nupkg DevTools.Application.Cli

# test
devtools nuget -h
```

## Features

### Nuget

#### set-to-latest-local

Scenario:

- The Package Solution: in one solution, you are working on 1 or more projects that are to be deployed as nuget packages. During development, the packages are built to a local folder.
- The Consumer Solution: in another solution, you are working on projects that require the nuget packages. It uses a NuGet source that is the nuget output folder of the package solution.

As you work on the Package solution, the Consumer solution needs the latest packages.

- if the Package solution compiles as the same version each time, ie: `2.2.3-prerelease`, then the Consumer solution won't see the changes. This can be alleviated by flushing the nuget cache. However, the flush deletes all cached packages, not just those that you are working on.
- if the Package solution compiles as a new version each time, ie: `2.2.3-prereleaseVARIABLE`, then the Consumer solution will see the changes via Nuget because they are new versions. However, you then have to update the projects to use the new versions. This may be alleviated by using nuget version ranges.

Both solutions are workable, but not entirely convenient. The `set-to-latest-local` command provides an alternative.

The command checks the nuget folder for the latest version of each package. Then, it updates the `.csproj` files with that version.

The latest version of the package is determined by:

- the semver of the package: `major.minor.patch`
- then, the file create date - this is only necessary when there are multiple packages with the same semver and different release names
- by default, it will consider both release and prerelease versions. You may specify to exclude either.

Example NuGet Packages

- MyTest.1.1.0, file date = 12/4/2/2021
- MyTest.1.1.1, file date = 12/4/2021
- MyTest.1.1.1-preview1, file date = 12/5/2021
- MyTest.1.1.1-preview2, file date = 12/6/2021

When ordering by semver, all 3 are identical because they are all 1.0.0. The suffix is ignored. The command uses the file date to determine the most recent.

- Latest of Release and PreRelease: 1.1.0-preview2. It is the latest version with the latest file date
- Latest Release: 1.1.1
- Latest PreRelease: 1.1.1-preview 2 - it is the latest version with the latest file date

#### Usage

Execute the help command to see all options:

```bash
devtools nuget set-to-latest-local -h
```

Example:

- add a nuget source to a local folder IE: `c:\mycode\.dev-nuget`
- package the Package solution to `c:\mycode\.dev-nuget` with version `1.1.1-dev1`
- in the Consumer solution, add package references to your nuget packages from the `dev-nuget` source
- package the Package solution to`c:\mycode\.dev-nuget` with version `1.1.1-dev2`
- run the `set-to-latest-local` command in the consumer folder - all package references will be updated to `1.1.1-dev2`

### Watch - Auto Update

If you run the command with the `--watch` option, then the tool will monitor the nuget package folder. Each time new versions of the packages are created, the projects will update automatically.

#### Limitations

- It uses only the global nuget.config. (IE: local configs in the work directory aren't used)

## set-specific-version

This command updates all package references, of a pattern, to a specific version.

For example: if you created a bunch of packages:

- MyStuff.Core-2.0.0
- MyStuff.Infrastructure-2.0.0
- MyStuff.Application-2.0.0

In a code folder, use the command to update all package references to `MyStuff*` to version 2.0.0
