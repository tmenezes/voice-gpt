using SimpleGPT.Core;
using VoiceGpt.Core;

namespace Speech2Text
{
    internal class Program
    {
        private const string AZURE_REGION = "westeurope";
        private const string REC_AUDIO_WAV = "rec-audio.wav";
        private const string TTS_AUDIO_WAV = "tts-audio.wav";

        private const string AZURE_SUBSCRIPTION_KEY = "[azure subscription key here]";
        private const string GPT_API_KEY            = "[openAI api key here]";

        private static readonly Random _random = new Random(DateTime.Now.Ticks.GetHashCode());

        private static readonly IAudioManager _audioManager = new AudioManager();

        static async Task Main(string[] args)
        {
            await ChatUsingChatGtp(useMicAsInput: true);
        }

        // chat gpt
        private static async Task ChatUsingChatGtp(bool useMicAsInput = false)
        {
            // Create a client for making GPT calls
            var gptClient = new GptClient(GPT_API_KEY);

            // menu
            Console.WriteLine("W E L C O M E  T O  V O I C E  G P T");
            Console.WriteLine("--------------------------------------------------");
            if (useMicAsInput)
            {
                Console.WriteLine("Info: press enter to start mic capture...");
                Console.WriteLine("      press enter again to finish capture.");
                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine();
            }

            // Start the conversation loop
            while (true)
            {
                if (useMicAsInput)
                {
                    Console.Write("Press any key chat...");
                    Console.ReadKey();
                }

                // Get user input
                var userInput = useMicAsInput
                    ? (await CaptureTextFromMicrophone()).text
                    : Console.ReadLine();

                if (string.IsNullOrWhiteSpace(userInput)) break;
                Console.WriteLine("\nYou: " + userInput);

                // Make GPT call 
                var gptResponse = await gptClient.CallGpt(userInput);

                var gptAnswerAudio = await CreateSpeechAudio(gptResponse);
                PlayAudio(gptAnswerAudio);

                WriteChatAnswerAnimated(gptResponse);
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine(gptClient.Conversation);
        }
        private static void WriteChatAnswerAnimated(string text)
        {
            var punctuationChars = new[] { '.', '!', '?', ':', ';' };

            Console.WriteLine();
            Console.Write("Chatbot: ");
            for (int i = 0; i < text.Length; i++)
            {
                Console.Write(text[i]);
                Thread.Sleep(_random.Next(25, 50));

                var isPunctuation = punctuationChars.Contains(text[i]);
                var isShortPause = text[i] == ' ';
                
                if (isPunctuation) Thread.Sleep(100);
                if (isShortPause) Thread.Sleep(50);
            }
            Console.WriteLine();
        }

        // recognition
        private static async Task<(string? text, MemoryStream recAudio)> CaptureTextFromMicrophone(bool debugMode = false)
        {
            var recAudio = RecordAudio(debugMode);
            _audioManager.SaveOnDisk(recAudio, REC_AUDIO_WAV);

            var speech2TextService = new SpeechToTextService(new SpeechTextServiceConfiguration(AZURE_SUBSCRIPTION_KEY, AZURE_REGION), debugMode);
            var text  = await speech2TextService.Run(REC_AUDIO_WAV);

            return (text, recAudio);
        }
        private static MemoryStream RecordAudio(bool debug = false)
        {
            if (debug)
            {
                Console.WriteLine("Recording started...");
                Console.WriteLine("Press any key to stop microphone capture...");
            }

            var recording = true;
            var micService = new MicrophoneAudioService();
            
            micService.StartCapture(data =>
            {
                if (recording) ShowAudioLevelMeter(data.buffer);
            });

            Console.ReadKey();
            var audioStream = micService.StopCapture();
            recording = false;

            if (debug) Console.WriteLine("Recording stopped.");

            audioStream.Position = 0;
            return audioStream;
        }

        private static async Task<MemoryStream> CreateSpeechAudio(string text)
        {
            var text2SpeechService = new TextToSpeechService(new SpeechTextServiceConfiguration(AZURE_SUBSCRIPTION_KEY, AZURE_REGION));
            var audioStream = await text2SpeechService.Run(text);

            _audioManager.SaveOnDisk(audioStream, TTS_AUDIO_WAV);

            return audioStream;
        }

        private static void PlayAudio(MemoryStream audioStream, bool debugMode = false)
        {
            if (debugMode) Console.WriteLine("\nPlaying audio...");

            _audioManager.Play(audioStream);
        }
        
        private static void ShowAudioLevelMeter(byte[] buffer)
        {
            // copy buffer into an array of integers
            var values = new Int16[buffer.Length / 2];
            Buffer.BlockCopy(buffer, 0, values, 0, buffer.Length);

            // determine the highest value as a fraction of the maximum possible value
            var fraction = (float)values.Max() / 32768;

            // print a level meter using the console
            var bar = new string('#', (int)(fraction * 70));
            var meter = "[" + bar.PadRight(60, '-') + "]";
            Console.CursorLeft = 0;
            Console.CursorVisible = false;
            Console.Write($"{meter} {fraction * 100:00.0}%");
        }
    }
}