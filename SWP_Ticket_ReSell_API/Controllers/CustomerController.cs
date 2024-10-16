using Azure.Core;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository;
using SWP_Ticket_ReSell_DAO.DTO.Authentication;
using SWP_Ticket_ReSell_DAO.DTO.Customer;
using SWP_Ticket_ReSell_DAO.Models;

namespace SWP_Ticket_ReSell_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {

        private readonly ServiceBase<Customer> _service;
        private readonly ServiceBase<Role> _serviceRole;
        private readonly ServiceBase<Package> _servicePackage;
        private readonly FirebaseStorageService _firebaseStorageService;

        public CustomerController(ServiceBase<Customer> service, ServiceBase<Role> serviceRole, ServiceBase<Package> servicePackage, FirebaseStorageService firebaseStorageService)
        {
            _service = service;
            _serviceRole = serviceRole;
            _servicePackage = servicePackage;
            _firebaseStorageService = firebaseStorageService;
        }

        [HttpGet]
        public async Task<ActionResult<IList<CustomerResponseDTO>>> GetCustomer()
        {
            var entities = await _service.FindListAsync<CustomerResponseDTO>();
            return Ok(entities);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerResponseDTO>> GetCustomer(string id)
        {
            var entity = await _service.FindByAsync(p => p.ID_Customer.ToString() == id);
            if (entity == null)
            {
                return Problem(detail: $"Customer id {id} cannot found", statusCode: 404);
            }
            return Ok(entity.Adapt<CustomerResponseDTO>());
        }

        [HttpPut]
        //[Authorize]
        public async Task<IActionResult> PutCustomer([FromForm] CustomerRequestDTO customerRequest)
        {
            // Tìm kiếm entity khách hàng theo ID
            var entity = await _service.FindByAsync(p => p.ID_Customer == customerRequest.ID_Customer);
            if (entity == null)
            {
                return Problem(detail: $"Customer_id {customerRequest.ID_Customer} cannot be found", statusCode: 404);
            }
            // Kiểm tra xem package có tồn tại không
            //if (!await _servicePackage.ExistsByAsync(p => p.ID_Package == customerRequest.ID_Package))
            //{
            //    return Problem(detail: $"Package_id {customerRequest.ID_Package} cannot be found", statusCode: 404);
            //}
            if (customerRequest.Name != null) 
            {
                entity.Name = customerRequest.Name;
            }
            if (customerRequest.Contact != null) 
            {
                entity.Contact = customerRequest.Contact;
            }
            if (!string.IsNullOrWhiteSpace(customerRequest.Password))
            {
                entity.Password = BCrypt.Net.BCrypt.HashPassword(customerRequest.Password);
            }
            if (customerRequest.Avatar != null && customerRequest.Avatar.Length > 0)
            {
                using (var stream = customerRequest.Avatar.OpenReadStream())
                {
                    var imageUrl = await _firebaseStorageService.UploadFileAsync(stream, customerRequest.Avatar.FileName);
                    entity.Avatar = imageUrl;  // Cập nhật đường dẫn ảnh
                }
            }
            await _service.UpdateAsync(entity);
            return Ok("Update customer successful.");
        }

        [HttpPost]
        public async Task<ActionResult<CustomerResponseDTO>> PostCustomer(CustomerCreateDTO customerRequest)
        {
            if (await _service.ExistsByAsync(p => p.Email.Equals(customerRequest.Email)))
            {
                return Problem(detail: $"Email {customerRequest.Email} already exists", statusCode: 400);
            }

            if (!await _servicePackage.ExistsByAsync(p => p.ID_Package == customerRequest.ID_Package))
            {
                return Problem(detail: $"Package_id {customerRequest.ID_Package} cannot found", statusCode: 404);
            }

            var customer = new Customer();

            customerRequest.Adapt(customer); // chuyển data vào request checking regex

            await _service.CreateAsync(customer);
            return Ok("Create customer successfull.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _service.FindByAsync(p => p.ID_Customer == id);
            if (customer == null)
            {
                return Problem(detail: $"customer_id {id} cannot found", statusCode: 404);
            }

            await _service.DeleteAsync(customer);
            return Ok("Delete customer successfull.");
        }
    }
}
