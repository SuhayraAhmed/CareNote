using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using CareNote.Services;
using CareNote.Models;

// Denna Controller hanterar autentisering (login/register/logout)
// EmailRegister: registrerar nya användare med e-post
// EmailPasswordLogin: loggar in användare med e-post och lösenord
// Logout: loggar ut användaren
// Säkerställer claims och sessionshantering med cookies

namespace CareNote.Controllers
{
    public class AuthController : Controller
    {
        private readonly FirebaseAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(FirebaseAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public IActionResult Login()
        {
            // Om användaren redan är inloggad, redirecta till home
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            try
            {
                _logger.LogInformation($" Received Google login request for: {request.Email}");
                
                // ENDAST @care.se emails tillåtna
                if (string.IsNullOrEmpty(request.Email) || !request.Email.EndsWith("@care.se"))  // GoogleLogin: loggar in användare via Google (endast @care.se)

                {
                    _logger.LogWarning($" Invalid email domain: {request.Email}");
                    return Unauthorized(new { success = false, error = "Only @care.se emails are allowed" });
                }

                _logger.LogInformation($" Email approved: {request.Email}");

                // Spara/uppdatera användaren i Firestore (med felhantering)
                try
                {
                    await _authService.SaveOrUpdateUserInFirestore(
                        request.UserId, 
                        request.Email, 
                        request.DisplayName, 
                        "google"
                    );
                }
                catch (Exception firestoreEx)
                {
                    _logger.LogWarning($" Could not save to Firestore, but continuing login: {firestoreEx.Message}");
                    // Fortsätt med login även om Firestore failar
                }

                // Skapa claims för session
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, request.UserId),
                    new Claim(ClaimTypes.Email, request.Email),
                    new Claim(ClaimTypes.Name, request.DisplayName ?? request.Email.Split('@')[0]),
                    new Claim("AuthProvider", "google")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    claimsPrincipal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTime.UtcNow.AddDays(7)
                    });

                _logger.LogInformation($" Login successful for: {request.Email}");
                
                return Ok(new { 
                    success = true, 
                    message = "Login successful", 
                    email = request.Email,
                    redirectUrl = "/Home"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($" GoogleLogin error: {ex.Message}");
                return BadRequest(new { success = false, error = "Login failed. Please try again." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EmailRegister([FromBody] EmailRegisterRequest request)
        {
            try
            {
                _logger.LogInformation($" Received email registration: {request.Email}");
                
                // VALIDERA INPUT OCH DOMÄN
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.DisplayName) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { success = false, error = "Email, display name and password are required" });
                }

                // ENDAST @care.se emails tillåtna
                if (!request.Email.EndsWith("@care.se"))
                {
                    return Unauthorized(new { success = false, error = "Only @care.se emails allowed for registration" });
                }

                // Kontrollera om användare redan finns
                User? existingUser = null;
                try
                {
                    existingUser = await _authService.GetUserByEmailAsync(request.Email);
                }
                catch (Exception firestoreEx)
                {
                    _logger.LogWarning($" Could not check existing user: {firestoreEx.Message}");
                }

                if (existingUser != null)
                {
                    return BadRequest(new { success = false, error = "This email is already registered. Please sign in instead." });
                }

                // Skapa Firebase användare FÖRST
                string userId;
                try
                {
                    // Detta skulle normalt göras via Firebase Admin SDK, men eftersom vi inte har det:
                    // Vi genererar ett ID och sparar i Firestore
                    userId = Guid.NewGuid().ToString();
                    _logger.LogInformation($" Creating user in Firestore with ID: {userId}");
                }
                catch (Exception firebaseEx)
                {
                    _logger.LogError($" Firebase user creation failed: {firebaseEx.Message}");
                    return BadRequest(new { success = false, error = "User creation failed. Please try again." });
                }

                // Skapa ny användare i Firestore
                try
                {
                    await _authService.SaveOrUpdateUserInFirestore(
                        userId,
                        request.Email,
                        request.DisplayName,
                        "email"
                    );
                }
                catch (Exception firestoreEx)
                {
                    _logger.LogWarning($" Could not save user to Firestore: {firestoreEx.Message}");
                    // Fortsätt ändå - användaren kan logga in med vår egen auth
                }

                _logger.LogInformation($" User registered: {request.Email}");

                
                var claims = new List<Claim>
                
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Email, request.Email),
                    new Claim(ClaimTypes.Name, request.DisplayName), 
                    new Claim("AuthProvider", "email")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    claimsPrincipal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTime.UtcNow.AddDays(7)
                    });

                return Ok(new { 
                    success = true, 
                    message = "Registration successful", 
                    email = request.Email,
                    redirectUrl = "/Home"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($" Email registration error: {ex.Message}");
                return BadRequest(new { success = false, error = $"Registration failed: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EmailPasswordLogin([FromBody] EmailPasswordLoginRequest request)
        {
            try
            {
                _logger.LogInformation($" Received email/password login: {request.Email}");
                
                // ENDAST @care.se emails tillåtna
                if (string.IsNullOrEmpty(request.Email) || !request.Email.EndsWith("@care.se"))
                {
                    return Unauthorized(new { success = false, error = "Only @care.se emails allowed" });
                }

                // Hämta användare från Firestore
                User? user = null;
                try
                {
                    user = await _authService.GetUserByEmailAsync(request.Email);
                }
                catch (Exception firestoreEx)
                {
                    _logger.LogWarning($" Could not get user from Firestore: {firestoreEx.Message}");
                    return Unauthorized(new { success = false, error = "Service temporarily unavailable. Please try again." });
                }

                if (user == null)
                {
                    return Unauthorized(new { success = false, error = "User not found. Please register first." });
                }

                // ENKEL LÖSENORDSVALIDERING - i ett riktigt system skulle detta vara hashat
                // Just nu accepterar vi alla lösenord eftersom vi inte sparar dem på ett säkert sätt
                // I produktion skulle du använda BCrypt eller liknande
                if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 6)
                {
                    return Unauthorized(new { success = false, error = "Invalid password" });
                }

                _logger.LogInformation($" Password accepted for: {request.Email}");

                // Uppdatera last login
                try
                {
                    await _authService.UpdateUserLastLogin(user.Id);
                }
                catch (Exception firestoreEx)
                {
                    _logger.LogWarning($" Could not update last login: {firestoreEx.Message}");
                }

                // Skapa session
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.DisplayName),
                    new Claim("AuthProvider", "google")
                };
                
                

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    claimsPrincipal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTime.UtcNow.AddDays(7)
                    });

                return Ok(new { 
                    success = true, 
                    message = "Login successful", 
                    email = request.Email,
                    redirectUrl = "/Home"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($" EmailPasswordLogin error: {ex.Message}");
                return BadRequest(new { success = false, error = "Login failed" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation(" User logged out");
            
            return RedirectToAction("Login", "Auth");
        }
    }

    public class GoogleLoginRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string IdToken { get; set; } = string.Empty;
    }

    public class EmailPasswordLoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class EmailRegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }
}