﻿using Azure.Core;
using Castle.Core.Smtp;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Repository;
using SWP_Ticket_ReSell_DAO.DTO.Authentication;
using SWP_Ticket_ReSell_DAO.DTO.Customer;
using SWP_Ticket_ReSell_DAO.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using SWP_Ticket_ReSell_API.Helper;
using Firebase.Auth;

namespace SWP_Ticket_ReSell_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ServiceBase<Customer> _serviceCustomer;
        private readonly ServiceBase<Role> _serviceRole;
        //private readonly ResetPassword _emailSender;

        public AuthController(IConfiguration configuration, ServiceBase<Customer> serviceCustomer, ServiceBase<Role> serviceRole)
        {
            _configuration = configuration;
            _serviceCustomer = serviceCustomer;
            _serviceRole = serviceRole;
            // _emailSender = emailSender;

        }
        [HttpPost("Login")]
        public async Task<ActionResult> Login(LoginRequestDTO login)
        {
            var user = await _serviceCustomer.FindByAsync(x => x.Email == login.Email); 
            if (user == null)
            {
                return Unauthorized("Email not find");
            }
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(login.Password, user.Password);
            if (!isPasswordValid)
            {
                return Unauthorized("Password wrong");
            }
            //
            if (user.EmailConfirm != "True")
            {
                await HttpContext.SignOutAsync("Cookies");
                return StatusCode(403, "You need to confirm your email address.");
            }

            List<Claim> claims = new List<Claim>
                {
                    //Name
                    new Claim(ClaimTypes.NameIdentifier, user.Name.ToString()),
                    //Email 
                    new Claim(ClaimTypes.Email, user.Email.ToString()),
                    //role 
                    new Claim(ClaimTypes.Role, user.ID_Role.ToString()!)

                };
            var key = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:SerectKey").Value!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            long expiredToken = 30;
            var token = new JwtSecurityToken(
                 claims: claims,
                 expires: DateTime.UtcNow.AddMinutes(expiredToken),
                 signingCredentials: creds);
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            //return Ok(new TokenRequest(jwt, user.Role));
            return Ok(new AccessTokenResponse { ID = user.ID_Customer, AccessToken = jwt, ExpiresIn = expiredToken });
        }

        [HttpPost("Register")]
        public async Task<ActionResult<RegisterResponseDTO>> Register(RegisterRequestDTO request)
        {
            if (ModelState.IsValid)
            {
                if (await _serviceCustomer.ExistsByAsync(p => p.Email.Equals(request.Email)))
                {
                    return Problem(detail: $"Email {request.Email} already exists", statusCode: 400);
                }
                if (request.Password != request.ConfirmPassWord)
                {
                    return Problem(detail: $"Password and Confirm Password different", statusCode: 400);
                }
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
                var customer = new Customer()
                {
                    Name = request.Name,
                    Email = request.Email,
                    Password = hashedPassword,
                    //Feed Back Avg
                    Average_feedback = 0,
                    //Customer Role = 2
                    ID_Role = 2,
                    EmailConfirm = "False",
                    Number_of_tickets_can_posted = 0,
                    //Basic Backet = 1 
                    Method_login = "Local"
                };
                //Email
                //string code = await UserManager.GenerateEmailConfirmationTokenAsync(customer.ID_Customer);
                //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { customer.ID_Customer, code = code }, protocol: Request.Scheme);
                //await UserManager.SendEmailAsync(customer.ID_Customer, "Confirm Email", "Please Confirm Email");

                //request.Adapt(customer);
                var frontendUrl = "https://resell-ticket-fe.vercel.app/";
                //var confirmationLink = $"{frontendUrl}/confirm?userId={Request.}";
                var emailBody = $"Please confirm your account by click: <a href=\"{frontendUrl}\">To here</a></a>";
                SendMail.SendEMail(request.Email, "Confirm your account", emailBody, "");

                await _serviceCustomer.CreateAsync(customer);
            }
            return Ok("Create customer successfull.");
        }

        //Login Google
        [HttpGet("login-google")]
        public async Task Login()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties
            {
                RedirectUri = "https://localhost:7216/api/auth/google-response" //chuyen sang trang nay neu dang nhap thanh cong
            });
        }

        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                return BadRequest("Authentication failed.");
            }

            var claims = result.Principal.Identities.FirstOrDefault().Claims.Select(claim => new
            {
                claim.Issuer,
                claim.Type,
                claim.Value
            }).ToList();
            var googleName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var googleEmail = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var issuer = claims.FirstOrDefault()?.Issuer;
            var existingCustomer = await _serviceCustomer.FindByAsync(x => x.Email == googleEmail);
            if (existingCustomer != null) 
            {
                return Ok("Customer already exists.");
            }
            var customer = new Customer()
            {
                Name = googleName,
                Email = googleEmail,
                Method_login = issuer,
                EmailConfirm = "True",
                Average_feedback = 0,
                Number_of_tickets_can_posted = 0,
                ID_Role = 2
            };
            await _serviceCustomer.CreateAsync(customer);
            return Ok("Create customer successfull.");
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await HttpContext.SignOutAsync();
                // Nếu đăng xuất thành công
                return Ok(new { message = "Logout successful" });
            }
            catch (Exception ex)
            {
                // Nếu đăng xuất thất bại
                return StatusCode(500, new { message = "Logout failed ", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(String emailUser, string token)
        {
            // Kiểm tra người dùng có tồn tại hay không
            var user = await _serviceCustomer.FindByAsync(x => x.EmailConfirm == emailUser);
            if (user == null)
                return BadRequest("Invalid user");

            // Giả sử token ở đây là đúng (bạn có thể kiểm tra token ở đây)
            if (token != null)
            {
                // Cập nhật ConfirmEmail thành true
                user.EmailConfirm = "true";
                await _serviceCustomer.UpdateAsync(user);
                return Ok("Email confirmed successfully. You can now log in.");
            }
            return BadRequest("Invalid token.");
        }


    }
}

