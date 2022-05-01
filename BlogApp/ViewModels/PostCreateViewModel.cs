using BlogApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.ViewModels
{
    public class PostCreateViewModel
    {
        public int ID { get; set; }
        public Post Post { get; set; }
        public List<SelectListItem> Categories { get; set; }
        [Display(Name ="Upload Photo:")]
        public IFormFile uploadPhoto { get; set; }
        //public List<SelectListItem> TagS { get; set; }
    }
}
