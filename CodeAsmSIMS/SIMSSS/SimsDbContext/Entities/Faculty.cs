using System.ComponentModel.DataAnnotations;

namespace SIMSS.SimsDbContext.Entities
{
    public class Faculty
    {
        [Key]
        public int FacultyID { get; set; }
        public int UserID { get; set; }
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }
        [StringLength(100)]
        public string Department { get; set; }
    }
}
