using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using InTheHand.Net;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;

namespace SyncStream
{
    public class BluetoothHelper
    {
        [DllImport("Irprops.cpl", SetLastError = true)]
        private static extern IntPtr BluetoothFindFirstDevice(ref BLUETOOTH_DEVICE_SEARCH_PARAMS pbtsp, ref BLUETOOTH_DEVICE_INFO pbtdi);

        [DllImport("Irprops.cpl", SetLastError = true)]
        private static extern bool BluetoothFindNextDevice(IntPtr hFind, ref BLUETOOTH_DEVICE_INFO pbtdi);

        [DllImport("Irprops.cpl", SetLastError = true)]
        private static extern bool BluetoothFindDeviceClose(IntPtr hFind);

        public List<BluetoothDeviceInfo> DiscoverDevices()
        {
            var devices = new List<BluetoothDeviceInfo>();

            // Try using 32feet.NET first
            try
            {
                Console.WriteLine("Attempting to discover devices using 32feet.NET...");
                using (var bluetoothClient = new BluetoothClient())
                {
                    var foundDevices = bluetoothClient.DiscoverDevices();
                    foreach (var device in foundDevices)
                    {
                        Console.WriteLine($"Found: {device.DeviceName} ({device.DeviceAddress})");
                        devices.Add(device);
                    }
                }

                if (devices.Count > 0)
                {
                    Console.WriteLine($"{devices.Count} device(s) discovered using 32feet.NET.");
                    return devices;
                }
                else
                {
                    Console.WriteLine("No devices found using 32feet.NET.");
                }
            }
            catch (PlatformNotSupportedException)
            {
                Console.WriteLine("32feet.NET failed. Falling back to Windows Bluetooth API...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in 32feet.NET: {ex.Message}");
            }

            // Fall back to Windows Bluetooth API
            try
            {
                Console.WriteLine("Attempting to discover devices using Windows Bluetooth API...");
                devices = DiscoverDevicesUsingWindowsAPI();

                if (devices.Count > 0)
                {
                    Console.WriteLine($"{devices.Count} device(s) discovered using Windows Bluetooth API.");
                }
                else
                {
                    Console.WriteLine("No devices found using Windows Bluetooth API.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Windows Bluetooth API: {ex.Message}");
            }

            return devices;
        }

        public async Task<List<BluetoothClient>> ConnectToDevicesAsync(List<BluetoothDeviceInfo> devices)
        {
            var connectedClients = new List<BluetoothClient>();

            foreach (var device in devices)
            {
                int retryCount = 3; // Retry up to 3 times

                for (int attempt = 1; attempt <= retryCount; attempt++)
                {
                    try
                    {
                        Console.WriteLine($"Connecting to {device.DeviceName}... (Attempt {attempt})");

                        // Try multiple profiles: Audio Sink, Hands-Free, etc.
                        var profiles = new[] { BluetoothService.AudioSink, BluetoothService.Handsfree };
                        foreach (var profile in profiles)
                        {
                            var bluetoothClient = new BluetoothClient();
                            await Task.Run(() => bluetoothClient.Connect(device.DeviceAddress, profile));
                            Console.WriteLine($"Connected to {device.DeviceName} using profile {profile}");

                            connectedClients.Add(bluetoothClient);
                            break; // Exit profile loop if successful
                        }

                        break; // Exit retry loop if successful
                    }
                    catch (PlatformNotSupportedException)
                    {
                        Console.WriteLine($"Failed to connect to {device.DeviceName}: Unsupported Bluetooth stack.");
                        break; // No need to retry if the stack is unsupported
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Attempt {attempt} failed for {device.DeviceName}: {ex.Message}");
                        if (attempt == retryCount)
                        {
                            Console.WriteLine($"Giving up on {device.DeviceName} after {retryCount} attempts.");
                        }
                    }
                }
            }

            return connectedClients;
        }

        private List<BluetoothDeviceInfo> DiscoverDevicesUsingWindowsAPI()
        {
            var devices = new List<BluetoothDeviceInfo>();
            BLUETOOTH_DEVICE_SEARCH_PARAMS searchParams = new BLUETOOTH_DEVICE_SEARCH_PARAMS
            {
                dwSize = Marshal.SizeOf(typeof(BLUETOOTH_DEVICE_SEARCH_PARAMS)),
                fReturnAuthenticated = true,
                fReturnRemembered = true,
                fReturnUnknown = true,
                fReturnConnected = true,
                cTimeoutMultiplier = 4
            };

            BLUETOOTH_DEVICE_INFO deviceInfo = new BLUETOOTH_DEVICE_INFO
            {
                dwSize = Marshal.SizeOf(typeof(BLUETOOTH_DEVICE_INFO))
            };

            IntPtr hFind = BluetoothFindFirstDevice(ref searchParams, ref deviceInfo);
            if (hFind != IntPtr.Zero)
            {
                do
                {
                    var bluetoothAddress = new BluetoothAddress(BitConverter.GetBytes(deviceInfo.Address));
                    var bluetoothDeviceInfo = new BluetoothDeviceInfo(bluetoothAddress)
                    {
                        DeviceName = deviceInfo.szName
                    };

                    devices.Add(bluetoothDeviceInfo);
                } while (BluetoothFindNextDevice(hFind, ref deviceInfo));

                BluetoothFindDeviceClose(hFind);
            }

            return devices;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BLUETOOTH_DEVICE_SEARCH_PARAMS
    {
        public int dwSize;
        public bool fReturnAuthenticated;
        public bool fReturnRemembered;
        public bool fReturnUnknown;
        public bool fReturnConnected;
        public bool fIssueInquiry;
        public byte cTimeoutMultiplier;
        public IntPtr hRadio;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BLUETOOTH_DEVICE_INFO
    {
        public int dwSize;
        public long Address;
        public uint ulClassofDevice;
        public bool fConnected;
        public bool fRemembered;
        public bool fAuthenticated;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 248)]
        public string szName;
    }
}