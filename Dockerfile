# Use the official ASP.NET image as a base
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the .NET SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the solution file and restore dependencies for all projects
COPY *.sln ./
COPY WEBAPI/WEBAPI.csproj WEBAPI/
COPY BL/BL.csproj BL/
COPY DAL/DAL.csproj DAL/
COPY DOM/DOM.csproj DOM/
COPY Tests/CulinaryCode.Tests/CulinaryCode.Tests.csproj Tests/CulinaryCode.Tests/
RUN dotnet restore

# Copy the rest of the project files
COPY . .

# Build the project
WORKDIR /src/WEBAPI
RUN dotnet build -c Release -o /app/build

# Publish the project
RUN dotnet publish -c Release -o /app/publish

# Use the runtime image to run the app
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WEBAPI.dll"]
