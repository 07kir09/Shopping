FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Shopping.OrdersService/Shopping.OrdersService.csproj", "src/Shopping.OrdersService/"]
RUN dotnet restore "src/Shopping.OrdersService/Shopping.OrdersService.csproj"
COPY . .
WORKDIR "/src/src/Shopping.OrdersService"
RUN dotnet build "Shopping.OrdersService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Shopping.OrdersService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Shopping.OrdersService.dll"] 