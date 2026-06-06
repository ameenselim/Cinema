using System.ComponentModel.DataAnnotations;

namespace Cinema_Project.ViewModel
{
    public class CreateCinemaVM
    {
        [Required(ErrorMessage = "Cinema name is required")]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Cinema description is required")]
        [StringLength(500, MinimumLength = 10)]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Cinema address is required")]
        [StringLength(200, MinimumLength = 3)]
        public string Address { get; set; } = null!;

        [Required(ErrorMessage = "Cinema image is required")]
        public IFormFile CinemaImg { get; set; } = null!;

        [Required]
        [Range(1, 26, ErrorMessage = "Rows count must be between 1 and 26")]
        public int RowsCount { get; set; }

        [Required]
        [Range(1, 20, ErrorMessage = "Seats per row must be between 1 and 20")]
        public int SeatsPerRow { get; set; }

        public string? Logo { get; set; }
    }
}
