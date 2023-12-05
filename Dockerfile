FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /App

# Copy only the necessary project files
COPY ./src/Org.Quickstart.API/Org.Quickstart.API.csproj ./
RUN dotnet restore

# Copy the entire project directory
COPY ./src/Org.Quickstart.API ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 
WORKDIR /App

COPY --from=build-env /App/out .
ENTRYPOINT ["dotnet", "Org.Quickstart.API.dll"]
