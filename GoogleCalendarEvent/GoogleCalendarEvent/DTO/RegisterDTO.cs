using System.ComponentModel.DataAnnotations;

namespace GoogleCalendarEvent.DTO
{
    public class RegisterDTO
    {
        [Required(ErrorMessage ="User Name is required")]
        public string UserName { get; set; }
        [Required(ErrorMessage ="Email is required")]
        public string Email { get; set; }
        [Required(ErrorMessage ="Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage ="Confirm password is required")]
        [Compare("Password")]
        [DataType(DataType.Password)]
        public string PasswordConfirme { get; set; }
    }
}
