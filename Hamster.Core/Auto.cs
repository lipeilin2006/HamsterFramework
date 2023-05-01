using MimeMapping;
using System.Net;

namespace Hamster.Core
{
	public class Auto : RouteAction
	{
		public string filename;
		public Auto(string filename)
		{
			this.filename = filename;
		}
		public override async Task Run(HttpListenerContext context)
		{
			context.Response.ContentType = MimeUtility.GetMimeMapping(filename);
			if (File.Exists(filename))
			{
				FileStream fs = File.OpenRead(filename);
				await fs.CopyToAsync(context.Response.OutputStream);
				context.Response.StatusCode = 200;
			}
			else
			{
				context.Response.StatusCode = 404;
			}
			context.Response.Close();
		}
	}
}
