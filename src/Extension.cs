﻿using AspNetMvcUrlHelper;
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
    public static class Extension
    {

        private static ExpressionCalculate calculte = new ExpressionCalculate();
        private static RouteValueDictionary GetRoute<TController>(MethodInfo method)
        {
            var actionName = method.Name;
            var controllerType = typeof(TController);
            var controllerName = controllerType.Name.Substring(0, controllerType.Name.Length - "Controller".Length);
            var areaName = GetArea(controllerType);

            return new RouteValueDictionary()
            {
                ["action"] = actionName,
                ["controller"] = controllerName,
                ["area"] = areaName
            };
        }
        private static string GetArea(Type controllerType)
        {
            var areaRegistrationType = controllerType
                .Assembly
                .DefinedTypes
                .Where(w => w.IsSubclassOf(typeof(AreaRegistration)))
                .Where(w => controllerType.Namespace.StartsWith(w.Namespace))
                .FirstOrDefault();

            if (areaRegistrationType == null) return null;

            var registration = Activator.CreateInstance(areaRegistrationType) as AreaRegistration;

            return registration.AreaName;

        }

        public static RouteValueDictionary GetRouteData<TController>(Expression<Func<TController, ActionResult>> location)
        {
            if (!(location.Body is MethodCallExpression)) throw new ArgumentException(nameof(location));
            
            var expression = (MethodCallExpression)location.Body;
            var methodParameters = expression.Method.GetParameters();
            var routeData = new Dictionary<string, object>();

            for (var i = 0; i < methodParameters.Count(); i++)
            {
                var arg = expression.Arguments[i];

                object val = calculte.Calculate(arg);
                routeData.Add(methodParameters[i].Name, val);
            }

            var result = GetRoute<TController>(expression.Method);
            foreach(var item in routeData)
            {
                if (result.ContainsKey(item.Key))
                {
                    result[item.Key] = item.Value;
                }else
                {
                    result.Add(item.Key, item.Value);
                }
            }

            return result;
            
        }

        public static ActionResult RedirectToAction<TController>(this TController controller, Expression<Func<TController, ActionResult>> location)
        {
            var data = GetRouteData(location);
            return new RedirectToRouteResult(data);
        }
        public static ActionResult RedirectToAction<TController>(this Controller controller, Expression<Func<TController, ActionResult>> location)
        {
            var data = GetRouteData(location);
            return new RedirectToRouteResult(data);
        }

        public static string Action<TController>(this UrlHelper url, Expression<Func<TController, ActionResult>> location)
        {
            var data = GetRouteData(location);
            return url.Action(data.GetAction(), data);
        }

        public static string ActionUrl<TController>(this TController controller, Expression<Func<TController, ActionResult>> location)
            where TController : Controller
        {
            var data = GetRouteData(location);
            return controller.Url.Action(data.GetAction(), data);
        }

        public static MvcHtmlString Action<TController>(this HtmlHelper html,Expression<Func<TController,ActionResult>> location)
        {
            var data = GetRouteData(location);
            return html.Action(data.GetAction().ToString(), data);
        }
        public static void RenderAction<TController>(this HtmlHelper html,Expression<Func<TController,ActionResult>> location)
        {
            var data = GetRouteData(location);
            html.RenderAction(data.GetAction().ToString(), data);
        }

    }
}
