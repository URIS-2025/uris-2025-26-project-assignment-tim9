using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Data;
using NotificationService.Exceptions;
using NotificationService.Models;
using NotificationService.Models.DTO.NotificationDTOs;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("notifications")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationRepository _repository;
        private readonly IMapper _mapper;

        public NotificationController(INotificationRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        // POST /notifications
        // Ugovor koji vec koristi WorkPackageService (i po njemu se mogu ugledati ostali servisi):
        // telo { userId, message, type }.
        [HttpPost]
        public async Task<ActionResult<NotificationDisplayDTO>> Create([FromBody] NotificationCreateDTO dto)
        {
            var notification = _mapper.Map<Notification>(dto);
            var created = await _repository.CreateAsync(notification);

            var result = _mapper.Map<NotificationDisplayDTO>(created);
            return CreatedAtAction(nameof(GetById), new { notificationId = result.Id }, result);
        }

        // GET /notifications?userId={userId}
        // Dok Auth/Gateway ne pocnu da injektuju identitet pozivaoca, userId se prosledjuje eksplicitno.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationDisplayDTO>>> GetAll([FromQuery] Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest("Parametar userId je obavezan.");
            }

            var notifications = await _repository.GetByUserIdAsync(userId);
            return Ok(_mapper.Map<IEnumerable<NotificationDisplayDTO>>(notifications));
        }

        // GET /notifications/{notificationId}
        [HttpGet("{notificationId}")]
        public async Task<ActionResult<NotificationDisplayDTO>> GetById(Guid notificationId)
        {
            var notification = await _repository.GetByIdAsync(notificationId);
            if (notification == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<NotificationDisplayDTO>(notification));
        }

        // PUT /notifications/{notificationId} - obelezava notifikaciju kao procitanu
        [HttpPut("{notificationId}")]
        public async Task<ActionResult<NotificationDisplayDTO>> MarkAsRead(Guid notificationId)
        {
            try
            {
                var updated = await _repository.MarkAsReadAsync(notificationId);
                return Ok(_mapper.Map<NotificationDisplayDTO>(updated));
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
