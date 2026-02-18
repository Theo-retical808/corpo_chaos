using System.IO;
using System.Windows.Media;

namespace CorporateChaos.Systems
{
    public class BackgroundMusicManager
    {
        private MediaPlayer mediaPlayer;
        private bool isPlaying = false;
        private bool isMuted = false;
        private double volume = 0.3; // Default volume (30%)

        public BackgroundMusicManager()
        {
            mediaPlayer = new MediaPlayer();
            mediaPlayer.MediaEnded += OnMediaEnded;
            mediaPlayer.MediaOpened += OnMediaOpened;
            mediaPlayer.MediaFailed += OnMediaFailed;
        }

        public void StartBackgroundMusic()
        {
            try
            {
                // Try loading from file system first (more reliable)
                string audioPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "audio", "background.mp3");
                System.Diagnostics.Debug.WriteLine($"Attempting to load audio from: {audioPath}");
                
                if (File.Exists(audioPath))
                {
                    var fileUri = new Uri(audioPath, UriKind.Absolute);
                    mediaPlayer.Open(fileUri);
                    mediaPlayer.Volume = isMuted ? 0 : volume;
                    System.Diagnostics.Debug.WriteLine($"Audio file loaded from: {audioPath}");
                }
                else
                {
                    // Fallback to resources
                    System.Diagnostics.Debug.WriteLine("File not found, trying resources...");
                    var resourceUri = new Uri("pack://application:,,,/audio/background.mp3");
                    mediaPlayer.Open(resourceUri);
                    mediaPlayer.Volume = isMuted ? 0 : volume;
                    System.Diagnostics.Debug.WriteLine("Audio loaded from resources");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load background music: {ex.Message}");
            }
        }

        private void OnMediaOpened(object? sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Media opened successfully, starting playback...");
            try
            {
                mediaPlayer.Play();
                isPlaying = true;
                System.Diagnostics.Debug.WriteLine("Background music started successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to start playback: {ex.Message}");
            }
        }

        private void OnMediaFailed(object? sender, ExceptionEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Media failed to load: {e.ErrorException.Message}");
        }

        public void ToggleMute()
        {
            isMuted = !isMuted;
            mediaPlayer.Volume = isMuted ? 0 : volume;
            System.Diagnostics.Debug.WriteLine($"Audio {(isMuted ? "muted" : "unmuted")}");
        }

        public bool IsMuted()
        {
            return isMuted;
        }

        public void StopBackgroundMusic()
        {
            if (isPlaying)
            {
                mediaPlayer.Stop();
                isPlaying = false;
            }
        }

        public void PauseBackgroundMusic()
        {
            if (isPlaying)
            {
                mediaPlayer.Pause();
                isPlaying = false;
            }
        }

        public void ResumeBackgroundMusic()
        {
            if (!isPlaying)
            {
                mediaPlayer.Play();
                isPlaying = true;
            }
        }

        public void SetVolume(double newVolume)
        {
            volume = Math.Max(0.0, Math.Min(1.0, newVolume)); // Clamp between 0 and 1
            if (!isMuted)
            {
                mediaPlayer.Volume = volume;
            }
        }

        public double GetVolume()
        {
            return volume;
        }

        public bool IsPlaying()
        {
            return isPlaying;
        }

        private void OnMediaEnded(object? sender, EventArgs e)
        {
            // Loop the music by restarting it
            try
            {
                mediaPlayer.Position = TimeSpan.Zero;
                mediaPlayer.Play();
                System.Diagnostics.Debug.WriteLine("Background music looped");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to loop background music: {ex.Message}");
                isPlaying = false;
            }
        }

        public void Dispose()
        {
            mediaPlayer?.Close();
            mediaPlayer = null!;
        }
    }
}