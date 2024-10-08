using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository;
using SWP_Ticket_ReSell_DAO.DTO.Package;
using SWP_Ticket_ReSell_DAO.DTO.Ticket;
using SWP_Ticket_ReSell_DAO.Models;

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

        //Update Package Customer
        //Customer chon goi 
        //[Authorize(Roles = "2")]
        [HttpPut("CustomerChoosePackage")]
        public async Task<IActionResult> ChoosePackage(PackageRequestDTO packageRequest)
        {
            // Kiểm tra xem Package đó có trong bảng Package không
            var checkPackage = await _servicePackage.FindByAsync(p => p.ID_Package == packageRequest.ID_Package);
            if (checkPackage == null)
            {
                return Problem(detail: $"Package_id {packageRequest.ID_Package} cannot be found", statusCode: 404);
            }

            // Lấy ID_Customer từ thông tin đăng nhập (Identity) và chuyển sang kiểu int
            var customerIdString = User.FindFirst("ID_Customer")?.Value;
            if (string.IsNullOrEmpty(customerIdString))
            {
                return Problem(detail: "Customer ID cannot be found from the logged-in user", statusCode: 400);
            }

            // Chuyển đổi ID_Customer từ string sang int
            if (!int.TryParse(customerIdString, out int customerId))
            {
                return Problem(detail: "Invalid Customer ID format", statusCode: 400);
            }

            // Tìm thông tin khách hàng theo ID_Customer
            var customer = await _serviceCustomer.FindByAsync(p => p.ID_Customer == customerId);
            if (customer == null)
            {
                return Problem(detail: $"Customer with ID {customerId} cannot be found", statusCode: 404);
            }

            // Kiểm tra xem người dùng đã có gói dịch vụ nào chưa
            if (customer.HSD_package != null)
            {
                // Nếu người dùng đã có gói, cộng thêm thời gian từ gói mới vào gói hiện tại của họ
                customer.HSD_package = customer.HSD_package.Value.AddMonths(checkPackage.Time_package);

                // Lưu lại cập nhật cho người dùng
                await _serviceCustomer.UpdateAsync(customer);
                return Ok("Package time updated successfully.");
            }
            else
            {
                // Nếu người dùng chưa có gói, gán gói mới cho người dùng
                customer.ID_Package = checkPackage.ID_Package;
                customer.HSD_package = DateTime.Now.AddMonths(checkPackage.Time_package); // Set thời gian hết hạn từ gói mới
                await _serviceCustomer.UpdateAsync(customer);
                return Ok("New package assigned successfully.");
            }
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
    }
}
