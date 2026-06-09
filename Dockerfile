# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file and all project files
COPY Relyf.sln ./
COPY Relyf.Api/Relyf.Api.csproj Relyf.Api/
COPY Relyf.Service/Relyf.Service.csproj Relyf.Service/
COPY Relyf.Repository/Relyf.Repository.csproj Relyf.Repository/

# Restore dependencies
RUN dotnet restore Relyf.Api/Relyf.Api.csproj

# Copy all source code
COPY Relyf.Api/ Relyf.Api/
COPY Relyf.Service/ Relyf.Service/
COPY Relyf.Repository/ Relyf.Repository/

# Build and publish
RUN dotnet publish Relyf.Api/Relyf.Api.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Relyf.Api.dll"]
