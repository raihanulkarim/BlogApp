using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BlogApp.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlogApp.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment webHostEnvironment;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, IWebHostEnvironment webHostEnvironment)
        {
            this.webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; }
      

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Description { get; set; }
            public string Occupation { get; set; }
            public string Organisation { get; set; }
            public string Facebook { get; set; }
            public string Twitter { get; set; }
            public string Instagram { get; set; }
            public string ProPic { get; set; }
            [NotMapped]
            public IFormFile uploadImage { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userInfo = _userManager.FindByIdAsync(_userManager.GetUserId(HttpContext.User)).Result;
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            Username = userName;
           
            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                FirstName = userInfo.FirstName,
                LastName = userInfo.LastName,
                Description = userInfo.Description,
                Occupation = userInfo.Occupation,
                Organisation = userInfo.Organisation,
                Facebook = userInfo.Facebook,
                Twitter = userInfo.Twitter,
                Instagram = userInfo.Instagram
             };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }
            user.FirstName = Input.FirstName;
            user.LastName = Input.LastName;
            user.Occupation = Input.Occupation;
            user.Organisation = Input.Organisation;
            user.Description = Input.Description;
            user.Facebook = Input.Facebook;
            user.Twitter = Input.Twitter;
            user.Instagram = Input.Instagram;

            if (Input.uploadImage != null)
            {
               if(user.ProPic != null)
                {
                    String ImgPath = Path.Combine(webHostEnvironment.WebRootPath + "/assets/img", user.ProPic);
                    FileInfo f = new FileInfo(ImgPath);
                    if (f != null)
                    {
                        System.IO.File.Delete(ImgPath);
                        f.Delete();
                    }
                }
                string extension = Path.GetExtension(Input.uploadImage.FileName);
                String fileName = "profile";
                user.ProPic = fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                string path = Path.Combine(webHostEnvironment.WebRootPath + "/assets/img", fileName);
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await Input.uploadImage.CopyToAsync(fileStream);
                }
            }
            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
