using System.Net;
using System.Text;
using System.Text.RegularExpressions;

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
		private List<Func<HttpListenerRequest, RouteAction>> funcs = new List<Func<HttpListenerRequest, RouteAction>>();
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
										if (Regex.Matches(context.Request.Url.LocalPath, routes[ii]).Count > 0)
										{
											isMatch = true;
											Task.Run(() => { funcs[ii](context.Request).Run(context); });
											break;
										}
									}
									if (!isMatch)
									{
										Task.Run(() => { other(context.Request).Run(context); });
									}
								}
							}
							catch (Exception ex)
							{
								Console.ForegroundColor = ConsoleColor.Red;
								Console.WriteLine("[" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "]" + ex.Message.ToString() + " while listening");
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

		public void Route(string path, Func<HttpListenerRequest, RouteAction> func)
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
						Console.WriteLine("[" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "][" + context.Request.RemoteEndPoint + "]" + context.Request.Url.ToString());
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
					Console.WriteLine("[" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "]" + ex.Message.ToString() + " while listening");
					Console.ForegroundColor = ConsoleColor.White;
				}
			}
		}
	}

	public abstract class RouteAction
	{
		public int statusCode = 200;
		public abstract void Run(HttpListenerContext context);
	}
	public class Text : RouteAction
	{
		public string text = "";
		public string contentType = "";
		public Text(string text)
		{
			this.text = text;
		}
		public Text(string text,string contentType)
		{
			this.text = text;
			this.contentType = contentType;
		}

		public Text(string text, string contentType, int statusCode)
		{
			this.text = text;
			this.contentType = contentType;
			this.statusCode = statusCode;
		}

		public override void Run(HttpListenerContext context)
		{
			context.Response.AddHeader("Content-type", contentType);
			context.Response.ContentEncoding = Encoding.UTF8;
			context.Response.StatusCode = statusCode;
			context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(text));
			context.Response.Close();
		}
	}
	public class KFile : RouteAction
	{
		public string contentType = "";
		public byte[] contents;
		public KFile(string path)
		{
			contents = File.ReadAllBytes(path);
		}
		public KFile(string path,string contentType)
		{
			this.contentType = contentType;
			contents = File.ReadAllBytes(path);
		}
		public KFile(string path, string contentType, int statusCode)
		{
			this.contentType = contentType;
			this.statusCode = statusCode;
			contents = File.ReadAllBytes(path);
		}

		public override void Run(HttpListenerContext context)
		{
			context.Response.AddHeader("Content-type", contentType);
			context.Response.StatusCode = statusCode;
			context.Response.OutputStream.Write(contents);
			context.Response.Close();
		}
	}
}