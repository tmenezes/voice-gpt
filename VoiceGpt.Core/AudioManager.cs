using NAudio.Wave;

namespace VoiceGpt.Core
{
    public interface IAudioManager
    {
        bool IsPlaying { get; }
        void Play(MemoryStream audio, bool waitAudioStop = false);

        void SaveOnDisk(MemoryStream audio, string? filename);
    }

    public class AudioManager : IAudioManager
    {
        private readonly WaveOutEvent _waveOut = new();
        
        public bool IsPlaying => _waveOut.PlaybackState == PlaybackState.Playing;

        public void Play(MemoryStream audio, bool waitAudioStop = false)
        {
            audio.Position = 0;
            var waveProvider = new RawSourceWaveStream(audio, Constants.WaveFormat);
            _waveOut.Init(waveProvider);
            _waveOut.Play();
            
            while (waitAudioStop && IsPlaying)
            {
                Thread.Sleep(Constants.DefaultWaitTimeIntervalInMs);
            }
        }

        public void SaveOnDisk(MemoryStream audio, string? filename)
        {
            audio.Position = 0;
            using WaveFileWriter writer = new WaveFileWriter(filename ?? Constants.DefaultAudionFilename, Constants.WaveFormat);
            writer.Write(audio.ToArray(), 0, (int)audio.Length);
        }
    }
}
