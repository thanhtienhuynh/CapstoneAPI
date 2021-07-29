using Firebase.Auth;
using Firebase.Storage;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CapstoneAPI.Helpers
{
    public class FirebaseHelper
    {
        private static readonly ILogger _log = Log.ForContext<FirebaseHelper>();
        public static async Task<string> UploadBase64ImgToFirebase(string content)
        {
            string result = content;
            if (result != null && result.Contains("base64,/"))
            {
                var images = Regex.Matches(result, "data:image/[a-z]+;base64,/(.*?)\"");
                foreach (var image in images)
                {
                    string base64String = image.ToString().Split("base64,")[1].Replace("\"", "");

                    string link = "";
                    var auth = new FirebaseAuthProvider(new FirebaseConfig(Consts.API_KEY));
                    var firebaseAuth = await auth.SignInWithEmailAndPasswordAsync(Consts.AUTH_MAIL, Consts.AUTH_PASSWORD);
                    var bytes = Convert.FromBase64String(base64String);
                    MemoryStream ms = new MemoryStream(bytes);
                    ms.Position = 0;
                    if (ms != null && ms.Length > 0)
                    {
                        // you can use CancellationTokenSource to cancel the upload midway
                        var cancellation = new CancellationTokenSource();

                        var task = new FirebaseStorage(
                            Consts.BUCKET,
                            new FirebaseStorageOptions
                            {
                                ThrowOnCancel = true, // when you cancel the upload, exception is thrown. By default no exception is thrown
                                AuthTokenAsyncFactory = () => Task.FromResult(firebaseAuth.FirebaseToken),
                            })
                            .Child(Consts.QUESTION_FOLDER)
                            .Child(DateTime.UtcNow.ToString("yyyyMMddHHmmssFFF"))
                            .PutAsync(ms, cancellation.Token);

                        try
                        {
                            link = await task;
                        }
                        catch (Exception ex)
                        {
                            _log.Error(ex.ToString());
                            return content;
                        }
                    }
                    result = result.Replace(image.ToString(), link + "\"");
                }
            }

            return result;
        }


        public static async Task<string> uploadImageLinkToFirebase(String content)
        {
            string result = content;
            WebClient client = new WebClient();
            if (result != null && result.Contains("images.tuyensinh247"))
            {
                var images = Regex.Matches(result, "https://images(.*?)(?=\")");
                foreach (var image in images)
                {
                    var auth = new FirebaseAuthProvider(new FirebaseConfig(Consts.API_KEY));
                    var firebaseAuth = await auth.SignInWithEmailAndPasswordAsync(Consts.AUTH_MAIL, Consts.AUTH_PASSWORD);
                    string link = "";
                    string name = image.ToString().Substring(image.ToString().LastIndexOf("/")+1);

                    Stream stream = client.OpenRead(image.ToString());

                    MemoryStream ms = new MemoryStream();

                    byte[] buffer = new byte[8192];
                    int bytesRead;
                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, bytesRead);
                    }
                    // Rewind ready for reading (typical scenario)
                    ms.Position = 0;
                    if (ms != null && ms.Length > 0)
                    {
                        // you can use CancellationTokenSource to cancel the upload midway
                        var cancellation = new CancellationTokenSource();

                        var task = new FirebaseStorage(
                            Consts.BUCKET,
                            new FirebaseStorageOptions
                            {
                                ThrowOnCancel = true, // when you cancel the upload, exception is thrown. By default no exception is thrown
                                AuthTokenAsyncFactory = () => Task.FromResult(firebaseAuth.FirebaseToken),
                            })
                            .Child(Consts.QUESTION_FOLDER)
                            .Child(name)
                            .PutAsync(ms, cancellation.Token);

                        try
                        {
                            link = await task;
                        }
                        catch (Exception ex)
                        {
                            _log.Error(ex.ToString());
                            return content;
                        }
                    }
                    result = result.Replace(image.ToString(), link);
                }
            }

            return result;
        }
    }
}
