namespace TwitchMonitor
{
    public class Stream
    {

        public Stream(string url)
        {
            Url = url;
        }

        // 

        public string Url { get; set; }

        public bool Live { get; set; }

        // 

        public void GoLive()
        {
            Live = true;
            var mainWindow = (MainWindow)System.Windows.Application.Current.MainWindow;
            mainWindow.LiveNotification(Url);
        }

        public void GoOffline()
        {
            Live = false;
        }

        public override string ToString()
        {
            return Url;
        }

    }
}