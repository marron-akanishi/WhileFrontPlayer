using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WhileFrontPlayer {
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window {
        private bool isPlay = false;
        private bool isEnd = false;
        private DispatcherTimer timer;
        private Duration Total;

        public MainWindow() {
            InitializeComponent();
            //イベント割当
            Handle.MouseLeftButtonDown += (o, e) => DragMove();
            CloseButton.MouseLeftButtonDown += delegate {
                if (MessageBox.Show("終了してよろしいですか？", "確認", MessageBoxButton.OKCancel) == MessageBoxResult.OK) this.Close();
            };
            this.MouseEnter += delegate { Controler.Visibility = Visibility.Visible; };
            this.MouseLeave += delegate { Controler.Visibility = Visibility.Hidden; };
            PlayButton.MouseLeftButtonUp += (o, e) => Menu_Pause(o, null);
            PauseButton.MouseLeftButtonUp += (o, e) => Menu_Pause(o, null);
            //経過時間表示用
            timer = new DispatcherTimer();
            timer.Tick += timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 600);
        }

        private void Window_Drop(object sender, DragEventArgs e) {
            string[] filenames = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (filenames != null && filenames.Length == 1) Media_Open(filenames[0]);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.Escape:
                    if (MessageBox.Show("終了してよろしいですか？", "確認", MessageBoxButton.OKCancel) == MessageBoxResult.OK) this.Close();
                    break;
                case Key.Space:
                    if (mediaElement.Source != null) Menu_Pause(null, null);
                    break;
                case Key.Left:
                    if (mediaElement.Source != null) Player_Seek(-5000);
                    break;
                case Key.Right:
                    if (mediaElement.Source != null) Player_Seek(5000);
                    break;
            }
        }

        private void Menu_Pause(object sender, RoutedEventArgs e) {
            if (isPlay) {
                mediaElement.Pause();
                PauseButton.Visibility = Visibility.Hidden;
                PlayButton.Visibility = Visibility.Visible;
                timer.Stop();
                isPlay = false;
            } else {
                if (isEnd) {
                    mediaElement.Stop();
                    isEnd = false;
                }
                try { mediaElement.Play(); }
                catch { return; }
                PlayButton.Visibility = Visibility.Hidden;
                PauseButton.Visibility = Visibility.Visible;
                timer.Start();
                isPlay = true;
            }
        }

        private void Player_Seek(int ms) {
            bool back = false;
            if (ms < 0) back = true;
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, Math.Abs(ms));
            if (back) mediaElement.Position -= ts;
            else mediaElement.Position += ts;
            timer_Tick(null, null);
        }

        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e) {
            PauseButton.Visibility = Visibility.Hidden;
            PlayButton.Visibility = Visibility.Visible;
            isPlay = false;
            isEnd = true;
        }

        private void timer_Tick(object sender, EventArgs e) {

            string temp = String.Format("{0:D2}:{1:D2}:{2:D2}", mediaElement.Position.Hours
                                                              , mediaElement.Position.Minutes
                                                              , mediaElement.Position.Seconds);
            temp += String.Format("/{0:D2}:{1:D2}:{2:D2}", Total.TimeSpan.Hours
                                                         , Total.TimeSpan.Minutes
                                                         , Total.TimeSpan.Seconds);
            Time.Content = temp;
        }

        private void mediaElement_MediaOpened(object sender, RoutedEventArgs e) {
            Total = mediaElement.NaturalDuration;
            PauseButton.Visibility = Visibility.Visible;
            timer.Start();
        }

        private void Media_Open(string FilePath) {
            mediaElement.Source = new Uri(FilePath,true); //警告が出るけど、こうしないと再生出来ないファイルがある
            FileName.Content = System.IO.Path.GetFileName(FilePath);
            try { mediaElement.Play(); }
            catch {
                MessageBox.Show("このメディアは再生出来ません");
                return;
            }
            isPlay = true;
        }

        private void FileName_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Title = "ファイルを開く";
            dialog.Filter = "動画ファイル|*.mp4;*.wmv";
            if (dialog.ShowDialog() == true) Media_Open(dialog.FileName);
        }
    }
}
