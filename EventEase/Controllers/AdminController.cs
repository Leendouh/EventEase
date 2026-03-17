using EventEase.Data;
using EventEase.Models;
using EventEase.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class AdminController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Admin/Index - List all admins
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .OrderByDescending(u => u.CreatedDate)
                .ToListAsync();

            var adminViewModels = new List<AdminListViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                // FIX: CreatedBy is now string, find by string ID
                ApplicationUser? createdByUser = null;
                if (!string.IsNullOrEmpty(user.CreatedBy))
                {
                    createdByUser = await _userManager.FindByIdAsync(user.CreatedBy);
                }

                adminViewModels.Add(new AdminListViewModel
                {
                    AdminId = user.AdminId,
                    FormattedAdminId = user.FormattedAdminId,
                    Username = user.UserName ?? string.Empty,
                    FullName = user.FullName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    Role = roles.FirstOrDefault() ?? "No Role",
                    IsActive = user.IsActive,
                    IsFirstLogin = user.IsFirstLogin,
                    CreatedDate = user.CreatedDate,
                    CreatedByName = createdByUser?.FullName ?? "System",
                    LastLoginDate = user.LastLoginDate
                });
            }

            return View(adminViewModels);
        }

        // GET: Admin/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Create - SuperAdmin creates SubAdmin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSubAdminViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if username already exists
                var existingUser = await _userManager.FindByNameAsync(model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Username already exists");
                    return View(model);
                }

                // Create new sub-admin user
                var subAdmin = new ApplicationUser
                {
                    UserName = model.Username,
                    FullName = model.FullName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Role = model.Role,
                    IsActive = true,
                    IsFirstLogin = true,
                    CreatedDate = DateTime.Now,
                    CreatedBy = GetCurrentUserId(), // FIX: Use string ID
                    EmailConfirmed = true,
                    AdminId = await GetNextAdminId()
                };

                // Create the user with temporary password
                var result = await _userManager.CreateAsync(subAdmin, model.TemporaryPassword);

                if (result.Succeeded)
                {
                    // Assign role
                    await _userManager.AddToRoleAsync(subAdmin, model.Role);

                    TempData["Success"] = $"Sub-Admin '{model.FullName}' created successfully! They will be required to change password on first login.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        // GET: Admin/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.AdminId == id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            var viewModel = new CreateSubAdminViewModel
            {
                Username = user.UserName ?? string.Empty,
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Role = roles.FirstOrDefault() ?? "BookingSpecialist"
            };

            return View(viewModel);
        }

        // POST: Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateSubAdminViewModel model)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.AdminId == id);
            if (user == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                user.FullName = model.FullName;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.UserName = model.Username;

                var updateResult = await _userManager.UpdateAsync(user);

                if (updateResult.Succeeded)
                {
                    // Update role if changed
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRoleAsync(user, model.Role);

                    // If a new temporary password is provided, reset it
                    if (!string.IsNullOrEmpty(model.TemporaryPassword))
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        var passwordResult = await _userManager.ResetPasswordAsync(user, token, model.TemporaryPassword);

                        if (passwordResult.Succeeded)
                        {
                            user.IsFirstLogin = true;
                            await _userManager.UpdateAsync(user);
                        }
                    }

                    TempData["Success"] = "Admin updated successfully!";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        // POST: Admin/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.AdminId == id);
            if (user == null)
            {
                return NotFound();
            }

            // Don't allow deactivating yourself
            if (user.AdminId == GetCurrentAdminId())
            {
                TempData["Error"] = "You cannot deactivate your own account!";
                return RedirectToAction(nameof(Index));
            }

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = $"Admin '{user.FullName}' {(user.IsActive ? "activated" : "deactivated")} successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/ResetPassword/5
        public async Task<IActionResult> ResetPassword(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.AdminId == id);
            if (user == null)
            {
                return NotFound();
            }

            // Don't allow resetting your own password here (use profile page)
            if (user.AdminId == GetCurrentAdminId())
            {
                TempData["Error"] = "Use your profile page to change your own password.";
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        // POST: Admin/ResetPassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int id, string newPassword)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.AdminId == id);
            if (user == null)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
            {
                TempData["Error"] = "Password must be at least 6 characters long.";
                return RedirectToAction(nameof(ResetPassword), new { id });
            }

            // Reset password and force change on next login
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                user.IsFirstLogin = true;
                await _userManager.UpdateAsync(user);

                TempData["Success"] = $"Password reset for '{user.FullName}'. They will be required to change it on next login.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                TempData["Error"] = error.Description;
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<int> GetNextAdminId()
        {
            var maxAdminId = await _userManager.Users.MaxAsync(u => (int?)u.AdminId) ?? 0;
            return maxAdminId + 1;
        }
    }
}