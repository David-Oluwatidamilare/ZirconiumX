using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ZirconiumX.Models
{
    public class UserRegisterViewModel
    {
        public UserRegisterViewModel()
        {

        }
        [Required]
        [Display(Name = "Name")]
        public string CName { get; set; }

        [Key]
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string CAddress { get; set; }


        [Required]
        [Display(Name = "Phone No.")]
        public string CPhoneNo { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string CEmail { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string CPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("CPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

    }
}