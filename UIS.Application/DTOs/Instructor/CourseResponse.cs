using System;
using System.Collections.Generic;
using System.Text;

namespace UIS.Application.DTOs.Instructor
{
    public class CourseResponse
    {
        
    public int CourseOfferingId { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int Credits { get; set; }
        public string Day { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Classroom { get; set; }
        public int EnrolledStudentsCount { get; set; }
        public int Quota { get; set; }
    
}
}
