using System.Collections.Generic;

namespace BlogApp.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UserId { get; set; }
        public ICollection<PostCat> PostCats { get; set; }
        public ApplicationUser User { get; set; }
    }
}
