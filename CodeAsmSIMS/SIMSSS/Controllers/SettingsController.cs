using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMSS.SimsDbContext;
using SIMSS.SimsDbContext.Entities;
using System;
using System.Linq;

namespace SIMSS.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly SimsDbContext.SimsDbContext _context;

        public SettingsController(SimsDbContext.SimsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Account()
        {
            // Lấy thông tin user hiện tại
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Index", "Login");

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Account(Users user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = _context.Users.FirstOrDefault(u => u.UserID == user.UserID);
                if (existingUser == null)
                    return NotFound();

                // Cập nhật thông tin (không cho phép đổi username, password từ đây)
                existingUser.Email = user.Email;
                existingUser.Phone = user.Phone;
                existingUser.UpdatedAt = DateTime.Now;

                _context.Users.Update(existingUser);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Account information updated successfully!";
                return RedirectToAction("Account");
            }
            return View(user);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Roles()
        {
            var users = _context.Users.ToList();
            
            // Group by role để hiển thị thống kê
            ViewBag.RoleStats = users
                .GroupBy(u => u.Role)
                .Select(g => new { Role = g.Key, Count = g.Count() })
                .ToList();

            return View(users);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateRole(int userId, string role)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserID == userId);
            if (user == null)
                return NotFound();

            if (new[] { "Admin", "Faculty", "Student" }.Contains(role))
            {
                user.Role = role;
                user.UpdatedAt = DateTime.Now;
                _context.Users.Update(user);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"Role updated successfully for user {user.Username}!";
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid role selected!";
            }

            return RedirectToAction("Roles");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatus(int userId, string status)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserID == userId);
            if (user == null)
                return NotFound();

            if (new[] { "Active", "Inactive", "Suspended" }.Contains(status))
            {
                user.Status = status;
                user.UpdatedAt = DateTime.Now;
                _context.Users.Update(user);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"Status updated successfully for user {user.Username}!";
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid status selected!";
            }

            return RedirectToAction("Roles");
        }
    }
}
