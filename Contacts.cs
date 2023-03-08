using System.ComponentModel.DataAnnotations;

namespace ContactManager.API
{
    public class Contacts
    {
        public int Id { get; set; }


        [Required]
        [MinLength(2, ErrorMessage ="Salutation length should be minimum of 2 characters")]
        [MaxLength(50)]
        public string Salutation { get; set; }

        [Required]
        [MinLength(2, ErrorMessage = "FirstName length should be minimum of 2 characters")]
        [MaxLength(50)]
        public string FirstName { get; set; }


        [Required]
        [MinLength(2, ErrorMessage = "LastName length should be minimum of 2 characters")]
        [MaxLength(50)]
        public string LastName { get; set; }

        [MaxLength(150)]
        public string DisplayName { get; set; }
        public DateTime BirthDate { get; set; }


        [EmailAddress]
        public string Email { get; set; }
        
        [Phone]
        public string? PhoneNumber { get; set; }
    }
}
