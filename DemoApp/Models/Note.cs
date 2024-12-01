using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Note
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; } = "";
        public string ImgName { get; set; } = "";

        [NotMapped]
        public IFormFile? Img { get; set; }
    }
}
