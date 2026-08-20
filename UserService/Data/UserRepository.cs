using AutoMapper;
using UserService.Context;
using UserService.Models;
using UserService.Models.DTO.AuthDtos;
using UserService.Models.DTO.UserDtos;
using UserService.Models.Enums;
using UserService.ServiceCalls.Notification;
using UserService.Services;

namespace UserService.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly UserContext _context;
        private readonly IMapper _mapper;
        private readonly IPasswordService _passwordService;
        private readonly INotificationService _notificationService;

        public UserRepository(
            UserContext context,
            IMapper mapper,
            IPasswordService passwordService,
            INotificationService notificationService)
        {
            _context = context;
            _mapper = mapper;
            _passwordService = passwordService;
            _notificationService = notificationService;
        }

        public IEnumerable<UserDto> GetUsers(string? search, UserRole? role, bool? isActive)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(u =>
                    u.Name.ToLower().Contains(term) ||
                    u.Username.ToLower().Contains(term) ||
                    u.Email.ToLower().Contains(term));
            }

            if (role.HasValue)
                query = query.Where(u => u.Role == role.Value);

            if (isActive.HasValue)
                query = query.Where(u => u.IsActive == isActive.Value);

            var users = query.ToList();
            var result = new List<UserDto>();
            foreach (var user in users)
            {
                result.Add(_mapper.Map<UserDto>(user));
            }

            return result;
        }

        public UserDto? GetUserById(Guid userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public UserDto? GetUserByUsername(string username)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public bool UsernameExists(string username)
        {
            return _context.Users.Any(u => u.Username == username);
        }

        public bool EmailExists(string email)
        {
            return _context.Users.Any(u => u.Email == email);
        }

        public UserConfirmationDto CreateUser(UserCreationDto userDto)
        {
            var (hash, salt) = _passwordService.HashPassword(userDto.Password);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Name = userDto.Name,
                Username = userDto.Username,
                Email = userDto.Email,
                ContactInfo = userDto.ContactInfo,
                PasswordHash = hash,
                Salt = salt,
                Role = UserRole.TeamMember,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            LogActivity(user.UserId, "Created", user.UserId, null);
            _context.SaveChanges();

            return _mapper.Map<UserConfirmationDto>(user);
        }

        public UserConfirmationDto? UpdateUser(Guid userId, UserUpdateDto userDto)
        {
            var existing = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (existing == null)
                return null;

            existing.Name = userDto.Name;
            existing.Email = userDto.Email;
            existing.ContactInfo = userDto.ContactInfo;
            _context.SaveChanges();

            return _mapper.Map<UserConfirmationDto>(existing);
        }

        public bool SetActiveStatus(Guid userId, bool isActive, Guid performedBy)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return false;

            user.IsActive = isActive;
            LogActivity(userId, isActive ? "Activated" : "Deactivated", performedBy, null);
            _context.SaveChanges();
            return true;
        }

        public async Task<bool> ChangeRole(RoleUpdateDto roleDto)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == roleDto.UserId);
            if (user == null)
                return false;

            // Admin ne sme sam sebi da ukine administratorska prava
            if (user.UserId == roleDto.ChangedBy && user.Role == UserRole.Admin && roleDto.NewRole != UserRole.Admin)
                return false;

            var oldRole = user.Role;
            user.Role = roleDto.NewRole;
            LogActivity(user.UserId, "RoleChanged", roleDto.ChangedBy, $"{oldRole} -> {roleDto.NewRole}");
            _context.SaveChanges();

            // "Korisnik prima email notifikaciju o promeni uloge sa opisom novih privilegija" (Admin story 2)
            await _notificationService.SendNotificationAsync(
                user.UserId,
                $"Vasa uloga je promenjena iz {oldRole} u {roleDto.NewRole}.",
                "RoleChanged");

            return true;
        }

        public void DeleteUser(Guid userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }

        public UserAuthVo ValidateCredentials(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user == null || !_passwordService.VerifyPassword(password, user.PasswordHash, user.Salt))
            {
                return new UserAuthVo { IsValid = false };
            }

            return new UserAuthVo
            {
                IsValid = true,
                UserId = user.UserId,
                Username = user.Username,
                Role = user.Role.ToString(),
                IsActive = user.IsActive
            };
        }

        public IEnumerable<UserActivityLogDto> GetActivityLog(Guid userId)
        {
            var logs = _context.UserActivityLogs
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.Timestamp)
                .ToList();

            var result = new List<UserActivityLogDto>();
            foreach (var log in logs)
            {
                result.Add(_mapper.Map<UserActivityLogDto>(log));
            }

            return result;
        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() > 0;
        }

        private void LogActivity(Guid userId, string action, Guid performedBy, string? details)
        {
            _context.UserActivityLogs.Add(new UserActivityLog
            {
                LogId = Guid.NewGuid(),
                UserId = userId,
                Action = action,
                PerformedBy = performedBy,
                Details = details,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
