using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Repository;
using SWP_Ticket_ReSell_DAO.DTO.Authentication;
using SWP_Ticket_ReSell_DAO.DTO.Customer;
using SWP_Ticket_ReSell_DAO.DTO.Report;
using SWP_Ticket_ReSell_DAO.DTO.Request;
using SWP_Ticket_ReSell_DAO.DTO.Ticket;
using SWP_Ticket_ReSell_DAO.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SWP_Ticket_ReSell_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestController : ControllerBase
    {
        private readonly ServiceBase<Request> _serviceRequest;
        private readonly ServiceBase<Customer> _serviceCustomer;
        private readonly ServiceBase<Ticket> _serviceTicket;
        private readonly ServiceBase<Notification> _serviceNotificate;


        public RequestController(ServiceBase<Request> serviceRequest, ServiceBase<Customer> serviceCustomer, ServiceBase<Ticket> serviceTicket, ServiceBase<Notification> serviceNotificate)
        {
            _serviceRequest = serviceRequest;
            _serviceCustomer = serviceCustomer;
            _serviceTicket = serviceTicket;
            _serviceNotificate = serviceNotificate;
        }

        [HttpGet]
        public async Task<ActionResult<IList<RequestResponseDTO>>> GetRequest()
        {
            var entities = await _serviceRequest.FindListAsync<RequestResponseDTO>();

            var result = new List<RequestResponseDTO>();
            foreach (var request in entities)
            {
                if (request.ID_Ticket != 0) 
                {
                    var ticket = await _serviceTicket.FindByAsync(t => t.ID_Ticket == request.ID_Ticket);
                    if (ticket != null)
                    {
                        request.TicketNavigation = new TicketNavigationDTO
                        {
                            Price = ticket.Price,
                            Ticket_category = ticket.Ticket_category,
                            Ticket_History = ticket.Ticket_History,
                            Ticket_type = ticket.Ticket_type,
                            Seat = ticket.Seat,
                            Status = ticket.Status,
                            Event_Date = ticket.Event_Date,
                            Show_Name = ticket.Show_Name,
                            Location = ticket.Location,
                            Description = ticket.Description,
                        };
                    }
                }
                result.Add(request);

            }
            return Ok(result);
        }


        [HttpGet("sellerId")]
        public async Task<ActionResult<IList<RequestResponseDTO>>> GetRequestBySellerId(int sellerId)
        {
            // Tìm tất cả các vé mà người bán là SellerId
            var tickets = await _serviceTicket.FindListAsync<Ticket>(t => t.ID_Customer == sellerId);
            if (!tickets.Any())
            {
                return NotFound("No tickets found for this seller.");
            }

            // Lấy danh sách các yêu cầu dựa trên ID_Ticket của vé mà sellerId là người bán
            var ticketIds = tickets.Select(t => t.ID_Ticket).ToList();
            var requests = await _serviceRequest.FindListAsync<Request>(r => ticketIds.Contains(r.ID_Ticket));
            var requestDtos = requests.Select(r => new RequestResponseDTO
            {
                ID_Request = r.ID_Request,
                ID_Ticket = r.ID_Ticket,
                ID_Customer = r.ID_Customer, // Người mua
                Price_want = r.Price_want,
                Quantity = r.Quantity,
                History = r.History
            }).ToList();
            return Ok(requestDtos);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<RequestResponseDTO>> GetRequest(string id)
        {
            var entity = await _serviceRequest.FindByAsync(p => p.ID_Request.ToString() == id);
            if (entity == null)
            {
                return Problem(detail: $"Request id {id} cannot found", statusCode: 404);
            }
            return Ok(entity.Adapt<RequestResponseDTO>());
        }

        [HttpPut]
        public async Task<IActionResult> PutRequest(RequestResponseDTO request)
        {
            var entity = await _serviceRequest.FindByAsync(p => p.ID_Request == request.ID_Request);
            if (entity == null)
            {
                return Problem(detail: $"Request id {request.ID_Request} cannot found", statusCode: 404);
            }
            request.Adapt(entity);
            await _serviceRequest.UpdateAsync(entity);
            return Ok("Update request successfull.");
        }

        [HttpPost]
        public async Task<ActionResult<RequestResponseDTO>> PostRequest(RequestRequestDTO requests)
        {
            var request = new Request()
            {
                History = DateTime.Now,
            };
            requests.Adapt(request); 
            await _serviceRequest.CreateAsync(request);
            return Ok("Create request successfull.");
        }

        [HttpPost("TicketID")]
        public async Task<ActionResult<RequestResponseDTO>> PostRequestID(int ticket,RequestRequestDTO requests)
        {
            // Lấy thông tin Ticket từ ID_Ticket để lấy thông tin người bán (SellerId)
            var tickets = await _serviceTicket.FindByAsync(t => t.ID_Ticket == ticket);
            if (tickets == null)
            {
                return NotFound("Ticket not found.");
            }
            var request = new Request()
            {
                History = DateTime.Now,
                ID_Customer = requests.ID_Customer, // Người mua 
                ID_Ticket = ticket, //Vé muon gui yeu cau 
                Price_want = requests.Price_want,
                Quantity = requests.Quantity,
            };
            await _serviceRequest.CreateAsync(request);
            return Ok($"Send request successful to {tickets.ID_Customer} with {ticket}, {tickets.Quantity} ");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var ticket = await _serviceRequest.FindByAsync(p => p.ID_Request == id);
            if (ticket == null)
            {
                return Problem(detail: $"Request id {id} cannot found", statusCode: 404);
            }

            await _serviceRequest.DeleteAsync(ticket);
            return Ok("Delete request successfull.");
        }
    }
}

