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
        public System.Windows.Forms.NotifyIcon MyNotifyIcon;
        List<String> list = new List<String>();
        System.Collections.Specialized.StringCollection lists = new System.Collections.Specialized.StringCollection();

        public MainWindow()
        {


            InitializeComponent();
            MyNotifyIcon = new System.Windows.Forms.NotifyIcon();
            var mainwindow = (MainWindow)System.Windows.Application.Current.MainWindow;
            mainwindow.StateChanged += this.frmMain_Resize;


            MyNotifyIcon.Visible = true;
            MyNotifyIcon.Icon = new System.Drawing.Icon(SystemIcons.Application, 40, 40);
            MyNotifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(MyNotifyIcon_MouseDoubleClick);

            if (Properties.Settings.Default.listStrings == null)
                Properties.Settings.Default.listStrings = new System.Collections.Specialized.StringCollection();

            lists = Properties.Settings.Default.listStrings;

            for (int i = 0; i < Properties.Settings.Default.listStrings.Count; ++i)
            {
                ListBox1.Items.Add(new Stream(Properties.Settings.Default.listStrings[i]));
            }

            CheckLive();

        }
        private void frmMain_Resize(object sender, EventArgs e)
        {
            if (WindowState.Minimized == this.WindowState)
            {
                MyNotifyIcon.Visible = true;
                this.Hide();
            }
            else if (WindowState.Normal == this.WindowState)
            {
                MyNotifyIcon.Visible = false;
            }
        }
        private void MyNotifyIcon_MouseDoubleClick(object sender,System.Windows.Forms.MouseEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ListBox1.Items.Add(new Stream(TextBox1.Text));
            lists.Add(TextBox1.Text);
            Properties.Settings.Default.listStrings = lists;
            Properties.Settings.Default.Save();
        }

        public void LiveNotification(string streamName)
        {
            MyNotifyIcon.BalloonTipText = streamName +" is now live!";
            MyNotifyIcon.BalloonTipTitle = "Twitch Monitor";
            MyNotifyIcon.ShowBalloonTip(5000);
        }

        private void watchButton_Click(object sender, RoutedEventArgs e)
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();
            cmd.StandardInput.WriteLine("streamlink " + ListBox1.SelectedValue.ToString() + " best");
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
        }



        private void delButton_Click(object sender, RoutedEventArgs e)
        {
            if (ListBox1.SelectedIndex != -1)
            {
                lists.Remove(ListBox1.SelectedItem.ToString());
                ListBox1.Items.RemoveAt(ListBox1.SelectedIndex);
            }
            Properties.Settings.Default.listStrings = lists;
            Properties.Settings.Default.Save();
        }

        private void CheckLive()
        {
            for (int i = 0; i != ListBox1.Items.Count; ++i)
            {
                AsyncRequest(((Stream)ListBox1.Items.GetItemAt(i)));
            }
        }
        async Task<bool> AsyncRequest(Stream check)
        {
            using (HttpClient request = new HttpClient())
            {
                request.DefaultRequestHeaders.TryAddWithoutValidation("Client-ID", "OAuth 6wkv85t0txkdm471qaimy8mugxhzda");
                using (HttpResponseMessage response = await request.GetAsync("https://api.twitch.tv/kraken/streams/" + check.ToString().Substring(17)))
                using (HttpContent content = response.Content)
                {
                    String result = await content.ReadAsStringAsync();

                    JObject deserialize = JObject.Parse(result);
                    IList<JToken> results = deserialize["stream"].Children().ToList();

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
            Timer timer = new Timer();
            timer.Tick += new EventHandler(timer_tick);
            timer.Interval = 50000;
            timer.Start();
        }

        private void timer_tick(object sender, EventArgs e)
        {
            CheckLive();
        }
    }
}
