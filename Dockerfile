FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Cats.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish Cats.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://+:8080
ENV DATA_DIR=/app/data
EXPOSE 8080
ENTRYPOINT ["dotnet", "Cats.dll"]
