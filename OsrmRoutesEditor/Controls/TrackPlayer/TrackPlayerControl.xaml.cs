using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace OsrmRoutesEditor.Controls.TrackPlayer
{
    /// <summary>
    /// Interaction logic for TrackPlayerControl.xaml
    /// </summary>
    public partial class TrackPlayerControl : UserControl, INotifyPropertyChanged
    {
        private int _currentPosition;

        /// <summary>
        /// Set true - play
        /// </summary>
        private bool _currentPlayPauseState;

        private int _maxPosition;

        public DispatcherTimer Timer;

        public TrackPlayerControl()
        {
            InitializeComponent();
            Timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(500) };
            PlayerContext.DataContext = this;
        }

        public int CurrentPosition
        {
            get { return _currentPosition; }
            set { _currentPosition = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Set true - play
        /// </summary>
        public bool CurrentPlayPauseState
        {
            get { return _currentPlayPauseState; }
            set
            {
                _currentPlayPauseState = value;
                OnPropertyChanged();
            }
        }

        public int MaxPosition
        {
            get { return _maxPosition; }
            set { _maxPosition = value;
                OnPropertyChanged();
            }
        }

        private void PlayPauseButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!this.CurrentPlayPauseState)
            {
                if (Timer == null) Timer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(500)};               
                Timer.Start();
                CurrentPlayPauseState = true;
                return;
            }

            Timer.Stop();
            CurrentPlayPauseState = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void StopButton_OnClick(object sender, RoutedEventArgs e)
        {
            Timer?.Stop();
            CurrentPlayPauseState = false;
            CurrentPosition = 0;
        }
    }
}
