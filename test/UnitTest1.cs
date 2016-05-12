using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Mvc;
using AspNetMvcUrlHelper;
using System.Web.Routing;
using System.Linq.Expressions;
using System.Text;

namespace TestUnit
{
    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void TestCalc()
        {
            var calc = new ExpressionCalculate();

            var tmp = "abc";
            var tmp2 = new string[] { "a", "b", "c" };
            Expression<Func<MockController, ActionResult>> e = (controller) => controller.ManyArgs(
                "hello",                        //ConstantExpression
                tmp.Length,                     //MemberExpression
                new StringBuilder(),            //NewExpression
                tmp2[1],                        //IndexExpression   Unhandle expression
                (object)tmp.Length,             //UnaryExpression (Convert)
                new int[] {1,2,3,4,5,6}[5]      //UnhandleExpression shuld Complie
            );

            var args = (e.Body as MethodCallExpression).Arguments;

            Assert.IsTrue(calc.Calculate(args[0]) as string == "hello");
            Assert.IsTrue((int)calc.Calculate(args[1]) == tmp.Length);
            Assert.IsNotNull((calc.Calculate(args[2]) as StringBuilder) ?? null);
            Assert.IsTrue(calc.Calculate(args[3]) as string == tmp2[1]);
            Assert.IsTrue((int)calc.Calculate(args[4]) == tmp.Length);
            Assert.IsTrue((int)calc.Calculate(args[5]) == 6);
            

        }

        [TestMethod]
        public void TestGetRoute()
        {
            var route = Extension.GetRouteData<MockController>(p => p.ManyArgs(1, 2, 3, 4, 5, 6));

            Assert.AreEqual("MockArea", route["area"]);
            Assert.AreEqual("Mock", route["controller"]);
            Assert.AreEqual("ManyArgs", route["action"]);

            Assert.AreEqual(1, route["a"]);
            Assert.AreEqual(2, route["b"]);
            Assert.AreEqual(3, route["c"]);
            Assert.AreEqual(4, route["d"]);
            Assert.AreEqual(5, route["e"]);
            Assert.AreEqual(6, route["f"]);

        }
    }
}
