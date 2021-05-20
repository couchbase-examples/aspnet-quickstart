using System;
namespace Org.Quickstart.API.Models
{
    public class ProfileUpdateRequestCommand
    {
        public Guid Id { get; set;  }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

	    public Profile GetProfile()
	    {
	        return new Profile
            {
                Id = this.Id,
		        FirstName = this.FirstName,
		        LastName = this.LastName,
		        Email = this.Email,
	            Password = this.Password
            };
	    } 
    }
}
