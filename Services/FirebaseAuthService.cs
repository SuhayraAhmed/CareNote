using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using CareNote.Models;

// Hanterar autentisering via Firebase/Firestore

namespace CareNote.Services 
{
    public class FirebaseAuthService
    {
        private readonly FirestoreDb? _firestoreDb;
        private readonly ILogger<FirebaseAuthService> _logger;

        public FirebaseAuthService(ILogger<FirebaseAuthService> logger, IConfiguration configuration) // Initierar Firestore med service account

        {
            _logger = logger;

            try
            {
                _logger.LogInformation("🚀 Initializing Firestore...");

                // 1️⃣ Läs sökväg från User Secrets eller environment
                var serviceAccountPath = configuration["Google:ServiceAccountPath"];
                if (string.IsNullOrEmpty(serviceAccountPath))
                {
                    serviceAccountPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
                }

                if (string.IsNullOrEmpty(serviceAccountPath) || !File.Exists(serviceAccountPath))
                {
                    _logger.LogError("❌ Service account file not found at configured path");
                    _logger.LogWarning("⚠️ Firestore will not be available");
                    _firestoreDb = null;
                    return;
                }

                _logger.LogInformation($"✅ Found service account: {serviceAccountPath}");

                var credential = GoogleCredential.FromFile(serviceAccountPath);
                _firestoreDb = new FirestoreDbBuilder
                {
                    ProjectId = "carenoteproject-af500", 
                    Credential = credential
                }.Build();

                _logger.LogInformation("✅ Firestore initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Firestore initialization failed: {ex.Message}");
                _logger.LogWarning("⚠️ Continuing without Firestore - authentication will still work");
                _firestoreDb = null;
            }
        }
        
         // Sparar/uppdaterar användare, hämtar användare via e-mail eller ID
        public async Task SaveOrUpdateUserInFirestore(string userId, string email, string displayName, string authProvider)
        {
            if (_firestoreDb == null)
            {
                _logger.LogWarning("⚠️ Firestore not available - skipping user save");
                return;
            }

            try
            {
                var userRef = _firestoreDb.Collection("users").Document(userId);
                var snapshot = await userRef.GetSnapshotAsync();

                var user = new User
                {
                    Id = userId,
                    Email = email.ToLower(),
                    DisplayName = displayName,
                    AuthProvider = authProvider,
                    LastLoginAt = DateTime.UtcNow,
                    IsActive = true,
                    Role = "user"
                };

                if (snapshot.Exists)
                {
                    await userRef.SetAsync(user, SetOptions.MergeAll);
                    _logger.LogInformation($"📝 Updated user in Firestore: {email}");
                }
                else
                {
                    user.CreatedAt = DateTime.UtcNow;
                    await userRef.SetAsync(user);
                    _logger.LogInformation($"💾 Saved new user to Firestore: {email}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error saving user to Firestore: {ex.Message}");
                throw;
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            if (_firestoreDb == null)
            {
                _logger.LogWarning("⚠️ Firestore not available");
                throw new Exception("Firestore is not available");
            }

            try
            {
                var query = _firestoreDb.Collection("users")
                    .WhereEqualTo("Email", email.ToLower())
                    .Limit(1);

                var querySnapshot = await query.GetSnapshotAsync();
                var document = querySnapshot.Documents.FirstOrDefault();

                return document?.Exists == true ? document.ConvertTo<User>() : null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting user by email: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateUserLastLogin(string userId) // Uppdaterar senaste inloggningstid
        {
            if (_firestoreDb == null)
            {
                _logger.LogWarning("⚠️ Firestore not available - skipping last login update");
                return;
            }

            try
            {
                var userRef = _firestoreDb.Collection("users").Document(userId);
                await userRef.UpdateAsync("LastLoginAt", DateTime.UtcNow);
                _logger.LogInformation($"🕒 Updated last login for user: {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error updating last login: {ex.Message}");
                throw;
            }
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            if (_firestoreDb == null)
            {
                _logger.LogWarning("⚠️ Firestore not available");
                return null;
            }

            try
            {
                var userRef = _firestoreDb.Collection("users").Document(userId);
                var snapshot = await userRef.GetSnapshotAsync();

                return snapshot.Exists ? snapshot.ConvertTo<User>() : null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting user by ID: {ex.Message}");
                return null;
            }
        }
    }// Används i AuthController för Google- och email-login

}
