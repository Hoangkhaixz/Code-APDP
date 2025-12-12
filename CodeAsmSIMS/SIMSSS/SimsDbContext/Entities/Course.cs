using System.ComponentModel.DataAnnotations;

namespace SIMSS.SimsDbContext.Entities
{
    public class Course
    {
        [Key]
        public int CourseID { get; set; }

        [Required]
        [StringLength(20)]
        public string CourseCode { get; set; }

        [Required]
        [StringLength(100)]
        public string CourseName { get; set; }

        [Range(1, 10)]
        public int Credits { get; set; }

        [StringLength(50)]
        public string Department { get; set; }
    }
}
