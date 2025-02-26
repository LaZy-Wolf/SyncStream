// File: AudioCaptureWithWASAPI.cs
using System;
using CSCore.CoreAudioAPI;
using CSCore.SoundIn;

namespace SyncStream
{
    public class AudioCaptureWithWASAPI
    {
        private WasapiLoopbackCapture? audioCapture;
        private Action<byte[]> onDataAvailable;

        public AudioCaptureWithWASAPI(Action<byte[]> onDataAvailable)
        {
            this.onDataAvailable = onDataAvailable;
        }

        public void StartCapture()
        {
            if (audioCapture != null)
                throw new InvalidOperationException("Audio capture is already running.");

            try
            {
                audioCapture = new WasapiLoopbackCapture
                {
                    // No need to set ShareMode or WaveFormat explicitly; defaults work fine
                };

                audioCapture.DataAvailable += (sender, e) =>
                {
                    try
                    {
                        onDataAvailable?.Invoke(e.Data);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing audio data: {ex.Message}");
                    }
                };

                audioCapture.Start();
                Console.WriteLine("WASAPI audio capture started.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start WASAPI capture: {ex.Message}");
                throw; // Re-throw the exception to notify the caller
            }
        }

        public void StopCapture()
        {
            if (audioCapture != null)
            {
                try
                {
                    audioCapture.Stop();
                    audioCapture.Dispose();
                    audioCapture = null;
                    Console.WriteLine("WASAPI audio capture stopped.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to stop WASAPI capture: {ex.Message}");
                }
            }
        }
    }
}