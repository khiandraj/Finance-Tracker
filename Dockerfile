# Stage 1: Build the API
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /App

# Copy only the API project file first
COPY FinanceTracker.Api/FinanceTracker.Api.csproj ./FinanceTracker.Api/
RUN dotnet restore ./FinanceTracker.Api/FinanceTracker.Api.csproj

# Copy everything else
COPY . ./

# Publish the API
RUN dotnet publish ./FinanceTracker.Api/FinanceTracker.Api.csproj -c Release -o /App/out

# Stage 2: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /App
COPY --from=build /App/out .
ENTRYPOINT ["dotnet", "FinanceTracker.Api.dll"]

