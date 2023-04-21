using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using YamlDotNet.Serialization;
using System.Reflection;
using Hamster.Core;
using System.Net;
using System.Diagnostics;
using CommandLine;
using CommandLine.Text;
using Google.Protobuf.WellKnownTypes;
using System.Text;

Dictionary<string,Global> globals = new Dictionary<string, Global>();
HttpServer? cmdserver = null;
bool stop = false;

try
{
	Parser.Default.ParseArguments<Options>(args)
	   .WithParsed(option =>
	   {
		   if (option.isInit)
		   {
			   ProcessStartInfo startInfo = new ProcessStartInfo(Process.GetCurrentProcess().MainModule.FileName);
			   startInfo.Arguments = "--work";
			   startInfo.CreateNoWindow = true;
			   Console.WriteLine(Process.Start(startInfo).Id);
			   Environment.Exit(0);
		   }
		   else if (option.isWork)
		   {
			   cmdserver = new HttpServer("localhost:26262", false, 1);
			   cmdserver.Route("^/Start$", (HttpListenerRequest request) =>
			   {
				   string? path = request.QueryString["path"];
				   string? name = request.QueryString["name"];
				   if (path != null && name != null)
				   {
					   if (globals.ContainsKey(name))
					   {
						   return new Text.Plane("Error", 202);
					   }
					   Global global = new Global() { root = path };
					   StartServer(global);
					   globals.Add(name, global);
					   return new Text.Plane("OK", 200);
				   }
				   return new Text.Plane("Error", 201);
			   });
			   cmdserver.Route("^/Restart$", (HttpListenerRequest request) =>
			   {
				   string? name = request.QueryString["name"];
				   if (name != null)
				   {
					   if (globals.ContainsKey(name))
					   {
						   globals[name].server.Stop();
						   StartServer(globals[name]);
						   return new Text.Plane("OK", 200);
					   }
					   return new Text.Plane("Error", 202);
				   }
				   return new Text.Plane("Error", 201);
			   });
			   cmdserver.Route("^/Stop$", (HttpListenerRequest request) =>
			   {
				   string? name = request.QueryString["name"];
				   if (name != null)
				   {
					   if (globals.ContainsKey(name))
					   {
						   globals[name].server.Stop();
						   globals.Remove(name);
						   return new Text.Plane("OK", 200);
					   }
					   return new Text.Plane("Error", 202);
				   }
				   return new Text.Plane("Error", 201);
			   });
			   cmdserver.Route("^/RestartAll$", (HttpListenerRequest request) =>
			   {
				   foreach (Global global in globals.Values)
				   {
					   global.server.Stop();
					   StartServer(global);
				   }
				   return new Text.Plane("OK", 200);
			   });
			   cmdserver.Route("^/StopAll$", (HttpListenerRequest request) =>
			   {
				   foreach (Global global in globals.Values)
				   {
					   global.server.Stop();
				   }
				   stop = true;
				   return new Text.Plane("OK", 200);
			   });
			   cmdserver.Start();
		   }
		   else if (option.isStart)
		   {
			   HttpClient client = new HttpClient();
			   client.Timeout = TimeSpan.FromSeconds(2);
			   Stream s = client.Send(new HttpRequestMessage(HttpMethod.Get, $"http://localhost:26262/Start?path={Environment.CurrentDirectory}&name={option.name}")).Content.ReadAsStream();
			   byte[] datas = new byte[s.Length];
			   s.Read(datas, 0, datas.Length);
			   Console.WriteLine(Encoding.UTF8.GetString(datas));
			   Environment.Exit(0);
		   }
		   else if (option.isRestart)
		   {
			   HttpClient client = new HttpClient();
			   client.Timeout = TimeSpan.FromSeconds(2);
			   Stream s = s = client.Send(new HttpRequestMessage(HttpMethod.Get, $"http://localhost:26262/Restart?name={option.name}")).Content.ReadAsStream();
			   byte[] datas = new byte[s.Length];
			   s.Read(datas, 0, datas.Length);
			   Console.WriteLine(Encoding.UTF8.GetString(datas));
			   Environment.Exit(0);
		   }
		   else if (option.isStop)
		   {
			   HttpClient client = new HttpClient();
			   client.Timeout = TimeSpan.FromSeconds(2);
			   Stream s = s = client.Send(new HttpRequestMessage(HttpMethod.Get, $"http://localhost:26262/Stop?name={option.name}")).Content.ReadAsStream();
			   byte[] datas = new byte[s.Length];
			   s.Read(datas, 0, datas.Length);
			   Console.WriteLine(Encoding.UTF8.GetString(datas));
			   Environment.Exit(0);
		   }
		   else if (option.isRestartAll)
		   {

		   }
		   else if (option.isStopAll)
		   {
			   HttpClient client = new HttpClient();
			   Stream s = client.Send(new HttpRequestMessage(HttpMethod.Get, $"http://localhost:26262/StopAll")).Content.ReadAsStream();
			   byte[] datas = new byte[s.Length];
			   s.Read(datas, 0, datas.Length);
			   Console.WriteLine(Encoding.UTF8.GetString(datas));
			   Environment.Exit(0);
		   }
		   else if (option.isHelp)
		   {
			   Console.WriteLine(HelpText.AutoBuild<Options>(Parser.Default.ParseArguments<Options>(args)).ToString());
			   Environment.Exit(0);
		   }
	   });
}
catch(Exception e)
{
	Console.ForegroundColor = ConsoleColor.Red;
	Console.WriteLine(e.Message);
	Console.ForegroundColor = ConsoleColor.White;
	Environment.Exit(0);
}

while (true)
{
	if (stop)
	{
		Thread.Sleep(1000);
		if (cmdserver != null)
		{
			cmdserver.Stop();
		}
		return;
	}
}

void StartServer(Global global)
{
	string configpath = Path.Combine(global.root, "config.yaml");
	string scriptpath = Path.Combine(global.root, "server.cs");
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
		foreach (string dllname in server.imports)
		{
			assemblies.Add(Assembly.LoadFile(AppDomain.CurrentDomain.BaseDirectory + dllname));
		}
		assemblies.Add(typeof(HttpListenerContext).Assembly);
		global.server = new HttpServer(server.host, server.useHttps, server.threadCount);
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
				"});\r\n" +
				"server.RouteOther((HttpListenerRequest request) =>\r\n" +
				"{\r\n\t\t" +
				"return new Text(\"404\");\r\n" +
				"});");
			scriptState = CSharpScript.RunAsync(File.ReadAllText(scriptpath), scriptOptions, global).Result;
		}
		global.server.Start();
		if (false)
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
	}
	catch (Exception e)
	{
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine(e.Message);
		Console.ForegroundColor = ConsoleColor.White;
	}
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
	public string root;
}

public class Command
{
	public CommandType commandType;
	public string commandArgs;
}

public enum CommandType
{
	StopAll = 0,
	RestartAll = 1,
	Stop = 2,
	Restart = 3,
	Start = 4
}

class Options
{
	[Option("name", Required = false, HelpText = "Set a name")]
	public string? name { get; set; }

	[Option("start", Required = false, HelpText = "Start a server")]
	public bool isStart { get; set; }

	[Option("restart", Required = false, HelpText = "Restart a server")]
	public bool isRestart { get; set; }

	[Option("stop", Required = false, HelpText = "Stop a server")]
	public bool isStop { get; set; }

	[Option("restartall", Required = false, HelpText = "Restart all server")]
	public bool isRestartAll { get; set; }

	[Option("stopall", Required = false, HelpText = "Restart all server")]
	public bool isStopAll { get; set; }
	[Option("init", Required = false, HelpText = "Init Hamster")]
	public bool isInit { get; set; }

	[Option("work", Required = false, HelpText = "Start a Hamster work unit.This is automatically controlled by Hamster,please do not use this option.")]
	public bool isWork { get; set; }

	[Option("help", Required = false, HelpText = "GetHelp")]
	public bool isHelp { get; set; }
}