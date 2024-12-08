using LibVLCSharp.Shared;
using System.Windows;
using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;
using Point = System.Windows.Point;
using System.Windows.Input;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfApp.Tools.Vlc
{
    /// <summary>
    /// VlcPlayerWindow.xaml 的交互逻辑
    /// </summary>
    public partial class VlcPlayerWindow : Window
    {
        public VlcPlayerWindow()
        {
            InitializeComponent();
        }
        string url = $@"E:\home\github\WPFOpenTK\WPF-OpenTK\WpfOpenTKApp\WpfApp1\Tools\Vlc\file\xy.jpg";//01.Docker -Docker 容器的数据管理简介.mp4
        LibVLC _libvlc;
        MediaPlayer player;
        private Point _mouseDownPoint;
        private List<string> _videoFiles;
        private int _currentVideoIndex;
        private bool _isMouseDown = false;
        float yaw = 0;//左右
        float pitch = 0;//上下
        float roll = 0;
        float changeViewFactor = 0.5f;
        long totalLength = 0;

        private float fov = 80;
        public float Fov
        {
            get
            {
                return fov;
            }
            set
            {
                if (value > 20 && value < 150)
                {
                    fov = value;
                }

            }
        }
        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            InitializeVlc();
            PlayNextVideo();
        }
        private void InitializeVlc()
        {
            var libOptions = new[]
           {
                ":spherical-video",
                ":video-projection=equirectangular",
                "--video-filter=sphere",
                "--video-filter=deinterlace",
            };
            // 加载媒体文件

            _libvlc = new LibVLC();
            player = new MediaPlayer(_libvlc);

            video_main.Width = this.Width * 0.8;
            video_main.Height = this.Height * 0.8;
            stackPanel.Width = video_main.Width;
            stackPanel.Height = video_main.Height;
            video_main.MediaPlayer = player;
            video_main.MediaPlayer.PositionChanged += MediaPlayer_MediaPlayerPositionChanged;
            //通过设置宽高比为窗体宽高可达到视频铺满全屏的效果
            player.AspectRatio = this.Width + ":" + this.Height;
            player.Scale = 0.3f;
            //this.video_main.MediaPlayer.Media= new Media(_libvlc, new Uri(url));
            // video_main.MediaPlayer.Play(video_main.MediaPlayer.Media);

            var mediaOptions = new[]
            {
            //            ":spherical-video",
            //            ":video-projection=equirectangular",
            //"--spherical=1", // 启用球面视频渲染
            "--video-filter=sphere",
            "--video-filter=deinterlace", // 启用去交错滤镜
   
            //":video-rescale=1280,720", // 设置视频分辨率为1280x720
            //"--sout", "#transcode{vqa=1,fps=24,deinterlace=1}:standard{access=file,mux=ogg,dst=vlc_vr_output.mp4}"
            };

            video_main.MediaPlayer.EnableMouseInput = false;
            video_main.MediaPlayer.EnableKeyInput = false;

            //video_main.MediaPlayer.Media = new Media(_libvlc, new Uri(url));
            //video_main.MediaPlayer.Play(video_main.MediaPlayer.Media);

            _videoFiles = new List<string>
        {
            url
        };
            _currentVideoIndex = 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (video_main.MediaPlayer == null)
                return;
            video_main.MediaPlayer.Scale += 0.1f;

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (video_main.MediaPlayer == null)
                return;
            yaw += 5;
            video_main.MediaPlayer.UpdateViewpoint(yaw, pitch, roll, Fov, true);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (video_main.MediaPlayer != null && video_main.MediaPlayer.IsPlaying)
            {
                playStaus.Content = "播放";
                video_main.MediaPlayer.Pause();
            }
            if (video_main.MediaPlayer != null && video_main.MediaPlayer.State == VLCState.Paused)
            {
                playStaus.Content = "暂停";
                video_main.MediaPlayer.Play();
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (video_main.MediaPlayer != null)
            {
                PlayNextVideo();

            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (video_main.MediaPlayer == null)
                return;
            if (rate.Content.ToString() == "加速")
            {
                video_main.MediaPlayer.SetRate(1.5f);
                rate.Content = "1.5倍速播放中";
            }
            else if (rate.Content.ToString() == "1.5倍速播放中")
            {
                video_main.MediaPlayer.SetRate(2f);
                rate.Content = "2倍速播放中";
            }
            else if (rate.Content.ToString() == "2倍速播放中")
            {
                video_main.MediaPlayer.SetRate(3f);
                rate.Content = "3倍速播放中";
            }
            else
            {
                video_main.MediaPlayer.SetRate(1.0f);
                rate.Content = "加速";
            }

        }

        private void stackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point currentPoint = e.GetPosition(video_main);
            _isMouseDown = true;
        }
        private void stackPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = false;
        }
        private void stackPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _isMouseDown)
            {
                Point currentPoint = e.GetPosition(video_main);
                double offsetX = (currentPoint.X - _mouseDownPoint.X) * changeViewFactor;
                double offsetY = (currentPoint.Y - _mouseDownPoint.Y) * changeViewFactor;
                _mouseDownPoint = currentPoint;
                if (video_main.MediaPlayer == null || Math.Abs(offsetX) > 10 || Math.Abs(offsetY) > 10)
                    return;
                yaw += (float)offsetX;
                pitch -= (float)offsetY;
                video_main.MediaPlayer.UpdateViewpoint(yaw, pitch, roll, Fov, true);
            }
        }

        private void stackPanel_MouseWheel_1(object sender, MouseWheelEventArgs e)
        {
            if (video_main.MediaPlayer == null)
                return;
            if (e.Delta > 0)
            {
                //video_main.MediaPlayer.Scale += 0.1f;
                Fov -= 1;
            }
            else
            {
                //video_main.MediaPlayer.Scale -= 0.1f;
                Fov += 1;
            }

            video_main.MediaPlayer.UpdateViewpoint(yaw, pitch, roll, Fov, true);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            video_main.Width = this.Width * 0.8;
            video_main.Height = this.Height * 0.8;
            stackPanel.Width = video_main.Width;
            stackPanel.Height = video_main.Height;

        }

        private void MediaPlayer_MediaPlayerPositionChanged(object sender, MediaPlayerPositionChangedEventArgs e)
        {
            progressSlider.ValueChanged -= progressSlider_ValueChanged;
            double newPosition = (float)e.Position;
            Dispatcher.Invoke(() =>
            {
                progressSlider.Value = newPosition;

                GetPlayTime(e.Position * totalLength);

            });
            progressSlider.ValueChanged += progressSlider_ValueChanged;
        }
        private void progressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (video_main.MediaPlayer != null)
            {
                video_main.MediaPlayer.PositionChanged -= MediaPlayer_MediaPlayerPositionChanged;
                video_main.MediaPlayer.Position = (float)progressSlider.Value;
                GetPlayTime(video_main.MediaPlayer.Position * totalLength);
                video_main.MediaPlayer.PositionChanged += MediaPlayer_MediaPlayerPositionChanged;
            }

        }

        public string GetPlayTime(float NowLength)
        {
            if (video_main.MediaPlayer == null)
            {
                this.playTime.Content = "";
                return "";
            }
            totalLength = video_main.MediaPlayer.Length / 1000;

            //float NowLength =video_main.MediaPlayer.Position / 1000;

            string result = totalLength == 0 ? "" : $@"{ConvertSecondsToHMSFormat((int)NowLength)}:{ConvertSecondsToHMSFormat((int)totalLength)}";
            this.playTime.Content = result;
            return result;
        }

        public static string ConvertSecondsToHMSFormat(int seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            return time.ToString("hh\\:mm\\:ss");
        }
        private void PlayNextVideo()
        {
            if (video_main.MediaPlayer == null)
            {
                return;
            }
            if (_currentVideoIndex >= _videoFiles.Count)
            {
                _currentVideoIndex = 0;
            }

            string videoFile = _videoFiles[_currentVideoIndex];
            video_main.MediaPlayer.Play(new Media(_libvlc, videoFile, FromType.FromPath));
            totalLength = this.video_main.MediaPlayer.Length;
            _currentVideoIndex++;

        }
        private void BindProgressSlider()
        {
            if (video_main.MediaPlayer == null)
                return;
            video_main.MediaPlayer.TimeChanged += (sender, args) =>
            {
                var position = (double)args.Time / video_main.MediaPlayer.Length;
                progressSlider.Dispatcher.Invoke(() =>
                {
                    progressSlider.Value = position;
                });
            };

        }

      
    }
}
