using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TranskriptTest.Data;
using TranskriptTest.Models;
using TranskriptTest.Models.DTO;
using System.IO;
using System.Text;

namespace TranskriptTest.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly VideoDbContext _db;
        private readonly IWebHostEnvironment _env;

        public HomeController(ILogger<HomeController> logger, VideoDbContext db, IWebHostEnvironment env)
        {
            _logger = logger;
            _db = db;
            _env = env;
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
        public async Task<IActionResult> AddVideo(Request request)
        {
            if (request.MyFile == null || request.MyFile.Length == 0)
                return BadRequest("No file uploaded.");
            var lastVideoLocation = await _db.Videos.OrderBy(x => x.Id).LastOrDefaultAsync();
            var fileName = Path.GetFileName(request.MyFile.FileName);
            var filePath = Path.Combine(_env.WebRootPath, "Videos", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.MyFile.CopyToAsync(stream);
            }

            var video = new Video
            {
                Path = filePath,
                FileName = fileName,
            };

            _db.Videos.Add(video);
            await _db.SaveChangesAsync();
            return View("Videos");
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

        [HttpPost]
        public async Task<IActionResult> CreateSubtitle([FromBody] Subtitle subtitleDto)
        {
            var filePath = Path.Combine(_env.WebRootPath, "Subtitles", subtitleDto.FileName);
            var result = await CreateSubtitleFileAsync(filePath, subtitleDto.Content);
            return Ok(result);
        }

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
                Content = text,
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
        public IActionResult VideoFrame()
        {
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
        public IActionResult GetSubsFromDb(int textTrackId)
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
            return Ok(isSaved >= 1);
        }
    }
}
