using BlogApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace BlogApp.ViewModels
{
    public class PostCreateViewModel
    {
        public int ID { get; set; }
        public Post Post { get; set; }
        public List<SelectListItem> Categories { get; set; }
        //public List<SelectListItem> TagS { get; set; }
    }
}
