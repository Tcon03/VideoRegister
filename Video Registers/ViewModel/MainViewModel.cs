using Microsoft.Win32;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Video_Registers.Commands;

namespace Video_Registers.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region
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
                RasieCanExecuteChanged();
            }
        }
        private bool _IsLoaded;
        public bool IsLoaded
        {
            get => _IsLoaded;
            set
            {
                _IsLoaded = value;
                Log.Information($"IsLoaded changed to: {_IsLoaded}");
                RaisePropertyChanged(nameof(IsLoaded));
                RasieCanExecuteChanged();
            }
        }

        private bool _isMuted;
        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                _isMuted = value;
                Log.Information($"IsMuted changed to: {_isMuted}");
                RaisePropertyChanged(nameof(IsMuted));
            }
        }

        private double _volume;
        public double Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                Log.Information($"Volume changed to: {_volume}");
                RaisePropertyChanged(nameof(Volume));
            }
        }

        private double _frameInterval = 1.0;
        public double FrameInterval
        {
            get => _frameInterval;
            set
            {
                _frameInterval = value;
                Log.Information($"FrameInterval changed to: {_frameInterval}");
                RaisePropertyChanged(nameof(FrameInterval));
            }
        }


        #endregion

        public ICommand UploadCommand { get; set; }
        public ICommand PlayPauseCommand { get; set; }
        public ICommand MuteCommand { get; set; }
        public ICommand ClearCommand { get; set; }
        public ICommand GenerateFramesCommands { get; set; }


        public MainViewModel()
        {
            UploadCommand = new VfxCommand(OnUpLoad, () => true);
            PlayPauseCommand = new VfxCommand(OnPlayPause, () => VideoSource != null);
            MuteCommand = new VfxCommand(OnMute, () => VideoSource != null);
            ClearCommand = new VfxCommand(OnClear, () => IsLoaded);
            GenerateFramesCommands = new VfxCommand(OnGenerate, () => VideoSource !=null);
        }

        private void OnGenerate(object obj)
        {
            throw new NotImplementedException();
        }

        private void OnClear(object obj)
        {
            VideoSource = null;
            IsLoaded = false;
            IsMuted = false;
            IsPlaying = false;
        }

        private void OnMute(object obj)
        {
            IsMuted = !IsMuted;
        }

        private void RasieCanExecuteChanged()
        {
            (PlayPauseCommand as VfxCommand)?.RaiseCanExecuteChanged();
            (MuteCommand as VfxCommand)?.RaiseCanExecuteChanged();
            (ClearCommand as VfxCommand)?.RaiseCanExecuteChanged(); 
            (GenerateFramesCommands as VfxCommand)?.RaiseCanExecuteChanged();

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
                //chuyển thành Uri để gán cho MediaElement
                VideoSource = new Uri(videoPath);
                IsLoaded = true;
                IsMuted = false;
                IsPlaying = true;
            }
        }
    }
}
