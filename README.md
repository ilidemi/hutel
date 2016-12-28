Human Telemetry
===

To run the project, grab the latest [.NET Core](https://www.microsoft.com/net/core) and run
```
$ dotnet restore
$ dotnet run
```
in the /server folder.

To add a new point to in memory storage, issue a HTTP request:
```
POST http://localhost:5000/api/points/7fdbd84a-2405-4c6c-8b12-e72c582e4b37
content-type:application/json

{
    'id': '7fdbd84a-2405-4c6c-8b12-e72c582e4b37',
    'tagId': 'testtag',
    'date': '2016-12-19T21:07:31Z',
    'int': 1337,
    'dateTime': '2016-12-20T21:07:31Z',
    'string': 'abacaba',
    'float': 1337.0
}
```

Deploying to Ubuntu with preinstalled Docker on Azure cloud
---

To do deployment on Auzre cloud you need to obtain subscription on [Azure](https://portal.azure.com)

0. After [Nginx setup](https://www.techrepository.in/serving-static-files-using-nginx-in-docker) and [Nginx as reverse proxy setup](https://www.techrepository.in/using-nginx-as-a-reverse-proxy-for-asp-net-core-web-app-in-docker)
you can make docker image by
```
$ cd ./server 
$ docker build -t revproxy .
```
1. Request Docker on Ubuntu Server (Canonical + Microsoft) [here](https://portal.azure.com)
Make ssh connection with server, so you can login with.
```
$ ssh 53.53.53.53
```
where 53.53.53.53 is ip-address of your server on Azure.

2. Save and transfer image to server
```
$ docker save -o revproxy.tar
$ scp ./revproxy.tar user_name@53.53.53.53:/home/user_name
```
3. Load and start docker image and reverse proxy server in the cloud.
```
$ docker load -i revproxy.tar
$ docker run -it -p 8018:8018 -d revproxy

```
4. Check your web server is up.
```
$ curl localhost:8018/api/hello
```