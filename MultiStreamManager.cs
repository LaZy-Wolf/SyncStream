// File: MultiStreamManager.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using InTheHand.Net.Sockets;

namespace SyncStream
{
    public class MultiStreamManager
    {
        private readonly List<BluetoothClient> connectedClients;

        public MultiStreamManager()
        {
            connectedClients = new List<BluetoothClient>();
        }

        public void AddClient(BluetoothClient client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));
            connectedClients.Add(client);
            Console.WriteLine($"Added client to MultiStreamManager: {client.RemoteMachineName}");
        }

        public async Task BroadcastAudioAsync(byte[] audioData)
        {
            if (connectedClients.Count == 0)
                return;

            Console.WriteLine($"Broadcasting {audioData.Length} bytes of audio data to {connectedClients.Count} clients.");

            var tasks = new List<Task>();

            foreach (var client in connectedClients.ToList()) // Use ToList to avoid modification issues
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        if (client.Connected)
                        {
                            using (var stream = client.GetStream())
                            {
                                if (stream.CanWrite)
                                {
                                    stream.Write(audioData, 0, audioData.Length);
                                    Console.WriteLine($"Sent data to client: {client.RemoteMachineName}");
                                }
                                else
                                {
                                    Console.WriteLine($"Stream for client {client.RemoteMachineName} is not writable. Removing...");
                                    connectedClients.Remove(client);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Client {client.RemoteMachineName} is not connected. Removing...");
                            connectedClients.Remove(client);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to send data to {client.RemoteMachineName}: {ex.Message}");
                        connectedClients.Remove(client); // Remove faulty clients
                    }
                }));
            }

            await Task.WhenAll(tasks);
        }

        public void DisconnectAllClients()
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