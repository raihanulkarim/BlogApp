using System;
using System.Collections;
using System.Collections.Generic;

namespace BlogApp.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Description { get; set; }
        public DateTime PostedDate { get; set; } = DateTime.Now;
        public string AuthorId { get; set; }
        public string Photo { get; set; }
        public ICollection<PostCat> PostCats { get; set; }
        public ApplicationUser Author { get; set; }
    }
}
