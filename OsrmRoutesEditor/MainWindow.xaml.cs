using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Maps.MapControl.WPF;
using OsrmRoutesEditor.Helpers;
using OsrmRoutesProvider;
using OsrmRoutesProvider.Model;

namespace OsrmRoutesEditor
{
    public partial class MainWindow
    {
        private readonly OsrmTrackProvider _provider;

        private ImitationTrack _track;

        // An indexable collection of tab validity states
        readonly ObservableCollection<bool> _tabItemsIsValidCollection = new ObservableCollection<bool>();

        public ObservableRangeCollection<DraggablePin> OsrmWayPointsPushpins { get; set; }

        // Intermediate points for the construction of the route Osrm
        public ObservableRangeCollection<TrackPosition> TrackPositions { get; set; }

        // Waypoints uploaded from Osrm
        public ObservableRangeCollection<GpsDevice> NewGpsDevices { get; set; }

        // Gps devices for a new test track
        public ObservableRangeCollection<GpsDevice> ImitationTrackGpsDevices { get; set; }

        // Icons on the map
        private Pushpin _pinStart;  // А
        private Pushpin _pinEnd;    // B
        private Pushpin _pinCar;    // ->

        private bool _wayPointsEditModeEnabled; // The mode of editing the control points (on/off)

        public MainWindow()
        {
            InitializeComponent();
            _provider = new OsrmTrackProvider();
            InitControls(); // Initializing Controls
        }

        private void ModelingPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            TabControlItems.DataContext = _tabItemsIsValidCollection;
            CheckTabItems();
        }

        /// <summary>
        /// Initializing Controls
        /// </summary>
        private void InitControls()
        {
            try
            {
                // Вкладка №0
                TrackNameTextBox.Text = $"Test track - ({DateTime.Now:HH:mm:ss dd.MM.yy})";
                _track = new ImitationTrack();
                _track.Author = "User1";
                _track.Description = "Put here your description...";

                _pinStart = new Pushpin { Content = "A" }; // Start marker
                _pinEnd = new Pushpin { Content = "B" };   // End marker
                _pinCar = new Pushpin                      // A marker for playing a route
                {
                    Content = "->",
                    Background = new SolidColorBrush(Color.FromRgb(53, 196, 53))
                };

                NewGpsDevices = new ObservableRangeCollection<GpsDevice>();
                TrackPositions = new ObservableRangeCollection<TrackPosition>();
                OsrmWayPointsPushpins = new ObservableRangeCollection<DraggablePin>();

                NewImitationTrackGpsDevicesDataGrid.ItemsSource = NewGpsDevices;
                TrackPositionsDataGrid.ItemsSource = TrackPositions;
                WayPointsForOsrmDataGrid.ItemsSource = OsrmWayPointsPushpins;

                NewGpsDevices.Add(new GpsDevice(0) { Imei = "000002245678915" });
                NewGpsDevices.Add(new GpsDevice(0) { Imei = "444555678154689" });

                foreach (var item in TabControlItems.Items)
                    _tabItemsIsValidCollection.Add(new bool());

                Player.Timer.Tick += Timer_Tick;
                Player.PlayerSlider.ValueChanged += PlayerSlider_ValueChanged;
                TrackPositions.CollectionChanged += TrackPositions_CollectionChanged;
                Player.CurrentPosition = 0;

                CommonData.DataContext = _track;
                TotalView.DataContext = _track;
            }
            catch (Exception exc)
            {
                LogWindowControl.AddMessage(exc.ToString());
            }
        }

        private void PlayerSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TrackPositions.Count > 0)
                TrackPositionsDataGrid.SelectRowByIndex((int)Player.PlayerSlider.Value);
        }

        private void TrackPositions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Player.MaxPosition = TrackPositions.Count - 1;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (TrackPositions.Any() && Player.CurrentPosition < TrackPositions.Count)
            {
                if (!MainMap.Children.Contains(_pinCar)) MainMap.Children.Add(_pinCar);

                var point = TrackPositions[Player.CurrentPosition];

                _pinCar.Location = new Location(point.Latitude, point.Longitude);
                TrackPositionsDataGrid.SelectRowByIndex(Player.CurrentPosition);
                Player.CurrentPosition++;

                if (NewImitationTrackGpsDevicesDataGrid.Items.Count > 0 &&
                    NewImitationTrackGpsDevicesDataGrid.SelectedIndex != -1)
                {
                    var device = (GpsDevice)NewImitationTrackGpsDevicesDataGrid.SelectedItem;
                    var devIndex = NewGpsDevices.IndexOf(device);
                    NewGpsDevices[devIndex].CurrentTrackPosition = Player.CurrentPosition;
                }
            }
            else
            {
                Player.Timer.Stop();
                Player.CurrentPlayPauseState = false;
            }
        }

        private void AddNewGpsDevice_OnClick(object sender, RoutedEventArgs e)
        {
            var exist = NewGpsDevices.FirstOrDefault(d => d.Imei.Equals(NewGpsDeviceTextBox.Text));

            if (exist == null)
            {
                NewGpsDevices.Add(new GpsDevice(TrackPositions.Count)
                {
                    Imei = NewGpsDeviceTextBox.Text
                });
            }
            else
            {
                MessageBox.Show($"Device {NewGpsDeviceTextBox.Text} already in the list");
            }
        }

        private void RemoveNewGpsDevice_OnClick(object sender, RoutedEventArgs e)
        {
            if (NewImitationTrackGpsDevicesDataGrid.Items.Count > 0)
            {
                var device = (GpsDevice)NewImitationTrackGpsDevicesDataGrid.SelectedItem;
                NewGpsDevices.Remove(device);
            }
        }

        private void NewImitationTrackGpsDevicesDataGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Player.Timer != null && Player.Timer.IsEnabled)
                Player.Timer.Stop();

            Player.CurrentPlayPauseState = false;

            var obj = (GpsDevice)NewImitationTrackGpsDevicesDataGrid.SelectedItem;

            Player.CurrentPosition = obj.CurrentTrackPosition;

            if (TrackPositions.Any() && Player.CurrentPosition < TrackPositions.Count)
            {
                if (!MainMap.Children.Contains(_pinCar)) MainMap.Children.Add(_pinCar);

                var point = TrackPositions[Player.CurrentPosition];

                _pinCar.Location = new Location(point.Latitude, point.Longitude);
            }
        }

        /// <summary>
        /// The method allows to select the necessary line in the datagrid and to shift focus to the line
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="rowIndex"></param>

        private void WayPointsForOsrmDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var pushpin = (DraggablePin)WayPointsForOsrmDataGrid.CurrentItem;

            if (pushpin != null)
            {
                MainMap.Center = pushpin.Location;

                if (WayPointsForOsrmDataGrid.SelectedIndex >= 0)
                    SetSelectedPushpinBackground(Color.FromRgb(37, 195, 50), WayPointsForOsrmDataGrid.SelectedIndex, OsrmWayPointsPushpins);
            }
        }

        /// <summary>
        /// Sets the color of the selected marker
        /// </summary>
        /// <param name="color">Цвет</param>
        /// <param name="indexInCollection">Индекс марекра в коллекции</param>
        /// <param name="pushpins">Ссылка на коллекцию с маркерами</param>
        private void SetSelectedPushpinBackground(Color color, int indexInCollection, ObservableCollection<DraggablePin> pushpins)
        {
            foreach (var item in pushpins)
            {
                item.Background = new SolidColorBrush(Color.FromRgb(58, 74, 216));
            }

            pushpins.ElementAt(indexInCollection).Background = new SolidColorBrush(color);
        }

        /// <summary>
        /// Delete Point - Button Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteWayPointButton_OnClick(object sender, RoutedEventArgs e)
        {
            DeleteWayPointPushpin();
        }

        /// <summary>
        /// Delete point
        /// </summary>
        private void DeleteWayPointPushpin()
        {
            if (WayPointsForOsrmDataGrid.SelectedIndex != -1)
            {
                int tmpLastSelectedIndex = WayPointsForOsrmDataGrid.SelectedIndex;

                var pin = OsrmWayPointsPushpins[WayPointsForOsrmDataGrid.SelectedIndex];

                if (MainMap.Children.Contains(pin)) MainMap.Children.Remove(pin);

                OsrmWayPointsPushpins.RemoveAt(WayPointsForOsrmDataGrid.SelectedIndex);

                SetPushinsEnumaration(OsrmWayPointsPushpins);

                if (tmpLastSelectedIndex <= WayPointsForOsrmDataGrid.Items.Count - 1)
                    WayPointsForOsrmDataGrid.SelectRowByIndex(tmpLastSelectedIndex);
                else if (tmpLastSelectedIndex - 1 >= 0)
                {
                    WayPointsForOsrmDataGrid.SelectRowByIndex(tmpLastSelectedIndex - 1);
                }
            }
        }

        /// <summary>
        /// Performs renumbering of markers on the map after editing
        /// </summary>
        /// <param name="pushpins">Ссылка на коллекцию с маркерами</param>
        private void SetPushinsEnumaration(ObservableCollection<DraggablePin> pushpins)
        {
            int i = 0;

            foreach (var item in pushpins)
            {
                i++;
                item.Content = i.ToString();
            }
        }

        /// <summary>
        /// Delete reference point - Delete key
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WayPointsForOsrmDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete) DeleteWayPointPushpin();
        }

        private async void LoadOsrmTrack_OnClick(object sender, RoutedEventArgs e)
        {
            await GetLoadOsrmTrack();
        }

        /// <summary>
        /// Build a route based on Osrm
        /// </summary>
        /// <returns></returns>
        private async Task GetLoadOsrmTrack()
        {
            try
            {
                MapPolyline.Locations?.Clear(); // Delete the current route

                var trackPositions =
                    OsrmWayPointsPushpins.Select(
                        point => new TrackPosition { Latitude = point.Location.Latitude, Longitude = point.Location.Longitude })
                        .ToArray();

                await GetDataFromOsrm(trackPositions);
            }
            catch (Exception exc)
            {
                LogWindowControl.AddMessage(exc.Message);
            }
        }

        /// <summary>
        /// Building a route from Osrm data
        /// </summary>
        /// <param name="waypoints"></param>
        /// <returns></returns>
        private async Task GetDataFromOsrm(TrackPosition[] waypoints)
        {
            if (waypoints.Length > 1)
            {
                LogWindowControl.AddMessage($"Creation of a route (provider OSRM)");

                var newOsrmPositions = await _provider.CreateTrack(waypoints);

                await InitAndDrowTrack(newOsrmPositions);
            }
            else LogWindowControl.AddMessage($"There are not enough points to build a route - you need at least 2 points");
        }

        private async Task InitAndDrowTrack(TrackPosition[] points)
        {
            TrackPositions.Clear();
            MapPolyline.Locations?.Clear();

            Player.CurrentPosition = 0;

            if (points == null || !points.Any())
            {
                LogWindowControl.AddMessage($"No data...");
                return;
            }

            TrackPositions.AddRange(points);

            if (points.Any())
            {
                foreach (var device in NewGpsDevices)
                {
                    device.TrackPositionsCount = TrackPositions.Count;
                }
            }

            if (!TrackPositions.Any())
            {
                foreach (var device in NewGpsDevices)
                {
                    device.TrackPositionsCount = 0;
                }
                return;
            }

            LogWindowControl.AddMessage($"Loaded {TrackPositions.Count} points.");

            LogWindowControl.AddMessage($"Drowing route...");

            MapPolyline.Locations = await ConvertTrackPositionsToLocations(TrackPositions);
            MainMap.Center = MapPolyline.Locations.FirstOrDefault();

            _pinStart.Location = MapPolyline.Locations.First();
            _pinEnd.Location = MapPolyline.Locations.Last();
            _pinCar.Location = MapPolyline.Locations.First();

            if (!MainMap.Children.Contains(_pinStart)) MainMap.Children.Add(_pinStart);
            if (!MainMap.Children.Contains(_pinEnd)) MainMap.Children.Add(_pinEnd);

            LogWindowControl.AddMessage($"Completed");

            CheckTabItems();
        }

        /// <summary>
        /// Turn on/off the mode of insertion of control points
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WayPointsEditModeButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_wayPointsEditModeEnabled)
            {
                WayPointsEditModeButton.Background = new SolidColorBrush(Colors.Gainsboro);
                MapGrid.Background = new SolidColorBrush();
                MainMap.Margin = new Thickness(0);
                _wayPointsEditModeEnabled = false;
            }
            else
            {
                WayPointsEditModeButton.Background = new SolidColorBrush(Colors.LightCoral);
                MainMap.Margin = new Thickness(10);
                MapGrid.Background = new SolidColorBrush(Colors.LightCoral);
                _wayPointsEditModeEnabled = true;
            }
        }

        /// <summary>
        /// Convert TrackPosition to LocationCollection to add to map layer
        /// </summary>
        private async Task<LocationCollection> ConvertTrackPositionsToLocations(ObservableCollection<TrackPosition> positions)
        {
            return await Task.Run(() =>
            {
                LocationCollection locationCollection = new LocationCollection();

                foreach (var position in positions)
                {
                    locationCollection.Add(new Location(position.Latitude, position.Longitude));
                }

                return locationCollection;
            });
        }

        private void ClearAllButton_OnClick(object sender, RoutedEventArgs e)
        {
            ClearMapInfo();
        }

        /// <summary>
        /// Clear all data
        /// </summary>
        private void ClearAll()
        {
            // Tab №1
            _track = new ImitationTrack();
            TrackNameTextBox.Text = String.Empty;
            AuthorTextBox.Text = String.Empty;
            TrackDescriptionTextBox.Text = String.Empty;

            // Tab №3
            NewGpsDevices.Clear();

            // Tabs №2-3
            ClearMapInfo();

            CheckTabItems();
        }

        /// <summary>
        /// Clear map data
        /// </summary>
        private void ClearMapInfo()
        {
            TrackPositions.Clear();

            if (MainMap.Children.Contains(_pinStart)) MainMap.Children.Remove(_pinStart);
            if (MainMap.Children.Contains(_pinEnd)) MainMap.Children.Remove(_pinEnd);
            if (MainMap.Children.Contains(_pinCar)) MainMap.Children.Remove(_pinCar);

            foreach (var pin in OsrmWayPointsPushpins)
            {
                if (MainMap.Children.Contains(pin)) MainMap.Children.Remove(pin);
            }

            OsrmWayPointsPushpins?.Clear();
            MapPolyline.Locations?.Clear();

        }

        /// <summary>
        /// Double-click to add a marker to the map to build a route
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MainMap_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Disables the default mouse double-click action.
            e.Handled = true;

            //Get the mouse click coordinates
            Point mousePosition = e.GetPosition(MainMap);
            //Convert the mouse coordinates to a locatoin on the map
            Location pinLocation = MainMap.ViewportPointToLocation(mousePosition);

            // The pushpin to add to the map.
            DraggablePin pin = new DraggablePin(MainMap)
            {
                Background = new SolidColorBrush(Color.FromRgb(58, 74, 216)),
                Foreground = new SolidColorBrush(Colors.White),
                Location = pinLocation,
                Content = OsrmWayPointsPushpins.Count + 1
            };

            if (_wayPointsEditModeEnabled && WayPointsForOsrmDataGrid.SelectedIndex >= 0)
            {
                OsrmWayPointsPushpins.Insert(WayPointsForOsrmDataGrid.SelectedIndex + 1, pin);
                SetPushinsEnumaration(OsrmWayPointsPushpins);
                WayPointsForOsrmDataGrid.SelectRowByIndex(WayPointsForOsrmDataGrid.SelectedIndex + 1);
            }
            else OsrmWayPointsPushpins.Add(pin);

            pin.MouseDown += Pin_MouseDown;

            MainMap.Children.Add(pin);

            if (AutoRouteDrowCheckBox.IsChecked != null && AutoRouteDrowCheckBox.IsChecked == true)
                await GetLoadOsrmTrack();

            CheckTabItems();
        }

        /// <summary>
        /// Marker click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Pin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var pin = sender as DraggablePin;

            if (pin != null)
            {
                var index = OsrmWayPointsPushpins.IndexOf(pin);
                WayPointsForOsrmDataGrid.SelectRowByIndex(index);

                if (WayPointsForOsrmDataGrid.SelectedIndex >= 0)
                    SetSelectedPushpinBackground(Color.FromRgb(37, 195, 50), WayPointsForOsrmDataGrid.SelectedIndex, OsrmWayPointsPushpins);

                Keyboard.Focus(pin);
            }
        }
        private void PositionsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TrackPositionsDataGrid.SelectedIndex != -1)
            {
                Player.CurrentPosition = TrackPositionsDataGrid.SelectedIndex;

                var point = TrackPositions[Player.CurrentPosition];
                _pinCar.Location = new Location(point.Latitude, point.Longitude);

                if (NewImitationTrackGpsDevicesDataGrid.Items.Count > 0 &&
                    NewImitationTrackGpsDevicesDataGrid.SelectedIndex != -1)
                {
                    var device = (GpsDevice)NewImitationTrackGpsDevicesDataGrid.SelectedItem;
                    device.CurrentTrackPosition = Player.CurrentPosition;
                }
            }
        }

        #region Status methods implement validation of data entry on tabs
        private bool Check0TabInfo()
        {
            if (!TrackNameTextBox.Text.Any())
                return false;

            if (!AuthorTextBox.Text.Any())
                return false;

            return true;
        }
        private bool Check1TabInfo()
        {
            if (OsrmWayPointsPushpins.Count < 2)
                return false;

            return true;
        }
        private bool Check2TabInfo()
        {
            if (!TrackPositions.Any())
                return false;

            return true;
        }
        private bool Check3TabInfo()
        {
            if (!NewGpsDevices.Any())
                return false;
            return true;
        }
        #endregion

        private void TabControlItems_MouseEnter(object sender, MouseEventArgs e)
        {
            CheckTabItems();
        }

        private void CheckTabItems()
        {
            _tabItemsIsValidCollection[0] = Check0TabInfo();
            _tabItemsIsValidCollection[1] = Check1TabInfo();
            _tabItemsIsValidCollection[2] = Check2TabInfo();
            _tabItemsIsValidCollection[3] = Check3TabInfo();

            bool result = true;

            for (int i = 0; i < _tabItemsIsValidCollection.Count - 1; i++)
            {
                if (!_tabItemsIsValidCollection[i]) { result = false; break; }
            }

            _tabItemsIsValidCollection[4] = result;

            _track.ImitationTrackPositionsCount = TrackPositions.Count;
        }

        private void SaveTrackButton_OnClick(object sender, RoutedEventArgs e)
        {
            SaveCreatedTrack();
        }

        /// <summary>
        /// Save/Export created track
        /// </summary>
        private void SaveCreatedTrack()
        {
            try
            {
                if (_track != null)
                {
                    // TODO Put here you custum code
                    throw new NotImplementedException("Need to create save/export implementation");
                }                          
            }
            catch (Exception exc)
            {
                LogWindowControl.AddMessage(exc.ToString());
            }
        }

        /// <summary>
        /// New track
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewTrackPageButton_OnClick(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show($"Do you want to save the current track?", "Saving data", MessageBoxButton.OKCancel, MessageBoxImage.Information);

            if (result.Equals(MessageBoxResult.OK)) SaveCreatedTrack();

            ClearAll();
        }

        /// <summary>
        /// Clone track
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloneTrackButton_OnClick(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show($"Do you want to save the current track?", "Saving data", MessageBoxButton.OKCancel, MessageBoxImage.Information);

            if (result.Equals(MessageBoxResult.OK)) SaveCreatedTrack();

            _track = _track.Clone() as ImitationTrack;

            if (_track != null)
            {
                _track.Name = _track.Name += " Cloned at " + DateTime.Now.ToString("HH:mm:ss dd.MM.yyyy");
                TrackNameTextBox.Text = _track.Name;

                LogWindowControl.AddMessage($"The track was successfully cloned as { _track.Name }");
            }       
        }
    }
}
