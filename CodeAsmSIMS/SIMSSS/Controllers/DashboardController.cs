using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMSS.SimsDbContext;
using System.Linq;
using SIMSS.Services;
using SIMSS.Dto;
using System;
using System.Security.Claims;
using SIMSS.Models;

namespace SIMSS.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly SimsDbContext.SimsDbContext _context;

        public DashboardController(IDashboardService dashboardService, SimsDbContext.SimsDbContext context)
        {
            _dashboardService = dashboardService;
            _context = context;
        }

        [Authorize(Roles = "Admin, Student, Faculty")]
        public IActionResult Index()
        {
            // Bạn có thể giữ lại ViewBag phía dưới nếu muốn, hoặc chuyển sang dùng Service tuỳ mục đích
            ViewBag.TotalStudents = _context.Students.Count();
            ViewBag.TotalCourses = _context.Courses.Count();
            ViewBag.TotalFaculties = _context.Faculties.Count();
            ViewBag.TotalEnrollments = _context.Enrollments.Count();
            return View();
        }

        // API cho chart: Số sinh viên theo khoa
        [HttpGet]
        public IActionResult StudentsByFaculty()
        {
            var data = _dashboardService.GetStudentCountsByFaculty();
            return Json(data);
        }
        // API cho chart: Số sinh viên đăng ký từng môn
        [HttpGet]
        public IActionResult EnrollmentsByCourse()
        {
            var data = _dashboardService.GetEnrollmentCountsByCourse();
            return Json(data);
        }
        [Authorize(Roles = "Student")]
        [HttpGet]
        public IActionResult EditProfile()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                Console.WriteLine("DEBUG: userIdClaim is null.");
                return Unauthorized();
            }

            Console.WriteLine($"DEBUG: userIdClaim.Value = {userIdClaim.Value}");
            if (!int.TryParse(userIdClaim.Value, out int currentUserId))
            {
                Console.WriteLine($"DEBUG: Failed to parse userIdClaim.Value to int. Value: {userIdClaim.Value}");
                return Unauthorized();
            }
            Console.WriteLine($"DEBUG: currentUserId = {currentUserId}");

            var student = _context.Students.FirstOrDefault(s => s.UserID == currentUserId);
            if (student == null) return NotFound("Student profile not found.");

            var user = _context.Users.FirstOrDefault(u => u.UserID == currentUserId);
            if (user == null) return NotFound("User account not found.");

            var model = new EditStudentProfileViewModel
            {
                StudentID = student.StudentID,
                UserID = user.UserID,
                FirstName = student.FirstName,
                LastName = student.LastName,
                DateOfBirth = student.DateOfBirth,
                Email = user.Email,
                Phone = user.Phone
            };

            return View(model);
        }

        [Authorize(Roles = "Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProfile(EditStudentProfileViewModel model)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                Console.WriteLine("DEBUG: POST - userIdClaim is null.");
                return Unauthorized();
            }

            Console.WriteLine($"DEBUG: POST - userIdClaim.Value = {userIdClaim.Value}");
            if (!int.TryParse(userIdClaim.Value, out int currentUserId) || currentUserId != model.UserID)
            {
                Console.WriteLine($"DEBUG: POST - Failed to parse userIdClaim.Value or userId mismatch. Value: {userIdClaim.Value}, Model.UserID: {model.UserID}");
                return Unauthorized();
            }
            Console.WriteLine($"DEBUG: POST - currentUserId = {currentUserId}");

            if (ModelState.IsValid)
            {
                var student = _context.Students.FirstOrDefault(s => s.UserID == currentUserId);
                var user = _context.Users.FirstOrDefault(u => u.UserID == currentUserId);

                if (student == null || user == null) return NotFound("Profile data not found.");

                // Update Student info
                student.FirstName = model.FirstName;
                student.LastName = model.LastName;
                student.DateOfBirth = model.DateOfBirth;

                // Update User info
                user.Email = model.Email;
                user.Phone = model.Phone;
                user.UpdatedAt = DateTime.Now;

                _context.SaveChanges();
                TempData["SuccessMessage"] = "Thông tin cá nhân đã được cập nhật thành công.";
                return RedirectToAction("Index");
            }
            return View(model);
        }
        [Authorize(Roles = "Student")]
        [HttpGet]
        public IActionResult MyGrades()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
            {
                return Unauthorized();
            }

            var student = _context.Students.FirstOrDefault(s => s.UserID == currentUserId);
            if (student == null) return NotFound("Student profile not found.");

            var grades = _context.Enrollments
                .Where(e => e.StudentID == student.StudentID)
                .Join(
                    _context.Courses,
                    enrollment => enrollment.CourseID,
                    course => course.CourseID,
                    (enrollment, course) => new StudentGradesViewModel
                    {
                        EnrollmentID = enrollment.EnrollmentID,
                        CourseCode = course.CourseCode,
                        CourseName = course.CourseName,
                        EnrollmentDate = enrollment.EnrollmentDate,
                        Class = enrollment.Class,
                        Grade = enrollment.Grade
                    }
                )
                .ToList();

            return View(grades);
        }
    }
}
