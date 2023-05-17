using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace net_il_mio_fotoalbum.Models
{
    [Table("photo")]
    public class Photo
    {
        [Key] public long Id { get; set; }

        [Required] public string Title { get; set; }

        [Required] public string Description { get; set; }

        public byte[]? Image { get; set; }

        [Required] public bool Visibility { get; set; }

        public List<Category>? Categories { get; set; }
    }
}
