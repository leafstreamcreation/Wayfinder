using System.ComponentModel.DataAnnotations;

namespace Wayfinder.API.Models
{
    /// <summary>
    /// Tag entity for categorizing tasks
    /// </summary>
    public class Tag
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public int TaskId { get; set; }
    }
}
