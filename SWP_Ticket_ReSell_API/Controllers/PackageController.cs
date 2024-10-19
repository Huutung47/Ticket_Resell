using Castle.Core.Resource;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository;
using SWP_Ticket_ReSell_DAO.DTO.Package;
using SWP_Ticket_ReSell_DAO.DTO.Ticket;
using SWP_Ticket_ReSell_DAO.Models;
using System.Security.Claims;

namespace SWP_Ticket_ReSell_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PackageController : ControllerBase
    {
        private readonly ServiceBase<Customer> _serviceCustomer;
        private readonly ServiceBase<Package> _servicePackage;
        private readonly ServiceBase<Role> _serviceRole;

        public PackageController(ServiceBase<Customer> serviceCustomer, ServiceBase<Role> serviceRole, ServiceBase<Package> servicePackage)
        {
            _serviceCustomer = serviceCustomer;
            _serviceRole = serviceRole;
            _servicePackage = servicePackage;
        }

        //Done 
        //Get all package
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IList<PackageResponseDTO>>> GetPackage()
        {
            var entities = await _servicePackage.FindListAsync<PackageResponseDTO>();
            return Ok(entities);
        }
        //Done 
        //Get package = id 
        [HttpGet("{id}")]
        public async Task<ActionResult<PackageResponseDTO>> GetPackage(string id)
        {
            var entity = await _servicePackage.FindByAsync(p => p.ID_Package.ToString() == id);
            if (entity == null)
            {
                return Problem(detail: $"Package id {id} cannot found", statusCode: 404);
            }
            return Ok(entity.Adapt<PackageResponseDTO>());
        }

        //Update package
        [HttpPut("id")]
        public async Task<IActionResult> PutTicket(PackageResponseDTO packageRequest)
        {
            var entity = await _servicePackage.FindByAsync(p => p.ID_Package == packageRequest.ID_Package);
            if (entity == null)
            {
                return Problem(detail: $"Package_id {packageRequest.ID_Package} cannot found", statusCode: 404);
            }
            packageRequest.Adapt(entity);
            await _servicePackage.UpdateAsync(entity);
            return Ok("Update ticket successfull.");
        }
        //Done
        //Create package 
        [HttpPost]
        public async Task<ActionResult<PackageResponseDTO>> PostTicket(PackageRequestDTO packageRequest)
        {
            var package = new Package();
            packageRequest.Adapt(package);
            await _servicePackage.CreateAsync(package);
            return Ok("Create package successfull.");
        }
        //Done 
        //Delete package 
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePackage(int id)
        {
            var package = await _servicePackage.FindByAsync(p => p.ID_Package == id);
            if (package == null)
            {
                return Problem(detail: $"Package_id {id} cannot found", statusCode: 404);
            }
            await _servicePackage.DeleteAsync(package);
            return Ok("Delete package successfull.");
        }


        //Cho nguoi dung chon Package
        [HttpPost("registerPackage")]
        [Authorize]
        public async Task<IActionResult> RegisterPackage([FromBody] PackageChoose request)
        {
            // Lấy CustomerId từ token đã xác thực
            var customerEmailClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            if (customerEmailClaim == null)
            {
                return Unauthorized(new { message = "Không tìm thấy thông tin người dùng." });
            }

            string customerEmail = customerEmailClaim.Value;

            // Tìm thông tin người dùng bằng CustomerId
            var customer = await _serviceCustomer.FindByAsync(x => x.Email == customerEmail);
            if (customer == null)
            {
                return BadRequest(new { message = "Người dùng không tồn tại." });
            }
            // Kiểm tra package có hợp lệ không
            var package = await _servicePackage.FindByAsync(x => x.ID_Package == request.ID_Package);
            if (package == null)
            {
                return BadRequest(new { message = "Package không hợp lệ." });
            }

            // Cập nhật thông tin package cho người dùng
            //var expirationDate = customer.Package_expiration_date ?? DateTime.Now;
            if (customer.Package_expiration_date.HasValue && customer.Package_expiration_date > DateTime.Now)
            {
                // Cộng thêm thời gian của package mới vào thời gian hết hạn hiện tại
                customer.Package_expiration_date = customer.Package_expiration_date.Value.AddMonths((int)package.Time_package); // Cộng số tháng
                customer.Number_of_tickets_can_posted += package.Ticket_can_post;
                customer.Package_registration_time = DateTime.Now;
            }
            else
            {
                // Nếu người dùng không có package hoặc đã hết hạn, đăng ký package mới
                customer.ID_Package = package.ID_Package;
                customer.Package_expiration_date = DateTime.Now.AddMonths((int)package.Time_package);
                customer.Number_of_tickets_can_posted += package.Ticket_can_post;
                customer.Package_registration_time = DateTime.Now;
            }
            // Cập nhật thời gian đăng ký package
            customer.Package_registration_time = DateTime.Now;
            // Lưu thông tin vào cơ sở dữ liệu
            await _serviceCustomer.UpdateAsync(customer);
            return Ok(new { message = "Đăng ký package thành công." });
        }
    }
}
