using Castle.Core.Resource;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Repository;
using Swashbuckle.AspNetCore.Annotations;
using SWP_Ticket_ReSell_API.Paginated;
using SWP_Ticket_ReSell_API.Utils;
using SWP_Ticket_ReSell_DAO.DTO.Customer;
using SWP_Ticket_ReSell_DAO.DTO.Ticket;
using SWP_Ticket_ReSell_DAO.Models;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace SWP_Ticket_ReSell_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : Controller
    {
        private readonly ServiceBase<Ticket> _service;
        private readonly ServiceBase<Customer> _customerService;
        private readonly ServiceBase<Role> _serviceRole;
        private readonly ServiceBase<Package> _servicePackage;
        private readonly FirebaseStorageService _firebaseStorageService;
        public TicketController(ServiceBase<Ticket> service, ServiceBase<Role> serviceRole, ServiceBase<Package> servicePackage, FirebaseStorageService firebaseStorageService)
        {
            _service = service;
            _serviceRole = serviceRole;
            _servicePackage = servicePackage;
            _firebaseStorageService = firebaseStorageService;
        }

        [HttpGet]
        public async Task<ActionResult<IList<TicketResponseDTO>>> GetTicket()
        {
            var entities = await _service.FindListAsync<TicketResponseDTO>();
            return Ok(entities);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TicketResponseDTO>> GetTicket(string id)
        {
            var entity = await _service.FindByAsync(p => p.ID_Ticket.ToString() == id);
            if (entity == null)
            {
                return Problem(detail: $"Ticket id {id} cannot found", statusCode: 404);
            }
            return Ok(entity.Adapt<TicketResponseDTO>());
        }


        [HttpGet("ticket/{sellerId:int}")]
        public async Task<ActionResult<IList<TicketResponseDTO>>> GetTicketsBySellerId(int sellerId)
        {
            var tickets = await _service.FindListAsync<TicketResponseDTO>();
            List<TicketResponseDTO> listTicketBySeller = new List<TicketResponseDTO>();
            foreach (var item in tickets)
            {
                if (item.ID_Customer == sellerId)
                {
                    listTicketBySeller.Add(item);
                }
            }
            return Ok(listTicketBySeller);
        }

        [HttpGet("filter")]
        [SwaggerOperation(Summary = "Get list ticket filter")]
        public async Task<ActionResult<IList<TicketResponseDTO>>> GetTicketsByLocation(string? ticketCategory, string? location)
        {
            var tickets = await _service.FindListAsync<TicketResponseDTO>(expression: GetTicketByQuery(ticketCategory, location));
            return Ok(tickets);
        }

        [HttpPut]
        [SwaggerOperation(Summary = "Update Ticket ")]
        public async Task<IActionResult> PutTicket(TicketResponseDTO ticketRequest)
        {
            var entity = await _service.FindByAsync(p => p.ID_Ticket == ticketRequest.ID_Ticket);
            if (entity == null)
            {
                return Problem(detail: $"Ticket_id {ticketRequest.ID_Ticket} cannot found", statusCode: 404);
            }
            if (ticketRequest.ID_Customer != null)
            {
                entity.ID_Customer = ticketRequest.ID_Customer;
            }
            if (ticketRequest.Price != null)
            {
                entity.Price = ticketRequest.Price;
            }
            if (ticketRequest.Ticket_category != null)
            {
                entity.Ticket_category = ticketRequest.Ticket_category;
            }
            if (ticketRequest.Ticket_type != null)
            {
                entity.Ticket_type = ticketRequest.Ticket_type;
            }
            if (ticketRequest.Quantity != null)
            {
                entity.Quantity = ticketRequest.Quantity;
            }
            if (ticketRequest.Status != null)
            {
                entity.Status = ticketRequest.Status;
            }
            if (ticketRequest.Event_Date != null)
            {
                entity.Event_Date = ticketRequest.Event_Date;
            }
            if (ticketRequest.Show_Name != null)
            {
                entity.Show_Name = ticketRequest.Show_Name;
            }
            if (ticketRequest.Location != null)
            {
                entity.Location = ticketRequest.Location;
            }
            if (ticketRequest.Description != null)
            {
                entity.Description = ticketRequest.Description;
            }
            if (ticketRequest.Seat != null)
            {
                entity.Seat = ticketRequest.Seat;
            }
            if (ticketRequest.Image != null && ticketRequest.Image.Length > 0)
            {
                using (var stream = ticketRequest.Image.OpenReadStream())
                {
                    var imageUrl = await _firebaseStorageService.UploadFileAsync(stream, ticketRequest.Image.FileName);
                    entity.Image = imageUrl;  // Cập nhật đường dẫn ảnh
                }
            }
            //ticketRequest.Adapt(entity);
            await _service.UpdateAsync(entity);
            return Ok("Update ticket successfull.");
        }

        [HttpPost("{customerID}")]
        [SwaggerOperation(Summary = "Create Ticket ")]
        public async Task<ActionResult<TicketResponseDTO>> PostTicket(TicketCreateDTO ticketRequest, int customerID)
        {
            var ticket = new Ticket()
            {
                ID_Customer = customerID,
                Ticket_History = DateTime.Now,
                Status = "Available",
                Ticketsold = 0,
                Price = ticketRequest.Price,
                Ticket_category = ticketRequest.Ticket_category,
                Ticket_type = ticketRequest.Ticket_type,
                Quantity = ticketRequest.Quantity,
                Event_Date = ticketRequest.Event_Date,
                Show_Name = ticketRequest.Show_Name,
                Location = ticketRequest.Location,
                Description = ticketRequest.Description,
                Seat = ticketRequest.Seat,
                Image = null,
            };
            //if (ticketRequest.Image != null && ticketRequest.Image.Length > 0)
            //{
            //    // Tải ảnh lên Firebase
            //    using (var stream = ticketRequest.Image.OpenReadStream())
            //    {
            //        var imageUrl = await _firebaseStorageService.UploadFileAsync(stream, ticketRequest.Image.FileName);
            //        ticket.Image = imageUrl;  // Cập nhật đường dẫn ảnh
            //    }
            //}
            //else
            //{
            //    return BadRequest("Add image ticket pls");
            //}
            //ticketRequest.Adapt(ticket);
            await _service.CreateAsync(ticket);
            return Ok("Create ticket successfull.\n" +
                $"ID_Ticket: {ticket.ID_Ticket}");
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Delete Ticket ")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var ticket = await _service.FindByAsync(p => p.ID_Ticket == id);
            if (ticket == null)
            {
                return Problem(detail: $"ticket_id {id} cannot found", statusCode: 404);
            }

            await _service.DeleteAsync(ticket);
            return Ok("Delete ticket successfull.");
        }

        [HttpPut("customer")]
        [SwaggerOperation(Summary = "Update Customer On Ticket ")]
        public async Task<IActionResult> PutTicketByCustomer(TicketUpdateCustomerDTO ticketUpdate)
        {
            var entity = await _service.FindByAsync(p => p.ID_Ticket == ticketUpdate.ID_Ticket);
            if (entity == null)
            {
                return Problem(detail: $"Ticket_id {ticketUpdate.ID_Ticket} cannot found", statusCode: 404);
            }
            ticketUpdate.Adapt(entity);
            await _service.UpdateAsync(entity);
            return Ok("Update ticket successfull.");
        }

        private static Expression<Func<Ticket, bool>> GetTicketByQuery(string? ticketCategory, string? location)
        {
            Expression<Func<Ticket, bool>> filterQuery = x => true;

            if (!string.IsNullOrEmpty(ticketCategory))
            {
                var categories = ticketCategory.ToLower().Split(',').Select(c => c.Trim().ToLower()).ToList();
                filterQuery = filterQuery.AndAlso(p => categories.Any(cate => p.Ticket_category.Equals(cate)));
            }

            if (!string.IsNullOrEmpty(location))
            {
                var locations = location.ToLower().Split(',').Select(l => l.Trim().ToLower()).ToList();
                filterQuery = filterQuery.AndAlso(p => locations.Any(loc => p.Location.Equals(loc)));
            }

            return filterQuery;
        }
    }
}
