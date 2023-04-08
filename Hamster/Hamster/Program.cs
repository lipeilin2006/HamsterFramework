using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using YamlDotNet.Serialization;
using System.Reflection;
using Hamster.Core;
using System.Net;

string configpath = Path.Combine(".", "config.yaml");
string scriptpath = "";
bool isDebug = false;
if (args.Length > 0)
{
	switch (args[0])
	{
		case "--start":
			if (args.Length > 1)
			{
				scriptpath = args[1];
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("No C# script was provided");
				Console.ForegroundColor = ConsoleColor.White;
				return;
			}
			break;
		case "--debug":
			if (args.Length > 1)
			{
				scriptpath = args[1];
				isDebug = true;
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("No C# script was provided");
				Console.ForegroundColor = ConsoleColor.White;
				return;
			}
			break;
		case "--restart":
			Environment.SetEnvironmentVariable("KDAction", "restart", EnvironmentVariableTarget.User);
			return;
		case "--stop":
			Environment.SetEnvironmentVariable("KDAction", "stop", EnvironmentVariableTarget.User);
			return;
		case "--help":
			Console.WriteLine("Use --help for help.");
			Console.WriteLine("--start [C# Script Path] to start server.");
			Console.WriteLine("--debug [C# Script Path] to start debug server.");
			Console.WriteLine("--restart to restart server.");
			Console.WriteLine("--stop to stop server.");
			return;
	}
}
else
{
	Console.WriteLine("Use --help for help.");
	Console.WriteLine("--start [C# Script Path] to start server.");
	Console.WriteLine("--debug [C# Script Path] to start debug server.");
	Console.WriteLine("--restart to restart server.");
	Console.WriteLine("--stop to stop server.");
	return;
}
Start:
try
{
	List<Assembly> assemblies = new List<Assembly>();
	Server server;
	if (File.Exists(configpath))
	{
		StringReader input = new StringReader(File.ReadAllText(configpath));
		var deserializer = new DeserializerBuilder().Build();
		server = deserializer.Deserialize<Server>(input);
		input.Close();
	}
	else
	{
		var serializer = new Serializer();
		TextWriter textWriter = File.CreateText(configpath);
		server = new Server()
		{
			host = "localhost:20000",
			useHttps = false,
			threadCount = 4,
			imports = { "Hamster.Core.dll", "MySql.Data.dll", "Microsoft.Data.Sqlite.dll", "Oracle.ManagedDataAccess.dll", "Microsoft.Data.SqlClient.dll", "Npgsql.dll", "Dapper.dll" },
			namespaces = { "System", "System.Net", "Hamster.Core", "System.Threading", "MySql.Data.MySqlClient", "Microsoft.Data.Sqlite", "Oracle.ManagedDataAccess.Client", "Microsoft.Data.SqlClient", "Npgsql" }
		};
		serializer.Serialize(textWriter, server);
		textWriter.Close();
	}
	foreach(string dllname in server.imports)
	{
		assemblies.Add(Assembly.LoadFile(AppDomain.CurrentDomain.BaseDirectory + dllname));
	}
	assemblies.Add(typeof(HttpListenerContext).Assembly);
	Global global = new Global() { server = new HttpServer(server.host, server.useHttps, server.threadCount) };
	ScriptOptions scriptOptions = ScriptOptions.Default
		.AddReferences(assemblies.ToArray())
		.WithImports(server.namespaces.ToArray());
	ScriptState scriptState;
	if (File.Exists(scriptpath))
	{
		scriptState = CSharpScript.RunAsync(File.ReadAllText(scriptpath), scriptOptions, global).Result;
	}
	else
	{
		File.WriteAllText(scriptpath, 
			"httpServer.Route(\"^/\", (HttpListenerRequest) =>\r\n" +
			"{\r\n\t" +
			"return new Text(\"Hello\");\r\n" +
			"});\r\n"+
			"server.RouteOther((HttpListenerRequest request) =>\r\n" +
			"{\r\n\t\t" +
			"return new Text(\"404\");\r\n" +
			"});");
		scriptState = CSharpScript.RunAsync(File.ReadAllText(scriptpath), scriptOptions, global).Result;
	}
	global.server.Start();
	if (isDebug)
	{
		while (true)
		{
			Console.Write("C#>>");
			string order = Console.ReadLine();
			if (order == "Stop()")
			{
				global.server.Stop();
				return;
			}
			else
			{
				try
				{
					scriptState = scriptState.ContinueWithAsync(order, scriptOptions).Result;
				}
				catch (Exception e)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine(e.Message);
					Console.ForegroundColor = ConsoleColor.White;
				}
			}
		}
	}
	else
	{
		while (true)
		{
			switch (Environment.GetEnvironmentVariable("KDAction", EnvironmentVariableTarget.User))
			{
				case "restart":
					global.server.Stop();
					goto Start;
				case "stop":
					global.server.Stop();
					return;
			}
		}
	}
}
catch (Exception e)
{
	Console.ForegroundColor = ConsoleColor.Red;
	Console.WriteLine(e.Message);
	Console.ForegroundColor = ConsoleColor.White;
}

public class Server
{
	public string host="localhost:5000";
	public bool useHttps;
	public byte threadCount;
	public HashSet<string> imports = new HashSet<string>();
	public HashSet<string> namespaces = new HashSet<string>();
}

public class Global
{
	public HttpServer server;
}