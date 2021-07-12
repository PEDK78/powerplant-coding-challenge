#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

# Declare ports above 1024 as an unprivileged non-root user cannot bind to < 1024
ENV ASPNETCORE_URLS http://+:8888
# ENV ASPNETCORE_ENVIRONMENT Development
EXPOSE 8888

# Declare user information 
ENV USERNAME=pcc
ENV USERID=140
ENV GROUP=apps
ENV HOME=/home/${USERNAME}
RUN mkdir -p ${HOME}

# Create a group and an user (system account) which will execute the app
RUN groupadd -r ${GROUP} &&\
    useradd -r -g ${GROUP} -d ${HOME} -s /sbin/nologin -c "Docker image user" -u ${USERID} ${USERNAME}

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Engie.PCC.Api.csproj", "./"]
RUN dotnet restore "Engie.PCC.Api.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "Engie.PCC.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Engie.PCC.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Change ownership on the app folder
RUN chown :${GROUP} /app -R
RUN chmod g+rwx /app -R

# Change to the app user.
USER ${USERNAME}

ENTRYPOINT ["dotnet", "Engie.PCC.Api.dll"]