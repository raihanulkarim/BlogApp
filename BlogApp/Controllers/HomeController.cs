using BlogApp.Data;
using BlogApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BlogApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            this.context = context;
        }

        public async Task<IActionResult> Index(int? pageNumber)
        {
            var res = from s in context.Posts
                           .Include(r=> r.Author)
                           select s;
            //var res = await context.Posts.OrderByDescending(p => p.Id).Include(r => r.Author).ToListAsync();
            if (res.Count() == 0)
            {
                ViewBag.flag = false;
            }
            int pageSize = 10;
            var posts = await PaginatedList<Post>.CreateAsync(res.AsNoTracking(), pageNumber ?? 1, pageSize);
            return View(posts);
        }

        public IActionResult Contact()
        {
            return View();
        }
        public IActionResult About()
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
