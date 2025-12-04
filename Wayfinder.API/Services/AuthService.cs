using Wayfinder.API.Models;

namespace Wayfinder.API.Services
{
    /// <summary>
    /// Service for authentication operations
    /// </summary>
    public class AuthService
    {
        private readonly UserRepository _userRepository;
        private readonly JwtService _jwtService;

        public AuthService(UserRepository userRepository, JwtService jwtService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        public (User User, string Token, string Error) Register(string email, string password, string color1 = null, string color2 = null, string color3 = null)
        {
            // Check if email already exists
            if (_userRepository.EmailExists(email))
            {
                return (null, null, "Email already registered");
            }

            // Hash the password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            // Create the user
            var user = new User
            {
                Email = email,
                PasswordHash = passwordHash,
                Color1 = color1,
                Color2 = color2,
                Color3 = color3
            };

            user = _userRepository.Create(user);

            // Generate token
            var token = _jwtService.GenerateToken(user.Id, user.Email);

            return (user, token, null);
        }

        /// <summary>
        /// Authenticate a user with email and password
        /// </summary>
        public (User User, string Token, string Error) Login(string email, string password)
        {
            // Find user by email
            var user = _userRepository.GetByEmail(email);
            if (user == null)
            {
                return (null, null, "Invalid email or password");
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return (null, null, "Invalid email or password");
            }

            // Generate token
            var token = _jwtService.GenerateToken(user.Id, user.Email);

            return (user, token, null);
        }

        /// <summary>
        /// Change a user's password
        /// </summary>
        public (bool Success, string Error) ChangePassword(int userId, string currentPassword, string newPassword)
        {
            var user = _userRepository.GetById(userId);
            if (user == null)
            {
                return (false, "User not found");
            }

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            {
                return (false, "Current password is incorrect");
            }

            // Update password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _userRepository.Update(user);

            return (true, null);
        }
    }
}
