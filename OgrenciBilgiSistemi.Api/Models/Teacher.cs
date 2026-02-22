using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentTrackingSystem.Api.Models
{
    public class Teacher
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string? ImagePath { get; set; }
        public bool IsActive { get; set; }  
        public int? UnitId { get; set; }
    }
}