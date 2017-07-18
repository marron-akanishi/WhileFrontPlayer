using System;
using System.Collections.Generic;
using System.Linq;
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

namespace WhileFrontPlayer
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isPlay = false;

        public MainWindow()
        {
            InitializeComponent();
            MouseLeftButtonDown += (o, e) => DragMove();
            this.Closing += delegate { if (mediaElement.Source != null) mediaElement.Stop(); };
            ExitMenu.Click += (o, e) => this.Close();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            string[] filenames = e.Data.GetData(DataFormats.FileDrop) as string[];
            if(filenames != null && filenames.Length != 0)
            {
                mediaElement.Source = new Uri(filenames[0]);
                mediaElement.Play();
                isPlay = true;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    if (MessageBox.Show("終了してよろしいですか？", "確認", MessageBoxButton.OKCancel) == MessageBoxResult.OK) this.Close();
                    break;
                case Key.Space:
                    if (mediaElement.Source != null) Menu_Pause(null,null);
                    break;
                case Key.Left:
                    if (mediaElement.Source != null) Player_Seek(-5000);
                    break;
                case Key.Right:
                    if (mediaElement.Source != null) Player_Seek(5000);
                    break;
            }
        }

        private void Menu_Pause(object sender, RoutedEventArgs e)
        {
            if (isPlay)
            {
                mediaElement.Pause();
                PauseMenu.Header = "再生";
                isPlay = false;
            }else
            {
                try { mediaElement.Play(); }
                catch { return; }
                PauseMenu.Header = "一時停止";
                isPlay = true;
            }
        }

        private void Player_Seek(int ms)
        {
            bool back = false;
            if (ms < 0) back = true;
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, Math.Abs(ms));
            if (back) mediaElement.Position -= ts;
            else mediaElement.Position += ts;
        }
    }
}
