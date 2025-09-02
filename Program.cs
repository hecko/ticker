using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
using IWshRuntimeLibrary; // Add COM reference: "Windows Script Host Object Model"

namespace ticker
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TrayApp());
        }
    }

    public class TrayApp : Form
    {
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private string logFile;
        private string startupFolder;

        public TrayApp()
        {
            // Paths
            string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            logFile = Path.Combine(docs, "TickerLog.csv");
            startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);

            // Ensure CSV header exists
            if (!System.IO.File.Exists(logFile))
            {
                System.IO.File.AppendAllText(logFile, "Timestamp,Event\n");
            }

            // log the program start
            System.IO.File.AppendAllText(logFile, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss},in" + Environment.NewLine);

            // Ensure shortcut exists in Startup
            EnsureStartupShortcut();

            // Tray menu
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Log a task", null, (s, e) => OpenLogTask());
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add("Open Logs Folder", null, (s, e) => OpenFolder(docs));
            trayMenu.Items.Add("Open Startup Folder", null, (s, e) => OpenFolder(startupFolder));
            trayMenu.Items.Add("Information", null, (s, e) => ShowInformation());
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add("Exit", null, OnExit);

            // Tray icon
            trayIcon = new NotifyIcon();
            trayIcon.Text = "Ticker";
            trayIcon.Icon = SystemIcons.Information;
            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.Visible = true;

            // Subscribe to lock/unlock events
            SystemEvents.SessionSwitch += OnSessionSwitch;

            // Show the info message box
            MessageBox.Show("Ticker App Started and minimized into tray icon.", "Ticker Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Hide the form
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
        }

        private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            string evt = null;

            if (e.Reason == SessionSwitchReason.SessionLock || e.Reason == SessionSwitchReason.SessionLogoff)
                evt = "out";
            else if (e.Reason == SessionSwitchReason.SessionUnlock || e.Reason == SessionSwitchReason.SessionLogon)
                evt = "in";

            if (evt != null)
            {
                string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{evt}";
                System.IO.File.AppendAllText(logFile, line + Environment.NewLine);
            }
        }

        private void OpenLogTask()
        {
            var form1 = new Form1();
            form1.Show();
        }

        private void EnsureStartupShortcut()
        {
            string shortcutPath = Path.Combine(startupFolder, "Ticker.lnk");

            if (!System.IO.File.Exists(shortcutPath))
            {
                string? exePath = Environment.ProcessPath;
                if (exePath == null)
                {
                    MessageBox.Show("Could not determine the process path. Not creating self-start link on Windows startup", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = exePath;
                shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
                shortcut.Description = "Ticker";
                shortcut.Save();
            }
        }

        private void OpenFolder(string path)
        {
            if (Directory.Exists(path))
            {
                System.Diagnostics.Process.Start("explorer.exe", path);
            }
        }

        private void OnExit(object sender, EventArgs e)
        {
            // log the program exit
            System.IO.File.AppendAllText(logFile, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss},out" + Environment.NewLine);

            trayIcon.Visible = false;
            Application.Exit();
        }

        private void ShowInformation()
        {
            string info = "Ticker - Session Tracking Application\n\n" +
                          "Purpose: Tracks Windows session lock/unlock events\n" +
                          $"Log File Path: {logFile}\n" +
                          "Startup: Auto-starts with Windows\n" +
                          "Version: 1.0.1 - 9/2025\n" +
                          "Written by https://github.com/hecko";

            MessageBox.Show(info, "Ticker Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SystemEvents.SessionSwitch -= OnSessionSwitch;
            base.OnFormClosing(e);
        }
    }
}
