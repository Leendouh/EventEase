using EventEase.Data;
using EventEase.Models;
using EventEase.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace EventEase.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(
            ApplicationDbContext context,
            ILogger<AuthController> logger,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // GET: Auth/Login
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (model == null || !ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(
                    model.Username,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(model.Username);

                    if (user != null)
                    {
                        user.LastLoginDate = DateTime.Now;
                        await _userManager.UpdateAsync(user);

                        if (user.IsFirstLogin)
                        {
                            TempData["FirstTimeUserId"] = user.Id;
                            TempData["FirstTimeUsername"] = user.UserName;
                            TempData["FirstTimeFullName"] = user.FullName;
                            return RedirectToAction("FirstTimePasswordChange");
                        }

                        TempData["Success"] = $"Welcome back, {user.FullName}!";
                    }

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LOGIN ERROR: {ex.Message}");
                ModelState.AddModelError("", "An error occurred. Please try again.");
                return View(model);
            }
        }

        // GET: Auth/FirstTimePasswordChange
        public IActionResult FirstTimePasswordChange()
        {
            if (TempData["FirstTimeUserId"] == null)
            {
                return RedirectToAction("Login");
            }

            var model = new FirstTimePasswordViewModel
            {
                Username = TempData["FirstTimeUsername"]?.ToString() ?? string.Empty
            };

            TempData.Keep("FirstTimeUserId");
            TempData.Keep("FirstTimeUsername");
            TempData.Keep("FirstTimeFullName");

            return View(model);
        }

        // POST: Auth/FirstTimePasswordChange
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FirstTimePasswordChange(FirstTimePasswordViewModel model)
        {
            try
            {
                if (TempData["FirstTimeUserId"] == null || !ModelState.IsValid)
                {
                    return View(model);
                }

                string userId = TempData["FirstTimeUserId"]?.ToString() ?? string.Empty;

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                var changePasswordResult = await _userManager.ChangePasswordAsync(
                    user, model.CurrentPassword, model.NewPassword);

                if (changePasswordResult.Succeeded)
                {
                    user.IsFirstLogin = false;
                    user.LastPasswordChangeDate = DateTime.Now;
                    await _userManager.UpdateAsync(user);
                    await _signInManager.SignInAsync(user, false);

                    TempData["Success"] = "Password changed successfully!";
                    return RedirectToAction("CompleteProfile");
                }

                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Password change error: {ex.Message}");
                ModelState.AddModelError("", "An error occurred.");
                return View(model);
            }
        }

        // GET: Auth/CompleteProfile
        public async Task<IActionResult> CompleteProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var model = new CompleteProfileViewModel
            {
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty
            };

            return View(model);
        }

        // POST: Auth/CompleteProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteProfile(CompleteProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.UserName = model.Email;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Profile completed successfully!";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        // POST: Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}