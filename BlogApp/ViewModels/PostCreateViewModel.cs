using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace BlogApp.ViewModels
{
    public class PostCreateViewModel
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Description { get; set; }
        public DateTime PublishDate { get; set; }
        public List<SelectListItem> Categories { get; set; }
    }
}
