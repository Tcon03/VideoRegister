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
    public class FfmpegRepository :ViewModelBase
    {


        // Cờ (flag) để theo dõi trạng thái, tránh kiểm tra file liên tục
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
            //5  Báo cho Xabe biết nơi tìm ffmpeg/ffprobe
            FFmpeg.SetExecutablesPath(_ffmpegFolderPath);
            IsInstalled = CheckFfmpegInstalled();

        }

        public void InitialFolder()
        {
            //1. lấy đường dẫn của app data cục bộ 
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            //2. lấy tên ứng dụng hiện tại 
            string appName = Assembly.GetExecutingAssembly().GetName().Name ?? "VideoRegisterFF";

            //3. Kết hợp đường dẫn của 2 cái trên với tên thư mục ffmpeg_files để tạo đường dẫn đầy đủ 
            _ffmpegFolderPath = Path.Combine(appData, appName, "ffmpeg-file");
            Log.Information("FFmpeg folder path: {FfmpegFolderPath}", _ffmpegFolderPath);

            // 4. Đây là đường dẫn đầy đủ tới file ta cần kiểm tra 
            _ffmpegPath = Path.Combine(_ffmpegFolderPath, "ffmpeg.exe");
            Log.Information("FFmpeg executable path: {FfmpegPath}", _ffmpegPath);

        }

        /// <summary>
        /// check ffmpeg đã được tải về chưa
        /// </summary>
        /// <returns> trả về true nếu file tồn tại và bỏ qua  </returns>
        private bool CheckFfmpegInstalled()
        {
            return  File.Exists(_ffmpegPath);
        }

        public async Task DownloadFfmpegAsync()
        {
            if (IsInstalled==true)
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
