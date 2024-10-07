using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Repository;
using SWP_Ticket_ReSell_DAO.DTO.Authentication;
using SWP_Ticket_ReSell_DAO.DTO.Customer;
using SWP_Ticket_ReSell_DAO.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SWP_Ticket_ReSell_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ServiceBase<Customer> _serviceCustomer;
        private readonly ServiceBase<Role> _serviceRole;
        //private readonly ServiceBase<Package> _servicePackage;
        //private readonly ServiceBase<Feedback> _serviceFeedback;
        //private readonly ServiceBase<Boxchat> _serviceBoxchat;
        //private readonly ServiceBase<Notification> _serviceNotification;
        //private readonly ServiceBase<Order> _serviceOrder;
        //private readonly ServiceBase<Report> _serviceReport;
        //private readonly ServiceBase<Request> _serviceRequest;
        //private readonly ServiceBase<Ticket> _serviceTicket;

        public AuthController(IConfiguration configuration, ServiceBase<Customer> serviceCustomer, ServiceBase<Role> serviceRole)
        {
            _configuration = configuration;
            _serviceCustomer = serviceCustomer;
            _serviceRole = serviceRole;
        }


        [HttpPost("Login")]
        public async Task<ActionResult> Login(LoginRequestDTO login)
        {
            var user = await _serviceCustomer
                .FindByAsync(x => x.Email == login.Email &&
                                  x.Password == login.Password);
            if (user == null)
            {
                return Unauthorized();
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
                var customer = new Customer()
                {
                    Email = request.Email,
                    Password = request.Password,
                    //Feed Back Avg
                    Average_feedback = 0,
                    //Customer Role = 2
                    ID_Role = 2,
                    //Basic Backet = 1 
                };
                //Email
                //string code = await UserManager.GenerateEmailConfirmationTokenAsync(customer.ID_Customer);
                //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { customer.ID_Customer, code = code }, protocol: Request.Scheme);
                //await UserManager.SendEmailAsync(customer.ID_Customer, "Confirm Email","Please Confirm Email");
                request.Adapt(customer);
                await _serviceCustomer.CreateAsync(customer);
            }
            return Ok("Create customer successfull.");
        }
    }
}

