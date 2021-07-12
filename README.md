# powerplant-coding-challenge

## Requirements

This project was made using .NET 5.0 framework. Use the appropriate [SDK](https://dotnet.microsoft.com/download/dotnet/5.0) to compile and run the solution.


## Build and test the app

### IDE

Visual Studio (project was made using VS 2019)

### Using CLI

In the project folder, use the dotnet tool to build and test the project

- `dotnet restore`: restore NuGet packages
- `dotnet build`: build the solution
- `dotnet test`: test the solution


## Launch the app

### Using Visual Studio 

Launch the `Engie.PCC.Api` project or `IIS Express` profile. The REST API is available on http://localhost:8888 
<br>A swagger is available at http://localhost:8888/swagger/index.html in the development environnement only.

### Using CLI

Go to the subfolder `src/Engie.PCC.Api` and execute `dotnet run --urls="http://localhost:8888"`.
<br>To access swagger document, use the `environment` switch: `dotnet run --urls="http://localhost:8888" --environment=development`
 
## Using Docker

A Dockerfile is available in the `src/Engie.PCC.Api` folder. It will build and publish the app. 
<br>The exposed port is 8888.

In the `src/Engie.PCC.Api` folder, execute the following commands:

- `docker build -t power-plant-challenge:latest .`
- `docker run --rm -it -p 8888:8888 power-plant-challenge:latest`


## WebSocket

WebSocket client can connect on `ws://localhost:8888/notifications`.