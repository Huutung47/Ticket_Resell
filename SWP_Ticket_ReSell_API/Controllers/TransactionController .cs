using Mapster;
using Microsoft.AspNetCore.Mvc;
using Repository;
using SWP_Ticket_ReSell_API.Utils;
using SWP_Ticket_ReSell_DAO.DTO.Payment;
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
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TransactionController(ServiceBase<Transaction> serviceTransaction,
            IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _serviceTransaction = serviceTransaction;
            _httpContextAccessor = httpContextAccessor;
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

        [HttpPost("/create/payment")]
        public async Task<IActionResult> CreatePayment(TransactionRequestDTO transactionRequest, double amount)
        {
            var currentTime = TimeUtils.GetCurrentSEATime();
            var currentTimeStamp = TimeUtils.GetTimestamp(currentTime);
            var txnRef = currentTime.ToString("yyMMdd") + "_" + currentTimeStamp;
            var pay = new VnPayLibrary();
            var urlCallBack = _configuration["VnPayPaymentCallBack:ReturnUrl"];
            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            //pay.AddRequestData("vnp_Amount", ((int)amount * 100).ToString());
            pay.AddRequestData("vnp_Amount", (amount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", currentTime.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(_httpContextAccessor.HttpContext));
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
            pay.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang {txnRef}");
            pay.AddRequestData("vnp_OrderType", "other");
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", txnRef);

            var paymentUrl = pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);

            var paymentResponse = new CreatePaymentResponse()
            {
                Message = "Đang tiến hành thanh toán VNPAY",
                Url = paymentUrl,
                DisplayType = "URL"
            };

            // thêm Transaction vs status là pending
            var transaction = new Transaction()
            {
                ID_Order = transactionRequest.ID_Order,
                ID_Customer = transactionRequest.ID_Customer,
                ID_Payment = transactionRequest.ID_Payment,
                Status = "PENDING",
            };
            
            transactionRequest.Adapt(transaction);
            await _serviceTransaction.CreateAsync(transaction);

            return Ok("Create payment successfull.");
        }
    }
}
