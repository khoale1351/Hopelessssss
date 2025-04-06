namespace Travel.Models
{
    public class TourImage
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public string ImageUrl { get; set; }
        public DateTime UploadDate { get; set; }
        public Tour Tour { get; set; }

    }
}
