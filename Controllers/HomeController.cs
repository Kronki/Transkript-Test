using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TranskriptTest.Data;
using TranskriptTest.Models;
using TranskriptTest.Models.DTO;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using VimeoDotNet;
using Newtonsoft.Json;
using TranskriptTest.Models.VimeoClasses;
using VimeoDotNet.Net;
//using AudioVisualizer;
using Xabe.FFmpeg;
using TranskriptTest.Models.VimeoClasses.VimeoService;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using VimeoDotNet.Models;
using Newtonsoft.Json.Linq;

namespace TranskriptTest.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly VideoDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly VimeoService _vimeoService;
        private readonly HttpClient _httpClient;

        string accessToken = "0bdb22134b168497f1f3ba85fe2beab5";

        public HomeController(ILogger<HomeController> logger, VideoDbContext db, IWebHostEnvironment env, VimeoService vimeoService, HttpClient httpClient)
        {
            _logger = logger;
            _db = db;
            _env = env;
            _vimeoService = vimeoService;
            _httpClient = httpClient;
        }
        [HttpPost]
        public async Task<IActionResult> UploadToVimeo(IFormFile videoFile, string videoName)
        {
            if (videoFile == null || videoFile.Length == 0)
            {
                return BadRequest("No video file was uploaded.");
            }

            try
            {
                var videoUri = await _vimeoService.UploadVideoAsync(videoFile, videoName);
                // Upload the video directly from the stream
                //using var videoStream = videoFile.OpenReadStream();
                //var videoUri = await _vimeoService.UploadVideoAsync(videoStream, videoFile.Length, videoName);
                
                return Ok(new { videoUri });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }
        public async Task<IActionResult> Vimeo()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Vimeo(IFormFile file)
        {
            var uploadStatus = "";
            try
            {
                VimeoClient vimeoClient = new VimeoClient(accessToken);
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await httpClient.GetAsync("https://api.vimeo.com/me");
                var jsonString = await response.Content.ReadAsStringAsync();

                var guidId = Guid.NewGuid().ToString();
                var fileName = "AccountInfo" + guidId + ".txt";
                var filePath = Path.Combine(_env.WebRootPath, "VimeoInfo", fileName);
                //await System.IO.File.WriteAllTextAsync(filePath, jsonString);

                var accountInfo = JsonConvert.DeserializeObject<CustomVimeoUser>(jsonString);

                if(accountInfo.Name != null)
                {
                    IUploadRequest uploadRequest = new UploadRequest();
                    BinaryContent binaryContent = new BinaryContent(file.OpenReadStream(), file.ContentType);
                    var chunkSize = 0L;
                    var temp1 = file.Length / 1024;

                    if(temp1 > 1)
                    {
                        chunkSize = temp1 / 1024;
                        chunkSize *= 1048576;
                    }
                    else
                    {
                        chunkSize = 1048576;//chunk size equal to 1MB
                    }

                    uploadRequest = await vimeoClient.UploadEntireFileAsync(binaryContent, chunkSize, null);
                    uploadStatus = string.Concat("File uploaded ", "https://vimeo.com/", uploadRequest.ClipId.Value.ToString(), "/none");
                }
                

                
                var authCheck = await vimeoClient.GetAccountInformationAsync();
            }
            catch(Exception ex)
            {
                uploadStatus = "not uploaded: " + ex.Message;
            }

            return View();
        }

        public async Task<IActionResult> Index()
        {
            var videos = await _db.Videos.ToListAsync();
            var selectListVideos = new SelectList(videos, "Id", "FileName");
            ViewData["SelectVideos"] = selectListVideos;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpPost]
        public async Task<IActionResult> AddVideo(IFormFile file, long start, long end, int chunkNumber, int totalChunks)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);
            var tempFilePath = Path.Combine(Path.GetTempPath(), fileNameWithoutExtension + ".mp4");

            // Append the uploaded chunk to the temporary file
            using (var stream = new FileStream(tempFilePath, FileMode.Append, FileAccess.Write))
            {
                await file.CopyToAsync(stream);
            }

            // Check if all chunks have been uploaded
            if (chunkNumber == totalChunks - 1)
            {
                // Start background task for MP3 conversion
                _ = Task.Run(() => ConvertToMp3(tempFilePath));

                return Ok("File uploaded and assembled successfully. Conversion to MP3 started.");
            }

            return Ok();
        }


        private async Task ConvertToMp3(string tempFilePath)
        {
            var outputMp3Path = Path.Combine(Path.GetTempPath(), "output.mp3");

            try
            {
                // Convert the file to MP3 with a lower bitrate for faster processing
                var conversion = await FFmpeg.Conversions.New()
                    .AddParameter($"-i \"{tempFilePath}\" -vn -ar 44100 -ac 2 -b:a 128k \"{outputMp3Path}\"")
                    .Start();

                var guidId = Guid.NewGuid().ToString();

                // Optionally handle the MP3 file (e.g., move it to a permanent location, notify the user, etc.)
                var permanentMp3Path = Path.Combine(_env.WebRootPath, "Audios", "convertedAudio" + guidId + ".mp3");

                if (System.IO.File.Exists(outputMp3Path))
                {
                    System.IO.File.Move(outputMp3Path, permanentMp3Path);
                }
            }
            catch (Exception ex)
            {
                // Handle conversion errors
                // Log the error or take appropriate actions
                Console.WriteLine($"Error during MP3 conversion: {ex.Message}");
            }
            finally
            {
                // Clean up the temporary chunk file
                if (System.IO.File.Exists(tempFilePath))
                {
                    System.IO.File.Delete(tempFilePath);
                }
            }
        }




        private async Task ExtractAudioFromFileAsync(string inputVideoPath, string outputAudioPath)
        {
            var conversion = await FFmpeg.Conversions.FromSnippet.ExtractAudio(inputVideoPath, outputAudioPath);
            await conversion.Start();
        }
        [HttpPost]
        public async Task<IActionResult> AddSubtitle(Request request)
        {
            if (request.MyFile == null || request.MyFile.Length == 0)
                return BadRequest("No file uploaded.");
            var lastSubtitleLocation = await _db.Subtitles.OrderBy(x => x.Id).LastOrDefaultAsync();
            var fileName = Path.GetFileName(request.MyFile.FileName);
            var filePath = Path.Combine(_env.WebRootPath, "Subtitles", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.MyFile.CopyToAsync(stream);
            }

            var subtitle = new Subtitle
            {
                Path = filePath,
                FileName = fileName,
                VideoId = request.VideoId!.Value,
            };

            _db.Subtitles.Add(subtitle);
            await _db.SaveChangesAsync();
            return View("Videos");
        }
        [HttpGet]
        public async Task<IActionResult> Videos()
        {
            var videos = await _db.Videos.ToListAsync();
            return View(videos);
        }
        [HttpGet]
        public async Task<IActionResult> Video(int id)
        {
            var video = await _db.Videos.Where(x=>x.Id == id).Include(x=>x.Subtitles).FirstOrDefaultAsync();
            return View(video);
        }

        [HttpGet("{fileName}")]
        public async Task<IActionResult> GetSubtitle(string fileName)
        {
            var filePath = Path.Combine(_env.WebRootPath, "Subtitles", fileName);
            try
            {
                var content = await ReadSubtitleFileAsync(filePath);
                return Ok(content);
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> CreateSubtitle([FromBody] Subtitle subtitleDto)
        //{
        //    var filePath = Path.Combine(_env.WebRootPath, "Subtitles", subtitleDto.FileName);
        //    var result = await CreateSubtitleFileAsync(filePath, subtitleDto.Content);
        //    return Ok(result);
        //}

        public async Task<string> ReadSubtitleFileAsync(string filePath)
        {
            if (System.IO.File.Exists(filePath))
                return await System.IO.File.ReadAllTextAsync(filePath);
            else
                throw new FileNotFoundException("Subtitle file not found.");
        }
        public async Task<string> CreateSubtitleFileAsync(string filePath, string content)
        {
            await System.IO.File.WriteAllTextAsync(filePath, content);
            return "Subtitle file created successfully.";
        }
        [HttpPost]
        public async Task<IActionResult> AddSub(string text, int videoId)
        {
            var video = await _db.Videos.FirstOrDefaultAsync(x => x.Id == videoId);
            var guidId = Guid.NewGuid().ToString();
            var fileName = $"Sub{guidId}.vtt";
            var filePath = Path.Combine(_env.WebRootPath, "Subtitles", fileName);
            var isCreated = await CreateSubtitleFileAsync(filePath, text);
            var subtitle = new Subtitle
            {
                Path = filePath,
                //Content = text,
                FileName = fileName,
                Video = video,
            };
            _db.Subtitles.Add(subtitle);
            await _db.SaveChangesAsync();
            return Ok(isCreated);
        }
        [HttpGet("Subtitles/{fileName}")]
        public async Task<IActionResult> GetSub(string fileName)
        {
            var filePath = Path.Combine(_env.WebRootPath, "Subtitles", fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileContent = await System.IO.File.ReadAllTextAsync(filePath);

            //fileContent = "WEBVTT\n\n" + fileContent;

            // If file is .txt, convert to WebVTT format
            if (fileName.EndsWith(".txt"))
            {
                fileContent = "WEBVTT\n\n" + fileContent;
            }
            var test = Content(fileContent, "text/vtt", Encoding.UTF8);
            return Content(fileContent, "text/vtt", Encoding.UTF8);
        }
        public async Task<IActionResult> VideoFrame()
        {
            try
            {
                //var audio = new Models.Test.AudioVisualizer("Titulli i intervistes", @"C:\Users\Pulse Electronics\Desktop\audioTest\audioFiles\audio.mp3", @"C:\Users\Pulse Electronics\Desktop\audioTest\audiosToVideos", new Settings(), @"C:\Users\Pulse Electronics\Desktop\audioTest\audioLogos\logo.png");
                //var ffmpegPath = @"C:\Users\Pulse Electronics\Desktop\ffmpeg-7.0.2-full_build\bin";
                //var currentPath = Environment.GetEnvironmentVariable("PATH");
                //if (!currentPath.Split(';').Contains(ffmpegPath, StringComparer.OrdinalIgnoreCase))
                //{
                //    // Append FFmpeg path to PATH environment variable if it's not already present
                //    Environment.SetEnvironmentVariable("PATH", currentPath + ";" + ffmpegPath);
                //}
                //var test = await audio.GenerateVideo();
            }
            catch(Exception ex)
            {

            }
            return View();
        }
        public IActionResult SaveTextTrackToDb(string content, string link, string uri, int id)
        {
            var subtitleRequest = new SubtitleRequest
            {
                TextTrackContent = content,
                TextTrackUri = uri,
                TextTrackId = id,
            };
            _db.SubtitleRequests.Add(subtitleRequest);
            var isSaved = _db.SaveChanges();
            return Ok(isSaved);
        }
        public IActionResult EditSubtitles()
        {
            var subtitleRequest = _db.SubtitleRequests.ToList();
            var selectList = new SelectList(subtitleRequest, "TextTrackId", "TextTrackId");
            ViewData["Subtitles"] = selectList;
            return View();
        }
        public IActionResult GetVideoSubsFromDb(int textTrackId)
        {
            var subtitleRequest = _db.SubtitleRequests.FirstOrDefault(x => x.TextTrackId == textTrackId);
            if (subtitleRequest == null)
                return Ok("");
            return Ok(subtitleRequest.TextTrackContent);
        }
        public IActionResult EditSubs(int trackId, string content)
        {
            var subtitleRequest = _db.SubtitleRequests.FirstOrDefault(x => x.TextTrackId == trackId);
            if (subtitleRequest == null)
                return Ok(false);
            subtitleRequest.TextTrackContent = content;
            _db.SubtitleRequests.Update(subtitleRequest);
            var isSaved = _db.SaveChanges();
            return Ok(new { IsSaved = isSaved >= 1, TrackURI = subtitleRequest.TextTrackUri });

        }
        public IActionResult EditAudio()
        {
            return View();
        }
        public IActionResult CreateAudio()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateAudio([FromForm]AudioFileRequest request)
        {
            if(request.File != null)
            {
                if (request.File.ContentType.Contains("audio"))
                {
                    var guidId = Guid.NewGuid().ToString();
                    string wwwRootPath = _env.WebRootPath;
                    string pathOfFolder = Path.Combine(wwwRootPath, "/Audios/");
                    if (!Directory.Exists(pathOfFolder))
                    {
                        Directory.CreateDirectory(pathOfFolder);
                    }
                    string fileName = Path.GetFileNameWithoutExtension(request.File.FileName);
                    string extension = Path.GetExtension(request.File.FileName);
                    var audioFile = new AudioFile();
                    audioFile.FilePath = fileName + guidId + extension;
                    audioFile.FileSize = request.File.Length;
                    string path = Path.Combine(wwwRootPath + "/Audios/", audioFile.FilePath);
                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await request.File.CopyToAsync(fileStream);
                    }
                    _db.AudioFiles.Add(audioFile);
                    var isSaved = await _db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Index");
        }
        public IActionResult EditAudios()
        {
            var audios = _db.AudioFiles.Include(x=>x.Subtitle).ToList();
            var audiosAndSubtitles = new List<AudioFileAndSubtitle>();
            foreach (var audioFile in audios)
            {
                var audioAndSubtitle = new AudioFileAndSubtitle
                {
                    Id = audioFile.Id,
                    FilePath = audioFile.FilePath,
                };
                foreach(var subtitle in audioFile.Subtitle)
                {
                    var subtitleDTO = new SubtitleDTO()
                    {
                        Path = subtitle.Path,
                        FileName = subtitle.FileName,
                        //Content = subtitle.Content,
                        Language = subtitle.Language,
                    };
                    audioAndSubtitle.Subtitles.Add(subtitleDTO);
                }
                audiosAndSubtitles.Add(audioAndSubtitle);
            }
            return View(audiosAndSubtitles);
        }
        public IActionResult GetSubsFromDb(int audioId)
        {
            var audio = _db.AudioFiles.Include(x=>x.Subtitle).Where(x=>x.Id == audioId).FirstOrDefault();
            return Ok(audio.Subtitle);
        }
        [HttpPost]
        public IActionResult VideoFrame([FromForm] SubtitleFormDTO model, [FromQuery]int videoId)
        {
            for(int i = 0; i < model.FirstTimes.Count; i++)
            {
                var transcript = new Transcript()
                {
                    StartTime = model.FirstTimes[i],
                    EndTime = model.SecondTimes[i],
                    Text = model.Texts[i],
                    VideoId = videoId,
                };
                _db.Transcripts.Add(transcript);
            }
            var isSaved = _db.SaveChanges();
            return Ok(isSaved >= 1);
        }
        public async Task<IActionResult> EditVideoFrame(int videoId)
        {
            var transcripts = _db.Transcripts.Where(x => x.VideoId == videoId).ToList();
            return View(transcripts);
        }
        [HttpPost]
        public async Task<IActionResult> AddTextTrack([FromBody] Request2 request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Text) || string.IsNullOrEmpty(request.VideoId))
                {
                    return BadRequest("Subtitle text or Video ID is missing.");
                }

                // Step 2: Get the upload link for the text track
                var createTrackContent = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(new
                    {
                        type = "subtitles",
                        language = "en",
                        name = request.Name
                    }),
                    Encoding.UTF8,
                    "application/json"
                );

                var createTrackRequest = new HttpRequestMessage(HttpMethod.Post, $"https://api.vimeo.com/videos/{request.VideoId}/texttracks")
                {
                    Headers =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", "0bdb22134b168497f1f3ba85fe2beab5"),
                },
                    Content = createTrackContent
                };

                var createTrackResponse = await _httpClient.SendAsync(createTrackRequest);
                if (!createTrackResponse.IsSuccessStatusCode)
                {
                    var error = await createTrackResponse.Content.ReadAsStringAsync();
                    return StatusCode((int)createTrackResponse.StatusCode, error);
                }

                var createTrackResult = await createTrackResponse.Content.ReadAsStringAsync();
                var uploadLink = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(createTrackResult).GetProperty("link").GetString();

                // Step 3: Upload the text track
                var uploadContent = new StringContent(request.Text, Encoding.UTF8, "text/vtt");
                var uploadRequest = new HttpRequestMessage(HttpMethod.Put, uploadLink)
                {
                    Content = uploadContent
                };

                uploadRequest.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/vnd.vimeo.*+json;version=3.4"));

                var uploadResponse = await _httpClient.SendAsync(uploadRequest);
                if (uploadResponse.IsSuccessStatusCode)
                {
                    var result = await uploadResponse.Content.ReadAsStringAsync();
                    return Ok(result);
                }

                var uploadError = await uploadResponse.Content.ReadAsStringAsync();
                return StatusCode((int)uploadResponse.StatusCode, uploadError);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> EditTextTrack([FromBody] EditTextTrackRequest request)
        {

            var getTextTrackUri = new HttpRequestMessage(HttpMethod.Get, $"https://api.vimeo.com/videos/{request.VideoId}/texttracks")
            {
                Headers =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", request.AccessToken),
                }
            };

            //getTextTrackUri.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/vnd.vimeo.*+json;version=3.4"));

            var getTextTrackResponse = await _httpClient.SendAsync(getTextTrackUri);

            if(!getTextTrackResponse.IsSuccessStatusCode)
            {
                var error = await getTextTrackResponse.Content.ReadAsStringAsync();
                return StatusCode((int)getTextTrackResponse.StatusCode, error);
            }

            var textTrackResult = await getTextTrackResponse.Content.ReadAsStringAsync();

            var textTrackResultJson = JObject.Parse(textTrackResult);
            string trackId = "";

            var activeTextTrack = textTrackResultJson["data"]
                .FirstOrDefault(track => (bool)track["active"] && (string)track["language"] == request.Language);
            if (activeTextTrack != null) 
            {
                trackId = activeTextTrack["id"].ToString();
            }

            // First, get the upload link
            var getUploadLinkRequest = new HttpRequestMessage(HttpMethod.Get, $"https://api.vimeo.com/videos/{request.VideoId}/texttracks/{trackId}")
            {
                Headers =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", request.AccessToken),
                }
            };

            var getLinkResponse = await _httpClient.SendAsync(getUploadLinkRequest);
            if (!getLinkResponse.IsSuccessStatusCode)
            {
                var error = await getLinkResponse.Content.ReadAsStringAsync();
                return StatusCode((int)getLinkResponse.StatusCode, error);
            }

            var linkResult = await getLinkResponse.Content.ReadAsStringAsync();
            var uploadLink = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(linkResult).GetProperty("link").GetString();

            // Now, upload the new content
            var uploadContent = new StringContent(request.NewContent, Encoding.UTF8, "text/vtt");
            var uploadRequest = new HttpRequestMessage(HttpMethod.Put, uploadLink)
            {
                Content = uploadContent
            };
            uploadRequest.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/vnd.vimeo.*+json;version=3.4"));

            var uploadResponse = await _httpClient.SendAsync(uploadRequest);
            if (uploadResponse.IsSuccessStatusCode)
            {
                var result = await uploadResponse.Content.ReadAsStringAsync();
                return Ok(result);
            }

            var uploadError = await uploadResponse.Content.ReadAsStringAsync();
            return StatusCode((int)uploadResponse.StatusCode, uploadError);
        }
    }
}
