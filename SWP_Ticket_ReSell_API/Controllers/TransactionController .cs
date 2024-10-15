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
using Microsoft.AspNetCore.Http;
using System.Net.Sockets;

namespace SWP_Ticket_ReSell_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : Controller
    {
        private readonly ServiceBase<Transaction> _serviceTransaction;
        private readonly ServiceBase<Order> _orderService;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TransactionController(ServiceBase<Transaction> serviceTransaction, ServiceBase<Order> orderService,
            IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _serviceTransaction = serviceTransaction;
            _orderService = orderService;
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
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
                Created_At = TimeUtils.GetCurrentSEATime(),
                Status = "PENDING",
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
        public async Task<IActionResult> CreatePayment(TransactionRequestDTO transactionRequest)
        {
            var currentTime = TimeUtils.GetCurrentSEATime();
            var currentTimeStamp = TimeUtils.GetTimestamp(currentTime);
            var txnRef = currentTime.ToString("yyMMdd") + "_" + currentTimeStamp;
            var pay = new VnPayLibrary();
            var urlCallBack = _configuration["VnPayPaymentCallBack:ReturnUrl"];
            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            pay.AddRequestData("vnp_Amount", (transactionRequest.FinalPrice * 100).ToString());
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

            var orderExist = await _orderService.FindByAsync(x => x.ID_Order == transactionRequest.ID_Order);

            if (orderExist == null)
            {
                return Problem(detail: "Order id cannot found", statusCode: 404);
            }

            var transaction = new Transaction()
            {
                ID_Order = transactionRequest.ID_Order,
                ID_Customer = transactionRequest.ID_Customer,
                ID_Payment = transactionRequest.ID_Payment,
                FinalPrice = Convert.ToDecimal(transactionRequest.FinalPrice),
                Created_At = TimeUtils.GetCurrentSEATime(),
                Status = "PENDING",
                TransactionCode = txnRef
            };

            await _serviceTransaction.CreateAsync(transaction);

            return Ok(paymentResponse);
        }

        [HttpGet("/callback/payment")]
        public async Task<IActionResult> VnPayCallBack(string? vnp_Amount, string? vnp_BankCode,
            string? vnp_BankTranNo, string? vnp_CardType, string? vnp_OrderInfo, string? vnp_PayDate,
            string? vnp_ResponseCode, string? vnp_TmnCode, string? vnp_TransactionNo, string? vnp_TxnRef,
            string? vnp_SecureHashType, string? vnp_SecureHash)
        {
            bool isSuccessful = await ExecuteVnPayCallBack(vnp_Amount, vnp_BankCode, vnp_BankTranNo,
                vnp_CardType, vnp_OrderInfo, vnp_PayDate, vnp_ResponseCode, vnp_TmnCode, vnp_TransactionNo, vnp_TxnRef,
                vnp_SecureHashType, vnp_SecureHash);
            if (isSuccessful && vnp_ResponseCode.Equals("00"))
            {
                return RedirectPermanent("https://www.google.com.vn/");
            }
            else
            {
                return RedirectPermanent("https://www.facebook.com");
            }
        }

        private async Task<bool> ExecuteVnPayCallBack(string? vnp_Amount, string? vnp_BankCode, string? vnp_BankTranNo,
            string? vnp_CardType, string? vnp_OrderInfo,
            string? vnp_PayDate, string? vnp_ResponseCode, string? vnp_TmnCode,
            string? vnp_TransactionNo, string? vnp_TxnRef, string? vnp_SecureHashType, string? vnp_SecureHash)
        {
            var currentTime = TimeUtils.GetCurrentSEATime();

            var transaction = await _serviceTransaction.FindByAsync(x => x.TransactionCode.Equals(vnp_TxnRef));
            if (vnp_ResponseCode.Equals("00"))
            {
                transaction.Status = "SUCCESS";
                transaction.Updated_At = currentTime;
            }
            else
            {
                transaction.Status = "FAILED";
                transaction.Updated_At = currentTime;
            }
            await _serviceTransaction.UpdateAsync(transaction);

            return !false;
        }
    }
}
