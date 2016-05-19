using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace TestUnit
{
    class MockController : Controller
    {
        public ActionResult Page1()
        {
            return Content("ABC");
        }

        public ActionResult ManyArgs(object a,object b,object c,object d,object e,object f,object g,object h)
        {
            return null;
        }
    }
}
