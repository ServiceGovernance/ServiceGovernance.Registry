using Microsoft.AspNetCore.Mvc;
using Registry.Models;
using ServiceGovernance.Registry.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Registry.Controllers
{
    public class HomeController : Controller
    {
        private readonly IServiceRegistry _serviceRegistry;

        public HomeController(IServiceRegistry serviceRegistry)
        {
            _serviceRegistry = serviceRegistry ?? throw new ArgumentNullException(nameof(serviceRegistry));
        }

        public async Task<IActionResult> Index()
        {
            return View(await _serviceRegistry.GetAllServicesAsync());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
