using Cinema_Project.Areas.Admin.Controllers;
using Cinema_Project.Areas.Customer.Controllers;
using Cinema_Project.Models;
using Cinema_Project.Repositories;
using Cinema_Project.Services;
using Cinema_Project.Utilities;
using Cinema_Project.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Cinema_Project.Areas.Identity.Controllers
{
    [Area(SD.IDENTITY_AREA)]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<ApplicationUserOTP> _applicationUserOTPRepository;
        private readonly AccountServices _accountServices;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<ApplicationUser> userManager, IRepository<ApplicationUserOTP> applicationUserOTPRepository, AccountServices accountServices, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _applicationUserOTPRepository = applicationUserOTPRepository;
            _accountServices = accountServices;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (_accountServices.IsAthenticated(User))
            {
                TempData["error_notification"] = "You Are Already Logged In";
                return RedirectToAction(nameof(HomeController.Index), SD.HOME_CONTROLLER, new { area = SD.CUSTOMER_AREA });
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid) return View(registerVM);

            ApplicationUser user = new ApplicationUser
            {
                UserName = registerVM.UserName,
                Email = registerVM.Email,
                FirstName = registerVM.FName,
                LastName = registerVM.LName,
                Address = registerVM.Address
            };

            var result = await _userManager.CreateAsync(user, registerVM.Password);

            if (!result.Succeeded)
            {
                TempData["error_notification"] = string.Join(", ", result.Errors.Select(e => e.Description));
                return View(registerVM);
            }

            await _accountServices.SendEmailConfirmation(user, Request, Url, EmailType.Register);
            await _userManager.AddToRoleAsync(user, RolesName.CUSTOMER);
            TempData["success_notification"] = "Registration successful! Please check your email to confirm your account.";

            return RedirectToAction(nameof(Login));
        }

        public async Task<IActionResult> Confirm(string id, string token)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return NotFound();

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (_accountServices.IsAthenticated(User))
            {
                TempData["error_notification"] = "You Are Already Logged In";
                return RedirectToAction(nameof(HomeController.Index), SD.HOME_CONTROLLER, new { area = SD.CUSTOMER_AREA });
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            var user = await _userManager.FindByEmailAsync(loginVM.EmailOrUserName) ?? await _userManager.FindByNameAsync(loginVM.EmailOrUserName);

            if (user is null)
            {
                TempData["error_notification"] = "Invalid login attempt..";

                ModelState.AddModelError(nameof(loginVM.EmailOrUserName), "Invalid Email Or UserName");
                ModelState.AddModelError(nameof(loginVM.Password), "Invalid Password");
                return View(loginVM);
            }
            var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.RememberMe, true);

            if (result.IsNotAllowed)
            {
                TempData["error_notification"] = "Please confirm your email before logging in.";

                ModelState.AddModelError(string.Empty, "Please confirm your email before logging in.");

                return View(loginVM);
            }

            if (!result.Succeeded)
            {
                TempData["error_notification"] = "Invalid login attempt.";

                ModelState.AddModelError(nameof(loginVM.EmailOrUserName), "Invalid Email Or UserName");
                ModelState.AddModelError(nameof(loginVM.Password), "Invalid Password");
                return View(loginVM);
            }

            TempData["success_notification"] = "Login successful!";

            //if (await _userManager.IsInRoleAsync(user, RolesName.SUPER_ADMIN) ||
            //    await _userManager.IsInRoleAsync(user, RolesName.ADMIN) ||
            //    await _userManager.IsInRoleAsync(user, RolesName.EMPLOYEE))
            //{
            //    return RedirectToAction(nameof(DashBoardController.DashBoard), SD.DASHBOARD_CONTROLLER, new { area = SD.ADMIN_AREA });
            //}
            return RedirectToAction(nameof(HomeController.Index), SD.HOME_CONTROLLER, new { area = SD.CUSTOMER_AREA });
        }
        [HttpGet]
        public IActionResult ResendEmailConfirmation()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.UserNameOrEmail) ?? await _userManager.FindByNameAsync(model.UserNameOrEmail);

            if (user is null)
            {
                ModelState.AddModelError(model.UserNameOrEmail, "UserName Or Email Is InValid");
                return View(model);
            }
            if (user.EmailConfirmed)
            {
                TempData["error_notification"] = "Email is already confirmed.";
                ModelState.AddModelError(model.UserNameOrEmail, "Email is already confirmed.");
                return View(model);
            }
            await _accountServices.SendEmailConfirmation(user, Request, Url, EmailType.ResendEmailConfirmation);

            TempData["success_notification"] = "Email confirmation resent successfully ,Please Check Your Email";


            return RedirectToAction(nameof(Login));
        }
        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordVM forgetPassword)
        {
            if (!ModelState.IsValid) return View(forgetPassword);

            var user = await _userManager.FindByEmailAsync(forgetPassword.UserNameOrEmail) ?? await _userManager.FindByNameAsync(forgetPassword.UserNameOrEmail);
            if (user is not null)
            {
                await _accountServices.SendEmailConfirmation(user, Request, Url, EmailType.ForgetPassword);
            }
            TempData["success_notification"] = "If Your Account Is Exist, You Will Recieve An Email To Reset Your Password ";

            TempData["userId"] = user?.Id;

            return RedirectToAction(nameof(ValidOTP));
        }
        [HttpGet]
        public IActionResult ValidOTP()
        {
            if (TempData.Peek("userId") is null)
                return NotFound();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ValidOTP(ValidOTPVM validOTPVM)
        {
            if (!ModelState.IsValid) return View(validOTPVM);

            var userId = TempData.Peek("userId")?.ToString();

            if (userId is null) return NotFound();

            var otp = await _applicationUserOTPRepository.GetOneAsync(e => e.ApplicationUserId == userId && e.OTP == validOTPVM.OTP && !e.IsUsed && e.ExpireAt > DateTime.UtcNow);

            if (otp is null)
            {
                TempData["error_notification"] = "Invalid OTP.";
                ModelState.AddModelError(validOTPVM.OTP, "Invalid OTP.");
                return View(validOTPVM);
            }
            otp.IsUsed = true;
            await _applicationUserOTPRepository.Commit();


            return RedirectToAction(nameof(NewPassword));
        }
        [HttpGet]
        public IActionResult NewPassword()
        {
            if (TempData.Peek("userId") is null)
                return NotFound();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> NewPassword(NewPasswordVM newPasswordVM)
        {
            if (!ModelState.IsValid) return View(newPasswordVM);

            var userId = TempData.Peek("userId")?.ToString();
            if (userId is null) return NotFound();

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPasswordVM.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(newPasswordVM);
            }
            TempData["success_notification"] = "Password reset successful! You can now log in with your new password.";
            TempData.Remove("userId");
            return RedirectToAction(nameof(Login));
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["success_notification"] = "Logout successful!";
            return RedirectToAction(nameof(Login));
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
