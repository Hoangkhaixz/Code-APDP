using Microsoft.EntityFrameworkCore;

namespace SIMSS.SimsDbContext
{
    public class SimsDbContext :DbContext
    {
        public SimsDbContext(DbContextOptions<SimsDbContext> options) : base(options)
        {
        }
        public DbSet<Entities.Users> Users { get; set;}
        public DbSet<Entities.Course> Courses { get; set; }
        public DbSet<Entities.Student> Students { get; set; }
        public DbSet<Entities.Faculty> Faculties { get; set; }
        public DbSet<Entities.Enrollment> Enrollments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entities.Users>().ToTable("Users");
            modelBuilder.Entity<Entities.Users>().HasKey(u => u.UserID);
            modelBuilder.Entity<Entities.Users>().Property(u => u.UserID).HasColumnName("UserID");
            modelBuilder.Entity<Entities.Users>().Property(u => u.PasswordHash).HasColumnName("PasswordHash");
            modelBuilder.Entity<Entities.Users>().Property(u => u.UpdatedAt).HasColumnName("UpdatedAt");
            modelBuilder.Entity<Entities.Users>().HasIndex("Username").IsUnique();
            modelBuilder.Entity<Entities.Users>().HasIndex("Email").IsUnique();
            modelBuilder.Entity<Entities.Users>().Property(u => u.Status).HasDefaultValue("Active");
            modelBuilder.Entity<Entities.Users>().Property(u => u.Role).HasDefaultValue("Admin");

            // Mapping cho Course
            modelBuilder.Entity<Entities.Course>().ToTable("Courses");
            modelBuilder.Entity<Entities.Course>().HasKey(c => c.CourseID);
            modelBuilder.Entity<Entities.Course>().HasIndex(c => c.CourseCode).IsUnique();

            modelBuilder.Entity<Entities.Student>().ToTable("Students");
            modelBuilder.Entity<Entities.Student>().HasKey(x => x.StudentID);

            modelBuilder.Entity<Entities.Faculty>().ToTable("Faculty");
            modelBuilder.Entity<Entities.Faculty>().HasKey(x => x.FacultyID);

            modelBuilder.Entity<Entities.Enrollment>().ToTable("Enrollments");
            modelBuilder.Entity<Entities.Enrollment>().HasKey(x => x.EnrollmentID);
            // Ignore Class property vì database chưa có column này
            // Nếu database đã có column Class, bỏ comment dòng dưới:
            modelBuilder.Entity<Entities.Enrollment>().Ignore(e => e.Class);
            // Hoặc nếu column có tên khác trong DB, dùng:
            // modelBuilder.Entity<Entities.Enrollment>().Property(e => e.Class).HasColumnName("TênCộtTrongDB");
        }
    }
}
