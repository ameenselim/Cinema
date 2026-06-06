using Azure.Core;
using Cinema_Project.Areas.Identity.Controllers;
using Cinema_Project.Models;
using Cinema_Project.Repositories;
using Cinema_Project.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Cinema_Project.Services
{
    public enum EmailType
    {
        Register,
        ResendEmailConfirmation,
        ForgetPassword
    }
    public class AccountServices
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<ApplicationUserOTP> _applicationUserOTPRrepository;
        private readonly IEmailSender _emailSender;


        public AccountServices(UserManager<ApplicationUser> userManager,IRepository<ApplicationUserOTP> applicationUserOTPRrepository, IEmailSender emailSender)
        {
            _userManager = userManager;
            _applicationUserOTPRrepository = applicationUserOTPRrepository;
            _emailSender = emailSender;
        }

        public async Task SendEmailConfirmation(ApplicationUser user,HttpRequest request ,IUrlHelper url,EmailType emailType = EmailType.Register)
        {

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var URL = url.Action(nameof(AccountController.Confirm), SD.ACCOUNT_CONTROLLER, new { area = SD.IDENTITY_AREA, token, user.Id }, request.Scheme);

            string subject = string.Empty;
            string message = string.Empty;
            switch (emailType)
            {
                case EmailType.Register:
                    subject = "Confirm your email";
                    message = $"<h1>Please confirm your account by <a href='{URL}'>clicking here</a></h1>";
                    break;
                case EmailType.ResendEmailConfirmation:
                    subject = "Resend email confirmation";
                    message = $"<h1>Please confirm your account by <a href='{URL}'>clicking here</a></h1>";
                    break;
                 case EmailType.ForgetPassword:
                    {
                        var otp = new Random().Next(10000, 99999).ToString();
                        await _applicationUserOTPRrepository.CreateAsync(new ApplicationUserOTP()
                        {
                            OTP = otp,
                            ApplicationUserId = user.Id,
                        });
                        await _applicationUserOTPRrepository.Commit();

                        subject = "Reset Your Password In Cinema App";
                        message = $"<h1>Use this otp: {otp} to reset your password></h1>";
                    }
                    break;
            }
            await _emailSender.SendEmailAsync(user.Email!, subject, message);
        }
        public bool IsAthenticated(ClaimsPrincipal User)
        {
            return User.Identity is not null && User.Identity.IsAuthenticated;
        }
    }
}
