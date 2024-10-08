﻿using System.Net;

namespace Hamster.Utils.Routing
{
    public class Image : IResponse
    {
        private string contentType = "";
        private byte[]? contents;
        private Stream? stream;
        public Image(string contentType, byte[] contents)
        {
            this.contentType = contentType;
            this.contents = contents;
        }
        public Image(string contentType, Stream stream)
        {
            this.contentType = contentType;
            this.stream = stream;
        }
        public async Task Produce(HttpListenerContext context)
        {
            context.Response.AddHeader("Content-type", contentType);
            context.Response.StatusCode = 200;
            if (stream != null)
            {
                await stream.CopyToAsync(context.Response.OutputStream);
            }
            else
            {
                await context.Response.OutputStream.WriteAsync(contents);
            }
            context.Response.Close();
        }
        public class JPEG : Image
        {
            public JPEG(byte[] contents) : base("image/jpeg", contents) { }
            public JPEG(Stream stream) : base("image/jpeg", stream) { }
        }
        public class PNG : Image
        {
            public PNG(byte[] contents) : base("image/png", contents) { }
            public PNG(Stream stream) : base("image/png", stream) { }
        }
    }
}
