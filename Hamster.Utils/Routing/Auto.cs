using MimeMapping;
using System.Net;

namespace Hamster.Utils.Routing
{
    public class Auto : IResponse
    {
        public string filename;
        public Auto(string filename)
        {
            this.filename = filename;
        }
        public async Task Produce(HttpListenerContext context)
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
