using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMSS.SimsDbContext;
using SIMSS.SimsDbContext.Entities;
using System.Threading.Tasks;
using System.Linq;

namespace SIMSS.Controllers
{
    public class CourseController : Controller
    {
        private readonly SimsDbContext.SimsDbContext _context;
        public CourseController(SimsDbContext.SimsDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin, Student, Faculty")]
        [HttpGet]
        public IActionResult Index()
        {
            var courses = _context.Courses.ToList();
            return View(courses);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Add()
        {
            // Load suggestions từ database
            ViewBag.ExistingCourseNames = _context.Courses.Select(c => c.CourseName).Distinct().ToList();
            ViewBag.ExistingDepartments = _context.Courses.Select(c => c.Department).Where(d => !string.IsNullOrEmpty(d)).Distinct().ToList();
            ViewBag.ExistingCourseCodes = _context.Courses.Select(c => c.CourseCode).Distinct().ToList();
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(Course course)
        {
            // Kiểm tra trùng CourseCode hoặc CourseName
            bool isDuplicate = _context.Courses.Any(c =>
                c.CourseCode == course.CourseCode ||
                c.CourseName == course.CourseName
            );

            if (isDuplicate)
            {
                // Thêm thông báo lỗi vào ModelState
                ModelState.AddModelError("", "Khóa học này đã tồn tại. Vui lòng nhập thông tin khác.");
            }

            if (ModelState.IsValid)
            {
                _context.Courses.Add(course);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            // Reload suggestions nếu có lỗi validation
            ViewBag.ExistingCourseNames = _context.Courses.Select(c => c.CourseName).Distinct().ToList();
            ViewBag.ExistingDepartments = _context.Courses.Select(c => c.Department).Where(d => !string.IsNullOrEmpty(d)).Distinct().ToList();
            ViewBag.ExistingCourseCodes = _context.Courses.Select(c => c.CourseCode).Distinct().ToList();

            return View(course);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var course = _context.Courses.FirstOrDefault(c => c.CourseID == id);
            if (course == null) return NotFound();
            // Load suggestions từ database
            ViewBag.ExistingCourseNames = _context.Courses.Select(c => c.CourseName).Distinct().ToList();
            ViewBag.ExistingDepartments = _context.Courses.Select(c => c.Department).Where(d => !string.IsNullOrEmpty(d)).Distinct().ToList();
            ViewBag.ExistingCourseCodes = _context.Courses.Select(c => c.CourseCode).Distinct().ToList();
            return View(course);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Courses.Update(course);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            // Reload suggestions nếu có lỗi validation
            ViewBag.ExistingCourseNames = _context.Courses.Select(c => c.CourseName).Distinct().ToList();
            ViewBag.ExistingDepartments = _context.Courses.Select(c => c.Department).Where(d => !string.IsNullOrEmpty(d)).Distinct().ToList();
            ViewBag.ExistingCourseCodes = _context.Courses.Select(c => c.CourseCode).Distinct().ToList();
            return View(course);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var course = _context.Courses.FirstOrDefault(c => c.CourseID == id);
            if (course == null) return NotFound();
            _context.Courses.Remove(course);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
