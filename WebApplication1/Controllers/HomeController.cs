using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDataDistributedCache _cache;
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public HomeController(IDataDistributedCache cache, IConnectionMultiplexer connectionMultiplexer)
        {
            _cache = cache;
            _connectionMultiplexer = connectionMultiplexer;
        }

        [ResponseCache(Duration = 3000)]
        public IActionResult Index()
        {
            return View(new HomeIndexVm
            {
                Time = DateTime.Now
            });
        }

        public IActionResult RedisSample()
        {
            var db = _connectionMultiplexer.GetDatabase();

            var expiry = TimeSpan.FromTicks(DateTime.Now.AddMinutes(1).Ticks);
            db.StringSet("k2", "v1", expiry, When.Always);

            var t1 = db.KeyTimeToLive("k2");


            db.SetAdd("k1", "v1");
            db.SetAdd("k1", "v2");
            db.SetAdd("k1", "v3");


            var items = db.SetMembers("k1");


            _cache.SetString("msg", "hi");
            var val = _cache.Get("msg");

            return Content("OK");
        }


        [HttpGet("timecache")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        public IActionResult GetTimeFromCache()
        {
            return Json(new
            {
                Message = DateTime.Now.ToString(CultureInfo.InvariantCulture)
            });
        }

        [HttpGet("time")]
        public IActionResult GetTime()
        {
            return Json(new
            {
                Message = DateTime.Now.ToString(CultureInfo.InvariantCulture)
            });
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
