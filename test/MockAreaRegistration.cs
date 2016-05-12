using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace TestUnit
{
    class MockAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "MockArea";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            throw new NotImplementedException();
        }
    }
}
