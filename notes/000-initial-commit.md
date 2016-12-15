The one where the initial commit preparations are described
===

So, here's the start of the project. It's always good to get something working first and build on top of that, so the work starts from the backend. And the backend starts with choosing appropriate tech and tooling:
* Version control system - [Git](https://git-scm.com) looks like an obvious choice for a layman, because it's so widespread and there are well-known platforms for code hosting, like GitHub.
* Backend tech - [ASP.NET Core](https://www.microsoft.com/net/core) is a solid candidate. .NET ecosystem is very mature, so the lack of documentation or features won't be a probem, and it's cross-platform and familiar for me as well.
* Code editor - [Visual Studio Code](https://code.visualstudio.com) sounds promising, as it has enough features and is easier to work with than the canonical editors, and at the same time it's less bloated than IDEs. As a bonus, it's cross-platform, despite being a Microsoft product.

Creating a simple .NET Core app is very easy:
```
$ dotnet new
```
A single command sets up a simplest console app which can be built and executed with `dotnet run`. After restoring the packages, the whole project consists of three files only. So far so good, but we need a web app. Official tutorials suggest running a sligtly more complicated bootstrap command:
```
$ dotnet new -t web
```
Let's look at the size:
```
$ find -mindepth 1 -type d | wc -l
16

$ find -type f | wc -l
75
```
Woah, 75 files across 16 folders looks like a lot! And it really is. The default example of web app showcases a lot of functionality, such as multiple views, multitenancy and even DB models, and we don't need any of that for a first iteration of a simple backend.

[This article](http://www.hanselman.com/blog/ExploringAMinimalWebAPIWithASPNETCore.aspx) by Scott Hanselman suggests using [Yeoman](http://yeoman.io/learning/) to generate a simplest WebAPI project. Yeoman is a scaffolding tool for kickstarting a new project development (which is exactly what we need), but using it to generate an ASP.NET project requires dependincies on its own: we'll need to install [Node.js](https://nodejs.org/en/) first and use npm to get
* [yo](https://github.com/yeoman/yo) (Yeoman CLI tool)
* [bower](https://bower.io/) (frontend package manager)
* [grunt](http://gruntjs.com) (a build system for JavaScript)
* [gulp](https://gulpjs.com) (yet another build system for JavaScript)
* generator-aspnet (which will actually generate our project)

That's a lot of JavaScript tooling for creating a simple .NET project, but unfortunately this is a world we have to live in. On the other side, all of this can be installed at once with a simple 
```
$ npm install -g yo bower grunt gulp generator-aspnet
```
Finally, we can run the generator:
```
$ yo aspnet
```
and select `Empty Web Application`. The resulting project only contains 2 directories and 9 files, which is quite bearable.

After inspecting the contents of `Program.cs` an interesting expression was found:
```csharp
var host = new WebHostBuilder()
    .UseConfiguration(config)
    .UseKestrel()
    .UseContentRoot(Directory.GetCurrentDirectory())
    .UseIISIntegration()
    .UseStartup<Startup>()
    .Build();
```
Two lines stand out: `.UseKestrel()` and `.UseIISIntegration()`. Turns out, Microsoft has built a special web server for .NET Core, [Kestrel](https://github.com/aspnet/KestrelHttpServer). It is a highly performant asynchronous web server that destroys the thread-per-request ones in the benchmarks and is tightly integrated with the rest of ASP.NET Core. Why does the code mention IIS, you may ask? Apparently, Kestrel is not secure enough to be exposed to the entire internet, so it needs *another* full-featured web server as a reverse proxy to guard it against all kinds of intruders.

The problem here is that even if IIS doesn't require additional configuration, the setup stops being cross-platform, and this is something we want to avoid. Official documentation mentions that Apache or nginx can be used on the other platforms, but this would complicate the development setup and put additional barriers for actually shipping something.

If we decide to stay with .NET Core as a backend tech, certainly something can be done: Docker to the rescue! It's supported on all major platforms and cloud providers and prevents us from fighting the configuraiton and integration issues. There's also a helpful [guide](http://www.techrepository.in/using-nginx-as-a-reverse-proxy-for-asp-net-core-web-app-in-docker) on setting up two containers with ASP.NET Core application and nginx web server and putting one behind another. Problem solved, now we can develop using just Kestrel and have the running-in-production part fully handled by Docker.