using System.ComponentModel.DataAnnotations;

namespace Core.Requests;

public class RevokeTokenRequest
{
    [Required]
    public string Token { get; set; }
}