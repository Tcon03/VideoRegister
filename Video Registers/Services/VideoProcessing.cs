using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Video_Registers.Services
{
    public class VideoProcessing
    {
        public async Task GenerateImageAsync(string videoPath , string folderOutput , double frameInterval)
        {

        }
        public async Task RunFFmpegCommand(string commadFfmpeg)
        {
            string ffmpegPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg.exe");
            Log.Information("FFmpeg Path: {ffmpegPath}", ffmpegPath);
            if (!File.Exists(ffmpegPath))
            {
                Debug.WriteLine("===== Not found file FFmpeg  =====" + ffmpegPath);
                return;
            }
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
                string errorrTask = await process.StandardError.ReadToEndAsync();
                string outPutTask = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();
                if (process.ExitCode == 0)
                {
                    if (!string.IsNullOrEmpty(errorrTask))
                    {
                        Debug.WriteLine("========== Export Success ==========\n" + errorrTask);
                    }
                }
                else
                {
                    Debug.WriteLine("===== Export Video Errorr =====\n" + outPutTask);
                }
            }
        }

    }
}
