using System;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Http;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebApplication1.Services
{
    public class PhotoUploadFTP
    {
        private readonly string ftpServer = "ftp://152.70.70.154/";
        private readonly string username = "fink";
        private readonly string password = "Theek@89";
        public string photoName;

        // Upload a photo to the FTP server
        public string? UploadPhoto(IFormFile photo)
        {
            try
            {
                // Create FTP request
                photoName = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
                var request = (FtpWebRequest)WebRequest.Create(new Uri($"{ftpServer}/{photoName}"));
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(username, password);

                // Get the photo stream from IFormFile and upload it
                using (var requestStream = request.GetRequestStream())
                {
                    using (var photoStream = photo.OpenReadStream())
                    {
                        photoStream.CopyTo(requestStream);
                    }
                }

                return photoName; // Photo uploaded successfully
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading photo: {ex.Message}");
                return null; // Failed to upload photo
            }
        }

        // Download a photo from the FTP server and return as IFormFile

        public string? DownloadPhoto(string photoName)
        {
            try
            {
                // Create FTP request to download file
                var request = (FtpWebRequest)WebRequest.Create(new Uri($"{ftpServer}/{photoName}"));
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(username, password);

                // Get response and download the image as a stream
                var response = (FtpWebResponse)request.GetResponse();
                using (var responseStream = response.GetResponseStream())
                {
                    // Convert the stream to a byte array
                    using (var memoryStream = new MemoryStream())
                    {
                        responseStream.CopyTo(memoryStream);
                        byte[] imageBytes = memoryStream.ToArray();

                        // Convert byte array to base64 string
                        return Convert.ToBase64String(imageBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading photo: {ex.Message}");
                return null; // Failed to download photo
            }
        }

        public bool DeletePhoto(string photoName)
        {
            try
            {
                // Construct the full FTP URI
                var uri = new Uri($"{ftpServer}/{photoName}");

                // Create FTP request
                var request = (FtpWebRequest)WebRequest.Create(uri);
                request.Method = WebRequestMethods.Ftp.DeleteFile;
                request.Credentials = new NetworkCredential(username, password);

                // Get response to ensure the operation completes
                using var response = (FtpWebResponse)request.GetResponse();
                Console.WriteLine($"Delete status: {response.StatusDescription}");

                return response.StatusCode == FtpStatusCode.FileActionOK;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting photo: {ex.Message}");
                return false; // Failed to delete photo
            }
        }

    }
}
