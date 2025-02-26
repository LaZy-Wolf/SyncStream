// File: UIDesigner.cs
using System.Drawing;
using System.Windows.Forms;

namespace SyncStream
{
    public static class UIDesigner
    {
        public static void InitializeUI(MainForm form)
        {
            // Set form properties
            form.Text = "SyncStream - Bluetooth Device Connector";
            form.BackColor = Color.FromArgb(45, 45, 48); // Dark background
            form.ForeColor = Color.White; // Light text
            form.Font = new Font("Segoe UI", 10); // Modern font
            form.Size = new Size(600, 500); // Larger form size
            form.MinimumSize = new Size(600, 500); // Minimum size to prevent shrinking
            form.FormBorderStyle = FormBorderStyle.FixedSingle; // Fixed border

            // Create a TableLayoutPanel for responsive layout
            var tableLayoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(10),
                BackColor = form.BackColor
            };
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(System.Windows.Forms.SizeType.Percent, 50));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(System.Windows.Forms.SizeType.Percent, 50));
            tableLayoutPanel.RowStyles.Add(new RowStyle(System.Windows.Forms.SizeType.Absolute, 150)); // Device list
            tableLayoutPanel.RowStyles.Add(new RowStyle(System.Windows.Forms.SizeType.Absolute, 50));  // Buttons row
            tableLayoutPanel.RowStyles.Add(new RowStyle(System.Windows.Forms.SizeType.Absolute, 50));  // Help button
            form.Controls.Add(tableLayoutPanel);

            // ListBox for devices
            form.lstDevices.Dock = DockStyle.Fill;
            form.lstDevices.BackColor = Color.FromArgb(60, 60, 60);
            form.lstDevices.ForeColor = Color.White;
            tableLayoutPanel.Controls.Add(form.lstDevices, 0, 0);
            tableLayoutPanel.SetColumnSpan(form.lstDevices, 2);

            // Discover Devices Button
            form.btnDiscover.Text = "Discover Devices";
            form.btnDiscover.BackColor = Color.FromArgb(0, 122, 204);
            form.btnDiscover.ForeColor = Color.White;
            form.btnDiscover.FlatStyle = FlatStyle.Flat;
            form.btnDiscover.Dock = DockStyle.Fill;
            form.btnDiscover.Click += form.BtnDiscover_Click; // Ensure BtnDiscover_Click is public
            tableLayoutPanel.Controls.Add(form.btnDiscover, 0, 1);

            // Connect Button
            form.btnConnect.Text = "Connect to Selected Devices";
            form.btnConnect.BackColor = Color.FromArgb(0, 122, 204);
            form.btnConnect.ForeColor = Color.White;
            form.btnConnect.FlatStyle = FlatStyle.Flat;
            form.btnConnect.Dock = DockStyle.Fill;
            form.btnConnect.Click += form.BtnConnect_Click; // Ensure BtnConnect_Click is public
            tableLayoutPanel.Controls.Add(form.btnConnect, 1, 1);

            // Help Button
            var btnHelp = new Button
            {
                Text = "?",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill
            };
            btnHelp.Click += form.BtnHelp_Click; // Ensure BtnHelp_Click is public
            tableLayoutPanel.Controls.Add(btnHelp, 0, 2);
            tableLayoutPanel.SetColumnSpan(btnHelp, 2);
        }
    }
}