using AspNetMvcUrlHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace AspNetMvcUrlHelper
{
    public class ExpressionRouteData
    {
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public RouteValueDictionary RouteDictionray { get; set; }
    }

    public static class MvcExtension
    {
        private static ExpressionRouteData GetRoute<TController>(MethodInfo method)
        {
            var actionName = method.Name;
            var controllerType = typeof(TController);
            var controllerName = controllerType.Name.Substring(0, controllerType.Name.Length - "Controller".Length);

            return new ExpressionRouteData
            {
                ActionName = actionName,
                ControllerName = controllerName,
                RouteDictionray = null
            };
        }

        public static ExpressionRouteData GetRouteData<TController>(Expression<Func<TController, ActionResult>> location)
        {
            if (location.Body is MethodCallExpression)
            {
                var expression = (MethodCallExpression)location.Body;
                var methodParameters = expression.Method.GetParameters();
                var routeData = new Dictionary<string, object>();

                for (var i = 0; i < methodParameters.Count(); i++)
                {
                    var arg = expression.Arguments[i];
                    object val = null;
                    if (arg.TryGetValue(ref val))
                    {
                        if (val == null) val = "";
                        routeData.Add(methodParameters[i].Name, val);
                    }
                    else
                    {
                        throw new ArgumentException("表达式不符合要求。");
                    }
                }

                var result = GetRoute<TController>(expression.Method);
                result.RouteDictionray = new RouteValueDictionary(routeData);

                return result;
            }
            else
            {
                throw new ArgumentException(nameof(location));
            }
        }

        public static RedirectResult RedirectToAction<TController>(this Controller controller, Expression<Func<TController, ActionResult>> location)
        {
            return new RedirectResult(controller.LocationTo(location));
        }
        public static RedirectResult RedirectToAction<TController>(this TController controller, Expression<Func<TController, ActionResult>> location)
            where TController : Controller
        {
            return new RedirectResult(controller.LocationTo(location));
        }
        public static string LocationTo<TController>(this TController controller, Expression<Func<TController, ActionResult>> location)
            where TController : Controller
        {
            var data = GetRouteData(location);
            return controller.Url.Action(data.ActionName, data.ControllerName, data.RouteDictionray);
        }
        public static string LocationTo<TController>(this Controller controller, Expression<Func<TController, ActionResult>> location)
        {
            var data = GetRouteData(location);
            return controller.Url.Action(data.ActionName, data.ControllerName, data.RouteDictionray);
        }
        public static string LocationTo<TController>(this Controller controller, Func<TController, Func<ActionResult>> location)
        {
            var data = GetRoute<TController>(location.Method);
            return controller.Url.Action(data.ActionName, data.ControllerName);
        }

        public static MvcHtmlString Action<TController>(this HtmlHelper html, Expression<Func<TController, ActionResult>> location)
        {
            var data = GetRouteData(location);
            return html.Action(data.ActionName, data.ControllerName, data.RouteDictionray);
        }
        public static void RenderAction<TController>(this HtmlHelper html, Expression<Func<TController, ActionResult>> location)
        {
            var data = GetRouteData(location);
            html.RenderAction(data.ActionName, data.ControllerName, data.RouteDictionray);
        }

        public static string Action<TController>(this UrlHelper url, Expression<Func<TController, ActionResult>> location)
        {
            var data = GetRouteData(location);
            return url.Action(data.ActionName, data.ControllerName, data.RouteDictionray);
        }
        public static string LocationTo<TController>(this HtmlHelper html, Expression<Func<TController, ActionResult>> location)
        {
            var data = GetRouteData(location);
            UrlHelper url = new UrlHelper(html.ViewContext.RequestContext);
            return url.Action(data.ActionName, data.ControllerName, data.RouteDictionray);
        }

        public static bool TryGetValue(this Expression expression, ref object result)
        {
            if (expression is LambdaExpression)
            {
                return TryGetValue(((LambdaExpression)expression).Body, ref result);
            }
            else if (expression is MemberExpression)
            {
                if (TryGetValue(((MemberExpression)expression).Expression, ref result))
                {
                    return TryGetValue(((MemberExpression)expression).Member, result, out result);
                }
                else
                {
                    result = false;
                    return false;
                }
            }
            else if (expression is UnaryExpression)
            {
                return TryGetValue(((UnaryExpression)expression).Operand, ref result);
            }
            else if (expression is ConstantExpression)
            {
                result = ((ConstantExpression)expression).Value;
                return true;
            }
            else if (expression is ParameterExpression)
            {
                return true;
            }

            result = null;
            return false;
        }
        public static bool TryGetValue(this MemberInfo member, object obj, out object result)
        {
            if (obj == null)
            {
                result = null;
                return true;
            }
            switch (member.MemberType)
            {
                case MemberTypes.Property:
                    result = ((PropertyInfo)member).GetValue(obj);
                    return true;
                case MemberTypes.Method:
                    result = ((MethodInfo)member).Invoke(obj, new object[0]);
                    return true;
                case MemberTypes.Field:
                    result = ((FieldInfo)member).GetValue(obj);
                    return true;
                default:
                    result = null;
                    return false;
            }
        }

    }
}
