## Definition of Done
A backlog item is Done when:
- [ ] Code is committed with a descriptive message
- [ ] Tests are passing
- [ ] Related documentation is updated

## How to set up Scheducate
- Download Visual Studio 2022 or later with ASP.NET and web development, .NET desktop development, and Data storage and processing.
- Ensure you have .NET version 9.0 installed.
- Clone the repository.
- Open the project in Visual Studio.
- Run “Update-Database” from the Package Manager Console.
- Build and run project.

## How to run the code tests
- Open a terminal.
- Run 'dotnet test' for a full test suite.
- Run 'dotnet test --collect:"XPlat Code Coverage"' to save test results.

## How to view coverage report
- Run 'dotnet test --collect:"XPlat Code Coverage"' to save test results.
- Run 'cd Scheducate.Tests' to move to the test project directory.
- Run 'reportgenerator "-reports:TestResults/**/coverage.cobertura.xml" "-targetdir:coveragereport" "-reporttypes:Html"' to generate the report.
- Navigate to 'index.html' at \ScheducateCS415\Scheducate.Tests\coveragereport\index.html
- Right-click and select open with integrated browser
- Resulting 
