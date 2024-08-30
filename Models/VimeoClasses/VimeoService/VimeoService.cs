using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace TranskriptTest.Models.VimeoClasses.VimeoService
{
    public class VimeoService
    {
        private readonly HttpClient _httpClient;
        private readonly string _accessToken;

        public VimeoService(HttpClient httpClient, IOptions<VimeoSettings> settings)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = Timeout.InfiniteTimeSpan; // Disable timeout at HttpClient level
            _accessToken = settings.Value.AccessToken;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        public async Task<string> UploadVideoAsync(Stream videoStream, long videoSize, string videoName)
        {
            // Step 1: Create an upload ticket
            var requestContent = new
            {
                upload = new { approach = "tus", size = videoSize },
                name = videoName
            };

            var createVideoResponse = await _httpClient.PostAsJsonAsync("https://api.vimeo.com/me/videos", requestContent);

            if (!createVideoResponse.IsSuccessStatusCode)
            {
                var errorContent = await createVideoResponse.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create upload ticket: {createVideoResponse.ReasonPhrase}. Response: {errorContent}");
            }

            var createVideoData = await createVideoResponse.Content.ReadAsStringAsync();
            using var jsonDocument = JsonDocument.Parse(createVideoData);
            var rootElement = jsonDocument.RootElement;
            var link = "";
            var uploadLink = "";
            var videoUri = "";
            if (rootElement.TryGetProperty("upload", out JsonElement uploadElement) && rootElement.TryGetProperty("link", out JsonElement linkElement) && rootElement.TryGetProperty("uri", out JsonElement uriElement))
            {
                link = linkElement.GetString();
                videoUri = uriElement.GetString();
                if(uploadElement.TryGetProperty("upload_link", out JsonElement uploadLinkElement))
                {
                    uploadLink = uploadLinkElement.GetString();
                }
            }
            else
            {
                throw new Exception("Failed to extract upload link and video URI from response.");
            }

            // Step 2: Upload the video stream using the upload link
            using var uploadRequest = new HttpRequestMessage(HttpMethod.Patch, uploadLink)
            {
                Content = new StreamContent(videoStream)
            };
            uploadRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");
            uploadRequest.Headers.Add("Tus-Resumable", "1.0.0");
            uploadRequest.Headers.Add("Upload-Offset", "0");
            uploadRequest.Headers.Add("Upload-Length", videoSize.ToString());
            uploadRequest.Headers.Add("Authorization", $"Bearer {_accessToken}");

            var uploadResponse = await _httpClient.SendAsync(uploadRequest, HttpCompletionOption.ResponseHeadersRead);
            if (!uploadResponse.IsSuccessStatusCode)
            {
                var errorContent = await uploadResponse.Content.ReadAsStringAsync();
                throw new Exception($"Failed to upload video: {uploadResponse.ReasonPhrase}. Response: {errorContent}");
            }

            // Step 3: Confirm the upload and get the video URI
            return videoUri;
        }

        public async Task<string> UploadVideoAsync(IFormFile file, string videoName)
        {
            var createVideoResponse = await CreateVideoResourceAsync(videoName, file.Length);
            var uploadLink = JsonSerializer.Deserialize<JsonElement>(createVideoResponse).GetProperty("upload").GetProperty("upload_link").GetString();
            var videoUri = JsonSerializer.Deserialize<JsonElement>(createVideoResponse).GetProperty("uri").GetString();

            await UploadVideoFileAsync(uploadLink, file);
            await VerifyUploadAsync(videoUri);

            return videoUri;
        }

        private async Task<string> CreateVideoResourceAsync(string videoName, long fileSize)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.vimeo.com/me/videos")
            {
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", _accessToken) },
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    //name = videoName,
                    upload = new
                    {
                        approach = "tus",
                        size = fileSize
                    }
                }), System.Text.Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error creating video resource: {responseContent}");
            }

            return responseContent;
        }

        private async Task UploadVideoFileAsync(string uploadLink, IFormFile file)
        {
            const int MaxRetries = 5;
            const int ChunkSize = 5 * 1024 * 1024; // Reduced to 1 MB chunks

            using var stream = file.OpenReadStream();
            var buffer = new byte[ChunkSize];
            long bytesRead = 0;
            int n;

            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                try
                {
                    while ((n = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        Console.WriteLine($"Uploading chunk: Offset {bytesRead}, Size {n}");

                        using var content = new ByteArrayContent(buffer, 0, n);
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/offset+octet-stream");
                        content.Headers.ContentLength = n;

                        var request = new HttpRequestMessage(HttpMethod.Patch, uploadLink)
                        {
                            Content = content
                        };

                        request.Headers.Add("Tus-Resumable", "1.0.0");
                        request.Headers.Add("Upload-Offset", bytesRead.ToString());
                        //if (bytesRead == 0)
                        //{
                        //    request.Headers.Add("Upload-Length", file.Length.ToString());
                        //}
                        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5)); // 5-minute timeout per chunk
                        var response = await _httpClient.SendAsync(request, cts.Token);
                        
                        response.EnsureSuccessStatusCode();

                        if (response.Headers.TryGetValues("Upload-Offset", out var offsetValues))
                        {
                            bytesRead = long.Parse(offsetValues.First());
                        }
                        else
                        {
                            bytesRead += n;
                        }

                        Console.WriteLine($"Chunk uploaded successfully. New offset: {bytesRead}");
                    }

                    Console.WriteLine("File upload completed successfully.");
                    return; // Upload completed successfully
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Attempt {attempt + 1} failed:");
                    Console.WriteLine($"Error: {ex.GetType().Name} - {ex.Message}");
                    if (ex is HttpRequestException httpEx)
                    {
                        Console.WriteLine($"Status code: {httpEx.StatusCode}");
                    }
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
                    }

                    if (attempt < MaxRetries - 1)
                    {
                        int delay = (int)Math.Pow(2, attempt) * 1000; // Exponential backoff
                        Console.WriteLine($"Retrying in {delay / 1000} seconds...");
                        await Task.Delay(delay);
                        stream.Position = bytesRead; // Reset stream position to last successful byte
                    }
                    else
                    {
                        throw new Exception("Max retries reached. Upload failed.", ex);
                    }
                }
            }
        }

        private async Task VerifyUploadAsync(string videoUri)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.vimeo.com{videoUri}")
            {
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", _accessToken) }
            };

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
    }
}
