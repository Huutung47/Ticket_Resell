﻿using Mapster;
using Microsoft.AspNetCore.Authorization;
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
using System.Net.Sockets;
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
       //[Authorize]
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
        //[Authorize]
        public async Task<ActionResult<IList<RequestResponseDTO>>> GetRequestBySellerId(int sellerId)
        {
            var tickets = await _serviceTicket.FindListAsync<Ticket>(t => t.ID_Customer == sellerId);
            if (!tickets.Any())
            {
                return NotFound("No tickets found for this seller.");
            }
            var ticketIds = tickets.Select(t => t.ID_Ticket).ToList();
            var customerId = await _serviceCustomer.FindByAsync(c => c.ID_Customer == sellerId);
            var requests = await _serviceRequest.FindListAsync<Request>(r => ticketIds.Contains(r.ID_Ticket));

            // Lấy thông tin vé cho từng yêu cầu
            var requestDtos = new List<RequestResponseDTO>();
            foreach (var request in requests)
            {
                if (request.ID_Ticket != 0)
                {
                    var ticket = await _serviceTicket.FindByAsync(t => t.ID_Ticket == request.ID_Ticket);
                    if (ticket != null)
                    {
                        requestDtos.Add(new RequestResponseDTO
                        { 
                            ID_Request = request.ID_Request,
                            ID_Ticket = request.ID_Ticket,
                            ID_Customer = request.ID_Customer, // Người mua
                            Price_want = request.Price_want,
                            Quantity = request.Quantity,
                            History = request.History,
                            Status = request.Status,
                            TicketNavigation = new TicketNavigationDTO
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
                            },ID_CustomerNavigation = new CustomerResponseDTO 
                            {
                                ID_Customer = customerId.ID_Customer,
                                Name = customerId.Name,
                                Password = customerId.Password,
                                Email = customerId.Email,
                                Contact = customerId.Contact,
                                Average_feedback = customerId.Average_feedback,
                                ID_Role = customerId.ID_Role,
                                ID_Package = customerId.ID_Package,
                                Package_expiration_date = customerId.Package_expiration_date,
                                Package_registration_time = customerId.Package_registration_time,
                                Number_of_tickets_can_posted = customerId.Number_of_tickets_can_posted,
                                Avatar = customerId.Avatar,
                            },
    });
                    }
                }
            }
            return Ok(requestDtos);
        }



        [HttpGet("{id}")]
        [Authorize]
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
        [Authorize]
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

        [HttpPut("change-status-request")]
        [Authorize]
        public async Task<IActionResult> UpdateRequestStatus(int requestId, string status)
        {
            var request = await _serviceRequest.FindByAsync(p => p.ID_Request == requestId);
            if (request == null)
            {
                return Problem(detail: $"Request id {requestId} cannot be found");
            }
            var updatedRequests = new List<Request>(); 
            if (status == "Rejected")
            {
                request.Status = "Rejected";
                await _serviceRequest.UpdateAsync(request);
                updatedRequests.Add(request);
                return Ok(updatedRequests.Adapt<List<RequestResponseDTO>>()); 
            }
            else if (status == "Completed")
            {
                var ticket = await _serviceTicket.FindByAsync(t => t.ID_Ticket == request.ID_Ticket);
                if (ticket == null)
                {
                    return BadRequest("Ticket not found");
                }
                if (ticket.Quantity <= 0 || ticket.Quantity < request.Quantity)
                {
                    return BadRequest("Ticket is unavailable quantity");
                }
                ticket.Quantity -= request.Quantity;
                await _serviceTicket.UpdateAsync(ticket);
                request.Status = "Completed";
                await _serviceRequest.UpdateAsync(request);
                updatedRequests.Add(request);
                var pendingRequests = await _serviceRequest.FindListAsync<Request>(p => p.ID_Ticket == request.ID_Ticket && p.Status == "Pending");
                foreach (var pendingRequest in pendingRequests)
                {
                    if (ticket.Quantity < pendingRequest.Quantity)
                    {
                        pendingRequest.Status = "Rejected";
                        await _serviceRequest.UpdateAsync(pendingRequest);
                        updatedRequests.Add(pendingRequest);
                    }  
                }
                return Ok(updatedRequests.Adapt<List<RequestResponseDTO>>());
            }
            else
            {
                return BadRequest("You need rejected or completed ");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<RequestResponseDTO>> PostRequest(RequestRequestDTO requests)
        {
            var request = new Request()
            {
                
                History = DateTime.Now,
                Status = "Pending"
            };
            requests.Adapt(request); 
            await _serviceRequest.CreateAsync(request);
            return Ok("Create request successfull.");
        }

        [HttpPost("TicketID")]
        [Authorize]
        public async Task<ActionResult<RequestResponseDTO>> PostRequestID(int ticket,RequestRequestDTO requests)
        {
            var tickets = await _serviceTicket.FindByAsync(t => t.ID_Ticket == ticket);
            if (tickets == null)
            {
                return NotFound("Ticket not found.");
            }
            var request = new Request()
            {
                History = DateTime.Now,
                ID_Customer = requests.ID_Customer, // Người mua 
                ID_Ticket = ticket, //Vé gui yeu cau 
                Price_want = requests.Price_want,
                Quantity = requests.Quantity,
                Status = "Pending"
            };
            await _serviceRequest.CreateAsync(request);
            return Ok($"Send request successful to {tickets.ID_Customer} with {ticket}, {tickets.Quantity} ");
        }

        [HttpDelete("{id}")]
        [Authorize]
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

