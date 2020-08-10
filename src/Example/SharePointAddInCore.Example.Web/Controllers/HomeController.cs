using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using SharePointAddInCore.Core.SharePointContext;
using SharePointAddInCore.Example.Web.Models;

namespace SharePointAddInCore.Example.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ISharePointContext _sharePointContext;

        public HomeController(
            ILogger<HomeController> logger,
            ISharePointContext sharePointContext)
        {
            _logger = logger;
            _sharePointContext = sharePointContext;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _sharePointContext.GetUserAccessTokenForSPHost();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
