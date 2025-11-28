#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
RUN apt-get update; apt-get install libfontconfig1 -y

WORKDIR /app
EXPOSE 9100  #这里端口要与Program.cs一样

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["HDPro.WebApi/HDPro.WebApi.csproj", "HDPro.WebApi/"]
COPY ["HDPro.System/HDPro.Sys.csproj", "HDPro.System/"]
COPY ["HDPro.Core/HDPro.Core.csproj", "HDPro.Core/"]
COPY ["HDPro.Entity/HDPro.Entity.csproj", "HDPro.Entity/"]
COPY ["HDPro.Order/HDPro.Order.csproj", "HDPro.Order/"]
COPY ["HDPro.AppManager/HDPro.AppManager.csproj", "HDPro.AppManager/"]
COPY ["HDPro.Builder/HDPro.Builder.csproj", "HDPro.Builder/"]
RUN dotnet restore "HDPro.WebApi/HDPro.WebApi.csproj"
COPY . .
WORKDIR "/src/HDPro.WebApi"
RUN dotnet build "HDPro.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HDPro.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HDPro.WebApi.dll"]