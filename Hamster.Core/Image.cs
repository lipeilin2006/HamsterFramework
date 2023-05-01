using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Hamster.Core
{
	public class Image : RouteAction
	{
		public string contentType = "";
		public byte[] contents;
		public Image(string contentType, byte[] contents)
		{
			this.contentType = contentType;
			this.contents = contents;
		}
		public override async Task Run(HttpListenerContext context)
		{
			context.Response.AddHeader("Content-type", contentType);
			context.Response.StatusCode = statusCode;
			await context.Response.OutputStream.WriteAsync(contents);
			context.Response.Close();
		}
		public class JPEG : Image
		{
			public JPEG(byte[] contents) : base("", contents) { }
		}
		public class PNG : Image
		{
			public PNG(byte[] contents) : base("", contents) { }
		}
	}
}
