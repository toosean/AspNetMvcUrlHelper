using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;

namespace AspNetMvcUrlHelper
{
    public static class RouteHelper
    {
        public static string GetAction(this RouteValueDictionary routes)
        {
            object value = null;
            if(routes.TryGetValue("action", out value))
            {
                return value.ToString();
            }
            else
            {
                return null;
            }
        }
    }
}
