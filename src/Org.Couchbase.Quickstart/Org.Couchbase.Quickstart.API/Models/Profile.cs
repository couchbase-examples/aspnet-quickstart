using System;
namespace Org.Couchbase.Quickstart.API.Models
{
    public class Profile
    {
        public Guid Id { get; set; }
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
