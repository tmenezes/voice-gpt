using NAudio.Wave;

namespace VoiceGpt.Core;

internal class Constants
{
    internal const string DefaultRegion = "westeurope";
    internal const string DefaultAudionFilename = "default-audio.wav";
    internal const string TtsAudioFileName = "tts-audio.wav";
    internal const int DefaultWaitTimeIntervalInMs = 250;

    internal const string BasePrompt    = "You're helping people on a voice chat. Please respond the request/question below concisely/direct to the point using a coloquial language and somehow thoughtful: \n";
    internal const string Gpt3ModelName = "text-davinci-003";
    internal const string OpenAiBaseUrl = "https://api.openai.com";

    internal static readonly WaveFormat WaveFormat = new WaveFormat(16000, 1);
}