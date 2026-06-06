using CinemaProject.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Cinema_Project.ViewModel
{
    public class CreateMovieVM
    {
        [Required(ErrorMessage = "Movie name is required")]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, MinimumLength = 10)]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Price is required")]
        [Range(1, 10000, ErrorMessage = "Price must be between 1 and 10000")]
        public double Price { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Cinema is required")]
        public int CinemaId { get; set; }

        [Required(ErrorMessage = "Main image is required")]
        public IFormFile? MainImgFile { get; set; }

        public List<IFormFile>? SubImgs { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public String Status { get; set; } = null!;

        public IEnumerable<Category> Categories { get; set; } = [];
        public IEnumerable<Cinema> Cinemas { get; set; } = [];
    }
}