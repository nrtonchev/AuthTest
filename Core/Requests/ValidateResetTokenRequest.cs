using System.ComponentModel.DataAnnotations;

namespace Core.Requests
{
    public class ValidateResetTokenRequest
    {
        [Required]
        public string Token { get; set; }
    }
}
