FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim
COPY . /app
WORKDIR /app
EXPOSE 443/tcp
ENTRYPOINT dotnet server.dll --server.urls "http://*:80,https://*:443"