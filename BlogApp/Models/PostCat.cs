namespace BlogApp.Models
{
    public class PostCat
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public int CatId { get; set; }
        public Post Post { get; set; }
        public Category Category { get; set; }
    }
}
