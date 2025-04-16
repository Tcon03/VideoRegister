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

        public ChangeProperty()
        {
            _imageList = new ObservableCollection<ImageData>();
        }

        public ObservableCollection<ImageData> ImageList
        {

            get { return _imageList; }
            set
            {
                _imageList = value;
                OnPropertyChanged(nameof(ImageList));
            }
        }

        private string _videoPath;
        public string videoPath
        {
            get { return _videoPath; }
            set
            {
                _videoPath = value;
                OnPropertyChanged("videoPath");
            }
        }
        private BitmapImage _image;
        public BitmapImage Images
        {
            get { return _image; }
            set
            {
                _image = value;
                OnPropertyChanged("Images");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
