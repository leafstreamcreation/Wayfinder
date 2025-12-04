using System;
using System.ComponentModel.DataAnnotations;

namespace Wayfinder.API.Models
{
    /// <summary>
    /// Task entity for tracking repetitive tasks
    /// </summary>
    public class TaskItem
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        public int UserId { get; set; }

        public DateTime? LastFinishedDate { get; set; }

        /// <summary>
        /// Refresh interval in days
        /// </summary>
        public int RefreshInterval { get; set; }

        /// <summary>
        /// Alert threshold as a percentage (0-100)
        /// </summary>
        [Range(0, 100)]
        public int AlertThresholdPercentage { get; set; }

        public bool IsActive { get; set; }

        /// <summary>
        /// Initial refresh interval in days (for reset purposes)
        /// </summary>
        public int InitialRefreshInterval { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
