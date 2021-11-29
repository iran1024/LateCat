using CSCore;
using CSCore.DSP;
using CSCore.SoundIn;
using CSCore.SoundOut;
using CSCore.Streams;
using CSCore.Streams.Effects;
using LateCat.CefPlayer.Visualization;

namespace LateCat.CefPlayer.Services
{
    public class SystemAudio : IDisposable
    {
        private WasapiCapture _soundIn;
        private ISoundOut _soundOut;
        private IWaveSource _source;
        private LineSpectrum _lineSpectrum;
        private PitchShifter _pitchShifter;
        private readonly System.Windows.Forms.Timer wasapiAudioTimer;

        public event EventHandler<float[]> AudioData;

        public SystemAudio()
        {
            wasapiAudioTimer = new System.Windows.Forms.Timer
            {
                Interval = 33
            };
            wasapiAudioTimer.Tick += AudioTimer;

            try
            {
                InitializeAudio();
            }
            catch (Exception)
            {

            }
        }

        private void InitializeAudio()
        {
            _soundIn = new WasapiLoopbackCapture(100, new WaveFormat(48000, 24, 2));

            _soundIn.Initialize();

            var soundInSource = new SoundInSource(_soundIn);
            ISampleSource source = soundInSource.ToSampleSource().AppendSource(x => new PitchShifter(x), out _pitchShifter);

            SetupSampleSource(source);

            var buffer = new byte[_source.WaveFormat.BytesPerSecond / 2];
            soundInSource.DataAvailable += (s, aEvent) =>
            {
                int read;
                while ((read = _source.Read(buffer, 0, buffer.Length)) > 0) ;
            };

            _soundIn.Start();
        }

        BasicSpectrumProvider spectrumProvider;
        private bool disposedValue;
        const FftSize fftSize = FftSize.Fft128;

        private void SetupSampleSource(ISampleSource aSampleSource)
        {
            spectrumProvider = new BasicSpectrumProvider(aSampleSource.WaveFormat.Channels,
                aSampleSource.WaveFormat.SampleRate, fftSize);

            _lineSpectrum = new LineSpectrum(fftSize)
            {
                SpectrumProvider = spectrumProvider,
                UseAverage = true,
                BarCount = 128,
                BarSpacing = 2,
                IsXLogScale = true,
                ScalingStrategy = ScalingStrategy.Sqrt,
                MaximumFrequency = 20000,
                MinimumFrequency = 20,

            };

            var notificationSource = new SingleBlockNotificationStream(aSampleSource);

            notificationSource.SingleBlockRead += (s, a) => spectrumProvider.Add(a.Left, a.Right);

            _source = notificationSource.ToWaveSource(16);

        }

        private void AudioTimer(object sender, EventArgs e)
        {
            try
            {
                var fftBuffer = new float[(int)fftSize];
                fftBuffer = _lineSpectrum.livelyGetSystemAudioSpectrum();
                AudioData?.Invoke(this, fftBuffer);
            }
            catch (Exception)
            {

            }
        }

        public void Start()
        {
            wasapiAudioTimer?.Start();
        }

        public void Stop()
        {
            wasapiAudioTimer?.Stop();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    wasapiAudioTimer?.Stop();

                    _soundOut?.Stop();
                    _soundOut?.Dispose();
                    _soundOut = null;

                    _soundIn?.Stop();
                    _soundIn?.Dispose();
                    _soundIn = null;

                    _source?.Dispose();
                    _source = null;

                    _lineSpectrum = null;
                }

                disposedValue = true;
            }
        }

        ~SystemAudio()
        {
            //Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
