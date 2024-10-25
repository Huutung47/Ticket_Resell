using Azure.Core;
using Castle.Core.Resource;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository;
using SWP_Ticket_ReSell_DAO.DTO.Authentication;
using SWP_Ticket_ReSell_DAO.DTO.Customer;
using SWP_Ticket_ReSell_DAO.DTO.Dashboard;
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
        public async Task<IActionResult> PutCustomer(CustomerRequestDTO customerRequest)
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
            customer.Number_of_tickets_can_posted = 0;
            customer.EmailConfirm = "False";
            customer.Method_login = "Local";
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(customerRequest.Password);
            customer.Password = hashedPassword;
            customerRequest.Adapt(customer); 
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

        [HttpGet("new-customer")]
        public async Task<ActionResult<IList<DashboardCustomer>>> GetLastCustomers()
        {
            var entities = await _service.FindListAsync<DashboardCustomer>();
            var result = entities.OrderByDescending(c => c.ID_Customer).Take(7).ToList();
            //var customer = result.OrderDescending();
            return Ok(result);
        }

        [HttpGet("Count-buy-package-by-month-year")]
        public async Task<ActionResult<int>> GetOrderCompletedByDate(int month, int year)
        {
            var customer = await _service.FindListAsync<Customer>(o => o.ID_Package != null 
            && o.Package_registration_time.HasValue 
            && o.Package_registration_time.Value.Month == month
            && o.Package_registration_time.Value.Year == year);
            var package = _servicePackage.FindListAsync<Package>();
            if (customer == null || !customer.Any())
            {
                return BadRequest($"Khong co ai mua goi trong thang {month} nam {year}");
            }
            return Ok(customer.Count());
        }

        [HttpGet("Total-price-package-by-month-year")]
        public async Task<ActionResult<decimal>> GetRevenueByDate(int month, int year)
        {
            var customers = await _service.FindListAsync<Customer>(o => o.ID_Package != null
            && o.Package_registration_time.HasValue
            && o.Package_registration_time.Value.Month == month
            && o.Package_registration_time.Value.Year == year);
            if (customers == null || !customers.Any())
            {
                return BadRequest($"Khong co ai mua goi trong thang {month}, nam {year}");
            }
            decimal? totalRevenue = 0;
            foreach (var customer in customers)
            {
                var packages = await _servicePackage.FindByAsync(o => o.ID_Package == customer.ID_Package );
                if (packages != null)
                {
                    totalRevenue += packages.Price;
                }
            }

            return Ok(totalRevenue);
        }

    }
}
