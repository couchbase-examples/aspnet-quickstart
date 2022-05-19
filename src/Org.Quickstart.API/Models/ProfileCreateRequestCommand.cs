using System;
namespace Org.Quickstart.API.Models
{
    public class ProfileCreateRequestCommand
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public decimal OnBoardCredit { get; set; }

        public Profile GetProfile()
        {
            return new Profile
            {
		        Pid = new Guid(),
                FirstName = this.FirstName,
                LastName = this.LastName,
                Email = this.Email,
                Password = this.Password,
                OnBoardCredit = this.OnBoardCredit
            };
        }
    }
}
