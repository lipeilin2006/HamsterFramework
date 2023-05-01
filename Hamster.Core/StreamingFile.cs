using System.Net;

namespace Hamster.Core
{
	public class StreamingFile : RouteAction
	{
		public string contentType = "";
		public byte[] contents;
		public StreamingFile(string path)
		{
			contents = File.ReadAllBytes(path);
		}
		public StreamingFile(string path, string contentType)
		{
			this.contentType = contentType;
			contents = File.ReadAllBytes(path);
		}
		public StreamingFile(string path, string contentType, int statusCode)
		{
			this.contentType = contentType;
			this.statusCode = statusCode;
			contents = File.ReadAllBytes(path);
		}

		public override async Task Run(HttpListenerContext context)
		{
			context.Response.AddHeader("Content-type", contentType);
			context.Response.StatusCode = statusCode;
			await context.Response.OutputStream.WriteAsync(contents);
			context.Response.Close();
		}
	}
}
