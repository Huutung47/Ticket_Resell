using Castle.Core.Resource;
using Mapster;
using Microsoft.AspNetCore.Authorization;
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
        private readonly ServiceBase<Ticket> _serviceTicket;
        private readonly ServiceBase<Customer> _serviceCustomer;
        private readonly ServiceBase<Role> _serviceRole;
        private readonly ServiceBase<Package> _servicePackage;
        private readonly FirebaseStorageService _firebaseStorageService;
        public TicketController(ServiceBase<Ticket> serviceTicket, ServiceBase<Customer> serviceCustomer, ServiceBase<Role> serviceRole, ServiceBase<Package> servicePackage, FirebaseStorageService firebaseStorageService)
        {
            _serviceTicket = serviceTicket;
            _serviceCustomer = serviceCustomer;
            _serviceRole = serviceRole;
            _servicePackage = servicePackage;
            _firebaseStorageService = firebaseStorageService;
        }
        [HttpGet("get-all-ticket")]
        [AllowAnonymous]
        public async Task<ActionResult<IList<TicketResponseDTO>>> GetAllTicket()
        {
            var entities = await _serviceTicket.FindListAsync<TicketResponseDTO>();
            return Ok(entities);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IList<TicketResponseDTO>>> GetTicketCanBuy()
        {
            var entities = await _serviceTicket.FindListAsync<TicketResponseDTO>(t => t.Status == "Available" && t.Event_Date > DateTime.Now);
            return Ok(entities);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<TicketResponseDTO>> GetTicket(string id)
        {
            var entity = await _serviceTicket.FindByAsync(p => p.ID_Ticket.ToString() == id);
            if (entity == null)
            {
                return Problem(detail: $"Ticket id {id} cannot found", statusCode: 404);
            }
            return Ok(entity.Adapt<TicketResponseDTO>());
        }


        [HttpGet("ticket/{sellerId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<IList<TicketResponseDTO>>> GetTicketsBySellerId(int sellerId)
        {
            var tickets = await _serviceTicket.FindListAsync<TicketResponseDTO>();
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
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Get list ticket filter")]
        public async Task<ActionResult<IList<TicketResponseDTO>>> GetTicketsByLocation(string? ticketCategory, string? location, decimal? price, string? show_name)
        {
            var tickets = await _serviceTicket.FindListAsync<TicketResponseDTO>(expression: GetTicketByQuery(ticketCategory, location, price, show_name));
            return Ok(tickets);
        }

        [HttpPut]
        [Authorize]
        [SwaggerOperation(Summary = "Update Ticket ")]
        public async Task<IActionResult> PutTicket(TicketRequestDTO ticketRequest)
        {
            var entity = await _serviceTicket.FindByAsync(p => p.ID_Ticket == ticketRequest.ID_Ticket);
            if (entity == null)
            {
                return Problem(detail: $"Ticket_id {ticketRequest.ID_Ticket} cannot found", statusCode: 404);
            }
            if (ticketRequest.Price != null)
            {
                entity.Price = ticketRequest.Price;
            }
            if (ticketRequest.Quantity != null)
            {
                entity.Quantity = ticketRequest.Quantity;
            }
            if (ticketRequest.Status != null)
            {
                entity.Status = ticketRequest.Status;
            } 
                //ticketRequest.Adapt(entity);
                await _serviceTicket.UpdateAsync(entity);
                return Ok("Update ticket successfull.");
            }

        [HttpPost("{customerID}")]
        [Authorize]
        [SwaggerOperation(Summary = "Create Ticket ")]
        public async Task<ActionResult<TicketResponseDTO>> PostTicket(TicketCreateDTO ticketRequest, int customerID)
        {
            var customer = await _serviceCustomer.FindByAsync(x => x.ID_Customer == customerID);
            if (customer.Package_expiration_date < DateTime.UtcNow || customer.Number_of_tickets_can_posted == 0)
            {
                return BadRequest("You need register Package pls");
            }
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
                Image = ticketRequest.Image,
            };
            customer.Number_of_tickets_can_posted -= 1;
            await _serviceTicket.CreateAsync(ticket);
            await _serviceCustomer.UpdateAsync(customer);
            return Ok("Create ticket successfull.\n" + $"ID_Ticket: {ticket.ID_Ticket}");
        }

        [HttpDelete("{id}")]
        [Authorize]
        [SwaggerOperation(Summary = "Delete Ticket ")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var ticket = await _serviceTicket.FindByAsync(p => p.ID_Ticket == id);
            if (ticket == null)
            {
                return Problem(detail: $"ticket_id {id} cannot found", statusCode: 404);
            }

            await _serviceTicket.DeleteAsync(ticket);
            return Ok("Delete ticket successfull.");
        }

        [HttpPut("customer")]
        [Authorize]
        [SwaggerOperation(Summary = "Update Customer On Ticket ")]
        public async Task<IActionResult> PutTicketByCustomer(TicketUpdateCustomerDTO ticketUpdate)
        {
            var entity = await _serviceTicket.FindByAsync(p => p.ID_Ticket == ticketUpdate.ID_Ticket);
            if (entity == null)
            {
                return Problem(detail: $"Ticket_id {ticketUpdate.ID_Ticket} cannot found", statusCode: 404);
            }
            ticketUpdate.Adapt(entity);
            await _serviceTicket.UpdateAsync(entity);
            return Ok("Update ticket successfull.");
        }

        private static Expression<Func<Ticket, bool>> GetTicketByQuery(string? ticketCategory, string? location, decimal? price, string? show_name)
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
            if (price.HasValue)
            {
                filterQuery = filterQuery.AndAlso(p => p.Price <= price.Value);
            }
            if (!string.IsNullOrEmpty(show_name))
            {
                var shows = show_name.ToLower().Split(',').Select(s => s.Trim().ToLower()).ToList();
                filterQuery = filterQuery.AndAlso(p => shows.Any(show => p.Show_Name.Equals(show)));
            }
            return filterQuery;
        }

        [HttpGet("total-ticket")]
        [Authorize(Roles = "1")]
        public async Task<ActionResult<IList<int>>> CountTicket()
        {
            var entities = await _serviceTicket.FindListAsync<Ticket>();
            var countTicket = entities.Count();
            return Ok(countTicket);
        }

        [HttpGet("total-ticket-available")]
        [Authorize(Roles = "1")]
        public async Task<ActionResult<IList<int>>> CountTicketAvailable()
        {
            var entities = await _serviceTicket.FindListAsync<Ticket>(t => t.Status == "Available");
            var countTicketAvailable = entities.Count();
            return Ok(countTicketAvailable);
        }

        [HttpGet("total-ticket-unavaliable")]
        [Authorize(Roles = "1")]
        public async Task<ActionResult<IList<int>>> CountTicketUnavaliable()
        {
            var entities = await _serviceTicket.FindListAsync<Ticket>(t => t.Status == "Unavailable");
            var countTicketUnavaliable = entities.Count();
            return Ok(countTicketUnavaliable);
        }
    }
}
