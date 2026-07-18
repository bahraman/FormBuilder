FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY FormBuilder.slnx ./
COPY src/FormBuilder.Domain/FormBuilder.Domain.csproj src/FormBuilder.Domain/
COPY src/FormBuilder.Application/FormBuilder.Application.csproj src/FormBuilder.Application/
COPY src/FormBuilder.Infrastructure/FormBuilder.Infrastructure.csproj src/FormBuilder.Infrastructure/
COPY src/FormBuilder.Api/FormBuilder.Api.csproj src/FormBuilder.Api/

RUN dotnet restore src/FormBuilder.Api/FormBuilder.Api.csproj

COPY src/ src/
RUN dotnet publish src/FormBuilder.Api/FormBuilder.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FormBuilder.Api.dll"]
