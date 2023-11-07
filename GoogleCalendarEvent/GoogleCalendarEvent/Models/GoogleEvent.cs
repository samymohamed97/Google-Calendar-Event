using System.ComponentModel.DataAnnotations;

namespace GoogleCalendarEvent.Models
{
    public class GoogleEvent
    {

        [Required]
        public string Summary { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string Location { get; set; }
        [Required]
        public DateTime StartDateTime { get; set; }
        [Required]
        public DateTime EndDateTime { get; set; }
         
    }

   
}
