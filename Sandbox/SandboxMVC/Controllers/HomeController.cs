using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using RazorLight;
using RazorLight.MVC;
using System.Linq.Expressions;
using System;

namespace SandboxMVC.Controllers
{
	public class HomeController : Controller
	{
		private readonly IRazorLightEngine engine;

		public HomeController(IRazorLightEngine engine)
		{
			this.engine = engine;
		}

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult About()
		{
			ViewData["Message"] = "Your application description page.";

			return View();
		}

		public IActionResult Contact()
		{
			ViewData["Message"] = "Your contact page.";

			return View();
		}

		public IActionResult Go()
		{
			//const int MAX = 10000000;
			//var sw = new System.Diagnostics.Stopwatch();
			//var model = new TestViewModel();
			//var prop = model.GetType().GetTypeInfo().GetProperty("Title");

			string result = engine.Parse("Test.cshtml", new TestViewModel());

			return Content("ewfwef");
		}

		public Action<object, object> Exp(PropertyInfo info)
		{
			var instance = Expression.Parameter(typeof(object), "instance");
			var value = Expression.Parameter(typeof(object), "value");

			UnaryExpression instanceCast = Expression.TypeAs(instance, info.DeclaringType);
			UnaryExpression valueCast = Expression.TypeAs(value, info.PropertyType);


			var setDelegate = Expression.Lambda<Action<object, object>>(Expression.Call(instanceCast, info.GetSetMethod(), valueCast),
				new ParameterExpression[] { instance, value }).Compile();

			return setDelegate;
		}

		public IActionResult Error()
		{
			return View();
		}
	}
}
