using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    private static async Task Main(string[] args)
    {
        string apiKey = "";

        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("API key not found. Please set the OPENAI_API_KEY environment variable.");
            return;
        }

        if (args.Length == 0)
        {
            Console.WriteLine("Please provide the path to an audio file.");
            return;
        }

        string audioFilePath = args[0];

        if (!File.Exists(audioFilePath))
        {
            Console.WriteLine("The specified audio file does not exist.");
            return;
        }

        try
        {
            using HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            string transcription = await TranscribeAudioAsync(httpClient, audioFilePath);
            Console.WriteLine("Transcription:");
            Console.WriteLine(transcription);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    private static async Task<string> TranscribeAudioAsync(HttpClient httpClient, string audioFilePath)
    {
        using var content = new MultipartFormDataContent();
        
        byte[] audioBytes = await File.ReadAllBytesAsync(audioFilePath);
        var audioContent = new ByteArrayContent(audioBytes);
        audioContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/mpeg");
        content.Add(audioContent, "file", Path.GetFileName(audioFilePath));

        var response = await httpClient.PostAsync("https://api.openai.com/v1/audio/transcriptions", content);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to transcribe audio: {response.ReasonPhrase}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(jsonResponse);
        return doc.RootElement.GetProperty("text").GetString();
    }
}
