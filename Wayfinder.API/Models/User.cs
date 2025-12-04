using System;
using System.ComponentModel.DataAnnotations;

namespace Wayfinder.API.Models
{
    /// <summary>
    /// User entity for authentication and task ownership
    /// </summary>
    public class User
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; }

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; }

        [MaxLength(7)]
        public string Color1 { get; set; }

        [MaxLength(7)]
        public string Color2 { get; set; }

        [MaxLength(7)]
        public string Color3 { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
