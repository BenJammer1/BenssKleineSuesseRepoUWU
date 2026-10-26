# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy all source files and publish release
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Run
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "App.dll"]
