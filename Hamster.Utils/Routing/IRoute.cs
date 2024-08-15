using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hamster.Utils.Routing
{
    public interface IRoute
    {
        public IResponse Process(HttpListenerRequest request, MatchCollection matches);
    }
}
