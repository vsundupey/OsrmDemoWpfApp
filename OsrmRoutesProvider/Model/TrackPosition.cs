using System;

namespace OsrmRoutesProvider.Model
{
    public class TrackPosition
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public TimeSpan SentOn { get; set; }

        public float Speed { get; set; }

        public float Course { get; set; }

        public TrackPosition()
        {

        }

        public TrackPosition(double longitude, double latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }
    }
}
