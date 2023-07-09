using System.Text;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace VoiceGpt.Core
{
    public interface ISpeechToTextService
    {
        Task<string> Run(string audioFile);
    }

    public class SpeechToTextService : ISpeechToTextService
    {
        private readonly SpeechTextServiceConfiguration _configuration;
        private readonly bool _debugMode;

        public SpeechToTextService(SpeechTextServiceConfiguration configuration, bool debugMode = false)
        {
            _configuration = configuration;
            _debugMode = debugMode;
        }

        public async Task<string> Run(string audioFile)
        {
            var config = SpeechConfig.FromSubscription(_configuration.Subscription, _configuration.Region);

            using var audioInput = AudioConfig.FromWavFileInput(audioFile);
            using var recognizer = new SpeechRecognizer(config, audioInput);
            var (text, finished) = (new StringBuilder(), false);

            // Subscribe to events
            if (_debugMode)
            {
                recognizer.Recognizing += (s, e) => Console.WriteLine($"\nRecognizing... {e.Result.Text} ({e.Result.Duration}/{e.Result.OffsetInTicks})");
                recognizer.Canceled += (s, e) => Console.WriteLine($"Canceled: {e.Reason}");
                recognizer.SessionStarted += (s, e) => Console.WriteLine("Session started");
            }
            recognizer.Recognized += (s, e) => text = text.Append(e.Result.Text).Append(" ");
            recognizer.SessionStopped += (s, e) => finished = true;

            await recognizer.StartContinuousRecognitionAsync();
            while (finished is false)
            {
                Thread.Sleep(Constants.DefaultWaitTimeIntervalInMs);
            }

            if (_debugMode) Console.WriteLine($"RECOGNIZED: {text}");
            return text.ToString().Trim();

            return null;
        }
    }
}
