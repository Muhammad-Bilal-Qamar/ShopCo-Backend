# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy your project file and restore dependencies
# (If your .csproj file has a different name, replace ShopCoAPI.csproj with it)
COPY ["ShopCoAPI.csproj", "./"]
RUN dotnet restore "ShopCoAPI.csproj"

# Copy the rest of the source code and build
COPY . .
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ShopCoAPI.dll"]
