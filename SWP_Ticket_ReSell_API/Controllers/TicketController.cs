using Mapster;
using Microsoft.AspNetCore.Mvc;
using Repository;
using SWP_Ticket_ReSell_DAO.DTO.Ticket;
using SWP_Ticket_ReSell_DAO.Models;

namespace SWP_Ticket_ReSell_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : Controller
    {
        private readonly ServiceBase<Ticket> _service;
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


        [HttpGet("seller/{sellerId}")]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTicketsBySellerId(int sellerId)
        {
            var tickets = await _service.GetByIdCustomer(sellerId);
            if (tickets == null || !tickets.Any())
            {
                return NotFound();
            }

            return Ok(tickets.Select(t => new Ticket
            {
                ID_Ticket = t.ID_Ticket,
                Buyer = t.Buyer,
                Price = t.Price,
                Ticket_category = t.Ticket_category,
                Ticket_type = t.Ticket_type,
                Quantity = t.Quantity,
                Ticket_History = t.Ticket_History,
                Status = t.Status,
                Event_Date = t.Event_Date,
                Show_Name = t.Show_Name,
                Location = t.Location,
                Description = t.Description,
                Seat = t.Seat,
                Ticketsold = t.Ticketsold,
                Image = t.Image,
            }));
        }

        [HttpPut]
        public async Task<IActionResult> PutTicket(TicketResponseDTO ticketRequest)
        {
            var entity = await _service.FindByAsync(p => p.ID_Ticket == ticketRequest.ID_Ticket);
            if (entity == null)
            {
                return Problem(detail: $"Customer_id {ticketRequest.ID_Ticket} cannot found", statusCode: 404);
            }
            ticketRequest.Adapt(entity);
            await _service.UpdateAsync(entity);
            return Ok("Update ticket successfull.");
        }

        [HttpPost]
        public async Task<ActionResult<TicketResponseDTO>> PostTicket(TicketCreateDTO ticketRequest)
        {
            //Validation

            var ticket = new Ticket()
            {
                Ticket_History = DateTime.Now,
                Status = "Available"
            };
            ticketRequest.Adapt(ticket);
            await _service.CreateAsync(ticket);
            return Ok("Create ticket successfull.");
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
    }
}
