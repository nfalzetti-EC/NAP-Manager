using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Win32;

namespace TestApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadPowerStatus();
        }

        private void RefreshStatus_Click(object sender, RoutedEventArgs e)
        {
            LoadPowerStatus();
        }

        private void LoadPowerStatus()
        {
            CheckModernStandbyStatus();
            CheckHibernateStatus();
            CheckSleepTimeouts();
            CheckDisplayTimeouts();
            CheckUSBPowerSettings();
        }

        private void RunPowerShell(string command)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"{command}\"",
                Verb = "runas",
                UseShellExecute = true
            });
        }

        private string RunPowerCfgQuery(string command)
        {
            var psi = new ProcessStartInfo("cmd.exe", $"/c {command}")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (var process = Process.Start(psi))
            {
                return process.StandardOutput.ReadToEnd();
            }
        }

        private void CheckModernStandbyStatus()
        {
            try
            {
                var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Power");
                var value = key?.GetValue("PlatformAoAcOverride");
                txtModernStandbyStatus.Text = (value != null && (int)value == 0)
                    ? "Modern Standby is DISABLED (S3 Sleep Enabled)"
                    : "Modern Standby is ENABLED (S0 Sleep Active)";
            }
            catch
            {
                txtModernStandbyStatus.Text = "Unable to read Modern Standby status.";
            }
        }

        private void CheckHibernateStatus()
        {
            var output = RunPowerCfgQuery("powercfg /query");
            txtHibernateStatus.Text = output.Contains("Hibernate") ? "Hibernate: ENABLED" : "Hibernate: DISABLED";
        }

        private void CheckSleepTimeouts()
        {
            txtSleepTimeoutAC.Text = $"Sleep Timeout (AC): {GetHexTimeout("29f6c1db-86da-48c5-9fdb-f2b67b1f44da", "AC")} min";
            txtSleepTimeoutDC.Text = $"Sleep Timeout (DC): {GetHexTimeout("29f6c1db-86da-48c5-9fdb-f2b67b1f44da", "DC")} min";
        }

        private void CheckDisplayTimeouts()
        {
            txtDisplayTimeoutAC.Text = $"Display Timeout (AC): {GetHexTimeout("3c0bc021-c8a8-4e07-a973-6b14cbcb2b7e", "AC")} min";
            txtDisplayTimeoutDC.Text = $"Display Timeout (DC): {GetHexTimeout("3c0bc021-c8a8-4e07-a973-6b14cbcb2b7e", "DC")} min";
        }

        private void CheckUSBPowerSettings()
        {
            txtUSBPowerAC.Text = $"USB Suspend (AC): {GetUSBStatus("AC")}";
            txtUSBPowerDC.Text = $"USB Suspend (Battery): {GetUSBStatus("DC")}";
        }

        private string GetHexTimeout(string settingGuid, string mode)
        {
            var output = RunPowerCfgQuery($"powercfg /query");
            var pattern = $@"Power Setting GUID: {settingGuid}.*?Current {mode} Power Setting Index: 0x([0-9a-fA-F]+)";
            var match = Regex.Match(output, pattern, RegexOptions.Singleline);
            if (match.Success)
            {
                int seconds = Convert.ToInt32(match.Groups[1].Value, 16);
                return (seconds / 60).ToString();
            }
            return "Unknown";
        }

        private string GetUSBStatus(string mode)
        {
            var output = RunPowerCfgQuery("powercfg /query");
            var pattern = $@"Power Setting GUID: 2a737441-1930-4402-8d77-b2bebba308a3.*?Current {mode} Power Setting Index: 0x([0-9a-fA-F]+)";
            var match = Regex.Match(output, pattern, RegexOptions.Singleline);
            if (match.Success)
            {
                return match.Groups[1].Value == "0" ? "OFF" : "ON";
            }
            return "Unknown";
        }

        // Hibernate ON/OFF
        private void EnableHibernate_Click(object sender, RoutedEventArgs e)
        {
            RunPowerShell("powercfg /hibernate on");
            LoadPowerStatus();
        }

        private void DisableHibernate_Click(object sender, RoutedEventArgs e)
        {
            RunPowerShell("powercfg /hibernate off");
            LoadPowerStatus();
        }

        // Hibernate Timeout
        private void ApplyHibernateTimeoutAC_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtHibernateAC.Text, out int minutes))
                RunPowerShell($"powercfg /change hibernate-timeout-ac {minutes}");
            else
                MessageBox.Show("Please enter a valid number for AC Hibernate timeout.");
        }

        private void ApplyHibernateTimeoutDC_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtHibernateDC.Text, out int minutes))
                RunPowerShell($"powercfg /change hibernate-timeout-dc {minutes}");
            else
                MessageBox.Show("Please enter a valid number for DC Hibernate timeout.");
        }

        // Sleep Timeout
        private void ApplySleepTimeoutAC_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtSleepAC.Text, out int minutes))
                RunPowerShell($"powercfg /change standby-timeout-ac {minutes}");
            else
                MessageBox.Show("Please enter a valid number for AC Sleep timeout.");
        }

        private void ApplySleepTimeoutDC_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtSleepDC.Text, out int minutes))
                RunPowerShell($"powercfg /change standby-timeout-dc {minutes}");
            else
                MessageBox.Show("Please enter a valid number for DC Sleep timeout.");
        }

        // Display Timeout
        private void ApplyDisplayTimeoutAC_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtDisplayAC.Text, out int minutes))
                RunPowerShell($"powercfg /change monitor-timeout-ac {minutes}");
            else
                MessageBox.Show("Please enter a valid number for AC Display timeout.");
        }

        private void ApplyDisplayTimeoutDC_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtDisplayDC.Text, out int minutes))
                RunPowerShell($"powercfg /change monitor-timeout-dc {minutes}");
            else
                MessageBox.Show("Please enter a valid number for DC Display timeout.");
        }

        // Modern Standby ON/OFF
        private void EnableModernStandby_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Enabling Modern Standby (S0) may affect battery life and wake behavior.\n\nAre you sure?",
                "Enable Modern Standby",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                RunPowerShell("Set-ItemProperty -Path 'HKLM\\SYSTEM\\CurrentControlSet\\Control\\Power' -Name 'PlatformAoAcOverride' -Value 1");
                LoadPowerStatus();
            }
        }

        private void DisableModernStandby_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Disabling Modern Standby will attempt to restore legacy S3 sleep mode.\n\nThis may not be supported on all systems.\n\nAre you sure?",
                "Disable Modern Standby",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                RunPowerShell("Set-ItemProperty -Path 'HKLM\\SYSTEM\\CurrentControlSet\\Control\\Power' -Name 'PlatformAoAcOverride' -Value 0");
                LoadPowerStatus();
            }
        }

        // USB Suspend ON/OFF
        private void EnableUSBAC_Click(object sender, RoutedEventArgs e)
        {
            RunPowerShell("powercfg /setacvalueindex SCHEME_CURRENT SUB_USB 2a737441-1930-4402-8d77-b2bebba308a3 1");
            LoadPowerStatus();
        }

        private void DisableUSBAC_Click(object sender, RoutedEventArgs e)
        {
            RunPowerShell("powercfg /setacvalueindex SCHEME_CURRENT SUB_USB 2a737441-1930-4402-8d77-b2bebba308a3 0");
            LoadPowerStatus();
        }

        private void EnableUSBDC_Click(object sender, RoutedEventArgs e)
        {
            RunPowerShell("powercfg /setdcvalueindex SCHEME_CURRENT SUB_USB 2a737441-1930-4402-8d77-b2bebba308a3 1");
            LoadPowerStatus();
        }

        private void DisableUSBDC_Click(object sender, RoutedEventArgs e)
        {
            RunPowerShell("powercfg /setdcvalueindex SCHEME_CURRENT SUB_USB 2a737441-1930-4402-8d77-b2bebba308a3 0");
            LoadPowerStatus();
        }
    }
}