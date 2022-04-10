using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BlogApp.Data;
using BlogApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using BlogApp.ViewModels;

namespace BlogApp.Controllers
{
    [Authorize]
    public class BlogsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> userManager;

        public BlogsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            this.userManager = userManager;
        }

        // GET: Blog
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Posts.Where(r => r.AuthorId == userManager.GetUserId(HttpContext.User)).Include(r => r.Author).Include(p => p.PostCats).ThenInclude(r=>r.Category);
            if (applicationDbContext.Count() == 0)
            {
                ViewBag.flag = false;
            }
            ViewBag.cat = _context.Categories.Where(r => r.UserId == userManager.GetUserId(HttpContext.User));
            return View(await applicationDbContext.OrderByDescending(r=> r.Id).ToListAsync());
        }

        // GET: Blog/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Blog/Create
        public IActionResult Create()
        {
            var cat = _context.Categories.ToList();
            PostCreateViewModel viewModel = new PostCreateViewModel();
            viewModel.Categories = cat.Select(r => new SelectListItem()
            {
                Text = r.Name,
                Value = r.Id.ToString()
            }).ToList();
            return View(viewModel);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PostCreateViewModel viewModel)
        {
            var selectedValue = viewModel.Categories.Where(r => r.Selected).Select(r => r.Value).Select(int.Parse).ToList();
            if (ModelState.IsValid)
            {
                var post = new Post
                {
                    Title = viewModel.Title,
                    Description = viewModel.Description,
                    SubTitle = viewModel.SubTitle,
                    PostedDate = viewModel.PublishDate,
                    AuthorId = userManager.GetUserId(HttpContext.User)
                };
                _context.Add(post);
                await _context.SaveChangesAsync();

                foreach (var cat in selectedValue)
                {
                    _context.PostCats.Add(
                        new PostCat
                        {
                            PostId = post.Id,
                            CatId = cat
                        }
                        );
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Blog/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }
            if (UserExists(post) == false)
            {
                return NotFound();
            }
            return View(post);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Post post)
        {
            if (id != post.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }

        // GET: Blog/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            if (UserExists(post) == false)
            {
                return NotFound();
            }
            return View(post);
        }

        // POST: Blog/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
        public bool UserExists(Post post)
        {
            if (userManager.GetUserId(HttpContext.User) != post.AuthorId )
            {
                return false;
            }
            return true;
        }
    }
}
