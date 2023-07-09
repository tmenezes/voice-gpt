using Microsoft.CognitiveServices.Speech;

namespace VoiceGpt.Core
{
    public interface ITextToSpeechService
    {
        Task<MemoryStream> Run(string text);
    }

    public class TextToSpeechService : ITextToSpeechService
    {
        private readonly SpeechTextServiceConfiguration _configuration;

        public TextToSpeechService(SpeechTextServiceConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<MemoryStream> Run(string text)
        {
            // Create a configuration object
            var config = SpeechConfig.FromSubscription(_configuration.Subscription, _configuration.Region);

            // Create a speech synthesizer
            using var synthesizer = new SpeechSynthesizer(config, null);

            // Get the audio stream
            var result = await synthesizer.SpeakTextAsync(text);
            var stream = new MemoryStream(result.AudioData);

            return stream;

        }
    }
}
