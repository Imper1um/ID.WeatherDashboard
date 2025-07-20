using ID.WeatherDashboard.API.Config;
using ID.WeatherDashboard.API.Data;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using System.ClientModel;
using System.Text.Json;

namespace ID.WeatherDashboard.API.Services
{
    public class EncoderService(ILogger<EncoderService> logger, IConfigManager configManager) : IEncoderService
    {
        private readonly ILogger<EncoderService> Log = logger;
        private readonly IConfigManager ConfigManager = configManager;

        private static readonly string[] SupportedExtensions = [".jpg", ".jpeg", ".png"];
        private static readonly byte[] JpgHeader = [0xFF, 0xD8];
        private static readonly byte[] PngHeader = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

        private void ThrowIfNotProperImage(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException($"{nameof(path)} cannot be empty.", nameof(path));
            if (!File.Exists(path)) throw new FileNotFoundException($"{nameof(path)} is not found.", path);

            var ext = Path.GetExtension(path).ToLowerInvariant();

            if (!SupportedExtensions.Contains(ext))
                throw new NotSupportedException($"{ext} extension is not supported by the encoder.");

            var file = new FileInfo(path);
            if (file.Length < 8)
                throw new NotSupportedException($"{path} was too small to be an image file.");

            using (var fileRead = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                byte[] header = new byte[8];
                int read = fileRead.Read(header, 0, 8);
                Log.LogTrace("{Path} has the header bytes {HeaderBytes}", path, JsonSerializer.Serialize(header));
                if (header.AsSpan(0, 2).SequenceEqual(JpgHeader))
                {
                    Log.LogTrace("{Path} is a JPG.", path);
                    return;
                }
                if (header.AsSpan(0, 8).SequenceEqual(PngHeader))
                {
                    Log.LogTrace("{Path} is a PNG.", path);
                    return;
                }
            }
            throw new NotSupportedException($"{path} was not a valid supported file, or was encoded incorrectly.");
        }

        public async Task EncodeWeatherDataAsync(string path, ImageWeatherData data)
        {
            ThrowIfNotProperImage(path);
            try
            {
                using var image = await Image.LoadAsync(path);
                string jsonData = JsonSerializer.Serialize(data);

                var exif = image.Metadata.ExifProfile ?? new ExifProfile();
                exif.SetValue(ExifTag.UserComment, jsonData);
                image.Metadata.ExifProfile = exif;

                await image.SaveAsync(path);
            } 
            catch (Exception ex)
            {
                Log.LogError(ex, "Could not encode EXIF information: {ExceptionString}", ex.GetFullMessage());
                throw;
            }
        }

        private string GetMimeType(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
        }

        private const string SystemPrompt = "You are a weather image analyzer. You will be provided an image. This image should be of some kind of outdoor scene." +
            "You will read the image, its image file name, and determine the following properties:" +
            " - 'CloudCover': A percentage expressed as a decimal value from 0.000 to 1.000 which is the amount of sky the clouds are covering." +
            " - 'Rain': A percentage expressed as a decimal value from 0.000 to 1.000 on the severity of the rain (if any). Extreme Downpours (like Hurricanes) should be closer to 1.000." +
            " - 'Fog': A percentage expressed as a decimal value from 0.000 to 1.000 on the amount of fog in the scene." +
            " - 'Lightning': A percentage expressed as a decimal value from 0.000 to 1.000 on the severity of lighting (if any). A lot of lightning that covers the screen is 1.000, while far off, small lightning strikes in the background would be closer to 0.100." +
            " - 'Wind': A percentage expressed as a decimal value from 0.000 to 1.000 on the amount of wind (if any). Trees being blown over or fallen by the wind would be closer to 1.000, while a little bit of wafting wind will be closer to 0.100" +
            " - 'Extreme': A percentage expressed as a decimal value from 0.000 to 1.000 on the percentage likliness that the image shows extreme weather (tornado, hurricane, tropical storm, monsoon, etc.)" +
            " - 'Snow': A percentage expressed as a decimal value from 0.000 to 1.000 on the amount of snow in the scene. Extreme snow, such as a Blizzard would be closer to 1.000, while a light set of flurries with no ground accumulation is closer to 0.100" +
            " - 'MinTimeOfDay': A string TimeSpan in the format of HH:mm on the minimum time of day that you think this image depicts. Assume that Astronomical Twilight ends at 06:00, and begins at 20:00, and the day occurs between 09:00 and 18:00." +
            " - 'MaxTimeOfDay': A string TimeSpan in the format of HH:mm on the maximum time of day that you think this image depicts. Use the same rules for MinTimeOfDay. MaxTimeOfDay can be before MinTimeOfDay if you think the image occurs at night. For example, you could have MinTimeOfDay = 21:00 and MaxTimeOfDay = 05:00 to depict that you think the image occurs over the course of night time." +
            " - 'Description': A short and simple, one to two sentences on describing the image in terms of what weather it is." +
            "\r\nYou will compile this all into a JSON Object and output just that JSON Object. For example:" +
            "\r\n{\"CloudCover\":0.430,\"Rain\":0.552,\"Fog\":0.000,\"Lightning\":0.200,\"Wind\":0.411,\"Extreme\":0.020,\"Snow\":0.000,\"MinTimeOfDay\":\"08:00\",\"MaxTimeOfDay\":\"16:00\",\"Description\":\"Cloudy sky with some rain and lightning in the distance\"}" +
            "\r\nDo not output any justification or additional information, just provide the JSON Object.";

        public async Task<ImageWeatherData?> GenerateImageWeatherDataAsync(string path)
        {
            ThrowIfNotProperImage(path);

            var base64Image = await File.ReadAllBytesAsync(path);
            var encodedImage = Convert.ToBase64String(base64Image);
            var mimeType = GetMimeType(path);

            ChatClient client = new(
                model: ConfigManager.Config.ChatGpt.Model,
                credential: new ApiKeyCredential(ConfigManager.Config.ChatGpt.ApiKey)
            );
            var messages = new List<ChatMessage>()
            {
                ChatMessage.CreateSystemMessage(SystemPrompt),
                ChatMessage.CreateUserMessage(
                    ChatMessageContentPart.CreateTextPart($"Image File Name: {Path.GetFileName(path)}\r\nAnalyze the attached image and provide the JSON object."),
                    ChatMessageContentPart.CreateImagePart(new Uri($"data:{mimeType};base64,{encodedImage}"))
                )
            };
            bool properResponse = false;
            int apiCalls = 0;
            while (!properResponse && apiCalls < 3)
            {
                try
                {
                    var response = await client.CompleteChatAsync(messages.ToArray());
                    apiCalls++;
                    messages.Add(ChatMessage.CreateAssistantMessage(response.Value.Content));

                    var imageWeatherData = JsonSerializer.Deserialize<ImageWeatherData>(response.Value.Content[0].Text);
                    if (imageWeatherData != null)
                    {
                        return imageWeatherData;
                    }
                    messages.Add(ChatMessage.CreateUserMessage("You did not provide a response, or the response was malformed that it couldn't be parsed. Remember, *only* provide the JSON Object."));
                }
                catch (Exception ex)
                {
                    Log.LogWarning(ex, "{Exception} caused when attempting to ask ChatGPT for a response.{ExceptionInfo}", ex.GetType().Name, ex.GetFullMessage());
                    messages.Add(ChatMessage.CreateUserMessage($"Your response caused an {ex.GetType().Name} when attempting to parse it. Remember, only provide the JSON Object, with the listed properties, with nothing else."));
                }
            }
            Log.LogError("Attempted to ask ChatGPT for a proper response to parse the image, but it produced malformed output all three times.");
            return null;
        }

        public async Task<ImageWeatherData?> GetCurrentImageWeatherDataAsync(string path)
        {
            ThrowIfNotProperImage(path);

            try
            {
                using var image = await Image.LoadAsync(path);
                var exif = image.Metadata.ExifProfile;

                if (exif == null)
                {
                    Log.LogWarning("{Path} has no EXIF profile.", path);
                    return null;
                }

                if (!exif.TryGetValue(ExifTag.UserComment, out var userCommentValue))
                {
                    Log.LogWarning("{Path} does not have any user comment data.", path);
                    return null;
                }
                
                if (string.IsNullOrWhiteSpace(userCommentValue.Value.Text))
                {
                    Log.LogWarning("{Path} does not have parsable user comment data.", path);
                    return null;
                }

                try
                {
                    var data = JsonSerializer.Deserialize<ImageWeatherData>(userCommentValue.Value.Text);
                    if (data == null)
                    {
                        Log.LogWarning("{Path} contained invalid JSON data in UserComment.", path);
                        return null;
                    }
                    Log.LogTrace("{Path} loaded weather data: {WeatherData}", path, JsonSerializer.Serialize(data));
                    return data;
                }
                catch (JsonException jex)
                {
                    Log.LogWarning(jex, "{Path} contained invalid JSON in UserComment.", path);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log.LogError(ex, "Failed to read EXIF data from {Path}: {ExceptionMessage}", path, ex.GetFullMessage());
                throw;
            }
        }
    }
}
