#FROM filiosoft/dotnetcore-nodejs:latest
FROM microsoft/dotnet:1.0-sdk
COPY . /app
WORKDIR /app
# RUN curl -sL https://deb.nodesource.com/setup_6.x | bash -
# RUN apt-get -y install nodejs
# RUN ["dotnet", "restore"]
# RUN ["dotnet", "build"]
EXPOSE 5000/tcp
# CMD ["dotnet", "run", "--server.urls", "http://*:5000"]
CMD ["dotnet", ".\server.dll", "--server.urls", "http://*:5000"]