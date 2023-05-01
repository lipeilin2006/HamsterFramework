using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using MimeMapping;

namespace Hamster.Core
{
	public class HttpServer
	{
		HttpListener? listener = null;
		public string host { get; private set; } = "*";
		public bool useHttps { get; set; } = false;
		public byte threadCount { get; private set; } = 1;
		private Thread[] threads;
		private Thread? listenLoop;
		private byte nextThread = 0;
		private Queue<HttpListenerContext>[] contexts;
		private bool isExit = false;

		private List<string> routes = new List<string>();
		private List<Func<HttpListenerRequest, MatchCollection, RouteAction>> funcs = new List<Func<HttpListenerRequest, MatchCollection, RouteAction>>();
		private Func<HttpListenerRequest, RouteAction>? other;
		public HttpServer()
		{
			listener = new HttpListener();
			threads = new Thread[threadCount];
			contexts=new Queue<HttpListenerContext>[threadCount];
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		}
		public HttpServer(string host, bool useHttps,byte threadCount)
		{
			this.host = host;
			this.useHttps = useHttps;
			this.threadCount = threadCount;
			listener = new HttpListener();
			threads = new Thread[threadCount];
			contexts = new Queue<HttpListenerContext>[threadCount];
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		}
		public void Start()
		{
			try
			{
				Console.WriteLine("Starting Server");
				for (int i = 0; i < contexts.Length; i++)
				{
					contexts[i] = new Queue<HttpListenerContext>();
				}
				for(int i=0; i<contexts.Length; i++)
				{
					object obj = i;
					ThreadStart start = () =>
					{
						int id = (int)obj;
						while (!isExit)
						{
							try
							{
								if (contexts[id].Count > 0)
								{
									HttpListenerContext context = contexts[id].Dequeue();
									bool isMatch = false;
									for(int ii = 0; ii < routes.Count; ii++)
									{
										MatchCollection matches = Regex.Matches(context.Request.Url.LocalPath, routes[ii]);
										if (matches.Count > 0)
										{
											isMatch = true;
											Task.Run(async () => { await funcs[ii](context.Request, matches).Run(context); });
											break;
										}
									}
									if (!isMatch)
									{
										Task.Run(async () => { await other(context.Request).Run(context); });
									}
								}
							}
							catch (Exception ex)
							{
								Console.ForegroundColor = ConsoleColor.Red;
								Console.WriteLine("[" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "]" + ex.Message.ToString() + " while listening(0)");
								Console.ForegroundColor = ConsoleColor.White;
							}
						}
					};
					threads[i] = new Thread(start);
					threads[i].Start();
				}
				listener.Prefixes.Add((useHttps == true ? "https://" : "http://") + host + "/");
				listener.Start();
				listenLoop = new Thread(Listen);
				listenLoop.Start();
				Console.WriteLine("Listening on " + host);
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("[" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "]" + ex.Message.ToString() + " while starting");
				Console.ForegroundColor = ConsoleColor.White;
			}
		}
		public void Stop()
		{
			try
			{
				Console.WriteLine("Stopping server");
				isExit = true;
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

		public void Route(string path, Func<HttpListenerRequest, MatchCollection, RouteAction> func)
		{
			routes.Add(path);
			funcs.Add(func);
		}
		public void RouteOther(Func<HttpListenerRequest, RouteAction> func)
		{
			other = func;
		}
		private void Listen()
		{
			while (!isExit)
			{
				try
				{
					if (listener != null)
					{
						HttpListenerContext context = listener.GetContext();
						contexts[nextThread].Enqueue(context);
						//Console.WriteLine("[" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "][" + context.Request.RemoteEndPoint + "]" + context.Request.Url.ToString());
						nextThread++;
						if (nextThread >= threads.Length)
						{
							nextThread = 0;
						}
					}
				}
				catch (Exception ex)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("[" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "]" + ex.Message.ToString() + " while listening(1)");
					Console.ForegroundColor = ConsoleColor.White;
				}
			}
		}
	}
	public abstract class RouteAction
	{
		public int statusCode = 200;
		public abstract Task Run(HttpListenerContext context);
	}
}