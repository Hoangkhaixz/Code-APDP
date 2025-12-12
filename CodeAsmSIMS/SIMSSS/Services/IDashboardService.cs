using SIMSS.Dto;

namespace SIMSS.Services
{
    public interface IDashboardService
    {
        IEnumerable<DepartmentStudentsDto> GetStudentCountsByFaculty();
        IEnumerable<CourseEnrollmentDto> GetEnrollmentCountsByCourse();
    }
}
