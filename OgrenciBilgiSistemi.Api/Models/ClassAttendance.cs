using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentTrackingSystem.Api.Models
{
    public class ClassAttendance
    {
        public int AttendanceId { get; set; }
        public int? Lesson1 { get; set; }
        public int? Lesson2 { get; set; }
        public int? Lesson3 { get; set; }
        public int? Lesson4 { get; set; }
        public int? Lesson5 { get; set; }
        public int? Lesson6 { get; set; }
        public int? Lesson7 { get; set; }
        public int? Lesson8 { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int StudentId { get; set; }
        public int TeacherId { get; set; }
    }
}
