using System;
namespace Org.Quickstart.API.Models
{
    public class Profile
    {
        public Guid Pid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public decimal OnBoardCredit { get; set; }

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

        public void TransferTo(Profile to, decimal amount)
        {
            // TODO: logic to make sure amount can't go negative
            // and that amount isn't negative, etc

            this.OnBoardCredit -= amount;
            to.OnBoardCredit += amount;
        }
    }
}
