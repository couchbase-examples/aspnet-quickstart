using System;
namespace Org.Quickstart.API.Models
{
    public class ProfileUpdateRequestCommand
    {
        public Guid Pid { get; set;  }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

	    public Profile GetProfile()
	    {
	        return new Profile
            {
                Pid = this.Pid,
		        FirstName = this.FirstName,
		        LastName = this.LastName,
		        Email = this.Email,
	            Password = this.Password
            };
	    } 
    }
}
