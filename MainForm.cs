// File: MainForm.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using InTheHand.Net.Sockets;

namespace SyncStream
{
    public partial class MainForm : Form
    {
        internal CheckedListBox lstDevices;
        internal Button btnDiscover, btnConnect, btnStartStream, btnStopStream;
        private BluetoothHelper bluetoothHelper;
        private AudioCaptureWithWASAPI audioCapture;
        private VirtualAudioDevice virtualAudioDevice;
        private MultiStreamManager multiStreamManager;
        private List<InTheHand.Net.Sockets.BluetoothDeviceInfo> discoveredDevices;
        private bool useFallback = false; // Flag to indicate fallback mode
        private bool isAudioCaptureRunning = false; // Flag to track audio capture state

        public MainForm()
        {
            InitializeComponent();
            Text = "SyncStream";
            lstDevices = new CheckedListBox { SelectionMode = SelectionMode.One };
            btnDiscover = new Button();
            btnConnect = new Button();
            btnStartStream = new Button();
            btnStopStream = new Button();
            InitializeComponents();
            bluetoothHelper = new BluetoothHelper();
            discoveredDevices = new List<InTheHand.Net.Sockets.BluetoothDeviceInfo>();
            audioCapture = new AudioCaptureWithWASAPI(OnAudioDataAvailable);
            virtualAudioDevice = new VirtualAudioDevice();
            multiStreamManager = new MultiStreamManager();
        }

        private void InitializeComponents()
        {
            // CheckedListBox for devices
            lstDevices.Location = new System.Drawing.Point(10, 10);
            lstDevices.Size = new System.Drawing.Size(300, 100);
            Controls.Add(lstDevices);

            // Discover Devices Button
            btnDiscover.Text = "Discover Devices";
            btnDiscover.Location = new System.Drawing.Point(10, 120);
            btnDiscover.Size = new System.Drawing.Size(140, 30);
            btnDiscover.Click += BtnDiscover_Click;
            Controls.Add(btnDiscover);

            // Connect Button
            btnConnect.Text = "Connect to Selected Devices";
            btnConnect.Location = new System.Drawing.Point(160, 120);
            btnConnect.Size = new System.Drawing.Size(150, 30);
            btnConnect.Click += BtnConnect_Click;
            Controls.Add(btnConnect);

            // Start Stream Button
            btnStartStream.Text = "Start Streaming";
            btnStartStream.Location = new System.Drawing.Point(10, 160);
            btnStartStream.Size = new System.Drawing.Size(140, 30);
            btnStartStream.Click += BtnStartStream_Click;
            Controls.Add(btnStartStream);

            // Stop Stream Button
            btnStopStream.Text = "Stop Streaming";
            btnStopStream.Location = new System.Drawing.Point(160, 160);
            btnStopStream.Size = new System.Drawing.Size(150, 30);
            btnStopStream.Click += BtnStopStream_Click;
            Controls.Add(btnStopStream);

            // Help Button
            var btnHelp = new Button
            {
                Text = "?",
                Location = new System.Drawing.Point(10, 200),
                Size = new System.Drawing.Size(140, 30)
            };
            btnHelp.Click += BtnHelp_Click;
            Controls.Add(btnHelp);
        }

        public void BtnDiscover_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    var devices = bluetoothHelper.DiscoverDevices();
                    discoveredDevices = devices.ToList();

                    Invoke((Action)(() =>
                    {
                        lstDevices.Items.Clear();
                        if (discoveredDevices.Count > 0)
                        {
                            foreach (var device in discoveredDevices)
                            {
                                lstDevices.Items.Add($"{device.DeviceName} ({device.DeviceAddress})", false);
                            }
                        }
                        else
                        {
                            MessageBox.Show("No Bluetooth devices found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }));
                }
                catch (Exception ex)
                {
                    Invoke((Action)(() =>
                    {
                        MessageBox.Show($"Discovery failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                }
            });
        }

        public async void BtnConnect_Click(object sender, EventArgs e)
        {
            var selectedIndices = lstDevices.CheckedIndices.Cast<int>().ToList();
            var selectedDevices = selectedIndices
                .Where(index => index >= 0 && index < discoveredDevices.Count)
                .Select(index => discoveredDevices[index])
                .ToList();

            if (selectedDevices.Count == 0)
            {
                MessageBox.Show("Please select at least one device.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var connectedClients = await bluetoothHelper.ConnectToDevicesAsync(selectedDevices);

                if (connectedClients.Count == 0)
                {
                    MessageBox.Show("Failed to connect to any devices.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                foreach (var client in connectedClients)
                {
                    multiStreamManager.AddClient(client);
                }

                MessageBox.Show($"Connected to {connectedClients.Count} device(s).");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void BtnStartStream_Click(object sender, EventArgs e)
        {
            if (isAudioCaptureRunning)
            {
                MessageBox.Show("Audio capture is already running.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Attempt direct Bluetooth streaming
                useFallback = false;
                audioCapture.StartCapture();
                isAudioCaptureRunning = true;
                MessageBox.Show("Audio streaming started.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // Switch to fallback mode if Bluetooth streaming fails
                MessageBox.Show($"Bluetooth streaming failed: {ex.Message}. Switching to fallback mode.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                useFallback = true;

                try
                {
                    // Ensure audio capture is not already running
                    if (!isAudioCaptureRunning)
                    {
                        audioCapture.StartCapture();
                        isAudioCaptureRunning = true;
                        MessageBox.Show("Fallback mode activated: Audio routed through virtual device.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception fallbackEx)
                {
                    MessageBox.Show($"Fallback mode failed: {fallbackEx.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public void BtnStopStream_Click(object sender, EventArgs e)
        {
            try
            {
                if (isAudioCaptureRunning)
                {
                    audioCapture.StopCapture();
                    isAudioCaptureRunning = false;
                }

                virtualAudioDevice.Stop();
                multiStreamManager.DisconnectAllClients();
                MessageBox.Show("Audio streaming stopped.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to stop streaming: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void BtnHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "If you encounter Bluetooth compatibility issues:\n\n" +
                "1. Ensure your Bluetooth adapter uses the Microsoft Bluetooth stack.\n" +
                "2. Uninstall proprietary drivers (e.g., Realtek, Intel) and let Windows install the default driver.\n" +
                "3. Restart your computer and try again.\n\n" +
                "For more information, refer to the documentation.",
                "Help",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void OnAudioDataAvailable(byte[] audioData)
        {
            try
            {
                if (useFallback)
                {
                    // Route audio through the virtual device in fallback mode
                    virtualAudioDevice.Initialize(audioData);
                    virtualAudioDevice.Play();
                }
                else
                {
                    // Broadcast audio to all connected Bluetooth devices
                    multiStreamManager.BroadcastAudioAsync(audioData).Wait();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected exception: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}