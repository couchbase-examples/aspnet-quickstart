using System;
namespace Org.Quickstart.API.Models
{
    public class Profile
    {
        public Guid Pid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        private string _password;
        public string Password {
            get
            {
                return _password;
            }
            set
            {
                _password = BCrypt.Net.BCrypt.HashPassword(value);
            }
         }
    }
}
