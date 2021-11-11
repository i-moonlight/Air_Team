namespace AirTeamApi.Services.Models
{
    public sealed class ImageDto
    {
        public string ImageId { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string BaseImageUrl { get; set; } = default!;
        public string DetailUrl { get; set; } = default!;
    }
}
