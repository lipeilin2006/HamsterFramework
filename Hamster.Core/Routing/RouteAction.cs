using System.Net;

namespace Hamster.Core.Routing
{
	public abstract class RouteAction
	{
		public int statusCode = 200;
		public abstract Task Run(HttpListenerContext context);
	}
}
