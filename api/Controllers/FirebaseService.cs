using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using System.Threading.Tasks;

namespace api.Controllers
{
    public class FirebaseService
    {
        public FirebaseService()
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                string credentialPath = "./Controllers/service.json";
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(credentialPath)
                });
            }
        }

        // Create a new user and return their UID
        public async Task<string> CreateUserWithEmailAndPasswordAsync(string email, string password)
        {
            var userRecordArgs = new UserRecordArgs()
            {
                Email = email,
                Password = password,
                EmailVerified = false,
            };
            var userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(userRecordArgs);
            return userRecord.Uid;
        }

        // Sign in the user and return both the token and userId
        public async Task<(string Token, string UserId)> SignInWithEmailAndPasswordAsync(string email, string password)
        {
            var client = new HttpClient();
            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=AIzaSyCQ19rP56ZT6hBbJzZoBBi3G6wxEMauNrc";

            var response = await client.PostAsJsonAsync(url, new
            {
                email = email,
                password = password,
                returnSecureToken = true
            });

            if (!response.IsSuccessStatusCode)
                throw new System.Exception("Authentication failed. Please check email and password.");

            var authResult = await response.Content.ReadFromJsonAsync<FirebaseAuthResponse>();

            if (authResult == null || string.IsNullOrEmpty(authResult.IdToken) || string.IsNullOrEmpty(authResult.LocalId))
                throw new System.Exception("Failed to retrieve token or user ID.");

            return (authResult.IdToken, authResult.LocalId);
        }

        // Verify the Google ID token
        public async Task<FirebaseToken> VerifyGoogleIdTokenAsync(string idToken)
        {
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
            return decodedToken;
        }
    }

    // Firebase Authentication Response Model
    public class FirebaseAuthResponse
    {
        public string IdToken { get; set; }
        public string LocalId { get; set; }
    }
}
