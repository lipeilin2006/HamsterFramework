﻿using System.Net;
using System.Text;

namespace Hamster.Utils.Routing
{
    public class Text : IResponse
    {
        public string text = "";
        public string contentType = "";
        public int statusCode = 200;
        public Text(string text)
        {
            this.text = text;
        }
        public Text(string text, string contentType)
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

        public async Task Produce(HttpListenerContext context)
        {
            context.Response.AddHeader("Content-type", contentType);
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.StatusCode = statusCode;
            await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(text));
            context.Response.Close();
        }
        public class Plane : Text
        {
            public Plane(string text) : base(text, "text/plane") { }
            public Plane(string text, int statusCode) : base(text, "text/plane", statusCode) { }
        }
        public class HTML : Text
        {
            public HTML(string text) : base(text, "text/html") { }
            public HTML(string text, int statusCode) : base(text, "text/html", statusCode) { }
        }
        public class JavaScript : Text
        {
            public JavaScript(string text) : base(text, "text/javascript") { }
            public JavaScript(string text, int statusCode) : base(text, "text/javascript", statusCode) { }
        }
        public class CSS : Text
        {
            public CSS(string text) : base(text, "text/css") { }
            public CSS(string text, int statusCode) : base(text, "text/css", statusCode) { }
        }
    }
}
