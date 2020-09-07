# DevRef
dotnet global tool to swap references in a .csproj between PackageReferences and ProjectReferences

# Installation

`dotnet tool install -g devref`

# DevRef's use case

The use case for DevRef is when you are simultaneously working on a project and one of its dependencies, if that dependency is usually hosted on a NuGet feed.

Imagine you were working on project `Thing`, which has a reference to `CoolLib` in `Thing.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp3.1</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="CoolLib" Version="1.0.1" />
  </ItemGroup>

</Project>
```

But now you find you need to _also_ work on `CoolLib`!  Use could use a couple of different workflows for this:

### Workflow A: Iterate CoolLib and publish each version to your NuGet feed

Each time you make a change to `CoolLib`, you repackage it and push the new version to your NuGet feed.  Then you restore this new version in `Thing` (possibly after changing the version number in the `<PackageReference>` tage).

DevRef is not going to help with this workflow.

### Workflow B: Change the PackageReference to a ProjectReference while you're working on it

In this workflow, you change the `<PackageReference>` tag to a local `<ProjectReference>`, like this:

```xml
  <ItemGroup>
    <ProjectReference Include="../CoolLib/CoolLib.csproj" />
  </ItemGroup>
```

Then, when you're all done with the changes and you've published the new version to your NuGet feed, you switch the reference back to a `<PackageReference>` in `Thing.csproj`.

**DevRef helps you with this second workflow.**

# Using DevRef to solve the problem

`dotnet tool install -g devref`

`cd Thing`

`devref manage --package CoolLib --local-path ../CoolLib/CoolLib.csproj`

Several things now happen:
1. A new file is created in the current working directory called `.devref`.  This file contains information that connects the `CoolLib` package with the local version of it located at `--local-path`.
2. If it's not already there, `.devref` is added to an existing `.gitignore` file.
3. An implicit `devref local CoolLib` is executed (see below for more information).

You are now in `local` mode, which means `Thing` is referencing the local copy of `CoolLib`.

To switch back to the NuGet version, run:

`devref remote CoolLib`

# `devref local`

When you run `devref local Package`, this is what happens:
* If `Package` is not registered in `.devref`, error out.
* Modify the `.csproj` file in the current working directory, changing any `<PackageReference>` for `Package` to `<ProjectReference>` (using the path registered in `.devref`).
* Add the `.csproj` file to the `.gitignore` file so this chnage isn't permanent.
* Execute `dotnet restore` in the current working directory.

# `devref remote`

When you run `devref remote Package`, this is what happens:
* If `Package` is not registered in `.devref`, error out.
* Modify the `.csproj` file in the current working directory, changing any `<ProjectReference>` for `Package` to `<PackageReference>` (using the version registered in `.devref`).
* Comment out the `.csproj` file's entry in `.gitignore` to allow `git` to start tracking the `.csproj` file again.
* Execute `dotnet restore` in the current working directory.

# TODO

- [ ] Add automatic addition of `.devref` to `.gitignore`
- [ ] Add `.csproj` file handling in `.gitignore`