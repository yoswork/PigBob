using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Security;
using System.Web.Profile;

namespace PigBob.Models
{

    public class ChangePasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterModel
    {
        [Required]
        [Display(Name = "First name")]
        [MaxLength(100, ErrorMessage = "Too Long! Please stop trying to hack me.")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last name")]
        [MaxLength(100, ErrorMessage = "Too Long! Please stop trying to hack me.")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "User name")]
        [MaxLength(100, ErrorMessage = "Too Long! Please stop trying to hack me.")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email address")]
        public string Email { get; set; }

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

    public class UserProfile : ProfileBase
    {
        [SettingsAllowAnonymous(false)]
        public string FirstName
        {
            get { return this["FirstName"] as string; }
            set { this["FirstName"] = value; }
        }

        [SettingsAllowAnonymous(false)]
        public string LastName
        {
            get { return this["LastName"] as string; }
            set { this["LastName"] = value; }
        }

        public static UserProfile GetUserProfile(string username)
        {
            return Create(username) as UserProfile;
        }

        public static UserProfile GetUserProfile()
        {
            var user = Membership.GetUser();

            if (user != null)
            {
                return Create(Membership.GetUser().UserName) as UserProfile;
            }
            else
            {
                FormsAuthentication.SignOut();
                return null;
            }
        }
    }
}