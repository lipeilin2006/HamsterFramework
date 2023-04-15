# HamsterFramework
这是一个非常简单的C#后端框架。这个框架实现了C#脚本化。你只需要编写C#脚本，把脚本名称通过命令行参数的方式传给Hamster，无需自行编译C#代码就能直接运行。
## 示例
server.cs
```CSharp
server.Route("^/$", (HttpListenerRequest req) => { return new Text("Hello"); });
server.RouteOther((HttpListenerRequest req) => { return new Text("404"); });
```
#### 运行：
Windows:
```
.\Hamster.exe --debug .\server.cs
```
Linux:
```
./Hamster --debug ./server.cs
```
## 配置文件
config.yaml
```
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
- MySql.Data.MySqlClient
- Microsoft.Data.Sqlite
- Oracle.ManagedDataAccess.Client
- Microsoft.Data.SqlClient
- Npgsql
```
