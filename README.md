<div align="center">
  
  # HamsterFramework
  
</div>

<p align="center">
  <img src="https://github.com/lipeilin2006/HamsterFramework/blob/main/Hamster.jpeg" width="200" height="200" alt="icon"/>
</p>

<p align="center">
  [![Test](https://github.com/lipeilin2006/HamsterFramework/actions/workflows/Test.yml/badge.svg)](https://github.com/lipeilin2006/HamsterFramework/actions/workflows/Test.yml)
  <a href="https://raw.githubusercontent.com/lipeilin2006/HamsterFramework/main/LICENSE">
    <img src="https://img.shields.io/github/license/lipeilin2006/HamsterFramework" alt="license">
  </a>
  <a href="https://github.com/lipeilin2006/HamsterFramework/releases">
    <img src="https://img.shields.io/github/v/release/lipeilin2006/HamsterFramework?color=blueviolet&include_prereleases" alt="release">
  </a>
</p>

<br />

这是一个非常简单的C#后端框架。这个框架实现了C#脚本化。你只需要编写C#脚本，无需自行编译C#代码就能直接运行。使用多线程和异步来提高处理请求的速度。可以使用Http API来管理Hamster。
## 示例
server.cs和config.yaml必须放在网站根目录中。
server.cs（注意：这是一个脚本，无需编译。）
```CSharp
server.Route("^/$", (HttpListenerRequest request, MatchCollection matches) => { return new Text.Plane("Hello Hamster"); });
server.RouteOther((HttpListenerRequest request, MatchCollection matches) => { return new Text.Plane("404",404); });
```
#### 调试模式运行（无需初始化）：
Windows:
```
.\Hamster.exe --debug
```
Linux:
```
./Hamster --debug
```
#### 初始化Hamster：
这会创建一个后台进程，用于统一管理所有Hamster服务。
Windows:
```
.\Hamster.exe --init
```
Linux:
```
./Hamster --init
```
#### 启动一个Hamster服务：
此命令会自动获取当前路径，把当前路径作为网站根目录。目录中的server.cs为服务器脚本，config.yaml为配置文件（没有则自动创建）。需name参数的值是唯一的，以便管理服务。
Windows:
```
.\Hamster.exe --start --name xxx
```
Linux:
```
./Hamster --start --name xxx
```
HttpAPI:
```
http://localhost:26262/Start?path=网站根目录&name=名称
```
#### 重启指定名称的Hamster服务：
Windows:
```
.\Hamster.exe --restart --name xxx
```
Linux:
```
./Hamster --restart --name xxx
```
HttpAPI:
```
http://localhost:26262/Restart?name=名称
```
#### 停止指定名称的Hamster服务：
Windows:
```
.\Hamster.exe --stop --name xxx
```
Linux:
```
./Hamster --stop --name xxx
```
HttpAPI:
```
http://localhost:26262/Stop?name=名称
```
#### 重启所有Hamster服务：
Windows:
```
.\Hamster.exe --restartall
```
Linux:
```
./Hamster --restartall
```
HttpAPI:
```
http://localhost:26262/RestartAll
```
#### 停止所有Hamster服务并关闭后台进程：
Windows:
```
.\Hamster.exe --stopall
```
Linux:
```
./Hamster --stopall
```
HttpAPI:
```
http://localhost:26262/StopAll
```
## 配置文件
config.yaml
```YAML
host: localhost:20000 #要监听的URL
useHttps: false #是否使用Https
threadCount: 4 #自定义开启的线程数
imports: #需要使用的库（可以自己导入第三方库，导入的库必须与Hamster在同一目录下）
- Hamster.Core.dll
- MySql.Data.dll
- Microsoft.Data.Sqlite.dll
- Oracle.ManagedDataAccess.dll
- Microsoft.Data.SqlClient.dll
- Npgsql.dll
- Dapper.dll
namespaces: #需要使用的命名空间（等同于using）
- System
- System.Net
- Hamster.Core
- System.Threading
- System.Text.RegularExpressions
- MySql.Data.MySqlClient
- Microsoft.Data.Sqlite
- Oracle.ManagedDataAccess.Client
- Microsoft.Data.SqlClient
- Npgsql
```
