using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SWP_Ticket_ReSell_DAO.DTO.Ticket;
using SWP_Ticket_ReSell_DAO.Models;
using Repository;

[ApiController]
[Route("api/[controller]")]
public class FirebaseController : ControllerBase
{
    private readonly FirebaseStorageService _firebaseStorageService;
    private readonly ServiceBase<Ticket> _serviceTicket;

    public FirebaseController(FirebaseStorageService firebaseStorageService, ServiceBase<Ticket> serviceTicket)
    {
        _firebaseStorageService = firebaseStorageService;
        _serviceTicket = serviceTicket;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<List<string>>> UploadImages(List<IFormFile> images)
    {
        if (images != null && images.Count > 0)
        {
            var imageUrls = new List<(string Url, int DisplayOrder)>();
            int displayOrder = 1; 

            foreach (var image in images)
            {
                if (image.Length > 0)
                {
                    using (var stream = image.OpenReadStream())
                    {
                        var imageUrl = await _firebaseStorageService.UploadFileAsync(stream, image.FileName);
                        imageUrls.Add((imageUrl, displayOrder)); 
                    }
                    displayOrder++; 
                }
            }
            var sortedImageUrls = imageUrls.OrderBy(i => i.DisplayOrder).Select(i => i.Url).ToList();
            if (sortedImageUrls.Count > 0)
            {
                return Ok(sortedImageUrls); 
            }
            else
            {
                return BadRequest("No images were uploaded successfully."); 
            }
        }
        else
        {
            return BadRequest("Please add at least one image."); 
        }

    }
}
