using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Threading;
using Video_Registers.Commands;
using Video_Registers.Model;
using Video_Registers.Repositories;
using Video_Registers.Services;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

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
        public bool IsFrameProcessing
        {
            get => _frameProcess;
            set
            {
                _frameProcess = value;
                Log.Information($"FrameImage changed to: {_frameProcess}");
                RaisePropertyChanged(nameof(IsFrameProcessing));


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
                Log.Information("Stage Image Change :" + _stageImage);
                RaisePropertyChanged(nameof(StageImage));
                (RejectImageCommand as VfxCommand)?.RaiseCanExecuteChanged();
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
                CurrentFrameIndex = StageImage.IndexOf(SelectedImage);

            }
        }
        FfmpegRepository _ffmpegRepository = new FfmpegRepository();
        private readonly VideoProcessing _videoProcessing;

        public string DisplayIndexString
        {
            get
            {
                return $"{CurrentFrameIndex + 1}/{TotalImage}";
            }
        }

        private int _currentFrameIndex = -1;
        public int CurrentFrameIndex
        {
            get => _currentFrameIndex;
            set
            {
                _currentFrameIndex = value;
                Log.Information("--- Current Image ---" + _currentFrameIndex);
                RaisePropertyChanged(nameof(CurrentFrameIndex));
                RaisePropertyChanged(nameof(DisplayIndexString));

                (NextImageCommand as VfxCommand)?.RaiseCanExecuteChanged();
                (PreviousImageCommand as VfxCommand)?.RaiseCanExecuteChanged();
            }
        }

        private int _totalImage;
        public int TotalImage
        {
            get => _totalImage;
            set
            {
                _totalImage = value;
                Log.Information("Total Image change" + _totalImage);
                RaisePropertyChanged(nameof(TotalImage));
                RaisePropertyChanged(nameof(DisplayIndexString));
            }

        }

        public ICommand UploadCommand { get; set; }
        public ICommand PlayPauseCommand { get; set; }
        public ICommand MuteCommand { get; set; }
        public ICommand ClearCommand { get; set; }
        public ICommand GenerateFramesCommand { get; set; }
        public ICommand NextImageCommand { get; set; }
        public ICommand PreviousImageCommand { get; set; }

        public ICommand DeleteSelectedCommand { get; set; }
        public ICommand AcceptCommands { get; set; }
        public ICommand RejectImageCommand { get; set; }


        public MainViewModel()
        {
            UploadCommand = new VfxCommand(OnUpLoad, () => true);
            PlayPauseCommand = new VfxCommand(OnPlayPause, () => VideoSource != null);
            MuteCommand = new VfxCommand(OnMute, () => VideoSource != null);
            ClearCommand = new VfxCommand(OnClear, () => IsLoaded);
            GenerateFramesCommand = new VfxCommand(OnGenerate, () => VideoSource != null);
            NextImageCommand = new VfxCommand(OnNext, CanNext);
            PreviousImageCommand = new VfxCommand(OnPrevious, CanPrevious);
            _videoProcessing = new VideoProcessing();
            RejectImageCommand = new VfxCommand(OnReject, CanReject);
            AcceptCommands = new VfxCommand(OnSave, () => true);

        }

        private bool CanActOnFrames()
        {
            throw new NotImplementedException();
        }

        private async void OnSave(object obj)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true, // <-- Dòng này biến nó thành Folder Picker
                Title = "Chọn thư mục để lưu ảnh"
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                // 3. LẤY ĐƯỜNG DẪN THƯ MỤC
                string selectedFolder = dialog.FileName;
                bool checkProcess = await _videoProcessing.SaveStageFrame(StageImage, selectedFolder);
                if (checkProcess)
                {
                    MessageBox.Show($"Đã lưu {StageImage.Count} ảnh!");
                    OnReject(null);
                }
            }
        }

        private bool CanReject()
        {
            if (StageImage != null && StageImage.Count > 0)
                return true;
            return false;
        }

        private async void OnReject(object obj)
        {

            IsFrameProcessing = false;
            StageImage.Clear();
            TotalImage = 0;
            SelectedImage = null;

            // delete Folder Temp
            if (!string.IsNullOrEmpty(_tempFolderPath))
            {
                await _videoProcessing.DeleteFolderPath(_tempFolderPath);
                _tempFolderPath = null;
            }
        }

        private bool CanPrevious()
        {
            if (CurrentFrameIndex > 0)
                return true;
            return false;
        }

        private void OnPrevious(object obj)
        {
            SelectedImage = StageImage[CurrentFrameIndex - 1];
        }

        private bool CanNext()
        {
            if (CurrentFrameIndex < TotalImage - 1)
                return true;
            return false;
        }

        private void OnNext(object obj)
        {
            SelectedImage = StageImage[CurrentFrameIndex + 1];
        }

        /// <summary>
        ///  Generate Frame Image from video
        /// </summary>
        private async void OnGenerate(object obj)
        {
            bool ffmpegReady = await EnsureFfmpegIsReadyAsync();
            if (!ffmpegReady)
            {
                MessageBox.Show("Không thể tải FFmpeg. Vui lòng kiểm tra internet.", "Lỗi nghiêm trọng", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show("Bạn có chắc chắn muốn tạo Frame Image từ video không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }
            _tempFolderPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Log.Information("TempPath lưu ảnh ở đường dẫn :" + _tempFolderPath);
            bool processing = await _videoProcessing.GenerateImageAsync(VideoSource.LocalPath, _tempFolderPath, FrameInterval, _ffmpegRepository._ffmpegPath);

            if (processing)
            {

                // get image từ thư mục tạm
                var loadImage = _videoProcessing.LoadImageFolder(_tempFolderPath);
                MessageBox.Show("Generate Frame Image Success!!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                StageImage = new ObservableCollection<FrameImage>(loadImage);
                TotalImage = StageImage.Count;
                SelectedImage = StageImage.FirstOrDefault();
                IsFrameProcessing = true;

            }
            else
            {
                MessageBox.Show("Generate Frame Image Failed!!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Check download FFmpeg 
        /// </summary>
        private async Task<bool> EnsureFfmpegIsReadyAsync()
        {
            if (_ffmpegRepository.IsInstalled)
            {
                Log.Information("FFmpeg đã có sẵn trong thư mục.");
                return true;
            }

            try
            {
                Log.Information("FFmpeg chưa được cài đặt, bắt đầu tải...");
                await _ffmpegRepository.DownloadFfmpegAsync();

                if (_ffmpegRepository.IsInstalled)
                {
                    Log.Information("Tải FFmpeg thành công.");
                    return true;
                }
                Log.Error("Tải FFmpeg thất bại (sau khi chạy DownloadAsync).");
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Lỗi khi tải FFmpeg");
                return false;
            }
        }

        /// <summary>
        /// clear video source
        /// </summary>
        /// <param name="obj"></param>
        private void OnClear(object obj)
        {
            VideoSource = null;
            IsLoaded = false;
            IsMuted = false;
            IsPlaying = false;
            IsFrameProcessing = false;
        }

        /// <summary>
        /// trigger Mute command
        /// </summary>
        private void OnMute(object obj)
        {
            IsMuted = !IsMuted;
        }

        /// <summary>
        ///  transfer CanExecuteChanged to Command
        /// </summary>
        private void RasieCanExecuteChanged()
        {
            (PlayPauseCommand as VfxCommand)?.RaiseCanExecuteChanged();
            (MuteCommand as VfxCommand)?.RaiseCanExecuteChanged();
            (ClearCommand as VfxCommand)?.RaiseCanExecuteChanged();
            (GenerateFramesCommand as VfxCommand)?.RaiseCanExecuteChanged();

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
