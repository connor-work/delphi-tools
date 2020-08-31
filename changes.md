# About this file
This file tracks significant changes to the project setup that are not easily recognizable from file diffs (e.g., project creation wizard operations).

# Changes
1. Created a *[global.json file](https://docs.microsoft.com/en-us/dotnet/core/tools/global-json?tabs=netcore3x)* to fix .NET Core SDK version.

    ```powershell
    dotnet new globaljson --sdk-version $(dotnet --version)
    ```

2. Created a *[nuget.config file](https://docs.microsoft.com/en-us/nuget/reference/nuget-config-file)* to fix package sources.

    ```powershell
    dotnet new nugetconfig
    ```

3. Created new .NET Core project for a Class library (Delphi Source Code Writer).

    ```powershell
    dotnet new classlib --language C`# --name code-writer --framework netcoreapp3.1 --output code-writer
    ```

4. Created new .NET Core solution (Delphi Tools).

    ```powershell
    dotnet new sln --name delphi-tools
    ```

5. Added `code-writer` project to `delphi-tools` solution.

    ```powershell
    dotnet sln add code-writer
    ```

6. Created new xUnit test project (tests for Delphi Source Code Writer).

    ```powershell
    dotnet new xunit --name code-writer.tests --framework netcoreapp3.1 --output code-writer.tests
    ```

7. Added `code-writer.tests` project to `delphi-tools` solution.

    ```powershell
    dotnet sln add code-writer.tests
    ```

8. Added SonarAnalyzer for static code analysis to `code-writer` project. Further package additions do not need to be tracked here.

    ```powershell
    dotnet add code-writer package SonarAnalyzer.CSharp --version 8.12.0.21095
    ```

9. Created a *[manifest file](https://docs.microsoft.com/en-us/dotnet/core/tools/local-tools-how-to-use)* for .NET Core local tools.

    ```powershell
    dotnet new tool-manifest
    ```

10. Installed [`dotnet-grpc`](https://docs.microsoft.com/en-us/aspnet/core/grpc/dotnet-grpc?view=aspnetcore-3.1) tool to manage protobuf references.

    ```powershell
    dotnet tool install dotnet-grpc
    ```

11. Added first protobuf reference using `dotnet-grpc` to the `code-writer` project. Further reference additions do not need to be tracked here.
Note that this tool silently fails when invoked from the top level with `--project` reference (we consider this a bug).

    ```powershell
    cd code-writer
    dotnet grpc add-file --services None --access Public ..\proto\work\connor\delphi\source-code.proto
    ```

12. Added `code-writer` project as a dependency of its test project `code-writer.tests`.

    ```powershell
    dotnet add code-writer.tests reference code-writer
    ```

