using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Cinema_Project.ViewModel
{
    public class ShowTimeVM
    {
        public int Id { get; set; }

        [Required]
        public int MovieId { get; set; }

        [Required]
        public int CinemaId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        [Range(1, 10000)]
        public decimal Price { get; set; }

        public IEnumerable<SelectListItem> Movies { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Cinemas { get; set; } = new List<SelectListItem>();
    }
}
