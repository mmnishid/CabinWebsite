using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Security;

namespace CabinWebsite_Application.Models
{
    public class AccountManagerModel
    {
        [Required(ErrorMessage = "Your current password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string currentPassword { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string newPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        public string confirmPassword { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "E-mail")]
        public string emailAddress { get; set; }

        public string currentEmail { get; set; }

        [Display(Name = "First name")]
        public string firstName { get; set; }

        public string currentFirstName { get; set; }

        [Display(Name = "Last name")]
        public string lastName { get; set; }

        public string currentLastName { get; set; }

        public Dictionary<string, string> errorList { get; set; }

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

    /* Will likely need to add a token field to this so that only users with the Kenton token can register.
     * The mockup for Signup has the fields that will need to be here */
    public class RegisterModel
    {
        [Required]
        [Display(Name = "User name")]
        [RegularExpression("^[a-zA-Z0-9_]+$", ErrorMessage = "Usernames may only contain lowercase and uppercase letters, numbers, and underscores.")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Signup Token")]
        public string Token { get; set; }
    }



    /*
    public class UsersContext : DbContext
    {
        public UsersContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
    }

    [Table("UserProfile")]
    public class UserProfile
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }

    public class RegisterExternalLoginModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        public string ExternalLoginData { get; set; }
    }
    */
    public class LocalPasswordModel
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
    /*
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
        [Display(Name = "User name")]
        public string UserName { get; set; }

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

    public class ExternalLogin
    {
        public string Provider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderUserId { get; set; }
    }
    */
     
    public class CabinOverviewModel
    {
        public List<MessageModel> messages { get; set; }

    }

    public class MessageModel
    {
        //public int test { get; set; }

        public Guid messageID { get; set; }
        public int userID { get; set; }
        public DateTime date { get; set; }
        public string title { get; set; }
        public string body { get; set; }

    }

    public class NewMessageModel
    {
        /*POST*/
        [Required]
        [Display(Name = "Title:")]
        public string title { get; set; }

        [Display(Name = "Body:")]
        public string body { get; set; }
    }

    public class CalendarModel
    {
        public List<ReservedModel> myReservedDates { get; set; }
        public List<ReservedModel> otherReservedDates { get; set; }
    }

    public class ReservedModel
    {
        public Guid calendarID { get; set; }
        public int userID { get; set; }
        public string userFirstName { get; set; }
        public string userLastName { get; set; }
        public DateTime dateAdded { get; set; }
        public DateTime reservedDate { get; set; }
    }

    public class NewReservedModel
    {
        /*POST*/
        [Required]
        [Display(Name = "Year:")]
        public string year { get; set; }

        [Required]
        [Display(Name = "Month:")]
        public string month { get; set; }

        [Required]
        [Display(Name = "Day:")]
        public string day { get; set; }
    }

    public class xmlModel
    {
        public List<Unit> units { get; set; }
    }

    public class Unit
    {
        public string Name { get; set; }
        public List<Unit> Children { get; set; }
    }
}
