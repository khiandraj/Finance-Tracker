# Use the .NET 9.0 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /App

# Copy csproj files and restore dependencies first
COPY api/api.csproj ./api/
RUN dotnet restore ./api/api.csproj

# Copy the rest of the source code
COPY . ./

# Publish the app
RUN dotnet publish ./api/api.csproj -c Release -o /App/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /App
COPY --from=build /App/out .
ENTRYPOINT ["dotnet", "api.dll"]
