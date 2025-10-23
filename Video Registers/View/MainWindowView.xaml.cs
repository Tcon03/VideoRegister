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
using System.Windows.Shapes;
using System.Windows.Threading;
using Video_Registers.ViewModel;

namespace Video_Registers.View
{
    /// <summary>
    /// Interaction logic for MainWindowView.xaml
    /// </summary>
    public partial class MainWindowView : Window
    {
        private MainViewModel _viewModel;
        public MainWindowView()
        {
            InitializeComponent();
            TimeDisplaym();
        }

        private void TimeDisplaym()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Clip; // Giữ lại Timer
            timer.Start();
        }

        private void Timer_Clip(object? sender, EventArgs e)
        {
            if (videoPlayer.NaturalDuration.HasTimeSpan)
            {
                if (videoPlayer != null)
                {
                    if (videoPlayer.NaturalDuration.HasTimeSpan)
                    {
                        timeDisplay.Text = String.Format("{0} / {1}", videoPlayer.Position.ToString(@"mm\:ss"), videoPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
                    }
                }
            }
        }

  

        private void Rewind_Click(object sender, RoutedEventArgs e)
        {
            var currentPosition = videoPlayer.Position.TotalSeconds;
            if (currentPosition > 5)
            {
                videoPlayer.Position = videoPlayer.Position - TimeSpan.FromSeconds(10); // Rewind the video by 10 seconds
            }
            else
            {
                videoPlayer.Position = TimeSpan.Zero; // Rewind to the start
            }
        }
    }
}
