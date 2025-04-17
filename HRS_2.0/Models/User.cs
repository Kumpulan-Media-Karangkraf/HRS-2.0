using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HRS_2.Models
{
    [Index(nameof(Username), IsUnique = true)]
    public class User
    {
        [Key]
        [Required(ErrorMessage = "NoPekerja is required.")]
        [StringLength(5, ErrorMessage = "NoPekerja must be at most 5 characters.")]
        public string NoPekerja { get; set; }


        [Required(ErrorMessage = "FullName is required.")]
        [StringLength(100, ErrorMessage = "FullName cannot be longer than 100 characters.")]
        public string? Nama { get; set; } = "";

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(100, ErrorMessage = "Username cannot be longer than 100 characters.")]
        public string Username { get; set; }

        public string? Password { get; set; } = "";
        public bool UseWindowsAuth { get; set; }
        public bool BlockUser { get; set; }
        public string? Roles { get; set; } = "";
        public string? Email { get; set; }


    }
}
