using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Video_Registers.Services
{
    public class VideoProcessing
    {
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

                await RunFFmpegCommand(ffmpegPath, ffmpegCmd);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Error generating images from video: {ErrorMessage}", ex.Message);
                return false;
            }

        }

        /// <summary>
        /// Runffmpeg command
        /// </summary>
        /// <returns></returns>
        public async Task RunFFmpegCommand(string ffmpegPath, string commadFfmpeg)
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

                    // SỬA LỖI DEADLOCK: Bắt đầu đọc stream ngay
                    Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
                    Log.Information("FFmpeg process output " + outputTask);

                    Task<string> errorTask = process.StandardError.ReadToEndAsync();
                    Log.Information("FFmpeg process error " + errorTask);

                    // Chờ cả 3 tác vụ song song hoàn thành
                    await Task.WhenAll(process.WaitForExitAsync(), outputTask, errorTask);

                    // Lấy kết quả sau khi đã await
                    string outPutResult = await outputTask;
                    string errorResult = await errorTask;

                    // SỬA LỖI LOGIC: Kiểm tra lỗi trong 'errorResult'
                    if (process.ExitCode == 0)
                    {
                        // Thành công: 'errorResult' chứa thông tin tiến trình
                        if (!string.IsNullOrEmpty(errorResult))
                        {
                            Log.Information("========== Export Success (Info) ==========\n {ProgressInfo}", errorResult);
                        }
                    }
                    else
                    {
                        // Thất bại: 'errorResult' chứa thông báo lỗi
                        Log.Error("===== Export Video Errorr (ExitCode {Code}) =====\n {Error}", process.ExitCode, errorResult);
                    }

                }
            }
            catch (Exception ex)
            {
                Log.Error("Lỗi khi chạy FFmpeg: {ErrorMessage}", ex.Message);
            }
        }

    }
}
