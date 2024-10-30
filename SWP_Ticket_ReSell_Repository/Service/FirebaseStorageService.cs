using Firebase.Auth;
using Firebase.Storage;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

public class FirebaseStorageService
{
    private readonly IConfiguration _configuration;

    public FirebaseStorageService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
    {
        // Lấy thông tin cấu hình Firebase
        string apiKey = _configuration["FireBase:FirebaseApiKey"];
        string bucket = _configuration["FireBase:FirebaseBucket"];
        string authEmail = _configuration["FireBase:FirebaseAuthEmail"];
        string authPassword = _configuration["FireBase:FirebaseAuthPassword"];

        try
        {
            // Xác thực với Firebase
            var auth = new FirebaseAuthProvider(new FirebaseConfig(apiKey));
            var authResult = await auth.SignInWithEmailAndPasswordAsync(authEmail, authPassword);

            // Upload file lên Firebase
            var firebaseStorage = new FirebaseStorage(bucket, new FirebaseStorageOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(authResult.FirebaseToken),
                ThrowOnCancel = true
            });

            var uploadTask = firebaseStorage
                .Child("customers") 
                .Child($"{Guid.NewGuid()}_{fileName}")
                .PutAsync(fileStream);

            // Trả về URL ảnh 
            return await uploadTask;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            throw new Exception("Error uploading file to Firebase: " + ex.Message);
        }
    }
}


