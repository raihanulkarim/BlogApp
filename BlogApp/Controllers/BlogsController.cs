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
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace BlogApp.Controllers
{
    [Authorize]
    public class BlogsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IWebHostEnvironment webHostEnvironment;

        public BlogsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            this.userManager = userManager;
            this.webHostEnvironment = webHostEnvironment;
        }

        // GET: Blog
        public async Task<IActionResult> Index(int? pageNumber)
        {
            var posts = _context.Posts.Where(r => r.AuthorId == userManager.GetUserId(HttpContext.User)).Include(r => r.Author).Include(p => p.PostCats).ThenInclude(r=>r.Category).OrderByDescending(r=> r.Id);
            if (posts.Count() == 0)
            {
                ViewBag.flag = false;
            }
            ViewBag.cat = _context.Categories.Where(r => r.UserId == userManager.GetUserId(HttpContext.User));
            int pageSize = 8;
            var res = await PaginatedList<Post>.CreateAsync(posts.AsNoTracking(), pageNumber ?? 1, pageSize);
            return View(res);
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
                .Include(r=> r.PostCats)
                .ThenInclude(r=> r.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            //ViewBag.Cats = await _context.Categories.Where(r => r.Id == post.).Select(r=> r.).ToListAsync();
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Blog/Create
        public IActionResult Create()
        {
            var cat = _context.Categories.Where(r => r.UserId == userManager.GetUserId(HttpContext.User)).ToList();
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
            
            if (ModelState.IsValid)
            {
                var userId = userManager.GetUserId(HttpContext.User);
                var post = new Post
                {
                    Title = viewModel.Post.Title,
                    Description = viewModel.Post.Description,
                    SubTitle = viewModel.Post.SubTitle,
                    PostedDate = viewModel.Post.PostedDate,
                    Photo = await PhotoUpload(viewModel),
                    AuthorId = userId
                };
                _context.Add(post);
                await _context.SaveChangesAsync();
                if (_context.Categories.Where(r=> r.UserId == userId).ToList().Count() > 0)
                {
                    var selectedValue = viewModel.Categories.Where(r => r.Selected).Select(r => r.Value).Select(int.Parse).ToList();

                    foreach (var cat in selectedValue)
                    {
                        _context.PostCats.Add(
                            new PostCat
                            {
                                Post = post,
                                Category = await _context.Categories.FirstOrDefaultAsync(r => r.Id == cat),
                                CatId = cat,
                                PostId = post.Id
                            }
                            );
                    }
                }
                await _context.SaveChangesAsync();
                return RedirectToAction("details", new { post.Id }); ;
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

            var cat = _context.Categories.ToList();
            PostCreateViewModel viewModel = new PostCreateViewModel();
            viewModel.Post = await _context.Posts.FirstOrDefaultAsync(m => m.Id == id);
            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PostCreateViewModel viewModel)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    var post = await _context.Posts.FirstOrDefaultAsync(r => r.Id == id);
                    //post = viewModel.Post;
                    post.AuthorId = userManager.GetUserId(HttpContext.User);
                    post.Title = viewModel.Post.Title;
                    post.SubTitle = viewModel.Post.SubTitle;
                    post.Description = viewModel.Post.Description;
                    PhotoDelete(post);
                    post.Photo = await PhotoUpload(viewModel);
                    _context.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(viewModel.Post.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("details", new {id });
            }
            return View(viewModel);
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
        public async Task<string> PhotoUpload(PostCreateViewModel createViewModel)
        {
            if (createViewModel.uploadPhoto != null)
            {
                string extension = Path.GetExtension(createViewModel.uploadPhoto.FileName);
                String fileName = "profile";
                createViewModel.Post.Photo = fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                string path = Path.Combine(webHostEnvironment.WebRootPath + "/assets/img", fileName);
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await createViewModel.uploadPhoto.CopyToAsync(fileStream);
                }
                return createViewModel.Post.Photo;
            }
            else
            {
                return null;

            }
        }
        public void PhotoDelete(Post post)
        {
            if (post.Photo != null)
            {
                String ImgPath = Path.Combine(webHostEnvironment.WebRootPath + "/assets/img", post.Photo);
                FileInfo f = new FileInfo(ImgPath);
                if (f != null)
                {
                    System.IO.File.Delete(ImgPath);
                    f.Delete();
                }
            }
        }
}
    }
