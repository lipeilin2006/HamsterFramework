using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Hamster.Utils;
using Hamster.Utils.Components;
using Hamster.Utils.Routing;

namespace Hamster.Core
{
	public class HttpServer : IDisposable
	{
		HttpListener? listener = null;
		public string host { get; private set; } = "*";
		public bool useHttps { get; set; } = false;
		private Thread? listenLoop;
		private bool isExit = false;

		public List<string> routes = new List<string>();
		public List<Func<HttpListenerRequest, MatchCollection, IResponse>> funcs = new List<Func<HttpListenerRequest, MatchCollection, IResponse>>();
		private Func<HttpListenerRequest, IResponse>? other;

		private List<Component> components = new List<Component>();
		public HttpServer() : this("*", false) { }
		public HttpServer(string host, bool useHttps)
		{
			this.host = host;
			this.useHttps = useHttps;
			listener = new HttpListener();
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		}
		/// <summary>
		/// Start server.
		/// </summary>
		public void Start()
		{
			try
			{
				Console.WriteLine("Starting Server");
				listener?.Prefixes.Add((useHttps == true ? "https://" : "http://") + host + "/");
				listener?.Start();
				listenLoop = new Thread(Listen);
				listenLoop.Start();
				Console.WriteLine("Listening on " + host);
			}
			catch (Exception ex)
			{
				Logger.LogError(ex.Message.ToString() + " while starting");
			}
		}
		/// <summary>
		/// Stop server.
		/// </summary>
		public void Stop()
		{
			try
			{
				Console.WriteLine("Stopping server");
				Logger.Close();
				isExit = true;
				foreach(Component component in components)
				{
					component.OnRemoved();
					components.Remove(component);
				}
				if (listener != null)
				{
					listener.Stop();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message.ToString());
			}
		}
		/// <summary>
		/// Add a route.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="func"></param>
		public void Route(string path, Func<HttpListenerRequest, MatchCollection, IResponse> func)
		{
			routes.Add(path);
			funcs.Add(func);
		}
		/// <summary>
		/// Add a route which will be executed when routes not match.
		/// </summary>
		/// <param name="func"></param>
		public void RouteOther(Func<HttpListenerRequest, IResponse> func)
		{
			other = func;
		}
		/// <summary>
		/// Server listen loop.
		/// </summary>
		private async void Listen()
		{
			while (!isExit)
			{
				try
				{
					if (listener != null)
					{
						HttpListenerContext context = await listener.GetContextAsync();
						Logger.LogDebug($"[{context.Request.RemoteEndPoint}]{context.Request.Url.ToString()}");
						bool isMatch = false;
						for (int ii = 0; ii < routes.Count; ii++)
						{
							MatchCollection matches = Regex.Matches(context.Request.Url.LocalPath, routes[ii]);
							if (matches.Count > 0)
							{
								isMatch = true;
								await funcs[ii](context.Request, matches).Produce(context);
								break;
							}
						}
						if (!isMatch)
						{
							if (other != null)
							{
								await other(context.Request).Produce(context);
							}
							else
							{
								await new Text.Plane("404", 404).Produce(context);
                            }
						}
					}
				}
				catch (Exception ex)
				{
					Logger.LogError(ex.Message.ToString() + " while listening(position 1)");
				}
			}
		}

		public T? AddComponent<T>() where T : Component, new()
		{
			for (int i = 0; i < components.Count; i++)
			{
				if (components[i].GetType() == typeof(T))
				{
					return null;
				}
			}
			T component = new T();
			components.Add(component);
			component.OnAdded();
			return component;
		}
		public void RemoveComponent<T>() where T : Component
		{
			foreach (var component in components)
			{
				if(component.GetType() == typeof(T))
				{
					component.OnRemoved();
					components.Remove(component);
				}
			}
		}

		public void Dispose()
		{
			Stop();
		}
	}
}