using Microsoft.Win32;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Video_Registers.Commands;

namespace Video_Registers.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private Uri _videoSource;
        public Uri VideoSource
        {
            get => _videoSource;
            set
            {
                _videoSource = value;
                Log.Information($"VideoSource changed to: {_videoSource}");
                RaisePropertyChanged(nameof(VideoSource));
            }
        }

        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                _isPlaying = value;
                Log.Information($"IsPlaying changed to: {_isPlaying}");
                RaisePropertyChanged(nameof(IsPlaying));
                (PlayPauseCommand as VfxCommand)?.RaiseCanExecuteChanged();
            }
        }


        public ICommand UploadCommand { get; set; }
        public ICommand PlayPauseCommand { get; set; }
        public MainViewModel()
        {
            UploadCommand = new VfxCommand(OnUpLoad, () => true);
            PlayPauseCommand = new VfxCommand(OnPlayPause, CanPlayPause);
        }

        private bool CanPlayPause()
        {
            if (VideoSource == null)
                return false;
            return true;
        }

        private void OnPlayPause(object obj)
        {
            IsPlaying = !IsPlaying;
        }

        private void OnUpLoad(object obj)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Video files (*.mp4)|*.mp4";
            if (openFileDialog.ShowDialog() == true)
            {
                var videoPath = openFileDialog.FileName;
                VideoSource = new Uri(videoPath);
                IsPlaying = true;
            }
        }
    }
}
