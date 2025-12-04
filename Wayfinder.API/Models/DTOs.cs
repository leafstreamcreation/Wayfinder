using System;
using System.ComponentModel.DataAnnotations;

namespace Wayfinder.API.Models.DTOs
{
    #region Authentication DTOs

    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [MaxLength(7)]
        public string Color1 { get; set; }

        [MaxLength(7)]
        public string Color2 { get; set; }

        [MaxLength(7)]
        public string Color3 { get; set; }
    }

    public class AuthResponse
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public UserDto User { get; set; }
    }

    #endregion

    #region User DTOs

    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Color1 { get; set; }
        public string Color2 { get; set; }
        public string Color3 { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateUserRequest
    {
        [EmailAddress]
        public string Email { get; set; }

        [MinLength(6)]
        public string Password { get; set; }

        [MaxLength(7)]
        public string Color1 { get; set; }

        [MaxLength(7)]
        public string Color2 { get; set; }

        [MaxLength(7)]
        public string Color3 { get; set; }
    }

    #endregion

    #region Tag DTOs

    public class TagDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TaskId { get; set; }
    }

    public class CreateTagRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public int TaskId { get; set; }
    }

    public class UpdateTagRequest
    {
        [MaxLength(100)]
        public string Name { get; set; }

        public int? TaskId { get; set; }
    }

    #endregion

    #region Task DTOs

    public class TaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int UserId { get; set; }
        public DateTime? LastFinishedDate { get; set; }
        public int RefreshInterval { get; set; }
        public int AlertThresholdPercentage { get; set; }
        public bool IsActive { get; set; }
        public int InitialRefreshInterval { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateTaskRequest
    {
        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        [Required]
        public int RefreshInterval { get; set; }

        [Range(0, 100)]
        public int AlertThresholdPercentage { get; set; }

        public bool IsActive { get; set; } = true;

        public int? InitialRefreshInterval { get; set; }
    }

    public class UpdateTaskRequest
    {
        [MaxLength(255)]
        public string Title { get; set; }

        public int? RefreshInterval { get; set; }

        [Range(0, 100)]
        public int? AlertThresholdPercentage { get; set; }

        public bool? IsActive { get; set; }

        public int? InitialRefreshInterval { get; set; }
    }

    #endregion

    #region Record DTOs

    public class RecordDto
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public DateTime FinishedDate { get; set; }
        public string Status { get; set; }
    }

    public class CreateRecordRequest
    {
        [Required]
        public int TaskId { get; set; }

        public DateTime? FinishedDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; }
    }

    public class UpdateRecordRequest
    {
        public DateTime? FinishedDate { get; set; }

        [MaxLength(50)]
        public string Status { get; set; }
    }

    #endregion

    #region TaskTag DTOs

    public class TaskTagDto
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int TagId { get; set; }
    }

    public class CreateTaskTagRequest
    {
        [Required]
        public int TaskId { get; set; }

        [Required]
        public int TagId { get; set; }
    }

    #endregion
}
