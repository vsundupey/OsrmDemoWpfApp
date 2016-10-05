using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using OsrmRoutesProvider.Helpers;
using OsrmRoutesProvider.Model;
using RestSharp;
using ServiceStack.Text;
namespace OsrmRoutesProvider
{
    /// <summary>
    /// Open Source Routing Machine(ORSM).
    /// Computes shortest paths in a graph. It was designed to run well with map data from the 
    /// OpenStreetMap Project.
    /// https://github.com/Project-OSRM
    /// </summary>
    public sealed class OsrmTrackProvider : BaseRestClient
    {
        private string service = "route";
        private string version = "v1";
        private string profile = "driving";
        private string _coordinates = "";

        private bool overview = false;
        private bool steps = true;
        private string geometries = "geojson";
        private string overviewDescription = "full";

        public OsrmTrackProvider()
        {
            Client.BaseUrl = new Uri("http://localhost:5000");
        }

        public Task<TrackPosition[]> CreateTrack(TrackPosition[] waypoints)
        {         
            return Task.Run(() =>
            {
                var request = new RestRequest();

                _coordinates = String.Empty;

                request.Resource = "/{service}/{version}/{profile}/{coordinates}";

                foreach (var point in waypoints)
                {
                    _coordinates += $"{point.Longitude.ToGbString()},{point.Latitude.ToGbString()};";
                }

                _coordinates = _coordinates.Remove(_coordinates.Length - 1);

                // UrlSegment Parameters
                request.AddParameter("service", service, ParameterType.UrlSegment);
                request.AddParameter("version", version, ParameterType.UrlSegment);
                request.AddParameter("profile", profile, ParameterType.UrlSegment);
                request.AddParameter("coordinates", _coordinates, ParameterType.UrlSegment);

                // Optional Parameters
                request.AddParameter("overview", overview.ToString().ToLower());
                request.AddParameter("steps", steps.ToString().ToLower());
                request.AddParameter("geometries", geometries);
                request.AddParameter("overview", overviewDescription.ToLower());

                var response = Client.Execute(request);

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new HttpRequestException($"Osrm: {response.StatusDescription}");

                var osrmResponseDeserializer = new JsonSerializer<OsrmResponse>();
                var responseObj = osrmResponseDeserializer.DeserializeFromString(response.Content);

                var trackPositions = responseObj?.Routes[0].Geometry.Coordinates.Select(item => new TrackPosition(item[0], item[1])).ToList();

                return trackPositions?.ToArray();
            });
        }

        #region Api models

        #region Enums
        private enum OsrmResultCode
        {
            /// <summary>
            /// Request could be processed as expected.
            /// </summary>
            Ok,
            /// <summary>
            /// URL string is invalid.
            /// </summary>
            InvalidUrl,
            /// <summary>
            /// Service name is invalid.
            /// </summary>
            InvalidService,
            /// <summary>
            /// Version is not found.
            /// </summary>
            InvalidVersion,
            /// <summary>
            /// Options are invalid.
            /// </summary>
            InvalidOptions,
            /// <summary>
            /// One of the supplied input coordinates could not snap to street segment.
            /// </summary>
            NoSegment,
            /// <summary>
            /// The request size violates one of the service specific request size restrictions.
            /// </summary>
            TooBig
        }
        private enum GeometryType
        {
            LineString,
            Point
        }
        /// <summary>
        /// A string indicating the type of maneuver. new identifiers might be introduced without 
        /// API change Types unknown to the client should be handled like the turn type, 
        /// the existance of correct modifier values is guranteed.
        /// * NOTE: 
        /// Please note that even though there are new name and notification instructions, the mode and name can change 
        /// between all instructions. They only offer a fallback in case nothing else is to report.
        /// </summary>
        private enum ManeuverType
        {
            /// <summary>
            /// a basic turn into direction of the modifier
            /// </summary>
            [EnumMember(Value = "turn")]
            Turn,
            /// <summary>
            /// no turn is taken/possible, but the road name changes. The road can take a turn itself, following modifier.
            /// </summary>
            [EnumMember(Value = "new name")]
            NewName,
            /// <summary>
            /// indicates the departure of the leg
            /// </summary>
            [EnumMember(Value = "depart")]
            Depart,
            /// <summary>
            /// indicates the destination of the leg
            /// </summary>
            [EnumMember(Value = "arrive")]
            Arrive,
            /// <summary>
            /// merge onto a street (e.g. getting on the highway from a ramp, the modifier specifies the direction of the merge)
            /// </summary>
            [EnumMember(Value = "merge")]
            Merge,
            /// <summary>
            /// Deprecated. Replaced by on_ramp and off_ramp.
            /// </summary>
            [EnumMember(Value = "ramp")]
            Ramp,
            /// <summary>
            /// take a ramp to enter a highway (direction given my modifier)
            /// </summary>
            [EnumMember(Value = "on ramp")]
            OnRamp,
            /// <summary>
            /// take a ramp to exit a highway (direction given my modifier)
            /// </summary>
            [EnumMember(Value = "off ramp")]
            OffRamp,
            /// <summary>
            /// take the left/right side at a fork depending on modifier
            /// </summary>
            [EnumMember(Value = "fork")]
            Fork,
            /// <summary>
            /// road ends in a T intersection turn in direction of modifier
            /// </summary>
            [EnumMember(Value = "end of road")]
            EndOfRoad,
            /// <summary>
            /// going straight on a specific lane
            /// </summary>
            [EnumMember(Value = "use lane")]
            UseLane,
            /// <summary>
            /// Turn in direction of modifier to stay on the same road
            /// </summary>
            [EnumMember(Value = "continue")]
            Continue,
            /// <summary>
            /// traverse roundabout, has additional field exit with NR if the roundabout is left. the modifier specifies the direction of entering the roundabout
            /// </summary>
            [EnumMember(Value = "roundabout")]
            Roundabout,
            /// <summary>
            /// a larger version of a roundabout, can offer rotary_name in addition to the exit parameter.
            /// </summary>
            [EnumMember(Value = "rotary")]
            Rotary,
            /// <summary>
            /// Describes a turn at a small roundabout that should be treated as normal turn. The modifier indicates the turn direciton. 
            /// Example instruction: At the roundabout turn left.
            /// </summary>
            [EnumMember(Value = "roundabout turn")]
            RoundAboutTurn,
            /// <summary>
            /// not an actual turn but a change in the driving conditions. 
            /// For example the travel mode. If the road takes a turn itself, the modifier describes the direction
            /// </summary>
            [EnumMember(Value = "notification")]
            Notification
        }
        /// <summary>
        /// An optional string indicating the direction change of the maneuver.
        /// </summary>
        private enum Modifier
        {
            /// <summary>
            /// Indicates reversal of direction
            /// </summary>
            [EnumMember(Value = "uturn")]
            Uturn,
            /// <summary>
            /// A sharp right turn
            /// </summary>
            [EnumMember(Value = "sharp right")]
            SharpRight,
            /// <summary>
            /// A normal turn to the right
            /// </summary>
            [EnumMember(Value = "right")]
            Right,
            /// <summary>
            /// A slight turn to the right
            /// </summary>
            [EnumMember(Value = "slight right")]
            SlightRight,
            /// <summary>
            /// No relevant change in direction
            /// </summary>
            [EnumMember(Value = "straight")]
            Straight,
            /// <summary>
            /// A slight turn to the left
            /// </summary>
            [EnumMember(Value = "slight left")]
            SlightLeft,
            /// <summary>
            ///  A normal turn to the left
            /// </summary>
            [EnumMember(Value = "left")]
            Left,
            /// <summary>
            /// A sharp turn to the left
            /// </summary>
            [EnumMember(Value = "sharp left")]
            SharpLeft
        }
        /// <summary>
        /// An optional integer indicating number of the exit to take. The field exists for the following type field:
        /// </summary>
        private enum ExitType
        {
            /// <summary>
            /// Number of the roundabout exit to take. If exit is undefined the destination is on the roundabout.
            /// </summary>
            [EnumMember(Value = "roundabout")]
            Roundabout,
            /// <summary>
            /// Indicates the number of intersections passed until the turn. Example instruction: at the fourth intersection, turn left
            /// </summary>
            [EnumMember(Value = "else")]
            Else
        }
        /// <summary>
        /// A indication (e.g. marking on the road) specifying the turn lane. A road can have multiple indications (e.g. an arrow pointing straight and left).
        /// The indications are given in an array, each containing one of the following types. 
        /// Further indications might be added on without an API version change.
        /// </summary>
        private enum IndicationType
        {
            /// <summary>
            /// No dedicated indication is shown.
            /// </summary>
            [EnumMember(Value = "none")]
            None,
            /// <summary>
            /// An indication signaling the possibility to reverse (i.e. fully bend arrow).
            /// </summary>
            [EnumMember(Value = "uturn")]
            Uturn,
            /// <summary>
            /// An indication indicating a sharp right turn (i.e. strongly bend arrow).
            /// </summary>
            [EnumMember(Value = "sharp right")]
            Sharpright,
            /// <summary>
            /// An indication indicating a right turn (i.e. bend arrow).
            /// </summary>
            [EnumMember(Value = "right")]
            Right,
            /// <summary>
            /// An indication indicating a slight right turn (i.e. slightly bend arrow).
            /// </summary>
            [EnumMember(Value = "slight right")]
            SlightRight,
            /// <summary>
            /// No dedicated indication is shown (i.e. straight arrow).
            /// </summary>
            [EnumMember(Value = "straight")]
            Straight,
            /// <summary>
            /// An indication indicating a slight left turn (i.e. slightly bend arrow).
            /// </summary>
            [EnumMember(Value = "slight left")]
            SlightLeft,
            /// <summary>
            ///  An indication indicating a left turn (i.e. bend arrow).
            /// </summary>
            [EnumMember(Value = "left")]
            Left,
            /// <summary>
            /// An indication indicating a sharp left turn (i.e. strongly bend arrow).
            /// </summary>
            [EnumMember(Value = "sharp left")]
            SharpLeft
        }
        #endregion
        #region Models

        private class OsrmResponse
        {
            /// <summary>
            /// Response Code
            /// </summary>
            public OsrmResultCode Code { get; set; }
            /// <summary>
            /// Message is a optional human-readable error message.
            /// </summary>
            public string Message { get; set; } = null;

            public Route[] Routes { get; set; }
        }

        /// <summary>
        /// Osrm route result object
        /// RouteLeg represents a route between two waypoints.
        /// </summary>
        private class Route
        {
            public Leg[] Legs { get; set; }

            /// <summary>
            /// The distance traveled by the route, in float meters.
            /// </summary>
            public float Distance { get; set; }
            /// <summary>
            /// The estimated travel time, in float number of seconds.
            /// </summary>
            public float Duration { get; set; }

            public GeojsonGeometry Geometry { get; set; }
        }
        /// <summary>
        /// RouteLeg represents a route between two waypoints.
        /// </summary>
        private class Leg
        {
            /// <summary>
            /// The distance traveled by this route leg, in float meters.
            /// </summary>
            public float Distance { get; set; }
            /// <summary>
            /// The estimated travel time, in float number of seconds.
            /// </summary>
            public float Duration { get; set; }
            /// <summary>
            /// Summary of the route taken as string. Depends on the steps parameter:
            /// true - Names of the two major roads used. Can be empty if route is too short.
            /// false -	empty string
            /// </summary>
            public string Summary { get; set; }
            /// <summary>
            /// Optional
            /// Depends on the steps parameter.
            /// </summary>
            public Step[] Steps { get; set; }
            /// <summary>
            /// Optional
            /// Additional details about each coordinate along the route geometry
            /// </summary>
            public Annotation Annotation { get; set; }
        }
        
        //TODO need to complete
        /// <summary>
        /// A step consists of a maneuver such as a turn or merge, followed by a distance of travel along a single way to the subsequent step.
        /// </summary>
        private class Step
        {
            /// <summary>
            /// The distance traveled by this route leg, in float meters.
            /// </summary>
            public float Distance { get; set; }
            /// <summary>
            /// The estimated travel time, in float number of seconds.
            /// </summary>
            public float Duration { get; set; }
            /// <summary>
            /// The unsimplified geometry of the route segment, depending on the geometries parameter.
            /// </summary>
            public GeojsonGeometry Geometry { get; set; }
            /// <summary>
            /// The name of the way along which travel proceeds.
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// The pronunciation hint of the way name. Will be undefined if there is no pronunciation hit.
            /// </summary>
            public string Pronunciation { get; set; }
            /// <summary>
            /// The destinations of the way. Will be undefined if there are no destinations.
            /// </summary>
            public int[] Destinations { get; set; }
            /// <summary>
            /// A string signifying the mode of transportation.
            /// </summary>
            public string Mode { get; set; }
            /// <summary>
            /// A StepManeuver object representing the maneuver.
            /// </summary>
            public Maneuver Maneuver { get; set; }
            public Intersection[] Intersections { get; set; }
        }
        private class Annotation
        {
            public float[] Distance { get; set; }
            public float[] Duration { get; set; }
            public int[] DataSources { get; set; }
            public int[] Nodes { get; set; }
        }
        private class GeojsonGeometry
        {
            public GeometryType GeometryType { get; set; }

            public double[][] Coordinates { get; set; }
        }
        private class Waypoint
        {
            public string Name { get; set; }

            public Location Location { get; set; }

            public string Hint { get; set; }
        }
        private class Location
        {
            public double Longitude { get; set; }
            public double Latitude { get; set; }
        }
        private class Maneuver
        {
            /// <summary>
            /// A [longitude, latitude] pair describing the location of the turn.
            /// </summary>
            public Location Location { get; set; }
            /// <summary>
            /// The clockwise angle from true north to the direction of travel immediately before the maneuver.
            /// </summary>
            public int BearingBefore { get; set; }
            /// <summary>
            /// The clockwise angle from true north to the direction of travel immediately after the maneuver.
            /// </summary>
            public int BearingAfter { get; set; }
            public ManeuverType Type { get; set; }
            public Modifier Modifer { get; set; }
            public ExitType Exit { get; set; }
        }
        /// <summary>
        /// An intersection gives a full representation of any cross-way the path passes bay. 
        /// For every step, the very first intersection (intersections[0]) corresponds to the location of the StepManeuver. 
        /// Further intersections are listed for every cross-way until the next turn instruction.
        /// </summary>
        private class Intersection
        {
            /// <summary>
            /// A [longitude, latitude] pair describing the location of the turn.
            /// </summary>
            public Location Location { get; set; }
            /// <summary>
            /// A list of bearing values (e.g. [0,90,180,270]) that are available at the intersection. 
            /// The bearings describe all available roads at the intersection.
            /// </summary>
            public int[] Bearings { get; set; }
            /// <summary>
            /// A list of entry flags, corresponding in a 1:1 relationship to the bearings. 
            /// A value of true indicates that the respective road could be entered on a valid route. 
            /// false indicates that the turn onto the respective road would violate a restriction.
            /// </summary>
            public bool[] Entry { get; set; }
            /// <summary>
            ///  Index into bearings/entry array. Used to calculate the bearing just before the turn. Namely, 
            /// the clockwise angle from true north to the direction of travel immediately before the maneuver/passing the intersection. 
            /// Bearings are given relative to the intersection. To get the bearing in the direction of driving, the bearing has to be rotated by a value of 180. The value is not supplied for depart maneuvers.
            /// </summary>
            public int In { get; set; }
            /// <summary>
            /// Index into the bearings/entry array. Used to extract the bearing just after the turn. 
            /// Namely, The clockwise angle from true north to the direction of travel immediately after the maneuver/passing the intersection. 
            /// The value is not supplied for arrive maneuvers.
            /// </summary>
            public int Out { get; set; }
            /// <summary>
            /// Array of Lane objects that denote the available turn lanes at the intersection. 
            /// If no lane information is available for an intersection, the lanes property will not be present.
            /// </summary>
            public Lane[] Lanes { get; set; }

        }
        /// <summary>
        /// A Lane represents a turn lane at the corresponding turn location.
        /// </summary>
        private class Lane
        {
            public IndicationType[] Indications { get; set; }
            /// <summary>
            /// a boolean flag indicating whether the lane is a valid choice in the current maneuver
            /// </summary>
            public bool Valid { get; set; }
        }
        #endregion

        #endregion
    }
}
