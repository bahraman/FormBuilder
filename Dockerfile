FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Vendo.FormBuilder.slnx ./
COPY src/Vendo.FormBuilder.Domain/Vendo.FormBuilder.Domain.csproj src/Vendo.FormBuilder.Domain/
COPY src/Vendo.FormBuilder.Application/Vendo.FormBuilder.Application.csproj src/Vendo.FormBuilder.Application/
COPY src/Vendo.FormBuilder.Infrastructure/Vendo.FormBuilder.Infrastructure.csproj src/Vendo.FormBuilder.Infrastructure/
COPY src/Vendo.FormBuilder.Api/Vendo.FormBuilder.Api.csproj src/Vendo.FormBuilder.Api/

RUN dotnet restore src/Vendo.FormBuilder.Api/Vendo.FormBuilder.Api.csproj

COPY src/ src/
RUN dotnet publish src/Vendo.FormBuilder.Api/Vendo.FormBuilder.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Vendo.FormBuilder.Api.dll"]
