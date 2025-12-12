using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMSS.Models;
using SIMSS.Services;
using SIMSS.SimsDbContext;
using SIMSS.SimsDbContext.Entities;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SIMSS.Controllers
{
    public class FacultyController : Controller
    {
        private readonly SimsDbContext.SimsDbContext _context;
        public FacultyController(SimsDbContext.SimsDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin, Faculty")]
        [HttpGet]
        public IActionResult Index()
        {
            var faculties = _context.Faculties.ToList();
            return View(faculties);
        }

        // Chỉ Admin có quyền thêm
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(AddFacultyViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Kiểm tra Username
                if (_context.Users.Any(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Username already exists!");
                }

                // 2. Kiểm tra Email
                if (_context.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email already exists!");
                }

                // 3. Kiểm tra Phone
                if (!string.IsNullOrEmpty(model.Phone) && _context.Users.Any(u => u.Phone == model.Phone))
                {
                    ModelState.AddModelError("Phone", "Phone number already exists!");
                }

                // 4. Kiểm tra Password
                if (string.IsNullOrEmpty(model.Password) || model.Password.Length < 6 ||
                    !Regex.IsMatch(model.Password, @"[A-Z]") || // ít nhất 1 chữ hoa
                    !Regex.IsMatch(model.Password, @"[\W_]"))   // ít nhất 1 ký tự đặc biệt
                {
                    ModelState.AddModelError("Password", "Password must be at least 6 characters, include at least 1 uppercase letter and 1 special character!");
                }

                // 4b. Kiểm tra ConfirmPassword
                if (model.Password != model.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "Password and Confirm Password do not match!");
                }

                // 5. Kiểm tra tên giảng viên trùng trong cùng Department
                if (_context.Faculties.Any(f => f.FirstName == model.FirstName
                                               && f.LastName == model.LastName
                                               && f.Department == model.Department))
                {
                    ModelState.AddModelError("", "A faculty with the same name already exists in this department!");
                }

                // 6. Kiểm tra Department không trống
                if (string.IsNullOrWhiteSpace(model.Department))
                {
                    ModelState.AddModelError("Department", "Department cannot be empty!");
                }

                // Nếu có lỗi validation, trả lại View
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Tạo User
                var user = new Users
                {
                    Username = model.Username,
                    Email = model.Email,
                    Phone = model.Phone,
                    PasswordHash = UserService.HashPassword(model.Password),
                    Role = "Faculty",
                    Status = "Active",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.Users.Add(user);
                _context.SaveChanges();

                // Tạo Faculty
                var faculty = new Faculty
                {
                    UserID = user.UserID,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Department = model.Department
                };
                _context.Faculties.Add(faculty);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Faculty added successfully!";
                return RedirectToAction("Index");
            }

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var faculty = _context.Faculties.FirstOrDefault(f => f.FacultyID == id);
            if (faculty == null) return NotFound();
            ViewBag.Users = _context.Users.Where(u => u.Role == "Faculty" && (u.UserID == faculty.UserID || !_context.Faculties.Any(f => f.UserID == u.UserID))).Select(u => new { u.UserID, u.Username }).ToList();
            return View(faculty);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Faculty faculty)
        {
            if (ModelState.IsValid)
            {
                _context.Faculties.Update(faculty);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Users = _context.Users.Where(u => u.Role == "Faculty" && (u.UserID == faculty.UserID || !_context.Faculties.Any(f => f.UserID == u.UserID))).Select(u => new { u.UserID, u.Username }).ToList();
            return View(faculty);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var faculty = _context.Faculties.FirstOrDefault(f => f.FacultyID == id);
            if (faculty == null) return NotFound();

            // Kiểm tra sinh viên liên quan
            bool hasStudents = _context.Students.Any(s => s.FacultyID == id);
            if (hasStudents)
            {
                TempData["ErrorMessage"] = "This instructor has students involved; they cannot be removed!";
                return RedirectToAction("Index");
            }

            // Xóa user liên quan nếu có
            var user = _context.Users.FirstOrDefault(u => u.UserID == faculty.UserID);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            _context.Faculties.Remove(faculty);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Faculty removal successful!";
            return RedirectToAction("Index");
        }
    }
}
