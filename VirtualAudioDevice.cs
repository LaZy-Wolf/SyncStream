// File: VirtualAudioDevice.cs
using System;
using NAudio.Wave;

namespace SyncStream
{
    public class VirtualAudioDevice
    {
        private WaveOutEvent? waveOut; // NAudio's WaveOutEvent for audio playback
        private WaveFileReader? waveFileReader; // NAudio's WaveFileReader for reading audio data

        public void Initialize(byte[] audioData)
        {
            try
            {
                // Convert byte array to a memory stream
                var memoryStream = new System.IO.MemoryStream(audioData);

                // Create a WaveFileReader from the memory stream
                waveFileReader = new WaveFileReader(memoryStream);

                // Initialize the audio output
                waveOut = new WaveOutEvent();
                waveOut.Init(waveFileReader);
                Console.WriteLine("Virtual audio device initialized.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize virtual audio device: {ex.Message}");
            }
        }

        public void Play()
        {
            if (waveOut == null || waveFileReader == null)
                throw new InvalidOperationException("Virtual audio device is not initialized.");

            waveOut.Play();
            Console.WriteLine("Playing audio through virtual device.");
        }

        public void Stop()
        {
            if (waveOut != null)
            {
                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
                Console.WriteLine("Stopped virtual audio device.");
            }

            if (waveFileReader != null)
            {
                waveFileReader.Dispose();
                waveFileReader = null;
            }
        }
    }
}