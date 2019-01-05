using System;
using System.Collections.Generic;
using System.Windows.Input;
using TelloLib;

namespace TelloAltitudeUnlocker
{
    public class MainPageViewModel : BaseViewModel
    {
        private readonly Tello _tello;
        private readonly Action<string,IDictionary<string, string>> _logger;

        private const int MinMaxAltitude = 5;
        private const int MaxMaxAltitude = 30;

        public MainPageViewModel(Action<Exception,string> errorLog, Action<string,IDictionary<string, string>> logger)
        {
            _logger = logger;
            _tello = new Tello(false, false, errorLog);

            IncreaseAltitudeCommand = new Xamarin.Forms.Command(_ => IncreateMaxAltitude(), _ => CanIncreaseMaxAltitude);
            DecreaseAltitudeCommand = new Xamarin.Forms.Command(_ => DecreateMaxAltitude(), _ => CanDecreaseMaxAltitude);
        }

        private bool _canIncreaseMaxAltitude;
        public bool CanIncreaseMaxAltitude
        {
            get => _canIncreaseMaxAltitude;
            set => AssignProperty(ref _canIncreaseMaxAltitude, value);
        }

        private bool _canDecreaseMaxAltitude;
        public bool CanDecreaseMaxAltitude
        {
            get => _canDecreaseMaxAltitude;
            set => AssignProperty(ref _canDecreaseMaxAltitude, value);
        }

        private void IncreateMaxAltitude()
        {
            var newMax = CurrentMaxAltitude + 5;
            newMax = newMax > MaxMaxAltitude ? MaxMaxAltitude : newMax;

            _logger?.Invoke("IncreasingMaxAltitude", null);
            _tello.Messages.SetMaxHeight(newMax);
        }

        private void DecreateMaxAltitude()
        {
            var newMax = CurrentMaxAltitude - 5;
            newMax = newMax < MinMaxAltitude ? MinMaxAltitude : newMax;

            _logger?.Invoke("DecreasingMaxAltitude", null);
            _tello.Messages.SetMaxHeight(newMax);
        }

        private ConnectionState _connectionState;
        public ConnectionState ConnectionState 
        {
            get => _connectionState;
            set => AssignProperty(ref _connectionState, value);
        }

        private int _currentMaxAltitude;
        public int CurrentMaxAltitude
        {
            get => _currentMaxAltitude;
            set
            {
                AssignProperty(ref _currentMaxAltitude, value);
                MaxAltitudeDisplay = $"{value}m";
                _logger?.Invoke("CurrentMaxAltitude", null);
            }
        }

        private string _maxAltitudeDisplay = "---m";
        public string MaxAltitudeDisplay
        {
            get => _maxAltitudeDisplay;
            set => AssignProperty(ref _maxAltitudeDisplay, value);
        }

        public ICommand IncreaseAltitudeCommand { get; }
        public ICommand DecreaseAltitudeCommand { get; }

        private void RefreshState()
        {
            CanIncreaseMaxAltitude = ConnectionState == ConnectionState.Connected && CurrentMaxAltitude < MaxMaxAltitude;
            CanDecreaseMaxAltitude = ConnectionState == ConnectionState.Connected && CurrentMaxAltitude > MinMaxAltitude;
        }

        public void Init()
        {
            //Subscribe to Tello connection events. Called when connected/disconnected.
            _tello.OnConnection += newState =>
            {
                ConnectionState = newState;

                if (newState == ConnectionState.Connected)
                {
                    _tello.Messages.QueryMaxHeight();
                    _logger?.Invoke("TelloStateChanged", new Dictionary<string, string> { { "state", newState.ToString() } });
                }
            };

            //subscribe to Tello update events. Called when update data arrives from drone.
            _tello.OnUpdate += cmdId =>
            {
                if (cmdId == Commands.TELLO_CMD_ALT_LIMIT)
                {
                    CurrentMaxAltitude = _tello.State.MaxHeight;
                }
            };

            _logger?.Invoke("StartingTelloConnection", null);
            _tello.StartConnecting();   //Start trying to connect.
        }
    }
}
