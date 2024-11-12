using Mapster;
using Microsoft.AspNetCore.Mvc;
using Repository;
using SWP_Ticket_ReSell_API.Utils;
using SWP_Ticket_ReSell_DAO.DTO.Payment;
using SWP_Ticket_ReSell_DAO.DTO.Transaction;
using SWP_Ticket_ReSell_DAO.Models;
using Swashbuckle.AspNetCore.Annotations;
using SWP_Ticket_ReSell_DAO.DTO.OrderDetail;
using Microsoft.AspNetCore.Authorization;
using Net.payOS;
using Net.payOS.Types;
using Transaction = SWP_Ticket_ReSell_DAO.Models.Transaction;
namespace SWP_Ticket_ReSell_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : Controller
    {
        private readonly ServiceBase<Transaction> _serviceTransaction;
        private readonly ServiceBase<Order> _orderService;
        private readonly ServiceBase<Customer> _customerService;
        private readonly ServiceBase<Package> _packageService;
        private readonly ServiceBase<Ticket> _ticketService;
        private readonly ServiceBase<OrderDetail> _orderDetailService;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TransactionController(ServiceBase<Transaction> serviceTransaction,
            ServiceBase<Package> packageService, ServiceBase<Order> orderService,
            ServiceBase<Customer> customerService,
            ServiceBase<Ticket> ticketService,
            ServiceBase<OrderDetail> orderDetailService,
            IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _serviceTransaction = serviceTransaction;
            _orderService = orderService;
            _customerService = customerService;
            _packageService = packageService;
            _ticketService = ticketService;
            _orderDetailService = orderDetailService;
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        [HttpGet]
        public async Task<ActionResult<IList<TransactionResponseDTO>>> GetTransaction()
        {
            var entities = await _serviceTransaction.FindListAsync<TransactionResponseDTO>();
            return Ok(entities);
        }
        [HttpGet("customerId")]
        public async Task<ActionResult<IList<TransactionResponseDTO>>> GetTransactionByType(int customerId,string transactionType)
        {
            var entities = await _serviceTransaction.FindListAsync<TransactionResponseDTO>(p=>p.Transaction_Type.Equals(transactionType)&&(p.ID_Customer==customerId));
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

        [HttpPost("create-payment-link")]
        public async Task<IActionResult> CreatePaymentLink(TransactionRequestDTO transactionRequest)
        {
            var currentTime = TimeUtils.GetCurrentSEATime();
            var currentTimeStamp = TimeUtils.GetTimestamp(currentTime);
            var txnRef = currentTime.ToString("yyMMdd") + "_" + currentTimeStamp;
            long orderCode = (long)transactionRequest.ID_Order;
            String cancelUrl, returnUrl;
            var customer = await _customerService.FindByAsync(p => p.Orders.Any(od => od.ID_CustomerNavigation.ID_Customer == transactionRequest.ID_Customer));
            var clientId = customer.clientId;
            var apiKey = customer.apiKey;
            var checksumKey = customer.checksumKey;
           

            var transaction = new Transaction();
 
            switch (transactionRequest.Transaction_Type)
            {
                case "Ticket":
                    if (transactionRequest.ID_Order == null)
                    {
                        return Problem(detail: "Order id cannot found", statusCode: 404);
                    }
                    transaction.ID_Order = transactionRequest.ID_Order;
                    transaction.ID_Customer = transactionRequest.ID_Customer;
                    transaction.ID_Payment = transactionRequest.ID_Payment;
                    transaction.FinalPrice = Convert.ToDecimal(transactionRequest.FinalPrice);
                    transaction.Created_At = TimeUtils.GetCurrentSEATime();
                    transaction.Status = "PENDING";
                    transaction.TransactionCode = txnRef;
                    transaction.Transaction_Type = "Ticket";
                    transaction.ID_Package = null;

                    break;
                case "Package":
                    transaction.ID_Order = null;
                    transaction.ID_Customer = transactionRequest.ID_Customer;
                    transaction.ID_Payment = transactionRequest.ID_Payment;
                    transaction.FinalPrice = Convert.ToDecimal(transactionRequest.FinalPrice);
                    transaction.Created_At = TimeUtils.GetCurrentSEATime();
                    transaction.Status = "PENDING";
                    transaction.TransactionCode = txnRef;
                    transaction.Transaction_Type = "Package";
                    transaction.ID_Package = transactionRequest.ID_Package;

                    break;
            }

            await _serviceTransaction.CreateAsync(transaction);
            PayOS payOS = new PayOS(clientId, apiKey, checksumKey);
            ItemData item = new ItemData(transaction.Transaction_Type, 1,(int) transaction.FinalPrice);
            List<ItemData> items = new List<ItemData>();
            items.Add(item);
            PaymentData paymentData = new PaymentData(orderCode,(int)transactionRequest.FinalPrice, "Thanh toan don hang",
                 items,
                 cancelUrl = "http://localhost:3000/fail-payment",
                 returnUrl = "http://localhost:3000/success-payment");

            CreatePaymentResult createPayment = await payOS.createPaymentLink(paymentData);
            PaymentLinkInformation paymentLinkInformation = await payOS.getPaymentLinkInformation(orderCode);
            payOS.confirmWebhook("https://payos.vn/docs/du-lieu-tra-ve/webhook/");
            var paymentUrl = createPayment.checkoutUrl;
            var paymentResponse = new CreatePaymentResponse()
            {
                Message = "Đang tiến hành thanh toán payOS",
                Url = paymentUrl,
                DisplayType = "URL"
            };
            return Ok(paymentResponse);
        }

        [HttpPost("/create/payment")]
        [SwaggerOperation(Summary = "Create transaction to payment")]
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

            var transaction = new Transaction();

            switch (transactionRequest.Transaction_Type)
            {
                case "Ticket":
                    if (transactionRequest.ID_Order == null)
                    {
                        return Problem(detail: "Order id cannot found", statusCode: 404);
                    }
                    transaction.ID_Order = transactionRequest.ID_Order;
                    transaction.ID_Customer = transactionRequest.ID_Customer;
                    transaction.ID_Payment = transactionRequest.ID_Payment;
                    transaction.FinalPrice = Convert.ToDecimal(transactionRequest.FinalPrice);
                    transaction.Created_At = TimeUtils.GetCurrentSEATime();
                    transaction.Status = "PENDING";
                    transaction.TransactionCode = txnRef;
                    transaction.Transaction_Type = "Ticket";
                    transaction.ID_Package = null;

                    break;
                case "Package":
                    transaction.ID_Order = null;
                    transaction.ID_Customer = transactionRequest.ID_Customer;
                    transaction.ID_Payment = transactionRequest.ID_Payment;
                    transaction.FinalPrice = Convert.ToDecimal(transactionRequest.FinalPrice);
                    transaction.Created_At = TimeUtils.GetCurrentSEATime();
                    transaction.Status = "PENDING";
                    transaction.TransactionCode = txnRef;
                    transaction.Transaction_Type = "Package";
                    transaction.ID_Package = transactionRequest.ID_Package;

                    break;
            }

            await _serviceTransaction.CreateAsync(transaction);

            return Ok(paymentResponse);
        }

        [HttpGet("/callback/payment")]
        [SwaggerOperation(Summary = "Callback transaction to payment sucess")]
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
                return RedirectPermanent("http://localhost:3000/success-payment");
            }
            else
            {
                return RedirectPermanent("http://localhost:3000/fail-payment");
            }
        }

        private async Task<bool> ExecuteVnPayCallBack(string? vnp_Amount, string? vnp_BankCode, string? vnp_BankTranNo,
    string? vnp_CardType, string? vnp_OrderInfo,
    string? vnp_PayDate, string? vnp_ResponseCode, string? vnp_TmnCode,
    string? vnp_TransactionNo, string? vnp_TxnRef, string? vnp_SecureHashType, string? vnp_SecureHash)
        {
            var currentTime = TimeUtils.GetCurrentSEATime();

            var transaction = await _serviceTransaction.FindByAsync(x => x.TransactionCode.Equals(vnp_TxnRef));
            var order = await _orderService.FindByAsync(x => x.ID_Order == transaction.ID_Order);
            var package = await _packageService.FindByAsync(p => p.ID_Package == transaction.ID_Package);
            var customer = await _customerService.FindByAsync(x => x.ID_Customer == transaction.ID_Customer);


            if (vnp_ResponseCode.Equals("00"))
            {
                transaction.Status = "SUCCESS";
                transaction.Updated_At = currentTime;

                if (transaction.Transaction_Type.Equals("Ticket"))
                {
                    order.Status = "COMPLETED";
                    order.Update_At = currentTime;

                    var orderDetail = await _orderDetailService.FindListAsync<OrderDetailResponseDTO>(expression: e => e.ID_Order == order.ID_Order);
                    foreach (var item in orderDetail)
                    {
                        var ticket = await _ticketService.FindByAsync(t => t.ID_Ticket == item.ID_Ticket);
                        ticket.Quantity = ticket.Quantity - item.Quantity;
                    }
                }

                if (transaction.Transaction_Type.Equals("Package"))
                {
                    // update customer theo package
                    customer.ID_Package = transaction.ID_Package;

                    // update ngày đăng ký gói
                    if (customer.Package_registration_time == null)
                    {
                        customer.Package_registration_time = transaction.Created_At;
                    }

                    // update expirate date  
                    if (package != null)
                    {
                        int? numOfDayExpirate = package.Time_package; // 30 60 90 120 240 365
                        customer.Package_expiration_date = customer.Package_registration_time?.AddDays((double)numOfDayExpirate);

                        // update số lượng bài đăng
                        customer.Number_of_tickets_can_posted += package.Ticket_can_post;
                    }
                }

            }
            else
            {
                transaction.Status = "FAILED";
                transaction.Updated_At = currentTime;

                if (order != null)
                {
                    order.Status = "FAILED";
                    order.Update_At = currentTime;
                }

            }

            if (order != null)
            {
                await _orderService.UpdateAsync(order);
            }

            await _serviceTransaction.UpdateAsync(transaction);
            await _customerService.UpdateAsync(customer);


            return !false;
        }

        [HttpGet("get-transaction-buy-package")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TransactionInfoPackage>>> GetTransactionBuyPackage(int customerid)
        {
            var customers = await _serviceTransaction.FindListAsync<TransactionInfoPackage>(
                o => o.ID_Customer == customerid && o.Transaction_Type == "Package" );
            if (customers == null || !customers.Any())
            {
                return NotFound("Người dùng chưa đăng ký Package nào");
            }
            return Ok(customers);
        }

        [HttpGet("get-transaction-buy-package-successful")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TransactionInfoPackage>>> GetTransactionBuyPackageSuccess(int customerid)
        {
            var customers = await _serviceTransaction.FindListAsync<TransactionInfoPackage>(
                o => o.ID_Customer == customerid && o.Transaction_Type == "Package" && o.Status == "SUCCESS"
            );
            if (customers == null || !customers.Any())
            {
                return NotFound("Người dùng chưa đăng ký Package nào");
            }
            return Ok(customers);
        }

        [HttpGet("get-transaction-buy-ticket-successful")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TransactionInfoPackage>>> GetTransactionBuyTicket(int customerid)
        {
            var customers = await _serviceTransaction.FindListAsync<TransactionInfoPackage>(
                o => o.ID_Customer == customerid && o.Transaction_Type == "Ticket" && o.Status == "SUCCESS"
            );
            if (customers == null || !customers.Any())
            {
                return NotFound("Người dùng chưa đăng ký Ticket nào");
            }
            return Ok(customers);
        }

        [HttpGet("get-transaction-buy-all-successful")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TransactionInfoPackage>>> GetTransactionBuyAll(int customerid)
        {
            var customers = await _serviceTransaction.FindListAsync<TransactionInfoPackage>(
                o => o.ID_Customer == customerid && o.Status == "SUCCESS"
            );
            if (customers == null || !customers.Any())
            {
                return NotFound("Người dùng chưa thanh toan gi ca");
            }
            return Ok(customers);
        }

        [HttpGet("Total-price-package-by-month-year")]
        //[Authorize]
        public async Task<ActionResult<decimal>> GetRevenueByDate(int month, int year)
        {
            var transactionPackage = await _serviceTransaction.FindListAsync<Transaction>(o => o.ID_Package != null
            && o.Created_At.HasValue
            && o.Created_At.Value.Month == month
            && o.Created_At.Value.Year == year && o.Status == "SUCCESS" && o.Transaction_Type == "Package");
            if (transactionPackage == null || !transactionPackage.Any())
            {
                return BadRequest($"Khong co ai mua goi thanh cong trong thang {month}, nam {year}");
            }
            decimal? totalRevenue = 0;
            foreach (var transaction in transactionPackage)
            {
                var packages = await _packageService.FindByAsync(o => o.ID_Package == transaction.ID_Package);
                if (packages != null)
                {
                    totalRevenue += packages.Price;
                }
            }
            return Ok(totalRevenue);
        }

     

    }
}
