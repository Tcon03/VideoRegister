using Register1.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Xps.Serialization;

namespace Register1.ViewModel
{
    public class ChangeProperty : INotifyPropertyChanged
    {
       
        private ObservableCollection<ImageData> _imageList = new ObservableCollection<ImageData>();
        public ObservableCollection<ImageData> imageLists
        {

            get { return _imageList; }
            set
            {
                _imageList = value;
                OnPropertyChanged(nameof(imageLists));
            }
        }

        private string _imagepath;
        public string imagePath
        {
            get { return _imagepath; }
            set
            {
                _imagepath = value;
                OnPropertyChanged("imagePath");
            }
        }
        private BitmapImage _imageSelected; // thuộc tính này dùng để lưu trữ hình ảnh  khi UI thay đổi thì nó cũng sẽ thay đổi 
        public BitmapImage imageSelected
        {
            get { return _imageSelected; }
            set
            {
                _imageSelected = value;
                OnPropertyChanged("imageSelected");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}