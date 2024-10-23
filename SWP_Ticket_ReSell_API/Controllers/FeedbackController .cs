using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository;
using SWP_Ticket_ReSell_DAO.DTO.Feedback;
using SWP_Ticket_ReSell_DAO.DTO.Package;
using SWP_Ticket_ReSell_DAO.DTO.Ticket;
using SWP_Ticket_ReSell_DAO.Models;

namespace SWP_Ticket_ReSell_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly ServiceBase<Order> _serviceOrder;
        private readonly ServiceBase<Customer> _serviceCustomer;
        private readonly ServiceBase<Feedback> _serviceFeedback;


        public FeedbackController(ServiceBase<Order> serviceOrder, ServiceBase<Customer> serviceCustomer, ServiceBase<Feedback> serviceFeedback)
        {
            _serviceOrder = serviceOrder;
            _serviceCustomer = serviceCustomer;
            _serviceFeedback = serviceFeedback;
        }

        [HttpGet]
        public async Task<ActionResult<IList<FeedbackRequestDTO>>> GetFeedback()
        {
            var entities = await _serviceFeedback.FindListAsync<FeedbackRequestDTO>();
            return Ok(entities);
        }



        [HttpGet("{id}")]
        public async Task<ActionResult<FeedbackReponseDTO>> GetFeedback(string id)
        {
            var entity = await _serviceFeedback.FindByAsync(p => p.ID_Feedback.ToString() == id);
            if (entity == null)
            {
                return Problem(detail: $"Feedback id {id} cannot found", statusCode: 404);
            }
            return Ok(entity.Adapt<FeedbackReponseDTO>());
        }
        //Chinh sua feedback 
        [HttpPut] 
        public async Task<IActionResult> PutFeedBack(FeedbackReponseDTO feedbackRequest)
        {
            var feedback = await _serviceFeedback.FindByAsync(p => p.ID_Feedback == feedbackRequest.ID_Feedback);
            if (feedback == null)
            {
                return Problem(detail: $"Feedback_id {feedbackRequest.ID_Feedback} cannot found", statusCode: 404);
            }
            feedbackRequest.Adapt(feedback);
            await _serviceFeedback.UpdateAsync(feedback);
            return Ok("Update ticket successfull.");
        }

        [HttpPost]
        //[Authorize]
        public async Task<ActionResult<FeedbackReponseDTO>> PostFeedback(FeedbackRequestDTO feedbackRequest)
        {
            // Tìm Order theo ID_Order
            var order = await _serviceOrder.FindByAsync(o => o.ID_Order == feedbackRequest.ID_Order);
            if (order == null)
            {
                return NotFound("Order not found.");
            }
            var customerId = order.ID_Customer;
            var feedback = new Feedback();
            feedbackRequest.Adapt(feedback); // Gán thông tin từ DTO vào Feedback
            feedback.ID_Order = feedbackRequest.ID_Order; // Gán ID_Order cho Feedback
            await _serviceFeedback.CreateAsync(feedback);

            // Bước 5: Tìm tất cả các phản hồi thông qua ID_Order và lấy ID_Customer tương ứng
            //var feedbacks = await _serviceFeedback.FindByAsync(f => f.ID_Order == feedback.ID_Order);
            //if (feedbacks != null && feedbacks.Count() > 0)
            //{
            //    // Tính toán điểm trung bình mới từ tất cả các phản hồi
            //    var averageFeedback = feedbacks.Average(f => f.Stars ?? 0); // 'f.Stars ?? 0' để xử lý giá trị null

            //    // Bước 6: Cập nhật giá trị Average_feedback cho người dùng
            //    var customer = await _serviceCustomer.FindByAsync(c => c.ID_Customer == customerId);

            //    if (customer != null)
            //    {
            //        // Cập nhật điểm trung bình
            //        customer.Average_feedback = averageFeedback;

            //        // Lưu thay đổi cho Customer
            //        await _serviceCustomer.UpdateAsync(customer);
            //    }
            //}
            return Ok("Thank you for your feedback.");
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            var feedBack = await _serviceFeedback.FindByAsync(p => p.ID_Feedback == id);
            if (feedBack == null)
            {
                return Problem(detail: $"Feedback_id {id} cannot found", statusCode: 404);
            }
            await _serviceFeedback.DeleteAsync(feedBack);
            return Ok("Delete feedBack successfull.");
        }
    }
}
