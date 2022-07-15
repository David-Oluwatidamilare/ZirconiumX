using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ZirconiumX.Models
{
    public class BusinessRegisterViewModel
    {
        public BusinessRegisterViewModel()
        { }
        // Page 1
        // Business Details

        [Required]
        [Display(Name = "Business Name")]
        public string BName { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string BAddress { get; set; }

        [Required]
        [Display(Name = "Category")]
        public string Category { get; set; }

        [Required]
        [Display(Name = "Business Type")]
        public string BType { get; set; }

        [Key]
        [Required]
        [Display(Name = "Reg. No")]
        public int RNumber { get; set; }

        [Required]
        [Display(Name = "Tax No")]
        public int TNumber { get; set; }

        [Required]
        [Display(Name = "Company Size")]
        public string CSize { get; set; }

        [Required]
        [Display(Name = "Year Of Establishment")]
        public string YEstablishment { get; set; }

        [Required]
        [Display(Name = "Business Description")]
        public string BDescription { get; set; }

        // Page 2
        //Management Team

        [Required]
        [Display(Name = "Staff Name")]
        public string SName1 { get; set; }

        [Required]
        [Display(Name = "Position")]
        public string SPosition1 { get; set; }

        [Required]
        [Display(Name = "Email")]
        public string SEmail1 { get; set; }

        [Required]
        [Display(Name = "Phone No")]
        public string SPhoneNo1 { get; set; }

        [Display(Name = "Staff Name")]
        public string SName2 { get; set; }

        [Display(Name = "Position")]
        public string SPosition2 { get; set; }

        [Display(Name = "SEmail")]
        public string SEmail2 { get; set; }

        [Display(Name = "SPhoneNo")]
        public string SPhoneNo2 { get; set; }

        // Page 3
        // Login Details

        [Required]
        [EmailAddress]
        [Display(Name = "Business Email")]
        public string BEmail { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class BusinessDetailsViewModel
    {

        [Required]
        [Display(Name = "Business Name")]
        public string BName { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string BAddress { get; set; }

        [Required]
        [Display(Name = "Category")]
        public string Category { get; set; }

        [Required]
        [Display(Name = "Business Type")]
        public string BType { get; set; }

        [Key]
        [Required]
        [Display(Name = "Reg. No")]
        public int RNumber { get; set; }

        [Required]
        [Display(Name = "Tax No")]
        public int TNumber { get; set; }

        [Required]
        [Display(Name = "Company Size")]
        public string CSize { get; set; }

        [Required]
        [Display(Name = "Year Of Establishment")]
        public string YEstablishment { get; set; }

        [Required]
        [Display(Name = "Business Description")]
        public string BDescription { get; set; }
    }

    public class ManagementTeamViewModel
    {
        [Required]
        [Display(Name = "Staff Name")]
        public string SName { get; set; }

        [Required]
        [Display(Name = "Position")]
        public string SPosition { get; set; }

        [Required]
        [Display(Name = "Email")]
        public string SEmail { get; set; }

        [Required]
        [Display(Name = "Phone No")]
        public string SPhoneNo { get; set; }

    }

    public class BusinessLoginViewModel
    {

        [Required]
        [EmailAddress]
        [Display(Name = "Business Email")]
        public string BEmail { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}