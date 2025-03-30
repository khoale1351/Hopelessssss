﻿using System.ComponentModel.DataAnnotations;

namespace Travel.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
