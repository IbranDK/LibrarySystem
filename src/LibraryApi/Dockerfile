FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR C:/Users/Zero/source/repos/LibrarySystem/src

COPY src/LibraryApi/LibraryApi.csproj src/LibraryApi/
RUN dotnet restore src/LibraryApi/LibraryApi.csproj

COPY src/LibraryApi/ src/LibraryApi/
RUN dotnet publish src/LibraryApi/LibraryApi.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "LibraryApi.dll"]