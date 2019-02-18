using System;
using System.Collections.Generic;

namespace NI_Interface
{
    public class ChannelModel : ModelBase
    {
        private string _name;
        private string _device;
        private string _port;
        private Int32 _line;
        private string _deviceType;
        private string _inOut;
        private double _min;
        private double _max;
        private string _inMode;
        private double _value;  // TODO  private double[] _value;  for Moving Average ?
        private string _calibration; //TODO remove unneeded from project
        private bool _boolValue;

        private Int32 _errorCode;
        private string _errorText;
        private DateTime _lastUpdate;

        private string _type;
        private string _niName;
        private string _niPort;
        private Int32 _niIndex;
        private double[][] _calibrationTable;
        
        public delegate void PropertyChangedDelegate(string name);
        public new PropertyChangedDelegate PropertyChanged { get; set; }

        public ChannelModel()
        {
            //empty
        }


        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
                // c# shorthand for OnPropertyChanged
                PropertyChanged?.Invoke("Name");
            }
        }

        public string Device
        {
            get { return _device; }
            set
            {
                _device = value;
                OnPropertyChanged("Device");
                if (PropertyChanged != null)
                {
                    PropertyChanged("Device");
                }
            }
        }

        public string Port
        {
            get { return _port; }
            set
            {
                _port = value;
                OnPropertyChanged("Port");
                if (PropertyChanged != null)
                {
                    PropertyChanged("Port");
                }
            }
        }

        public Int32 Line
        {
            get { return _line; }
            set
            {
                _line = value;
                OnPropertyChanged("Line");
                if (PropertyChanged != null)
                {
                    PropertyChanged("Line");
                }
            }
        }

        public string DeviceType
        {
            get { return _deviceType; }
            set
            {
                _deviceType = value;
                OnPropertyChanged("DeviceType");
                if (PropertyChanged != null)
                {
                    PropertyChanged("DeviceType");
                }
            }
        }

        public string InOut
        {
            get { return _inOut; }
            set
            {
                _inOut = value;
                OnPropertyChanged("InOut");
                if (PropertyChanged != null)
                {
                    PropertyChanged("InOut");
                }
            }
        }

        public double Min
        {
            get { return _min; }
            set
            {
                _min = value;
                OnPropertyChanged("Min");
                if (PropertyChanged != null)
                {
                    PropertyChanged("Min");
                }
            }
        }

        public double Max
        {
            get { return _max; }
            set
            {
                _max = value;
                OnPropertyChanged("Max");
                if (PropertyChanged != null)
                {
                    PropertyChanged("Max");
                }
            }
        }

        public string InMode
        {
            get { return _inMode; }
            set
            {
                _inMode = value;
                OnPropertyChanged("InMode");
                if (PropertyChanged != null)
                {
                    PropertyChanged("InMode");
                }
            }
        }

        public double Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged("Value");

                if (PropertyChanged != null)
                {
                    PropertyChanged("Value");
                }
            }
        }

        public string Calibration
        {
            get { return _calibration; }
            set
            {
                _calibration = value;
                OnPropertyChanged("Calibration");
                if (PropertyChanged != null)
                {
                    PropertyChanged("Calibration");
                }
            }
        }

        public bool BoolValue
        {
            get { return _boolValue; }
            set
            {
                _boolValue = value;
                OnPropertyChanged("BoolValue");
                if (PropertyChanged != null)
                {
                    PropertyChanged("BoolValue");
                }
            }
        }

        public Int32 ErrorCode
        {
            get { return _errorCode; }
            set
            {
                _errorCode = value;
                OnPropertyChanged("ErrorCode");
                if (PropertyChanged != null)
                {
                    PropertyChanged("ErrorCode");
                }
            }
        }

        public string ErrorText
        {
            get { return _errorText; }
            set
            {
                _errorText = value;
                OnPropertyChanged("ErrorText");
                if (PropertyChanged != null)
                {
                    PropertyChanged("ErrorText");
                }
            }
        }

        public DateTime LastUpdate
        {
            get { return _lastUpdate; }
            set
            {
                _lastUpdate = value;
                OnPropertyChanged("LastUpdate");
                if (PropertyChanged != null)
                {
                    PropertyChanged("LastUpdate");
                }
            }
        }

        public string Type
        {
            get { return _type; }
            set
            {
                _type = value;
                OnPropertyChanged("Type");
                if (PropertyChanged != null)
                {
                    PropertyChanged("Type");
                }
            }
        }

        public string NIName
        {
            get { return _niName; }
            set
            {
                _niName = value;
                OnPropertyChanged("NIName");
                if (PropertyChanged != null)
                {
                    PropertyChanged("NIName");
                }
            }
        }

        public string NIPort
        {
            get { return _niPort; }
            set
            {
                _niPort = value;
                OnPropertyChanged("NIPort");
                if (PropertyChanged != null)
                {
                    PropertyChanged("NIPort");
                }
            }
        }

        public Int32 NIIndex
        {
            get { return _niIndex; }
            set
            {
                _niIndex = value;
                OnPropertyChanged("NIIndex");
                if (PropertyChanged != null)
                {
                    PropertyChanged("NIIndex");
                }
            }
        }

        public double[][] CalibrationTable
        {
            get { return _calibrationTable; }
            set
            {
                _calibrationTable = value;
                OnPropertyChanged("CalibrationTable");
                if (PropertyChanged != null)
                {
                    PropertyChanged("CalibrationTable");
                }
            }
        }

    }
}
