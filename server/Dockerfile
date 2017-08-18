FROM microsoft/dotnet:1.0-core
COPY . /app
WORKDIR /app
EXPOSE 5000/tcp
ENTRYPOINT dotnet server.dll --server.urls "http://*:5000"