namespace TwitchMonitor
{
    public class Stream
    {
        public string URL { get; set; }
        public bool live { get; set; } = false;
        public Stream(string URL)
        {
            this.URL = URL;
        }
        public void goLive()
        {
            live = true;
            var Test = (MainWindow)System.Windows.Application.Current.MainWindow;
            Test.LiveNotification(URL);
        }

        public void goOffline()
        {
            live = false;
        }

        public override string ToString()
        {
            return URL;
        }

    }
}