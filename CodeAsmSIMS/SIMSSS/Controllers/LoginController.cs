using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIMSS.Models;
using SIMSS.Services;
using System.Security.Claims;

namespace SIMSS.Controllers
{
    public class LoginController : Controller
    {
        private readonly UserService _userService;
        private readonly SimsDbContext.SimsDbContext _context;

        public LoginController(UserService service, SimsDbContext.SimsDbContext context)
        {
            _userService = service;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            // Lấy thông tin người dùng
            var userInfo = await _userService.LoginUserAsync(model.Username, model.Password);
            if (userInfo == null)
            {
                ViewData["InvalidAccount"] = "Your account is invalid.";
                return View();
            }
            // Tạo claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userInfo.UserID.ToString()), // Thêm UserID vào claims
                new Claim(ClaimTypes.Name, userInfo.Username),
                new Claim(ClaimTypes.Role, userInfo.Role)
            };
            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );
            // Đăng nhập
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity)
            );
            return RedirectToAction("Index", "Dashboard");
        }

        [Authorize(Roles = "Admin, Student, Faculty")]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // Xóa tất cả cookies
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }
            return RedirectToAction("Index", "Login");
        }

    }
}
