FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Shopping.OrdersService.csproj", "."]
COPY ["Shopping.Common/Shopping.Common.csproj", "Shopping.Common/"]
RUN dotnet restore "Shopping.OrdersService.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "Shopping.OrdersService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Shopping.OrdersService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Shopping.OrdersService.dll"] 