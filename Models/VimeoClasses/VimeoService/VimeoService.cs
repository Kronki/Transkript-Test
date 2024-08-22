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
    }
}
