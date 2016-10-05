using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsrmRoutesProvider.Model
{
    public class GpsDevice
    {
        public string Imei { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int TrackPositionsCount { get; set; }

        public int CurrentTrackPosition { get; set; }

        public GpsDevice(int trackPositionsCount)
        {
            TrackPositionsCount = trackPositionsCount;
        }
    }

}
