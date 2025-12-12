using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMSS.SimsDbContext;
using SIMSS.SimsDbContext.Entities;
using System.Linq;
using SIMSS.Models;

namespace SIMSS.Controllers
{
    public class EnrollmentController : Controller
    {
        private readonly SimsDbContext.SimsDbContext _context;
        public EnrollmentController(SimsDbContext.SimsDbContext context)
        {
            _context = context;
        }

        // Admin, Faculty xem toàn bộ; Student chỉ xem của mình
        [Authorize(Roles = "Faculty, Student")]
        [HttpGet]
        public IActionResult Index()
        {
            IQueryable<EnrollmentDetailViewModel> enrollmentQuery;
            
            if (User.IsInRole("Faculty"))
            {
                // Faculty xem được toàn bộ enrollment
                enrollmentQuery = _context.Enrollments
                    .Join(_context.Courses, e => e.CourseID, c => c.CourseID, (e, c) => new {e, c})
                    .Join(_context.Students, ec => ec.e.StudentID, st => st.StudentID, (ec, st) => new EnrollmentDetailViewModel
                    {
                        EnrollmentID = ec.e.EnrollmentID,
                        StudentID = ec.e.StudentID,
                        StudentName = st.FirstName + " " + st.LastName,
                        CourseID = ec.e.CourseID,
                        CourseCode = ec.c.CourseCode,
                        CourseName = ec.c.CourseName,
                        EnrollmentDate = ec.e.EnrollmentDate,
                        Class = ec.e.Class,
                        Grade = ec.e.Grade
                    });
            }
            else if (User.IsInRole("Student"))
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized();
                }

                var student = _context.Students.FirstOrDefault(s => s.UserID == currentUserId);
                if (student == null)
                {
                    return View(new List<EnrollmentDetailViewModel>());
                }

                enrollmentQuery = _context.Enrollments
                    .Where(e => e.StudentID == student.StudentID)
                    .Join(_context.Courses, e => e.CourseID, c => c.CourseID, (e, c) => new EnrollmentDetailViewModel
                    {
                        EnrollmentID = e.EnrollmentID,
                        StudentID = e.StudentID,
                        CourseID = e.CourseID,
                        CourseCode = c.CourseCode,
                        CourseName = c.CourseName,
                        EnrollmentDate = e.EnrollmentDate,
                        Class = e.Class,
                        Grade = e.Grade
                    });
            }
            else // Admin is not explicitly authorized for Enrollment Index, but they can see all for now.
            {
                 enrollmentQuery = _context.Enrollments
                    .Join(_context.Courses, e => e.CourseID, c => c.CourseID, (e, c) => new EnrollmentDetailViewModel
                    {
                        EnrollmentID = e.EnrollmentID,
                        StudentID = e.StudentID,
                        CourseID = e.CourseID,
                        CourseCode = c.CourseCode,
                        CourseName = c.CourseName,
                        EnrollmentDate = e.EnrollmentDate,
                        Class = e.Class,
                        Grade = e.Grade
                    });
            }

            var enrollments = enrollmentQuery.ToList();
            return View(enrollments);
        }

        // Student thêm mới (đăng ký), Admin cũng được thêm
        [Authorize(Roles = "Faculty, Student")]
        [HttpGet]
        public IActionResult Add()
        {
            ViewBag.Students = _context.Students.Select(s => new { s.StudentID, Name = s.FirstName + " " + s.LastName + " (" + s.StudentID + ")" }).ToList();
            ViewBag.Courses = _context.Courses.Select(c => new { c.CourseID, Name = c.CourseCode + " - " + c.CourseName }).ToList();
            // Load existing classes và grades từ database
            // Tạm thời comment lại nếu database chưa có column Class
            // ViewBag.ExistingClasses = _context.Enrollments.Where(e => !string.IsNullOrEmpty(e.Class)).Select(e => e.Class).Distinct().ToList();
            ViewBag.ExistingClasses = new List<string>(); // Empty list cho đến khi database có column Class
            ViewBag.ExistingGrades = _context.Enrollments.Where(e => !string.IsNullOrEmpty(e.Grade)).Select(e => e.Grade).Distinct().ToList();
            return View();
        }

        [Authorize(Roles = "Faculty, Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(Enrollment enrollment)
        {
            if (ModelState.IsValid)
            {
                // Check if StudentID exists
                if (!_context.Students.Any(s => s.StudentID == enrollment.StudentID))
                {
                    ModelState.AddModelError("StudentID", "StudentID không tồn tại.");
                }

                if (ModelState.IsValid)
                {
                    _context.Enrollments.Add(enrollment);
                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            ViewBag.Students = _context.Students.Select(s => new { s.StudentID, Name = s.FirstName + " " + s.LastName + " (" + s.StudentID + ")" }).ToList();
            ViewBag.Courses = _context.Courses.Select(c => new { c.CourseID, Name = c.CourseCode + " - " + c.CourseName }).ToList();
            // Tạm thời comment lại nếu database chưa có column Class
            // ViewBag.ExistingClasses = _context.Enrollments.Where(e => !string.IsNullOrEmpty(e.Class)).Select(e => e.Class).Distinct().ToList();
            ViewBag.ExistingClasses = new List<string>(); // Empty list cho đến khi database có column Class
            ViewBag.ExistingGrades = _context.Enrollments.Where(e => !string.IsNullOrEmpty(e.Grade)).Select(e => e.Grade).Distinct().ToList();
            return View(enrollment);
        }

        // Sửa điểm/chi tiết chỉ cho Admin hoặc Faculty
        [Authorize(Roles = "Faculty")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var enrollment = _context.Enrollments.FirstOrDefault(e => e.EnrollmentID == id);
            if (enrollment == null) return NotFound();
            ViewBag.Students = _context.Students.Select(s => new { s.StudentID, Name = s.FirstName + " " + s.LastName + " (" + s.StudentID + ")" }).ToList();
            ViewBag.Courses = _context.Courses.Select(c => new { c.CourseID, Name = c.CourseCode + " - " + c.CourseName }).ToList();
            // Tạm thời comment lại nếu database chưa có column Class
            // ViewBag.ExistingClasses = _context.Enrollments.Where(e => !string.IsNullOrEmpty(e.Class)).Select(e => e.Class).Distinct().ToList();
            ViewBag.ExistingClasses = new List<string>(); // Empty list cho đến khi database có column Class
            ViewBag.ExistingGrades = _context.Enrollments.Where(e => !string.IsNullOrEmpty(e.Grade)).Select(e => e.Grade).Distinct().ToList();
            return View(enrollment);
        }

        [Authorize(Roles = "Faculty")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Enrollment enrollment)
        {
            // Kiểm tra điểm nhập vào
            if (!string.IsNullOrEmpty(enrollment.Grade))
            {
                // Nếu không phải số
                if (!int.TryParse(enrollment.Grade, out int gradeInt))
                {
                    ModelState.AddModelError("Grade", "Điểm phải là số từ 0 đến 100!");
                }
                else
                {
                    // Nếu số nhưng vượt quá giới hạn
                    if (gradeInt < 0 || gradeInt > 100)
                    {
                        ModelState.AddModelError("Grade", "Chỉ được nhập điểm từ 0 đến 100!");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                _context.Enrollments.Update(enrollment);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            // Load dữ liệu cho dropdown / viewbag
            ViewBag.Students = _context.Students
                .Select(s => new { s.StudentID, Name = s.FirstName + " " + s.LastName + " (" + s.StudentID + ")" })
                .ToList();
            ViewBag.Courses = _context.Courses
                .Select(c => new { c.CourseID, Name = c.CourseCode + " - " + c.CourseName })
                .ToList();
            ViewBag.ExistingClasses = new List<string>(); // Nếu chưa có cột Class
            ViewBag.ExistingGrades = _context.Enrollments
                .Where(e => !string.IsNullOrEmpty(e.Grade))
                .Select(e => e.Grade)
                .Distinct()
                .ToList();

            return View(enrollment);
        }

        // Xoá chỉ cho Admin
        [Authorize(Roles = "Faculty")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var enrollment = _context.Enrollments.FirstOrDefault(e => e.EnrollmentID == id);
            if (enrollment == null) return NotFound();
            _context.Enrollments.Remove(enrollment);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
