using System.ComponentModel.DataAnnotations;

namespace GoogleCalendarEvent.DTO
{
    public class LoginDTO
    {
        [Required(ErrorMessage ="User Name is required")]
        public string Email { get; set; }

        [Required(ErrorMessage ="Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
