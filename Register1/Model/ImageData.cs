using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Register1.Model
{
   public class ImageData :INotifyPropertyChanged 
    {
        public ObservableCollection<ImageData> imageList { get; set; } // thuộc tính danh sách ảnh 
        public ImageData()
        {
            imageList = new ObservableCollection<ImageData>();

        }


        private string _imagePath; 
        public string imagePath // đường dẫn của ảnh 
        {
            get { return _imagePath; }
            set
            {
                _imagePath = value;
                RaisePropertyChanged("imagePath");
            }
        }

       

        private BitmapImage _imageBitMap; // thuộc tính ảnh 
        public BitmapImage imageBitMap
        {
            get { return _imageBitMap; }
            set
            {
                _imageBitMap = value;
                RaisePropertyChanged("imageBitMap");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;  
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
