namespace net_il_mio_fotoalbum.Models
{
    public class Photo
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public byte[] Image { get; set; }

        public bool Visible { get; set; }

        public List<Category>? Categories { get; set; }
    }
}
