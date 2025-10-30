using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Video_Registers.ViewModel;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace Video_Registers.Repositories
{
    public class FfmpegRepository : ViewModelBase
    {


        private string _ffmpegFolderPath;
        public string _ffmpegPath;


        private bool _isInstalled;
        public bool IsInstalled
        {
            get => _isInstalled;
            set
            {
                _isInstalled = value;
                Log.Information("FFmpeg installation status changed to: {IsInstalled}", IsInstalled);
                RaisePropertyChanged(nameof(IsInstalled));
            }
        }

        public FfmpegRepository()
        {
            InitialFolder();
            FFmpeg.SetExecutablesPath(_ffmpegFolderPath);
            IsInstalled = CheckFfmpegInstalled();

        }

        /// <summary>
        /// Get folder ffmpeg
        /// </summary>
        public void InitialFolder()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            string appName = Assembly.GetExecutingAssembly().GetName().Name ?? "VideoRegisterFF";

            _ffmpegFolderPath = Path.Combine(appData, appName, "ffmpeg-file");
            Log.Information("FFmpeg folder path: {FfmpegFolderPath}", _ffmpegFolderPath);

            _ffmpegPath = Path.Combine(_ffmpegFolderPath, "ffmpeg.exe");
            Log.Information("FFmpeg executable path: {FfmpegPath}", _ffmpegPath);

        }

        /// <summary>
        /// check ffmpeg đã được tải về chưa
        /// </summary>
        /// <returns> trả về true nếu file tồn tại và bỏ qua  </returns>
        private bool CheckFfmpegInstalled()
        {
            return File.Exists(_ffmpegPath);
        }

        public async Task DownloadFfmpegAsync()
        {
            if (IsInstalled == true)
            {
                Log.Information("FFmpeg đã có sẵn trong thư mục: {FfmpegPath}", _ffmpegPath);
                return;
            }

            try
            {
                Log.Information("Đang tải FFmpeg vào: {Folder}", _ffmpegFolderPath);
                await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, _ffmpegFolderPath);

                IsInstalled = CheckFfmpegInstalled();
                Log.Information("FFmpeg sẵn sàng tại: {Path}", _ffmpegPath);
            }
            catch (Exception ex)
            {
                Log.Error("Lỗi khi tải FFmpeg: {ErrorMessage}", ex.Message);
            }
        }
    }
}
