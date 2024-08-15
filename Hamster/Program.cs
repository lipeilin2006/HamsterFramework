using System.Net;
using System.Reflection;
using Hamster.Core;
using Hamster.Utils;
using Hamster.Utils.Routing;

string host = "localhost:5000";
bool useHttps = false;
LogLevel logLevel = LogLevel.Debug;
string logfile = $".{Path.DirectorySeparatorChar}log.txt";

Logger.Level = logLevel;
Logger.SetLogFile(logfile);
HttpServer server = new(host, useHttps);

Load();

server.Start();
AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
void CurrentDomain_ProcessExit(object? sender, EventArgs e)
{
	server.Stop();
}

while (true)
{
	string command = Console.ReadLine();
    if (command == "exit" || command == "quit")
    {
        break;
    }
	switch (command)
	{
		case "reload":
            Reload(); break;
	}
}

void Load()
{
    Assembly assembly = Assembly.Load(File.ReadAllBytes($"{AppDomain.CurrentDomain.BaseDirectory}Hamster.Hotfix.dll"));
    Type type = assembly.GetType("Hamster.Hotfix.Routes");
    var method1 = type.GetMethod("GetRoutes");

    Dictionary<string, IRoute> routes = (Dictionary<string, IRoute>)method1.Invoke(null, null);
    foreach (var route in routes.Keys)
    {
        server.Route(route, routes[route].Process);
    }

    server.RouteOther((Func<HttpListenerRequest, IResponse>)type.GetMethod("Other").Invoke(null, null));
}

void Reload()
{
	server.routes.Clear();
	server.funcs.Clear();
    Load();
}