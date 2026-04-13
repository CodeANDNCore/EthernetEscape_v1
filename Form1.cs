using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace CodeAndCore_NetTool
{
    public partial class Form1 : Form
    {
        private Dictionary<string, string> dnsProviders = new Dictionary<string, string>
        {
            { "Google", "8.8.8.8,8.8.4.4" },
            { "Cloudflare", "1.1.1.1,1.0.0.1" },
            { "AdGuard (No Ads)", "94.140.14.14,94.140.15.15" },
            { "GeoHide", "45.155.204.190,95.182.120.241" },
            { "Xbox", "178.22.122.100,185.51.200.2" },
            { "Comss", "76.76.2.22,76.76.10.22" },
            { "Malware blocking", "1.1.1.2,1.0.0.2" },
            { "Astracat", "185.159.13.2,185.159.13.2" },
            { "Mafioznik", "77.88.8.8,77.88.8.1" },
            { "Quad9", "9.9.9.9,149.112.112.112" },
            { "OpenDNS", "208.67.222.222,208.67.220.220" },
            { "CleanBrowsing", "185.228.168.9,185.228.169.9" }
        };

        private ComboBox comboDns;
        private Label lblStatus;
        private Button btnApply, btnPing, btnTurbo, btnReset;

        public Form1()
        {
            InitializeCustomComponent();
            SetupFormStyle();
            ApplyIconFix();
        }

        private void SetupFormStyle()
        {
            this.Text = "EthernetEscape | Network Tool";
            this.BackColor = Color.FromArgb(18, 18, 18);
            this.ForeColor = Color.White;
            this.Size = new Size(400, 600);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void InitializeCustomComponent()
        {
            Label lblTitle = new Label { Text = "ETHERNET ESCAPE", Font = new Font("Consolas", 20, FontStyle.Bold), ForeColor = Color.FromArgb(0, 255, 200), Dock = DockStyle.Top, Height = 60, TextAlign = ContentAlignment.MiddleCenter };

            comboDns = new ComboBox { Location = new Point(30, 90), Width = 325, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White, DropDownStyle = ComboBoxStyle.DropDownList };
            comboDns.Items.AddRange(dnsProviders.Keys.ToArray());
            comboDns.SelectedIndex = 0;

            btnApply = CreateBtn("УСТАНОВИТЬ DNS", new Point(30, 140), Color.FromArgb(0, 100, 100));
            btnApply.Click += (s, e) => SetDNS(dnsProviders[comboDns.SelectedItem.ToString()]);

            btnPing = CreateBtn("ПРОВЕРИТЬ ПИНГ", new Point(30, 200), Color.FromArgb(50, 50, 50));
            btnPing.Click += (s, e) => RunPing();

            btnTurbo = CreateBtn("TURBO MODE", new Point(30, 260), Color.FromArgb(100, 0, 100));
            btnTurbo.Click += (s, e) => OptimizeNetwork();

            btnReset = CreateBtn("СБРОС НАСТРОЕК", new Point(30, 320), Color.FromArgb(150, 0, 0));
            btnReset.Click += (s, e) => ResetNetworkSettings();

            lblStatus = new Label { Text = "СИСТЕМА ГОТОВА", Location = new Point(30, 390), Width = 325, Height = 80, Font = new Font("Consolas", 9), TextAlign = ContentAlignment.TopCenter };

            Label lblAuthor = new Label
            {
                Text = "by Code&&Core | t.me/CodeNCore",
                Dock = DockStyle.Bottom,
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 40,
                ForeColor = Color.FromArgb(80, 80, 80),
                Font = new Font("Segoe UI", 8, FontStyle.Italic)
            };

            this.Controls.AddRange(new Control[] { lblTitle, comboDns, btnApply, btnPing, btnTurbo, btnReset, lblStatus, lblAuthor });
        }

        private Button CreateBtn(string text, Point loc, Color color)
        {
            return new Button { Text = text, Location = loc, Width = 325, Height = 45, FlatStyle = FlatStyle.Flat, BackColor = color, ForeColor = Color.White, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand, FlatAppearance = { BorderSize = 0 } };
        }

        private async void RunPing()
        {
            btnPing.Enabled = false;
            lblStatus.Text = "Замер пинга...";
            try
            {
                Ping p = new Ping();
                long total = 0;
                for (int i = 0; i < 3; i++)
                {
                    var r = await p.SendPingAsync("google.com", 2000);
                    if (r.Status == IPStatus.Success) total += r.RoundtripTime;
                    await System.Threading.Tasks.Task.Delay(100);
                }
                lblStatus.Text = $"Средний пинг: {total / 3} ms";
                lblStatus.ForeColor = Color.SpringGreen;
            }
            catch { lblStatus.Text = "Ошибка сети."; lblStatus.ForeColor = Color.Red; }
            finally { btnPing.Enabled = true; }
        }

        private void SetDNS(string dns)
        {
            try
            {
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"])
                    {
                        ManagementBaseObject dnsParams = mo.GetMethodParameters("SetDNSServerSearchOrder");
                        dnsParams["DNSServerSearchOrder"] = dns.Split(',');
                        mo.InvokeMethod("SetDNSServerSearchOrder", dnsParams, null);
                    }
                }
                Process.Start(new ProcessStartInfo("ipconfig", "/flushdns") { WindowStyle = ProcessWindowStyle.Hidden });
                lblStatus.Text = "DNS ОБНОВЛЕН";
                lblStatus.ForeColor = Color.SpringGreen;
            }
            catch { lblStatus.Text = "ОШИБКА ПРАВ"; lblStatus.ForeColor = Color.Red; }
        }

        private void OptimizeNetwork()
        {
            try
            {
                string cmd = "/c netsh int tcp set global autotuninglevel=normal && netsh int tcp set global chimney=enabled && netsh int tcp set global timestamps=disabled";
                Process.Start(new ProcessStartInfo("cmd.exe", cmd) { WindowStyle = ProcessWindowStyle.Hidden, Verb = "runas" });
                lblStatus.Text = "TURBO ВКЛЮЧЕН";
                lblStatus.ForeColor = Color.Fuchsia;
            }
            catch { }
        }

        private void ResetNetworkSettings()
        {
            try
            {
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"]) mo.InvokeMethod("SetDNSServerSearchOrder", null);
                }
                Process.Start(new ProcessStartInfo("cmd.exe", "/c netsh winsock reset && netsh int ip reset && ipconfig /flushdns") { WindowStyle = ProcessWindowStyle.Hidden, Verb = "runas" });
                lblStatus.Text = "СЕТЬ СБРОШЕНА";
                lblStatus.ForeColor = Color.Yellow;
            }
            catch { }
        }

        private void ApplyIconFix()
        {
            try
            {
                if (File.Exists("Logo.ico")) this.Icon = new Icon("Logo.ico");
                else this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch { }
        }
    }
}