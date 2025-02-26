using NAudio.Wave;
using System;
using System.Collections.Generic;
using InTheHand.Net.Sockets;

namespace SyncStream
{
    public class AudioStreamer
    {
        private WaveInEvent? waveIn;
        private List<BluetoothClient> connectedClients;

        public AudioStreamer()
        {
            connectedClients = new List<BluetoothClient>();
        }

        public void AddClient(BluetoothClient client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));
            connectedClients.Add(client);
            Console.WriteLine($"Added client: {client.RemoteMachineName}");
        }

        public void StartCapture()
        {
            if (waveIn != null)
                throw new InvalidOperationException("Audio capture is already running.");
            waveIn = new WaveInEvent
            {
                WaveFormat = new WaveFormat(44100, 2) // 44.1kHz, stereo
            };
            waveIn.DataAvailable += OnDataAvailable;
            waveIn.StartRecording();
            Console.WriteLine("Audio capture started.");
        }

        private void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            if (connectedClients.Count == 0)
                return;

            Console.WriteLine($"Captured {e.BytesRecorded} bytes of audio data.");

            // Send audio data to all connected clients
            foreach (var client in connectedClients.ToList()) // Use ToList to avoid modification issues
            {
                try
                {
                    if (client.Connected)
                    {
                        var stream = client.GetStream();
                        stream.Write(e.Buffer, 0, e.BytesRecorded);
                        Console.WriteLine($"Sent data to client: {client.RemoteMachineName}");
                    }
                    else
                    {
                        Console.WriteLine($"Client {client.RemoteMachineName} is not connected. Removing...");
                        connectedClients.Remove(client); // Remove disconnected clients
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send data to {client.RemoteMachineName}: {ex.Message}");
                    connectedClients.Remove(client); // Remove faulty clients
                }
            }
        }

        public void StopCapture()
        {
            if (waveIn != null)
            {
                waveIn.StopRecording();
                waveIn.Dispose();
                waveIn = null;
                Console.WriteLine("Audio capture stopped.");
            }
        }

        public void DisconnectClients()
        {
            foreach (var client in connectedClients.ToList()) // Use ToList to avoid modification issues
            {
                try
                {
                    client.Close();
                    Console.WriteLine($"Disconnected client: {client.RemoteMachineName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to disconnect client {client.RemoteMachineName}: {ex.Message}");
                }
            }
            connectedClients.Clear();
        }
    }
}