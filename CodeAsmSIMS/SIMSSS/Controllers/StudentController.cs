using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMSS.SimsDbContext;
using SIMSS.SimsDbContext.Entities;
using System.Linq;
using System;
using SIMSS.Models;
using SIMSS.Services;
using System.Collections.Generic; // Added for List<StudentDetailViewModel>

namespace SIMSS.Controllers
{
    public class StudentController : Controller
    {
        private readonly SimsDbContext.SimsDbContext _context;
        public StudentController(SimsDbContext.SimsDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin, Faculty, Student")]
        [HttpGet]
        public IActionResult Index()
        {
            IQueryable<StudentDetailViewModel> studentQuery;

            if (User.IsInRole("Admin"))
            {
                // Admin sees all students
                studentQuery = _context.Students
                    .Join(_context.Users, s => s.UserID, u => u.UserID, (s, u) => new StudentDetailViewModel
                    {
                        StudentID = s.StudentID,
                        FirstName = s.FirstName,
                        LastName = s.LastName,
                        DateOfBirth = s.DateOfBirth,
                        Gender = s.Gender,
                        Program = s.Program,
                        EnrollmentDate = s.EnrollmentDate,
                        Email = u.Email,
                        Phone = u.Phone
                    });
            }
            else if (User.IsInRole("Faculty"))
            {
                // Faculty được xem toàn bộ sinh viên (giống như admin, nhưng không có quyền chỉnh sửa...)
                studentQuery = _context.Students
                    .Join(_context.Users, s => s.UserID, u => u.UserID, (s, u) => new StudentDetailViewModel
                    {
                        StudentID = s.StudentID,
                        FirstName = s.FirstName,
                        LastName = s.LastName,
                        DateOfBirth = s.DateOfBirth,
                        Gender = s.Gender,
                        Program = s.Program,
                        EnrollmentDate = s.EnrollmentDate,
                        Email = u.Email,
                        Phone = u.Phone
                    });
            }
            else if (User.IsInRole("Student"))
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized();
                }

                studentQuery = _context.Students
                    .Where(s => s.UserID == currentUserId)
                    .Join(_context.Users, s => s.UserID, u => u.UserID, (s, u) => new StudentDetailViewModel
                    {
                        StudentID = s.StudentID,
                        FirstName = s.FirstName,
                        LastName = s.LastName,
                        DateOfBirth = s.DateOfBirth,
                        Gender = s.Gender,
                        Program = s.Program,
                        EnrollmentDate = s.EnrollmentDate,
                        Email = u.Email,
                        Phone = u.Phone
                    });
            }
            else
            {
                return Unauthorized(); // Or Redirect to Access Denied
            }

            var students = studentQuery.ToList();
            return View(students);
        }

        // Chỉ Admin được thêm student
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Add()
        {
            ViewBag.Faculties = _context.Faculties.Select(f => new { f.FacultyID, Name = f.FirstName + " " + f.LastName + " - " + f.Department }).ToList();
            return View(new AddEditStudentViewModel());
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(AddEditStudentViewModel model)
        {
            // Populate ViewBag for returning the view in case of errors
            ViewBag.Faculties = _context.Faculties.Select(f => new { f.FacultyID, Name = f.FirstName + " " + f.LastName + " - " + f.Department }).ToList();

            if (ModelState.IsValid)
            {
                // Check for duplicate Username
                if (_context.Users.Any(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Username already exists!");
                    return View(model);
                }

                // Check for duplicate Email
                if (!string.IsNullOrEmpty(model.Email) && _context.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email already exists!");
                    return View(model);
                }

                // Check for duplicate Phone Number
                if (!string.IsNullOrEmpty(model.Phone) && _context.Users.Any(u => u.Phone == model.Phone))
                {
                    ModelState.AddModelError("Phone", "Phone number already exists!");
                    return View(model);
                }

                // Create new User
                var newUser = new Users
                {
                    Username = model.Username,
                    Email = model.Email,
                    Phone = model.Phone,
                    PasswordHash = UserService.HashPassword(model.Password),
                    Role = "Student", // Fixed role for Student
                    Status = "Active"
                };
                _context.Users.Add(newUser);
                _context.SaveChanges();

                // Create new Student
                var newStudent = new Student
                {
                    UserID = newUser.UserID,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    Program = model.Program,
                    EnrollmentDate = model.EnrollmentDate,
                    FacultyID = model.FacultyID
                };
                _context.Students.Add(newStudent);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var student = _context.Students.FirstOrDefault(s => s.StudentID == id);
            if (student == null) return NotFound();

            var user = _context.Users.FirstOrDefault(u => u.UserID == student.UserID);
            if (user == null) return NotFound();

            var model = new AddEditStudentViewModel
            {
                StudentID = student.StudentID,
                FirstName = student.FirstName,
                LastName = student.LastName,
                DateOfBirth = student.DateOfBirth,
                Gender = student.Gender,
                Program = student.Program,
                EnrollmentDate = student.EnrollmentDate,
                FacultyID = student.FacultyID,
                UserID = user.UserID,
                Username = user.Username,
                Email = user.Email,
                Phone = user.Phone
                // Password and ConfirmPassword are not loaded for security reasons
            };

            ViewBag.Faculties = _context.Faculties.Select(f => new { f.FacultyID, Name = f.FirstName + " " + f.LastName + " - " + f.Department }).ToList();
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(AddEditStudentViewModel model)
        {
            // Populate ViewBag for returning the view in case of errors
            ViewBag.Faculties = _context.Faculties.Select(f => new { f.FacultyID, Name = f.FirstName + " " + f.LastName + " - " + f.Department }).ToList();

            if (ModelState.IsValid)
            {
                var existingStudent = _context.Students.Find(model.StudentID);
                if (existingStudent == null) return NotFound();

                var existingUser = _context.Users.Find(model.UserID);
                if (existingUser == null) return NotFound();

                // Update User Information
                // Check for duplicate Email (if changed and exists for another user)
                if (!string.IsNullOrEmpty(model.Email) && existingUser.Email != model.Email && _context.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email already exists for another user!");
                    return View(model);
                }

                existingUser.Email = model.Email;
                existingUser.Phone = model.Phone;

                // Update Password only if provided
                if (!string.IsNullOrEmpty(model.Password))
                {
                    existingUser.PasswordHash = UserService.HashPassword(model.Password);
                }
                _context.Users.Update(existingUser);

                // Update Student Information
                existingStudent.FirstName = model.FirstName;
                existingStudent.LastName = model.LastName;
                existingStudent.DateOfBirth = model.DateOfBirth;
                existingStudent.Gender = model.Gender;
                existingStudent.Program = model.Program;
                existingStudent.EnrollmentDate = model.EnrollmentDate;
                existingStudent.FacultyID = model.FacultyID;
                _context.Students.Update(existingStudent);

                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var student = _context.Students.FirstOrDefault(s => s.StudentID == id);
            if (student == null) return NotFound();

            // Also delete the associated user account
            var user = _context.Users.FirstOrDefault(u => u.UserID == student.UserID);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            _context.Students.Remove(student);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
