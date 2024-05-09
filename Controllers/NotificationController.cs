using Blog.Interfaces;
using Blog.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Blog.Controllers
{
    [Route("api/v1/notification")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationRepository _notificationRepository;

        public NotificationController(IHubContext<NotificationHub> hubContext, INotificationRepository notificationRepository)
        {
            _hubContext = hubContext;
            _notificationRepository = notificationRepository;
        }

        [HttpGet("getnotification")]
        [Authorize(Roles = "Admin,Blogger")]
        public IActionResult GetNotificationData()
        {
            var revision =  _notificationRepository.GetNotificationData();
         
            return Ok(revision);
        }
    }
}
