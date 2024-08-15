using System.Net;

namespace Hamster.Utils.Routing
{
    public class StreamingFile : IResponse
    {
        public string contentType = "";
        public byte[] contents;
        public int statusCode = 200;
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

        public async Task Produce(HttpListenerContext context)
        {
            context.Response.AddHeader("Content-type", contentType);
            context.Response.StatusCode = statusCode;
            await context.Response.OutputStream.WriteAsync(contents);
            context.Response.Close();
        }
    }
}
