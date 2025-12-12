using System.ComponentModel.DataAnnotations.Schema;

namespace SIMSS.SimsDbContext.Entities
{
    public class Users
    {
        public int UserID { get; set; }
        public string Role { get; set; } = "Admin";
        public string Username { get; set; } = null;
        public string PasswordHash { get; set; } = null;
        public string Email { get; set; } = null;
        public string? Phone { get; set; }
        public string Status { get; set; } = "Active";
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;

    }
}
