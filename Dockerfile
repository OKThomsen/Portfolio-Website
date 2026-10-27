# Build with the SDK image, run on the smaller ASP.NET runtime image.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# Restore first, so the package layer is cached until the project file changes.
COPY src/Portfolio.Web/Portfolio.Web.csproj src/Portfolio.Web/
RUN dotnet restore src/Portfolio.Web/Portfolio.Web.csproj
COPY src/Portfolio.Web/ src/Portfolio.Web/
RUN dotnet publish src/Portfolio.Web/Portfolio.Web.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
# The image's built-in non-root user; the app listens on port 8080 (the image default).
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Portfolio.Web.dll"]
