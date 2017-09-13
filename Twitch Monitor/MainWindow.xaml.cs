using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using System.Drawing;

namespace TwitchMonitor
{
    public partial class MainWindow
    {
        public NotifyIcon MyNotifyIcon;

        private System.Collections.Specialized.StringCollection _lists;

        public MainWindow()
        {
            InitializeComponent();
            MyNotifyIcon = new NotifyIcon();
            var mainwindow = (MainWindow) System.Windows.Application.Current.MainWindow;
            mainwindow.StateChanged += MainWindow_Resize;


            MyNotifyIcon.Visible = true;
            MyNotifyIcon.Icon = new Icon(SystemIcons.Application, 40, 40);
            MyNotifyIcon.MouseDoubleClick += MyNotifyIcon_MouseDoubleClick;

            if (Properties.Settings.Default.listStrings == null)
                Properties.Settings.Default.listStrings = new System.Collections.Specialized.StringCollection();

            _lists = Properties.Settings.Default.listStrings;

            foreach (var url in Properties.Settings.Default.listStrings)
                ListBox1.Items.Add(new Stream(url));

            CheckLive();
        }

        private void MainWindow_Resize(object sender, EventArgs e)
        {
            if (WindowState.Minimized == WindowState)
            {
                MyNotifyIcon.Visible = true;
                Hide();
            }
            else if (WindowState.Normal == WindowState)
            {
                MyNotifyIcon.Visible = false;
            }
        }

        private void MyNotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ListBox1.Items.Add(new Stream(TextBox1.Text));
            _lists.Add(TextBox1.Text);
            Properties.Settings.Default.listStrings = _lists;
            Properties.Settings.Default.Save();
        }

        public void LiveNotification(string streamName)
        {
            MyNotifyIcon.BalloonTipText = streamName + " is now live!";
            MyNotifyIcon.BalloonTipTitle = "Twitch Monitor";
            MyNotifyIcon.ShowBalloonTip(5000);
        }

        private void WatchButton_Click(object sender, RoutedEventArgs e)
        {
            var cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();
            cmd.StandardInput.WriteLine("streamlink " + ListBox1.SelectedValue + " best");
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ListBox1.SelectedIndex != -1)
            {
                _lists.Remove(ListBox1.SelectedItem.ToString());
                ListBox1.Items.RemoveAt(ListBox1.SelectedIndex);
            }
            Properties.Settings.Default.listStrings = _lists;
            Properties.Settings.Default.Save();
        }

        private void CheckLive()
        {
            for (int i = 0; i != ListBox1.Items.Count; ++i)
            {
                AsyncRequest(((Stream) ListBox1.Items.GetItemAt(i)));
            }
        }

        async Task<bool> AsyncRequest(Stream check)
        {
            using (var request = new HttpClient())
            {
                request.DefaultRequestHeaders.TryAddWithoutValidation("Client-ID",
                    "OAuth 6wkv85t0txkdm471qaimy8mugxhzda");
                using (var response =
                    await request.GetAsync("https://api.twitch.tv/kraken/streams/" + check.ToString().Substring(17)))
                using (var content = response.Content)
                {
                    var result = await content.ReadAsStringAsync();
                    var deserialize = JObject.Parse(result);
                    var results = deserialize["stream"].Children().ToList();

                    if (results.Count != 0)
                    {
                        check.GoLive();
                        return true;
                    }
                    else if (results.Count == 0)
                    {
                        check.GoOffline();
                    }
                }
                return false;
            }
        }

        public void OnlineCheckTimer()
        {
            var timer = new Timer();
            timer.Tick += TimerTick;
            timer.Interval = 50000;
            timer.Start();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            CheckLive();
        }
    }
}