﻿using System.Text.Json.Serialization;

namespace Core.Responses
{
    public class AuthResponse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public bool IsVerified { get; set; }
        public string JwtToken { get; set; }
        [JsonIgnore]
        public string RefreshToken { get; set; }
    }
}
