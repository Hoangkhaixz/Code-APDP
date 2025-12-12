using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SIMSS.SimsDbContext;
using SIMSS.Dto;

namespace SIMSS.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly SIMSS.SimsDbContext.SimsDbContext _context;

        public DashboardService(SIMSS.SimsDbContext.SimsDbContext context)
        {
            _context = context;
        }
        public IEnumerable<DepartmentStudentsDto> GetStudentCountsByFaculty()
        {
            var data = _context.Students
                .Join(_context.Faculties, s => s.FacultyID, f => f.FacultyID, (s, f) => new { s, f })
                .GroupBy(sf => sf.f.Department)
                .Select(g => new DepartmentStudentsDto
                {
                    Department = g.Key,
                    StudentCount = g.Count()
                })
                .ToList();
            return data;
        }
        public IEnumerable<CourseEnrollmentDto> GetEnrollmentCountsByCourse()
        {
            var data = _context.Courses
                .Select(c => new CourseEnrollmentDto
                {
                    CourseName = c.CourseName,
                    EnrollmentCount = _context.Enrollments.Count(e => e.CourseID == c.CourseID)
                })
                .ToList();
            return data;
        }
    }
}
