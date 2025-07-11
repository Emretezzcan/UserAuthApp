﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserAuthApp.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "E-posta  gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre gereklidir.")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        public string PasswordHash { get; set; }
        public string Role { get; set; } = "User";

        [NotMapped]
        public string? NewPassword { get; set; }

        public string Name { get; set; } // Kullanıcı adı


    }

}
