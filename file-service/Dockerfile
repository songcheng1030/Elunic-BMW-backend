FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./file-service/*.csproj ./
COPY ./common /common
RUN dotnet restore

# Copy everything else and build
COPY ./file-service ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=builder /app/out .
ENTRYPOINT ["dotnet", "AIQXFileService.dll"]