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

        public RequestController(ServiceBase<Request> serviceRequest)
        {
            _serviceRequest = serviceRequest;
        }

        [HttpGet]
        public async Task<ActionResult<IList<RequestResponseDTO[]>>> GetRequest()
        {
            var entities = await _serviceRequest.FindListAsync<RequestResponseDTO>();
            return Ok(entities);
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

