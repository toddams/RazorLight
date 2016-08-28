using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RazorLight;

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
			string result = engine.Parse("Test.cshtml", new TestViewModel());

			return Content(result);
		}

        public IActionResult Error()
        {
            return View();
        }
    }
}
