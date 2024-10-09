using Mapster;
using Microsoft.AspNetCore.Mvc;
using Repository;
using SWP_Ticket_ReSell_DAO.DTO.Ticket;
using SWP_Ticket_ReSell_DAO.Models;
using System.Linq;

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
        [HttpGet("filter")]
        public async Task<ActionResult<IList<TicketResponseDTO>>> GetTicketsByLocation(string key, string value)
        {
            var tickets = await _service.FindListAsync<TicketResponseDTO>();
            List<TicketResponseDTO> listTicketByLocation = new List<TicketResponseDTO>();

            foreach (var item in tickets)
            {
                switch (key)
                {
                    case "location":
                        if (item.Location.ToLower().Contains(value.ToLower()))
                        {
                            listTicketByLocation.Add(item);
                        }
                        break;

                    case "ticketCategory":
                        if (item.Ticket_category.ToLower().Equals(value.ToLower()))
                        {
                            listTicketByLocation.Add(item);
                        }
                        break;
                        //case "all":
                        //    if (item.Ticket_category.ToLower().Equals(value.ToLower()) &&
                        //        item.Location.ToLower().Contains(value.ToLower()))
                        //    {
                        //        listTicketByLocation.Add(item);
                        //    }
                        //    break;
                }

            }
            if (listTicketByLocation == null)
            {
                return Problem(detail: $"ticket cannot found", statusCode: 404);
            }
            return Ok(listTicketByLocation);
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

        [HttpPost("ticket/{customerID:int}")]
        public async Task<ActionResult<TicketResponseDTO>> PostTicket(TicketCreateDTO ticketRequest,int customerID)
        {
            //Validation

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
    }
}
