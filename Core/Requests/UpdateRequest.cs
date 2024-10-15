using Core.Entities.Models;
using System.ComponentModel.DataAnnotations;

namespace Core.Requests
{
    public class UpdateRequest
    {
        private string password;
        private string confirmPassword;
        private string role;
        private string email;

        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [EnumDataType(typeof(Role))]
        public string Role
        {
            get => role;
            set => role = replaceEmptyWithNull(value);
        }

        [EmailAddress]
        public string Email
        {
            get => email;
            set => email = replaceEmptyWithNull(value);
        }

        [MinLength(6)]
        public string Password
        {
            get => password;
            set => password = replaceEmptyWithNull(value);
        }

        public string ConfirmPassword
        {
            get => confirmPassword;
            set => confirmPassword = replaceEmptyWithNull(value);
        }

        private string replaceEmptyWithNull(string value)
        {
            return string.IsNullOrEmpty(value) ? null : value;
        }
    }
}
