﻿using System;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using YoutubeExtractor;

namespace WhileFrontPlayer {
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window {
        private bool isPlay = false;
        private bool isEnd = false;
        private DispatcherTimer timer;
        private double totalms;

        public MainWindow() {
            InitializeComponent();
            //ウィンドウサイズ初期化
            //ウィンドウがあるモニターの画面サイズを取得するためにFormのAPIを呼び出してる
            WindowInteropHelper wih = new WindowInteropHelper(App.Current.MainWindow);
            int w = System.Windows.Forms.Screen.FromHandle(wih.Handle).Bounds.Width;
            this.Width = this.MinWidth = w / 4;
            this.Height = this.MinHeight = this.Width / 16 * 9;
            //イベント割当
            Handle.MouseLeftButtonDown += (o, e) => DragMove();
            CloseButton.MouseLeftButtonDown += delegate {
                if (MessageBox.Show("終了してよろしいですか？", "確認", MessageBoxButton.OKCancel) == MessageBoxResult.OK) this.Close();
            };
            this.MouseEnter += delegate {
                Preview.Visibility = Visibility.Hidden;
                Controler.Visibility = Visibility.Visible;
            };
            this.MouseLeave += delegate {
                Controler.Visibility = Visibility.Hidden;
                Preview.Visibility = Visibility.Visible;
            };
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

        private void Window_KeyUp(object sender, KeyEventArgs e) {
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
            //時間表示
            NowTime.Content = String.Format("{0:D2}:{1:D2}:{2:D2}", mediaElement.Position.Hours
                                                                  , mediaElement.Position.Minutes
                                                                  , mediaElement.Position.Seconds);
            //シークバー移動
            SeekBar.Width = SeekPreview.Width = this.Width * (mediaElement.Position.TotalMilliseconds / totalms);
        }

        private void mediaElement_MediaOpened(object sender, RoutedEventArgs e) {
            Duration Total = mediaElement.NaturalDuration;
            totalms = Total.TimeSpan.TotalMilliseconds;
            TotalTime.Content = String.Format("{0:D2}:{1:D2}:{2:D2}", Total.TimeSpan.Hours
                                                                    , Total.TimeSpan.Minutes
                                                                    , Total.TimeSpan.Seconds);
            PauseButton.Visibility = Visibility.Visible;
            timer.Start();
        }

        private void Media_Open(string FilePath) {
            mediaElement.Source = new Uri(FilePath,true); //警告が出るけど、こうしないと再生出来ないファイルがある
            if (!FilePath.StartsWith("http")) FileName.Content = System.IO.Path.GetFileName(FilePath);
            else FileName.Content = "Web";
            try {
                mediaElement.Play();
                isPlay = true;
            }
            catch {
                MessageBox.Show("このメディアは再生出来ません");
            }
        }

        private void FileName_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Title = "ファイルを開く";
            dialog.Filter = "動画ファイル|*.mp4;*.wmv";
            if (dialog.ShowDialog() == true) Media_Open(dialog.FileName);
        }

        private void SeekArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if(mediaElement.Source != null) {
                Point pos = e.GetPosition(this);
                double seekpos = totalms * (pos.X / this.Width) - mediaElement.Position.TotalMilliseconds;
                Player_Seek((int)seekpos);
            }
        }

        private void button_Click(object sender, RoutedEventArgs e) {
            string link = "https://www.youtube.com/watch?v=i9LyXB60Wdk";
            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link);

            VideoInfo video = videoInfos
                .First(info => info.VideoType == VideoType.Mp4 && info.Resolution == 720);

            if (video.RequiresDecryption) {
                DownloadUrlResolver.DecryptDownloadUrl(video);
            }
        }
    }
}
