using System;
using System.ComponentModel.DataAnnotations;

namespace Wayfinder.API.Models
{
    /// <summary>
    /// Record entity for tracking task completion history
    /// </summary>
    public class Record
    {
        public int Id { get; set; }

        public int TaskId { get; set; }

        public DateTime FinishedDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; }
    }
}
