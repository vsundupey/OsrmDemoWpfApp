using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace OsrmRoutesProvider.Model
{
    public class ImitationTrack : ICloneable, INotifyPropertyChanged
    {
        public string Name { get; set; }

        public string Author { get; set; }

        public string Description { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public List<GpsDevice> Devices { get; set; }

        public int ImitationTrackPositionsCount
        {
            get
            {
                return _imitationTrackPositionsCount;
            }
            set
            {
                if (Devices != null && Devices.Any())
                {
                    foreach (var device in Devices)
                    {
                        device.TrackPositionsCount = value;
                    }
                }
                OnPropertyChanged();
                _imitationTrackPositionsCount = value;
            }
        }

        private int _imitationTrackPositionsCount;

        public object Clone()
        {
            var result = (ImitationTrack)Activator.CreateInstance(GetType());

            result.Name = Name;
            result.Author = Author;
            result.Description = Description;
            result.CreatedDate = CreatedDate;
            result.StartDate = StartDate;
            result.EndDate = EndDate;

            return result;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
