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
        [Authorize]
        public async Task<ActionResult<FeedbackReponseDTO>> PostFeedback(FeedbackRequestDTO feedbackRequest)
        {
            var feedBack = new Feedback();
            feedbackRequest.Adapt(feedBack);
            await _serviceFeedback.CreateAsync(feedBack);
            //var feedbacks = await _serviceFeedback.FindByAsync(f => f. == feedback.CustomerId);
            //if (feedbacks.Count > 0)
            //{
            //    // Tính toán điểm trung bình mới
            //    var averageFeedback = feedbacks.Average(f => f.Rating);

            //    // Bước 3: Cập nhật giá trị Average_feedback cho người dùng
            //    var customer = await _serviceCustomer.FindByAsync(c => c.ID_Customer == feedback.CustomerId);
            //    if (customer != null)
            //    {
            //        customer.Average_feedback = averageFeedback;
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
