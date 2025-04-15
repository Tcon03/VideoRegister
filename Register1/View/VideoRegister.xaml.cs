using System;
using MahApps.Metro.IconPacks;
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
using Microsoft.Win32;
using System.Windows.Threading;
using System.IO;
using System.Diagnostics;
using Register1.Model;
using System.Collections.ObjectModel;
using Register1.ViewModel;
using Xceed.Wpf.AvalonDock.Layout;

namespace Register1.View
{


    public partial class VideoRegister : Window
    {


        private bool isPlaying = false;
        ChangeProperty changeProperty = new ChangeProperty();

        private string[] _imageFiles;
        private int _currentImageIndex = 0;
        private string FrameFolderPath;


        /// <summary>
        ///  Constructor for VideoRegister
        /// </summary>
        public VideoRegister()
        {
            DataContext = changeProperty; // gán DataContext cho đối tượng changeProperty 
            InitializeComponent();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);  // đặt khoang thời gian là 1 giây
            timer.Tick += Timer_Clip; // gọi hàm Timer_Clip 
            timer.Start(); // bắt đầu chạy hàm Timer_Clip
        }

        /// <summary>
        ///  Hàm để cập nhật thời gian video đang chạy
        /// </summary>
        private void Timer_Clip(object? sender, EventArgs e)
        {
            if (videoPlayer != null)
            {
                if (videoPlayer.NaturalDuration.HasTimeSpan)
                {
                    timeDisplay.Text = String.Format("{0} / {1}", videoPlayer.Position.ToString(@"mm\:ss"), videoPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
                }
            }
        }

        /// <summary>
        /// Upload video từ máy tính lên
        /// </summary>
        private void Click_UploadVideo(object sender, RoutedEventArgs e)
        {


            OpenFileDialog opFile = new OpenFileDialog();
            opFile.Filter = "Video files (*.mp4)|*.mp4|All files (*.*)|*.*";
            opFile.Title = "Select a video file";

            if (opFile.ShowDialog() == true)
            {
                changeProperty.videoPath = opFile.FileName; // Get the selected file path
                videoPlayer.Source = new Uri(changeProperty.videoPath); // Set the video source
                videoPlayer.Play(); // Play the video
            }


            //OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.Filter = "Image Files |*.png;*.jpg ;*.jpeg ";
            //openFileDialog.Title = "select Image File"; 
            //if(openFileDialog.ShowDialog() == true)
            //{
            //    string filePathImage = openFileDialog.FileName; // Get the selected file path 
            //    // chuyển đổi ảnh từ đường dẫn thành bitmap 
            //    BitmapImage bitmap = new BitmapImage(new Uri(filePathImage)); // tạo đối tượng bitmap từ đường dẫn file
            //    disPlayImage.Source = bitmap; // gán ảnh vào đối tượng disPlayImage
            //}
        }

        /// <summary>
        /// Dừng video hoặc Play 
        /// </summary>
        private void PauseVideo(object sender, MouseButtonEventArgs e)
        {
            if (isPlaying)
            {
                videoPlayer.Play(); // Play the video 
                playPauseIcon.Kind = PackIconMaterialKind.Pause;

            }
            else
            {
                videoPlayer.Pause();  // đang dùng là false 
                playPauseIcon.Kind = PackIconMaterialKind.Play;
            }
            isPlaying = !isPlaying;
        }

        /// <summary>
        /// Tắt bật tiếng video 
        /// </summary>
        private void MuteVideo(object sender, MouseButtonEventArgs e)
        {
            if (videoPlayer.IsMuted)
            {
                videoPlayer.IsMuted = false;  // false là mở âm thanh
                muteVideoOf.Kind = PackIconMaterialKind.VolumeHigh;
            }
            else
            {
                videoPlayer.IsMuted = true; // true là tắt âm thanh 
                muteVideoOf.Kind = PackIconMaterialKind.VolumeOff;
            }

        }



        /// <summary>
        /// Tua ngược video lại 10s 
        /// </summary>
        private void RewindVideo(object sender, MouseButtonEventArgs e)
        {
            /*  position :  là vị trí hiện tại của video 
             *  TotalSeconds :  là tổng số giây của video
             */
            if (videoPlayer != null)
            {
                var currentPosition = videoPlayer.Position.TotalSeconds;
                if (currentPosition > 5)
                {
                    videoPlayer.Position = videoPlayer.Position - TimeSpan.FromSeconds(10); // Rewind the video by 5 seconds
                }
                else
                {
                    videoPlayer.Position = TimeSpan.Zero; // Rewind to the start
                }
            }
        }


        /// <summary>
        /// Tạo ảnh từ video 
        /// </summary>
        private async void generate_ImageClick(object sender, RoutedEventArgs e)
        {

            if (videoPlayer.Source != null)
            {

                string timeInputVideo = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                string folderFrame = System.IO.Path.GetFileNameWithoutExtension(videoPlayer.Source.LocalPath);

                string resultFolder = $"{folderFrame}_{timeInputVideo}";

                FrameFolderPath = System.IO.Path.Combine("FrameImage", resultFolder);

                double frameInterval = (double)tolGiay.Value;

                if (!Directory.Exists(FrameFolderPath))
                {
                    Directory.CreateDirectory(FrameFolderPath); // tạo thư mục chứa ảnh
                }

                string ffmpegCmd = $"-i \"{changeProperty.videoPath}\" -vf fps={1.0 / frameInterval} \"{FrameFolderPath}/frame_%01d.png\"";


                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = ffmpegCmd,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                Process process = new Process
                {
                    StartInfo = processStartInfo
                };

                process.Start();
                await process.WaitForExitAsync();

                MessageBox.Show("Cắt ảnh thành công rồi nhé ! ", "Thông báo ", MessageBoxButton.OK, MessageBoxImage.Information);
                if (changeProperty?.ImageList != null)
                {
                    changeProperty.ImageList.Clear();
                }
                else
                {
                    changeProperty.ImageList = new ObservableCollection<ImageData>();
                }
                LoadImageFolder(FrameFolderPath);
            }

        }

        /// <summary>
        /// Hàm tải ảnh từ thư mục và hiển thị ảnh 
        /// </summary>
        private void LoadImageFolder(string folderImage)
        {

            try
            {
                _imageFiles = Directory.GetFiles(folderImage, "*.png");
                _currentImageIndex = 0; // đặt chỉ số ảnh hiện tại về 0
                foreach (string file in _imageFiles)
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = new Uri(file, UriKind.RelativeOrAbsolute);
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();

                    ImageData image = new ImageData
                    {
                        Image = bitmapImage,
                        videoPath = file // đường dẫn đến ảnh 
                    };

                    changeProperty.ImageList.Add(image);

                    disPlayImage.Source = bitmapImage;
                }
                if (_imageFiles.Length > 0)
                {
                    DisplayImageFromPath(_imageFiles[_currentImageIndex]);
                }
                else
                {
                    MessageBox.Show("Không tìm thấy ảnh trong thư mục đã chọn.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải ảnh: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Hàm hiển thị ảnh giao thức với lớp BitmapImage
        /// </summary>
        private void DisplayImageFromPath(string pathName)
        {
            try
            {
                /* BitmapImage là một lớp trong WPF dùng để hiển thị ảnh  - tạo ra 1 đối tượng ảnh trống 
                 * 
                 * B1 : Khời tạo đối tượng BitmapImage dùng để hiển thị ảnh 
                 * - dùng BeginInit() để khởi tạo đối tượng BitmapImage 
                 * - dùng UriSource để lấy đường dẫn đến ảnh mà bạn muốn truyền vào - pathName dường dẫn đến ảnh -  UriKind.RelativeOrAbsolute là kiểu đường dẫn tương đối và tuyệt đối
                 *  - CacheOption là tùy chọn bộ nhớ cache - OnLoad là tải ảnh vào bộ nhớ cache ngay lập tức - ảnh sẽ đc tải vào bộ nhớ BitmapImage khi được khởi tạo và giải phóng tài nguyên
                 *  - EndInit() để kết thúc khởi tạo đối tượng BitmapImage
                 *  - Freeze() để giải phóng tài nguyên 
                 * B2 : Khởi tạo đối tượng ImageData để lưu trữ ảnh 
                 *  - gán ảnh vào thuộc tính Image của đối tượng ImageData 
                 * B3 : gán ảnh vào đối tượng disPlayImage 
                 * B4 : thêm ảnh vào ImageList
                 *  
                 */
                BitmapImage imageData = new BitmapImage();
                imageData.BeginInit();
                imageData.UriSource = new Uri(pathName, UriKind.RelativeOrAbsolute);
                imageData.CacheOption = BitmapCacheOption.OnLoad;
                imageData.EndInit();
                imageData.Freeze();
                disPlayImage.Source = imageData;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi hiển thị ảnh: " + ex.Message);
            }
        }

        private void Delete_Click(object sender, MouseButtonEventArgs e)
        {
            var imageDelete = (sender as FrameworkElement)?.DataContext as ImageData; // lấy đường dẫn ảnh từ đối tượng ImageData

            if (imageDelete != null)
            {
                changeProperty.ImageList.Remove(imageDelete); // xóa ảnh trong ImageList

                string imagePath = imageDelete.videoPath; // lấy đường dẫn từ image path 

                File.Delete(imagePath); // xóa ảnh trong thư mục 

                _imageFiles = Directory.GetFiles(FrameFolderPath, "*.png"); // lấy lại danh sách ảnh trong thư mục
                if (changeProperty.ImageList.Count == 0)
                {
                    disPlayImage.Source = null; // nếu không còn ảnh nào thì xóa ảnh trong disPlayImage
                }


            }
        }

        private void Click_Back(object sender, MouseButtonEventArgs e)
        {
            if (_imageFiles != null) // kiểm tra xem mảng _imageFiles có khác null không
            {
                if (_currentImageIndex > 0) // nếu ảnh hiện tại > 0 
                {
                    _currentImageIndex--; // giảm chỉ số ảnh hiện tại xuống 1
                    DisplayImageFromPath(_imageFiles[_currentImageIndex]); // hiển thị ảnh hiện tại
                }
            }

        }

        private void Click_Next(object sender, MouseButtonEventArgs e)
        {
            if (_imageFiles != null)
            {
                if (_currentImageIndex < _imageFiles.Length - 1)
                {
                    _currentImageIndex++;
                    DisplayImageFromPath(_imageFiles[_currentImageIndex]);
                }
            }

        }

    }
}
