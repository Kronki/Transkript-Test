//namespace TranskriptTest.Models.Test
//{
//    using global::AudioVisualizer;
//    using NAudio.Dsp;
//    using NAudio.Wave;
//    using System.Drawing;
//    using System.Drawing.Drawing2D;
//    using System.Drawing.Imaging;
//    using Xabe.FFmpeg;
//#pragma warning disable CA1416
//    public class AudioVisualizer
//    {
//        private readonly Guid videoId;
//        private readonly string _tempPath;
//        private readonly string _title;
//        private readonly string _audioPath;
//        private readonly string _outputDirectory;
//        private readonly Settings _settings;
//        private readonly string? _logoPath;


//        public AudioVisualizer(string title, string audioPath, string rootDirectory, Settings settings, string? logoPath)
//        {
//            videoId = Guid.NewGuid();
//            _outputDirectory = Directory.CreateDirectory(Path.Combine(rootDirectory, videoId.ToString())).FullName;
//            _tempPath = Path.Combine(_outputDirectory, "temp");
//            _title = title;
//            _audioPath = audioPath;
//            _settings = settings;
//            _logoPath = logoPath;
//        }


//        public async Task<Guid> GenerateVideo()
//        {
//            var audioDuration = (await FFmpeg.GetMediaInfo(_audioPath)).Duration.TotalSeconds;
//            var samples = GetAudioSamples();
//            await CreateVisualizerFrames(audioDuration, samples.Item2, samples.Item1, _settings.Width, _settings.Height, _settings.Fps, 7, 500, 25, 70, 2f, 3, 50);
//            await EncodeVideo(_settings.Fps);

//            return videoId;
//        }

//        private Tuple<List<float>, int> GetAudioSamples()
//        {
//            var samples = new List<float>();
//            int sampleRate = 44100;

//            using (var reader = new AudioFileReader(_audioPath))
//            {
//                sampleRate = reader.WaveFormat.SampleRate;
//                float[] buffer = new float[reader.WaveFormat.SampleRate];
//                int read;
//                while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
//                {
//                    for (int i = 0; i < read; i++)
//                    {
//                        samples.Add(buffer[i]);
//                    }
//                }
//            }

//            return Tuple.Create(samples, sampleRate);
//        }

//        private void PerformFFT(float[] samples, int fftSize, out float[] fftResult)
//        {
//            var fftBuffer = new Complex[fftSize];
//            for (int i = 0; i < fftSize; i++)
//            {
//                fftBuffer[i] = new Complex { X = samples[i], Y = 0 };
//            }

//            FastFourierTransform.FFT(true, (int)Math.Log(fftSize, 2.0), fftBuffer);

//            fftResult = new float[fftSize / 2];
//            for (int i = 0; i < fftSize / 2; i++)
//            {
//                fftResult[i] = (float)Math.Sqrt(fftBuffer[i].X * fftBuffer[i].X + fftBuffer[i].Y * fftBuffer[i].Y);
//            }
//        }


//        private async Task CreateVisualizerFrames(double audioDuration, int sampleRate, List<float> samples, int width, int height, int fps, int barCount, int horizontalPadding, int spacerWidth, float scalingFactor, float amplitudeThreshold, int cornerRadius, float maxAmplitude)
//        {
//            var framesPath = Path.Combine(_tempPath, "frames");
//            int totalFrames = (int)(audioDuration * fps);
//            int fftSize = 1024;
//            int samplesPerFrame = sampleRate / fps;

//            Directory.CreateDirectory(framesPath);

//            float[] previousFrameMagnitudes = new float[barCount];
//            int visualizerHeight = height / 2;

//            // Use a thread pool to process frames concurrently
//            var tasks = new List<Task>();

//            for (int i = 0; i < totalFrames; i++)
//            {
//                if (i * samplesPerFrame + fftSize > samples.Count)
//                {
//                    break;
//                }

//                int frameIndex = i; // Capture the current value of i
//                tasks.Add(Task.Run(() =>
//                {
//                    float[] frameSamples = samples.Skip(frameIndex * samplesPerFrame).Take(fftSize).ToArray();
//                    PerformFFT(frameSamples, fftSize, out float[] fftResult);

//                    // Calculate current frame magnitudes
//                    float[] currentFrameMagnitudes = new float[barCount];
//                    for (int j = 0; j < barCount; j++)
//                    {
//                        int startBin = j * (fftResult.Length / barCount);
//                        int endBin = (j + 1) * (fftResult.Length / barCount);
//                        float averageMagnitude = fftResult.Skip(startBin).Take(endBin - startBin).Average();
//                        float amplitude = averageMagnitude * visualizerHeight / 2 * scalingFactor;

//                        if (amplitude < amplitudeThreshold)
//                        {
//                            amplitude = 0;
//                        }

//                        if (amplitude > maxAmplitude)
//                        {
//                            amplitude = maxAmplitude;
//                        }

//                        currentFrameMagnitudes[j] = amplitude;
//                    }

//                    var framePath = Path.Combine(framesPath, $"frame_{frameIndex:D6}.jpg");

//                    using (var bitmap = new Bitmap(width, height))
//                    using (var graphics = Graphics.FromImage(bitmap))
//                    {
//                        graphics.SmoothingMode = SmoothingMode.AntiAlias;
//                        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
//                        graphics.Clear(Color.FromArgb(35, 35, 35));

//                        if (_logoPath is not null)
//                        {
//                            var overlayImage = new Bitmap(_logoPath);
//                            graphics.DrawImage(overlayImage, new Rectangle((int)(width * .4f), (int)(height * .15f), (int)(width * .2f), (int)(height * .2f)), 0, 0, overlayImage.Width, overlayImage.Height, GraphicsUnit.Pixel);
//                        }

//                        Font drawFont = new Font("Lucida Sans", 30, GraphicsUnit.Pixel);
//                        SolidBrush drawBrush = new SolidBrush(Color.FromArgb(250, 250, 250));
//                        StringFormat stringFormat = new StringFormat
//                        {
//                            Alignment = StringAlignment.Center,
//                            LineAlignment = StringAlignment.Center,
//                        };
//                        graphics.DrawString(_title, drawFont, drawBrush, new Rectangle(0, 0, width, height), stringFormat);

//                        int visualizerTop = height - visualizerHeight;

//                        int totalPadding = 2 * horizontalPadding;
//                        int totalAvailableWidth = width - totalPadding;
//                        int totalBarWidth = (totalAvailableWidth - (barCount - 1) * spacerWidth) / barCount;

//                        int visualizerLeft = horizontalPadding;

//                        using (var brush = new SolidBrush(Color.FromArgb(240, 240, 240)))
//                        {
//                            for (int j = 0; j < barCount; j++)
//                            {
//                                int x = visualizerLeft + j * (totalBarWidth + spacerWidth);

//                                float interpolatedAmplitude = (previousFrameMagnitudes[j] + currentFrameMagnitudes[j]) / 2;

//                                if (interpolatedAmplitude > 0)
//                                {
//                                    using (GraphicsPath path = new GraphicsPath())
//                                    {
//                                        RectangleF rect = new RectangleF(x, visualizerTop + (visualizerHeight / 2 - interpolatedAmplitude), totalBarWidth - 1, interpolatedAmplitude * 2);
//                                        path.AddArc(rect.X, rect.Y, cornerRadius, cornerRadius, 180, 90);
//                                        path.AddArc(rect.X + rect.Width - cornerRadius, rect.Y, cornerRadius, cornerRadius, 270, 90);
//                                        path.AddArc(rect.X + rect.Width - cornerRadius, rect.Y + rect.Height - cornerRadius, cornerRadius, cornerRadius, 0, 90);
//                                        path.AddArc(rect.X, rect.Y + rect.Height - cornerRadius, cornerRadius, cornerRadius, 90, 90);
//                                        path.CloseFigure();

//                                        graphics.FillPath(brush, path);
//                                    }
//                                }
//                            }
//                        }

//                        bitmap.Save(framePath, ImageFormat.Jpeg); // Save as JPEG
//                    }

//                    previousFrameMagnitudes = currentFrameMagnitudes;
//                }));
//            }

//            await Task.WhenAll(tasks); // Wait for all frames to be processed
//        }



//        private async Task EncodeVideo(int fps)
//        {
//            string pattern = Path.Combine(_tempPath, "frames", "frame_%06d.jpg"); // Changed to .jpg
//            string videoWithoutAudio = Path.Combine(_tempPath, $"{videoId}_noaudio.mp4");
//            string finalVideo = Path.Combine(_outputDirectory, $"{videoId}.mp4");

//            // Ensure all paths are enclosed in double quotes
//            pattern = $"\"{pattern}\"";
//            videoWithoutAudio = $"\"{videoWithoutAudio}\"";
//            string audioPathQuoted = $"\"{_audioPath}\"";
//            string finalVideoQuoted = $"\"{finalVideo}\"";

//            var videoConversion = FFmpeg.Conversions.New()
//                .AddParameter($"-framerate {fps} -i {pattern} -pix_fmt yuv420p -r {fps}")
//                .AddParameter(videoWithoutAudio);

//            await videoConversion.Start();

//            var audioConversion = FFmpeg.Conversions.New()
//                .AddParameter($"-i {videoWithoutAudio}")
//                .AddParameter($"-i {audioPathQuoted}")
//                .AddParameter("-c:v copy")
//                .AddParameter("-c:a aac")
//                .AddParameter("-strict experimental")
//                .SetOutput(finalVideoQuoted);

//            await audioConversion.Start();

//            var tempDir = new DirectoryInfo(_tempPath);
//            tempDir.Delete(true);
//        }

//    }
//}
