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
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Video_Registers.Model;

namespace Video_Registers.Services
{
    public class VideoProcessing
    {

        public ObservableCollection<FrameImage> LoadImageFolder(string folderImageData)
        {
            try
            {
                var imageList = new ObservableCollection<FrameImage>();
                if (!Directory.Exists(folderImageData))
                {
                    Log.Error("Không tìm thấy dữ liệu ở folder ");
                    return imageList;
                }
                string[] imageFiles = Directory.GetFiles(folderImageData, "*.png");

                Log.Information(" Image Files " + imageFiles.ToString());
                foreach (var images in imageFiles)
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = new Uri(images, UriKind.Absolute);
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // Tải ngay
                    bitmapImage.EndInit();
                    bitmapImage.Freeze(); // Cho phép UI truy cập
                                          // thêm các dữ liệu vào List 
                    imageList.Add(new FrameImage
                    {
                        ImageSource = bitmapImage,
                        FilePathImage = images // Lưu lại đường dẫn tạm
                    });
                }
                return imageList;
            }
            catch (Exception ex)
            {
                Log.Error("gặp vấn đề lỗi ở Hàm LoadImageFolder!!" + ex);
                return default;
            }
        }


        public async Task<bool> GenerateImageAsync(string videoPath, string folderOutput, double frameInterval, string ffmpegPath)
        {
            if (!File.Exists(ffmpegPath))
            {
                Log.Information("Không tìm thấy file ffmpeg để chạy chương trình");
                return false;
            }
            try
            {

                if (!Directory.Exists(folderOutput))
                {
                    Log.Information("Creating output folder at: {FolderOutput}", folderOutput);
                    Directory.CreateDirectory(folderOutput);
                }
                var ffmpegCmd = $"-i \"{videoPath}\" -vf fps={1.0 / frameInterval} \"{folderOutput}/image_%01d.png\"";
                Log.Information("Running FFmpeg command: {FfmpegCmd}", ffmpegCmd);
                bool checkRun = await RunFFmpegCommand(ffmpegPath, ffmpegCmd);
                if (checkRun)
                {
                    Log.Information("==== Image generation completed successfully. ====");
                    return true;
                }
                else
                {
                    Log.Error("==== Image generation failed during FFmpeg execution.=====");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error("=== Error generating images from video: {ErrorMessage} ==== ", ex.Message);
                return false;
            }

        }

        /// <summary>
        /// Runffmpeg command
        /// </summary>
        /// <returns></returns>
        public async Task<bool> RunFFmpegCommand(string ffmpegPath, string commadFfmpeg)
        {
            try
            {

                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = commadFfmpeg,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                };

                using (Process process = new Process())
                {
                    process.StartInfo = processStartInfo;
                    process.Start();

                    // đọc steam đầu ra và lỗi không đồng bộ
                    Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
                    Log.Information("==== FFmpeg process output ====== : \n" + outputTask);

                    Task<string> errorTask = process.StandardError.ReadToEndAsync();
                    Log.Information("==== FFmpeg process error ======= : \n" + errorTask);

                    // Chờ cả 3 tác vụ song song hoàn thành
                    await Task.WhenAll(process.WaitForExitAsync(), outputTask, errorTask);

                    string errorResult = await errorTask;

                    if (process.ExitCode == 0)
                    {
                        Log.Information("========== Export Success (Info) ==========\n {ProgressInfo}", errorResult);
                    }
                    else
                    {
                        MessageBox.Show("FFmpeg Error: " + errorResult, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    return true;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("FFmpeg Error: " + ex, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

    }
}
