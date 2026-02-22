using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StudentTrackingSystem.Api.Models
{
    #region Veli Veri Modeli Tanımı
    public class Parent
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Occupation { get; set; } 
        public string? Workplace { get; set; } 
        public string? Email { get; set; }
        public int? RelationshipType { get; set; }
        public bool IsActive { get; set; }
    }
    #endregion
}