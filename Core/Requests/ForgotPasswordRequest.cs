using System.ComponentModel.DataAnnotations;

namespace Core.Requests
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
