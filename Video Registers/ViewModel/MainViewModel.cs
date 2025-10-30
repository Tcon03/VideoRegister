using Microsoft.Win32;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Video_Registers.Commands;
using Video_Registers.Model;
using Video_Registers.Repositories;
using Video_Registers.Services;

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
        private bool _frameProcess;
        public bool ProcessFrame
        {
            get => _frameProcess;
            set
            {
                _frameProcess = value;
                Log.Information($"FrameImage changed to: {_frameProcess}");
                RaisePropertyChanged(nameof(ProcessFrame));
            }
        }
        private string _tempFolderPath;

        #endregion

        private ObservableCollection<FrameImage> _stageImage;
        public ObservableCollection<FrameImage> StageImage
        {
            get => _stageImage;
            set
            {
                _stageImage = value;
                RaisePropertyChanged(nameof(StageImage));
            }
        }

        private FrameImage _selectedImage; 
        public FrameImage SelectedImage
        {
            get => _selectedImage;
            set
            {
                _selectedImage = value;
                RaisePropertyChanged(nameof(SelectedImage));
            }
        }
        private readonly FfmpegRepository _ffmpegRepository = new FfmpegRepository();
        private readonly VideoProcessing _videoProcessing;
        public ICommand UploadCommand { get; set; }
        public ICommand PlayPauseCommand { get; set; }
        public ICommand MuteCommand { get; set; }
        public ICommand ClearCommand { get; set; }
        public ICommand GenerateFramesCommands { get; set; }

        public ICommand DownloadFFmpegCommands { get; set; }


        public MainViewModel()
        {
            UploadCommand = new VfxCommand(OnUpLoad, () => true);
            PlayPauseCommand = new VfxCommand(OnPlayPause, () => VideoSource != null);
            MuteCommand = new VfxCommand(OnMute, () => VideoSource != null);
            ClearCommand = new VfxCommand(OnClear, () => IsLoaded);
            GenerateFramesCommands = new VfxCommand(OnGenerate, () => VideoSource != null);
            _videoProcessing = new VideoProcessing();
            OnDownloadFFmpeg();
        }

        public async void OnDownloadFFmpeg()
        {
            if (_ffmpegRepository.IsInstalled)
            {
                Log.Information("FFmpeg is already installed. No need to download.");
                return;
            }
            try
            {
                await _ffmpegRepository.DownloadFfmpegAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error downloading FFmpeg: " + ex.Message);
            }
        }

        private async void OnGenerate(object obj)
        {
            _tempFolderPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Log.Information("TempPath lưu ảnh ở đường dẫn :" + _tempFolderPath);
            bool processing = await _videoProcessing.GenerateImageAsync(VideoSource.LocalPath, _tempFolderPath, FrameInterval, _ffmpegRepository._ffmpegPath);
            if (processing)
            {
                var loadImage = _videoProcessing.LoadImageFolder(_tempFolderPath);
                MessageBox.Show("Generate Frame Image Success!!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                StageImage = new ObservableCollection<FrameImage>(loadImage); 
                SelectedImage = StageImage.FirstOrDefault();
                ProcessFrame = true;

            }
            else
            {
                MessageBox.Show("Generate Frame Image Failed!!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnClear(object obj)
        {
            VideoSource = null;
            IsLoaded = false;
            IsMuted = false;
            IsPlaying = false;
            ProcessFrame = false;
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
