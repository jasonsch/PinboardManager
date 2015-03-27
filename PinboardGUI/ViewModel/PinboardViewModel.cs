using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pinboard;

namespace PinboardGUI.ViewModel
{
    public class PinboardViewModel : INotifyPropertyChanged
    {
        private PinboardManager Pinboard;
        private PinboardBookmark _SelectedItem;
        private System.Windows.Threading.Dispatcher Dispatcher;

        public ObservableCollection<PinboardBookmark> BookmarkList { get; private set; }

        public PinboardViewModel(System.Windows.Threading.Dispatcher Dispatcher, string ApiToken)
        {
            System.Diagnostics.Debug.WriteLine("ApiToken ==> " + ApiToken); // TODO
            this.Dispatcher = Dispatcher;
            Pinboard = new PinboardManager(ApiToken);
            BookmarkList = new ObservableCollection<PinboardBookmark>();
            Pinboard.GetRecentBookmarks(30).ContinueWith((task) =>
            {
                Dispatcher.Invoke(() =>
                {
                    List<PinboardBookmark> Bookmarks = task.Result;
                    foreach (PinboardBookmark Bookmark in Bookmarks)
                    {
                        System.Diagnostics.Debug.WriteLine("Adding bookmark ==> " + Bookmark.Title);
                        BookmarkList.Add(Bookmark);
                    }
                });
            });
        }

        public PinboardBookmark SelectedItem
        {
            get
            {
                return _SelectedItem;
            }

            set
            {
                if (value != _SelectedItem)
                {
                    _SelectedItem = value;
                    OnPropertyChanged("SelectedItem");
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
            }
        }

        #endregion
    }
}
