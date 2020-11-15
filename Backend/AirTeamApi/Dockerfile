FROM mcr.microsoft.com/dotnet/core/sdk:5.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -v m -o pubdir

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:5.0-alpine
WORKDIR /app
COPY --from=build-env /app/pubdir .

ENV TZ=Asia/Tehran
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

EXPOSE 80
ENTRYPOINT ["dotnet", "AirTeamApi.dll"]