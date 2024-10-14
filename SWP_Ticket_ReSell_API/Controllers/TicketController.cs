using Mapster;
using Microsoft.AspNetCore.Mvc;
using Repository;
using Swashbuckle.AspNetCore.Annotations;
using SWP_Ticket_ReSell_API.Utils;
using SWP_Ticket_ReSell_DAO.DTO.Ticket;
using SWP_Ticket_ReSell_DAO.Models;
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

        public TicketController(ServiceBase<Ticket> service, ServiceBase<Role> serviceRole, ServiceBase<Package> servicePackage)
        {
            _service = service;
            _serviceRole = serviceRole;
            _servicePackage = servicePackage;
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

        [HttpGet("/filter")]
        public async Task<ActionResult<IList<TicketResponseDTO>>> GetTicketsByLocation(string? ticketCategory, string? location)
        {
            var tickets = await _service.FindListAsync<TicketResponseDTO>(expression: BuildGetPartnersQuery(ticketCategory, location));

            return Ok(tickets);
        }

        [HttpPut]
        public async Task<IActionResult> PutTicket(TicketResponseDTO ticketRequest)
        {
            var entity = await _service.FindByAsync(p => p.ID_Ticket == ticketRequest.ID_Ticket);
            if (entity == null)
            {
                return Problem(detail: $"Ticket_id {ticketRequest.ID_Ticket} cannot found", statusCode: 404);
            }
            ticketRequest.Adapt(entity);
            await _service.UpdateAsync(entity);
            return Ok("Update ticket successfull.");
        }

        [HttpPost("/{customerID}")]
        public async Task<ActionResult<TicketResponseDTO>> PostTicket(TicketCreateDTO ticketRequest, int customerID)
        {
            var ticket = new Ticket()
            {
                ID_Customer = customerID,
                Ticket_History = DateTime.Now,
                Status = "Available",
                Ticketsold = 0
            };
            ticketRequest.Adapt(ticket);
            await _service.CreateAsync(ticket);
            return Ok("Create ticket successfull.\n" +
                $"ID_Ticket: {ticket.ID_Ticket}");
        }

        [HttpDelete("{id}")]
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

        [HttpPut("/customer")]
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

        private static Expression<Func<Ticket, bool>> BuildGetPartnersQuery(string? ticketCategory, string? location)
        {
            Expression<Func<Ticket, bool>> filterQuery = x => true;

            if (!string.IsNullOrEmpty(ticketCategory))
            {
                filterQuery = filterQuery.AndAlso(p => p.Ticket_category.Contains(ticketCategory));
            }

            if (!string.IsNullOrEmpty(location))
            {
                filterQuery = filterQuery.AndAlso(p => p.Location.Contains(location));
            }

            return filterQuery;
        }
    }
}
