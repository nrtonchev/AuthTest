using System.ComponentModel.DataAnnotations;

namespace Core.Requests;

public class VerifyEmailRequest
{
    [Required]
    public string Token { get; set; }
}