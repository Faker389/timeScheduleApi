using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace api.Controllers
{
    public class FirebaseAuthController : Controller
    {
        private readonly FirebaseService _firebaseService;

        public FirebaseAuthController()
        {
            _firebaseService = new FirebaseService();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Email and password are required.");

            try
            {
                var userId = await _firebaseService.CreateUserWithEmailAndPasswordAsync(request.Email, request.Password);
                return Ok(new { UserId = userId, Message = "User registered successfully." });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Email and password are required.");

            try
            {
                var (token, userId) = await _firebaseService.SignInWithEmailAndPasswordAsync(request.Email, request.Password);

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
                    return StatusCode(500, "Failed to retrieve token or userId.");

                return Ok(new {  userId = userId });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            if (string.IsNullOrEmpty(request.IdToken))
                return BadRequest("ID Token is required.");

            try
            {
                var decodedToken = await _firebaseService.VerifyGoogleIdTokenAsync(request.IdToken);
                return Ok(new { Uid = decodedToken.Uid, Message = "Google authentication successful." });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }

    // Request Models
    public class RegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class GoogleLoginRequest
    {
        public string IdToken { get; set; }
    }
}
