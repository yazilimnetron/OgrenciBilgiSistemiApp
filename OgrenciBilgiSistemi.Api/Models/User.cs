using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace StudentTrackingSystem.Api.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; }
        public int? UnitId { get; set; }
    }
}
