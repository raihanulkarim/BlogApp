using BlogApp.Data;
using BlogApp.Models;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index(int? pageNumber)
        {
            var res = from s in context.Posts
                           .Include(r=> r.Author)
                           .OrderByDescending(r=> r.Id)
                           select s;
            //var res = await context.Posts.OrderByDescending(p => p.Id).Include(r => r.Author).ToListAsync();
            if (res.Count() == 0)
            {
                ViewBag.flag = false;
            }
            int pageSize = 8;
            var posts = await PaginatedList<Post>.CreateAsync(res.AsNoTracking(), pageNumber ?? 1, pageSize);
            return View(posts);
        }

        public IActionResult Contact()
        {
            return View();
        }
        [Route("/profile")]
        public async Task<IActionResult> ProfileAsync(int? pageNumber, string uId)
        {
            var user = userManager.FindByIdAsync(uId).Result;
            var currentUser = userManager.GetUserId(HttpContext.User);

            if(uId == currentUser)
            {
                ViewBag.isSelfUser = true;
            }
            ViewBag.userId = user;
            if (user.Occupation == null && user.Organisation == null && user.ProPic == null)
            {
                //return RedirectToPage("./Areas/Identity/Pages/Account/Manage/Index"); 
                //return LocalRedirect("/Identity/Account/Manage/Index");
                return RedirectToPage("/Account/Manage/Index", new { area = "Identity" });
            }
            ViewBag.NumberOfDays = (DateTime.Now.Date - ViewBag.userId.JoinedDate).Days;
            var posts = context.Posts.Where(r => r.AuthorId == uId).Include(r => r.Author).Include(p => p.PostCats).ThenInclude(r => r.Category);
            if (posts.Count() == 0)
            {
                ViewBag.flag = false;
            }
            ViewBag.cat = context.Categories.Where(r => r.UserId == userManager.GetUserId(HttpContext.User));
            int pageSize = 8;
            var res = await PaginatedList<Post>.CreateAsync(posts.AsNoTracking(), pageNumber ?? 1, pageSize);
            return View(res);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
