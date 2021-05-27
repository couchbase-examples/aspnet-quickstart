using System;
namespace Org.Quickstart.API.Models
{
    public class ProfileListRequestQuery
    {
        public string Search { get; set; }
        public int Limit { get; set; } = 5;
        public int Skip { get; set; } 
    }
}
