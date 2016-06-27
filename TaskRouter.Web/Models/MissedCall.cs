using System;

namespace TaskRouter.Web.Models
{
    public class MissedCall
    {
        public int Id { get; set; }
        public string Product { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}