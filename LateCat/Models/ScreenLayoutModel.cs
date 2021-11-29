using LateCat.PoseidonEngine.Abstractions;

namespace LateCat.Models
{
    public class ScreenLayoutModel : ObservableObject, IScreenLayoutModel
    {
        public ScreenLayoutModel(IMonitor monitor, string propertyFilePath, string title)
        {
            Monitor = monitor;
            PropertyPath = propertyFilePath;
            Title = title;
        }

        private IMonitor _monitor;
        public IMonitor Monitor
        {
            get { return _monitor; }
            set
            {
                _monitor = value;
                OnPropertyChanged();
            }
        }

        private string _propertyPath;
        public string PropertyPath
        {
            get { return _propertyPath; }
            set
            {
                _propertyPath = value;
                OnPropertyChanged();
            }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }
    }
}
