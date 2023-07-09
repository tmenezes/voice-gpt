using NAudio.Wave;

namespace VoiceGpt.Core
{
    public interface IMicrophoneAudioService
    {
        void StartCapture(Action<(byte[] buffer, int bytesCaptured)> onDataCaptured);
        MemoryStream StopCapture();
    }

    public class MicrophoneAudioService : IMicrophoneAudioService
    {
        private MemoryStream _memoryStream;
        private WaveInEvent  _waveIn;

        public void StartCapture(Action<(byte[] buffer, int bytesCaptured)> onDataCaptured)
        {
            _memoryStream = new MemoryStream();
            _waveIn = new WaveInEvent
            {
                WaveFormat = Constants.WaveFormat
            };

            _waveIn.DataAvailable += (sender, e) =>
            {
                _memoryStream.Write(e.Buffer, 0, e.BytesRecorded);
                onDataCaptured((e.Buffer, e.BytesRecorded));
            };

            _waveIn.StartRecording();
        }

        public MemoryStream StopCapture()
        {
            _waveIn.StopRecording();
            _memoryStream.Position = 0;

            return _memoryStream;
        }
    }
}
