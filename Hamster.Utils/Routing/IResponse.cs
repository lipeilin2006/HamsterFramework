using System.Net;

namespace Hamster.Utils.Routing
{
	public interface IResponse
	{
		public Task Produce(HttpListenerContext context);
	}
}
