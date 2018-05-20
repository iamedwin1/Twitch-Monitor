namespace TwitchMonitor
{
    public class Stream
    {

        public Stream(string url)
        {
            Url = url;
            DisplayStr = " [Offline]";
        }

        public string Url { get; set; }

        public bool Live { get; set; }

        public string DisplayStr { get; set; }

        public void GoLive()
        {
            Live = true;
            DisplayStr = " [Live!]";
            var mainWindow = (MainWindow)System.Windows.Application.Current.MainWindow;
            mainWindow.LiveNotification(Url);
        }

        public void GoOffline()
        {
            Live = false;
            DisplayStr = " [Offline]";
        }

        public override string ToString()
        {
            return Url + DisplayStr;
        }
    }
}