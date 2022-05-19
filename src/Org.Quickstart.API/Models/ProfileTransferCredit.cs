using System;

namespace Org.Quickstart.API.Models
{
    public class ProfileTransferCredit
    {
        public Guid Pfrom { get; set; }
        public Guid Pto { get; set; }
        public decimal Amount { get; set; }
    }
}