using Mapster;
using Microsoft.AspNetCore.Mvc;
using Repository;
using SWP_Ticket_ReSell_DAO.DTO.Request;
using SWP_Ticket_ReSell_DAO.DTO.Ticket;
using SWP_Ticket_ReSell_DAO.DTO.Transaction;
using SWP_Ticket_ReSell_DAO.Models;
using System.Linq;

namespace SWP_Ticket_ReSell_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : Controller
    {
        private readonly ServiceBase<Transaction> _serviceTransaction;

        public TransactionController(ServiceBase<Transaction> serviceTransaction)
        {
            _serviceTransaction = serviceTransaction;
        }

        [HttpGet]
        public async Task<ActionResult<IList<TransactionResponseDTO>>> GetTransaction()
        {
            var entities = await _serviceTransaction.FindListAsync<TransactionResponseDTO>();
            return Ok(entities);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionResponseDTO>> GetTransaction(string id)
        {
            var entity = await _serviceTransaction.FindByAsync(p => p.ID_Transaction.ToString() == id);
            if (entity == null)
            {
                return Problem(detail: $"Transaction id {id} cannot found", statusCode: 404);
            }
            return Ok(entity.Adapt<TransactionResponseDTO>());
        }

        [HttpPut]
        public async Task<IActionResult> PutTicket(TransactionResponseDTO TransactionRequest)
        {
            var entity = await _serviceTransaction.FindByAsync(p => p.ID_Transaction == TransactionRequest.ID_Transaction);

            if (entity == null)
            {
                return Problem(detail: $"Ticket_id {TransactionRequest.ID_Transaction} cannot found", statusCode: 404);
            }
            TransactionRequest.Adapt(entity);
            await _serviceTransaction.UpdateAsync(entity);
            return Ok("Update transaction successfull.");
        }


        [HttpPost]
        public async Task<ActionResult<TransactionResponseDTO>> PostRequest(TransactionRequestDTO requests)
        {
            //Validation

            var transaction = new Transaction()
            {
                ID_Order = requests.ID_Order,
                ID_Customer = requests.ID_Customer,
                ID_Payment = requests.ID_Payment,
                Status = "Payment in progress",
            };
            requests.Adapt(transaction);
            await _serviceTransaction.CreateAsync(transaction);
            return Ok("Create transaction successfull.");
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var ticket = await _serviceTransaction.FindByAsync(p => p.ID_Transaction == id);
            if (ticket == null)
            {
                return Problem(detail: $"Transaction id {id} cannot found", statusCode: 404);
            }

            await _serviceTransaction.DeleteAsync(ticket);
            return Ok("Delete transaction successfull.");
        }
    }
}
