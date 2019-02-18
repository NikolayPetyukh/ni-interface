using NationalInstruments.DAQmx;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml;

namespace NI_Interface
{
    public sealed class NIConfig : ModelBase
    {
        //private static readonly Lazy<ProgramConfig> lazy = new Lazy<ProgramConfig>(() => new ProgramConfig(), true);
        //public static ProgramConfig ProgramConfigInstance { get { return lazy.Value; } }
        // Windows AutoCorrected version of above Singleton
        static NIConfig()
        {
        }
        private NIConfig()
        {
            LoadBinFile("config.xml");
        }
        public static NIConfig ProgramConfigInstance { get; } = new NIConfig();
        // Singleton

        public Int32 NI_TaskTimeout { get; set; }
        public Int32 AverageWindowWidth { get; set; }
        public Int32 MovingAverage { get; set; }
        public Int32 DebugLevel { get; set; }

        public List<string> NiDigitalInPorts = new List<string>();
        public List<string> NiDigitalOutPorts = new List<string>();
        public List<string> NiAnalogInDevices = new List<string>();
        public List<string> NiAnalogOutDevices = new List<string>();

        public List<Int32> CalculatedValueToWrite = new List<Int32>();

        public static System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("en-US");

        private ObservableCollection<ChannelModel> _niChannels;
        public ObservableCollection<ChannelModel> NIChannels
        {
            get
            {
                if (_niChannels == null)
                    _niChannels = new ObservableCollection<ChannelModel>();
                return _niChannels;
            }
            set
            {
                _niChannels = value;
                OnPropertyChanged("NIChannels");
            }
        }

        private ObservableCollection<ChannelModel> _niAllAnalogIn;
        public ObservableCollection<ChannelModel> NiAllAnalogIn
        {
            get
            {
                if (_niAllAnalogIn == null)
                    _niAllAnalogIn = new ObservableCollection<ChannelModel>();
                return _niAllAnalogIn;
            }
            set
            {
                _niAllAnalogIn = value;
                OnPropertyChanged("NiAllAnalogIn");
            }
        }

        private ObservableCollection<ChannelModel> _niAllAnalogOut;
        public ObservableCollection<ChannelModel> NiAllAnalogOut
        {
            get
            {
                if (_niAllAnalogOut == null)
                    _niAllAnalogOut = new ObservableCollection<ChannelModel>();
                return _niAllAnalogOut;
            }
            set
            {
                _niAllAnalogOut = value;
                OnPropertyChanged("NiAllAnalogOut");
            }
        }

		public DigitalSingleChannelWriter[] digitalWriter { get; set; }
		public DigitalSingleChannelReader[] digitalReader { get; set; }

		public AnalogSingleChannelWriter[] analogWriter { get; set; }
		public AnalogSingleChannelReader[] analogReader { get; set; }

		private void LoadBinFile(string path)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);

                XmlNodeList list = doc.DocumentElement.GetElementsByTagName("device");
                foreach (XmlNode deviceNode in list)
                {
                    string deviceName = "";
                    Int32 index = 1;
                    foreach (XmlNode node in deviceNode.ChildNodes)
                    {
                        if (node.Name == "name")
                        {
                            deviceName = node.InnerText;
                        }
                        else if (deviceName != "" && node.Name == "channels")
                        {
                            foreach (XmlNode channelNode in node.ChildNodes)
                            {
                                ChannelModel channel = new ChannelModel();
                                foreach (XmlNode property in channelNode.ChildNodes)
                                {
                                    channel.Device = deviceName;

                                    if ("name" == property.Name) channel.Name = property.InnerText;
                                    else if ("port" == property.Name) channel.Port = property.InnerText;
                                    else if ("line" == property.Name) channel.Line = Convert.ToInt32(property.InnerText);
                                    else if ("devicetype" == property.Name) channel.DeviceType = property.InnerText;
                                    else if ("in" == property.Name) channel.InOut = property.InnerText;
                                    else if ("min" == property.Name) channel.Min = Convert.ToDouble(property.InnerText);
                                    else if ("max" == property.Name) channel.Max = Convert.ToDouble(property.InnerText);
                                    else if ("inmode" == property.Name) channel.InMode = property.InnerText;
                                    else if ("value" == property.Name) channel.Value = Convert.ToDouble(property.InnerText);
                                    else if ("calibration" == property.Name) channel.Calibration = property.InnerText;
                                    channel.BoolValue = 1 == channel.Value ? true : false;

                                    channel.ErrorCode = 1;
                                    channel.ErrorText = "No NI Communication";
                                    channel.LastUpdate = DateTime.Now;
                                    channel.Type = CreateType(channel.DeviceType, channel.InOut, channel.Line);
                                    channel.NIName = CreateNIName(channel.Device, channel.Port, channel.Line);
                                    channel.NIPort = CreateNIPort(channel.Device, channel.Port);
                                    channel.NIIndex = index;
                                }

                                if (("ai" == channel.Port) || ("ao" == channel.Port) || ("port0" == channel.Port) || ("port1" == channel.Port))
                                {
                                    NIChannels.Add(channel);
                                    index++;
                                }
                            }
                        }
                    }
                }

                list = doc.DocumentElement.GetElementsByTagName("generics");
                foreach (XmlNode genericsNode in list)
                {
                    foreach (XmlNode genNode in genericsNode.ChildNodes)
                    {
                        string genericName = "";
                        string genericValue = "";
                        foreach (XmlNode node in genNode.ChildNodes)
                        {
                            if ("genericName" == node.Name) genericName = node.InnerText;
                            else if ("" != genericName && "genericValue" == node.Name) genericValue = node.InnerText;
                        }

                        if ("Logging Level" == genericName) DebugLevel = Convert.ToInt32(genericValue);
                        else if ("Averaging Window Width" == genericName) AverageWindowWidth = Convert.ToInt32(genericValue);
                        else if ("Moving Average" == genericName) MovingAverage = Convert.ToInt32(genericValue);
                        else if ("Serial Devices Timeout" == genericName) NI_TaskTimeout = Convert.ToInt32(genericValue);
                        if (NI_TaskTimeout <= 9) { NI_TaskTimeout = 10; }
                    }
                }
            }
            catch (Exception ex)
            {
                LogFiles.AddLogEntry(12, String.Format("LoadBinFile Config File Issue: " + "Exception: " + ex.Message));
                return;
            }
            return;
        }

        private string CreateType(string deviceType, string inOut, Int32 line)
        {
            string _type = deviceType;
            _type += "-";
            _type += inOut;
            _type += " ";
            _type += line.ToString();

            return _type;
        }

        private string CreateNIName(string device, string port, Int32 line)
        {
            string _niName = "";
            if ("port0" == port || "port1" == port) _niName = device + "/" + port + "/line" + Convert.ToString(line);
            else if ("ai" == port || "ao" == port) _niName = device + "/" + port + Convert.ToString(line);
            else _niName = port + Convert.ToString(line);

            return _niName;
        }

        private string CreateNIPort(string device, string port)
        {
            string _niPort = "";
            if ("port0" == port || "port1" == port) _niPort = device + "/" + port;
            else if ("ai" == port || "ao" == port) _niPort = device + "/" + port;
            else _niPort = port;

            return _niPort;
        }

        public void RebuildChannelSets()
        {
            LogFiles.AddLogEntry(12, String.Format("Port Collection Changed:"));
			System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
			TimeSpan ts = stopWatch.Elapsed;
			TimeSpan ts1;

			
			NiAllAnalogIn.Clear();
            NiAllAnalogOut.Clear();
            NiDigitalInPorts.Clear();
            NiDigitalOutPorts.Clear();
            NiAnalogInDevices.Clear();
            NiAnalogOutDevices.Clear();

            CalculatedValueToWrite.Clear();
			stopWatch.Start();

			string[] deviceList = DaqSystem.Local.Devices;
			stopWatch.Stop();
			ts1 = stopWatch.Elapsed;
			Console.WriteLine("cunt  "+ deviceList.LongLength+ "  "+deviceList.Length);
			foreach (string currentDevice in deviceList)
            {
				stopWatch.Start();

				string[] rowList = { String.Format("Device Name:{0}", currentDevice),
                        String.Format(" Device Type:{0}", DaqSystem.Local.LoadDevice(currentDevice).ProductType),
                        String.Format(DaqSystem.Local.LoadDevice(currentDevice).IsSimulated ? " Simulated Device":" Physical Device"),
                        String.Format(" Serialnr.:{0}", DaqSystem.Local.LoadDevice(currentDevice).SerialNumber.ToString("X")),
                        String.Format(" Device ID:{0}", Convert.ToString(DaqSystem.Local.LoadDevice(currentDevice).DeviceID))
                        // maybe more
                };

			

				LogFiles.AddLogEntry(12, String.Format("{0},{1},{2},{3},{4}", rowList[0], rowList[1], rowList[2], rowList[3], rowList[4]));

                for (Int32 i = 0; i < DaqSystem.Local.LoadDevice(currentDevice).AIPhysicalChannels.Count(); i++)
                {
                    ChannelModel entry = new ChannelModel();
                    entry.Device = " ";
                    entry.Name = " ";
                    entry.Port = "ai";
                    entry.Line = 0;
                    entry.DeviceType = " ";
                    entry.InOut = "In";
                    entry.Min = 0;
                    entry.Max = 5;
                    entry.InMode = " ";
                    entry.Value = 0;
                    entry.Calibration = "-";
                    //entry.CalibrationTable[20][1] = 0;
                    entry.NIName = String.Format(Convert.ToString(DaqSystem.Local.LoadDevice(currentDevice).AIPhysicalChannels[i]));

                    entry.ErrorCode = 1;
                    entry.ErrorText = "Channel Not In Use.";

                    try
                    {
                        ChannelModel match = new ChannelModel();
                        match = NIChannels.FirstOrDefault(item => item.NIName == Convert.ToString(DaqSystem.Local.LoadDevice(currentDevice).AIPhysicalChannels[i]));
                        if (null != match)
                        {
                            if (match.Min < match.Max) // TODO: check min & max in 0.2 1 5 10 ? TODO: for MFC check if calibration range and config range match
                            {
                                NiAllAnalogIn.Add(entry); // For analog, only add if used/matched

                                NiAllAnalogIn.Last().Name = match.Name;
                                NiAllAnalogIn.Last().Min = match.Min;
                                NiAllAnalogIn.Last().Max = match.Max;
                                NiAllAnalogIn.Last().InMode = match.InMode;
                                NiAllAnalogIn.Last().Calibration = match.Calibration;  // TODO: redundant line??
                                if (match.Calibration != "-")
                                {
                                    try
                                    {
                                        string[] lines = File.ReadAllLines("LookupTables/" + match.Calibration);
                                        Int32 linecounter = 0;
                                        foreach (string line in lines)
                                        {
                                            line.Trim();
                                            if (line == "" || line.StartsWith("#")) { continue; }
                                            string[] words = line.Split('=');

                                            NiAllAnalogIn.Last().CalibrationTable[linecounter][0] = double.Parse(words[0], ci);
                                            NiAllAnalogIn.Last().CalibrationTable[linecounter][1] = double.Parse(words[1], ci);
                                            linecounter++;
                                        }
                                    }
                                    catch
                                    {
                                        if (File.Exists("LookupTables/" + match.Calibration))
                                        {
                                            LogFiles.AddLogEntry(12, String.Format("RebuildChannelSets, Bad LookupTable for {0}", entry.NIName));
                                        }
                                        else
                                        {
                                            LogFiles.AddLogEntry(12, String.Format("RebuildChannelSets, LookupTable for {0} Not Found", entry.NIName));
                                        }
                                    }
                                }
                            }
                            else
                            {
                                LogFiles.AddLogEntry(12, String.Format("RebuildChannelSets, channel {0} Min/Max Issue", entry.NIName));
                            }
                        }
                        else
                        {
                            LogFiles.AddLogEntry(99, String.Format("RebuildChannelSets, No Match, channel {0} not added to AIN Channel Set", entry.NIName));
                        }
                    }
                    catch
                    {
                        LogFiles.AddLogEntry(12, String.Format("RebuildChannelSets, AIN Channel Error"));
                    }
                }

                for (Int32 i = 0; i < DaqSystem.Local.LoadDevice(currentDevice).AOPhysicalChannels.Count(); i++)
                {
                    ChannelModel entry = new ChannelModel();
                    entry.Device = " ";
                    entry.Name = " ";
                    entry.Port = "ao";
                    entry.Line = 0;
                    entry.DeviceType = " ";
                    entry.InOut = "Out";
                    entry.Min = 0;
                    entry.Max = 5;
                    entry.InMode = " ";
                    entry.Value = 0;
                    entry.Calibration = "-";
                    entry.NIName = String.Format(Convert.ToString(DaqSystem.Local.LoadDevice(currentDevice).AOPhysicalChannels[i]));

                    entry.ErrorCode = 1;
                    entry.ErrorText = "Channel Not In Use.";

                    try
                    {
                        ChannelModel match = new ChannelModel();
                        match = NIChannels.FirstOrDefault(item => item.NIName == Convert.ToString(DaqSystem.Local.LoadDevice(currentDevice).AOPhysicalChannels[i]));
                        if (null != match)
                        {
                            if (match.Min < match.Max) // TODO: check min & max in 0.2 1 5 10 ?
                            {
                                NiAllAnalogOut.Add(entry); // For analog, only add if used/matched

                                NiAllAnalogOut.Last().Name = match.Name;
                                NiAllAnalogOut.Last().Min = match.Min;
                                NiAllAnalogOut.Last().Max = match.Max;
                                NiAllAnalogOut.Last().Calibration = match.Calibration;  // TODO: redundant line??
                                if (match.Calibration != "-")
                                {
                                    try
                                    {
                                        string[] lines = File.ReadAllLines("LookupTables/" + match.Calibration);
                                        Int32 linecounter = 0;
                                        foreach (string line in lines)
                                        {
                                            line.Trim();
                                            if (line == "" || line.StartsWith("#")) { continue; }
                                            string[] words = line.Split('=');
                                            // x and y swapped for AOUT
                                            NiAllAnalogOut.Last().CalibrationTable[linecounter][1] = double.Parse(words[0], ci);
                                            NiAllAnalogOut.Last().CalibrationTable[linecounter][0] = double.Parse(words[1], ci);
                                            linecounter++;
                                        }
                                    }
                                    catch
                                    {
                                        if (File.Exists("LookupTables/" + match.Calibration))
                                        {
                                            LogFiles.AddLogEntry(12, String.Format("RebuildChannelSets, Bad LookupTable for {0}", entry.NIName));
                                        }
                                        else
                                        {
                                            LogFiles.AddLogEntry(12, String.Format("RebuildChannelSets, LookupTable for {0} Not Found", entry.NIName));
                                        }
                                    }
                                }
                            }
                            else
                            {
                                LogFiles.AddLogEntry(12, String.Format("RebuildChannelSets, channel {0} Min/Max Issue", entry.NIName));
                            }
                        }
                        else
                        {
                            LogFiles.AddLogEntry(99, String.Format("RebuildChannelSets, No Match, channel {0} not added to AOUT Channel Set", entry.NIName));
                        }
                    }
                    catch
                    {
                        LogFiles.AddLogEntry(12, String.Format("RebuildChannelSets, AOUT Channel Error"));
                    }
                }

                if (0 < DaqSystem.Local.LoadDevice(currentDevice).AIPhysicalChannels.Count())
                {
                    NiAnalogInDevices.Add(DaqSystem.Local.LoadDevice(currentDevice).DeviceID);
                }

                if (0 < DaqSystem.Local.LoadDevice(currentDevice).AOPhysicalChannels.Count())
                {
                    NiAnalogOutDevices.Add(DaqSystem.Local.LoadDevice(currentDevice).DeviceID);
                }

                for (Int32 i = 0; i < DaqSystem.Local.LoadDevice(currentDevice).DIPorts.Count(); i++)
                {
                    NiDigitalInPorts.Add(DaqSystem.Local.LoadDevice(currentDevice).DIPorts[i]);
                }

                for (Int32 i = 0; i < DaqSystem.Local.LoadDevice(currentDevice).DOPorts.Count(); i++)
                {
                    NiDigitalOutPorts.Add(DaqSystem.Local.LoadDevice(currentDevice).DOPorts[i]);
                    CalculatedValueToWrite.Add(-1);
                }
		

			}


			#region "print new digital port lists"
			LogFiles.AddLogEntry(94, String.Format("New Digital Port Lists"));
            for (Int32 i = 0; i < NiDigitalInPorts.Count(); i++)
            {
                LogFiles.AddLogEntry(94, String.Format("Digital In Port NiDigitalInPorts[{1}]: {0}", NiDigitalInPorts[i], i));
            }
            for (Int32 i = 0; i < NiDigitalOutPorts.Count(); i++)
            {
                LogFiles.AddLogEntry(94, String.Format("Digital Out Port NiDigitalOutPorts[{1}]: {0}", NiDigitalOutPorts[i], i));
            }
            #endregion

            #region "print new analog devices lists"
            LogFiles.AddLogEntry(94, String.Format("New Analog Devices Lists"));
            for (Int32 i = 0; i < NiAnalogInDevices.Count(); i++)
            {
                LogFiles.AddLogEntry(94, String.Format("Analog In Device NiAnalogInDevices[{1}]: {0}", NiAnalogInDevices[i], i));
            }
            for (Int32 i = 0; i < NiAnalogOutDevices.Count(); i++)
            {
                LogFiles.AddLogEntry(94, String.Format("Analog Out Device NiAnalogOutDevices[{1}]: {0}", NiAnalogOutDevices[i], i));
            }
            #endregion

            #region "print new channels list"
            LogFiles.AddLogEntry(94, String.Format("New Channels List"));
            for (Int32 i = 0; i < NiAllAnalogIn.Count(); i++)
            {
                LogFiles.AddLogEntry(94, String.Format("{0}, {1}", NiAllAnalogIn[i].NIName, NiAllAnalogIn[i].Name));
            }
            for (Int32 i = 0; i < NiAllAnalogOut.Count(); i++)
            {
                LogFiles.AddLogEntry(94, String.Format("{0}, {1}", NiAllAnalogOut[i].NIName, NiAllAnalogOut[i].Name));
            }
			#endregion
			stopWatch.Stop();
			ts1 = stopWatch.Elapsed;
			Console.WriteLine("ыtring[]  " + ts1);
		}

        public void RebuildTasks()
        {
            // http://zone.ni.com/reference/en-XX/help/370466AD-01/mxdevconsid/simultaneoustasks/
            // http://zone.ni.com/reference/en-XX/help/370466AD-01/mxcncpts/explicitimplicitstates/
            // http://zone.ni.com/reference/en-XX/help/370466AD-01/mxcncpts/taskstatemodel/

            #region "DIGITAL OUT TASKS"

            NationalInstruments.DAQmx.Task[] digitalWriteTask = new NationalInstruments.DAQmx.Task[NiDigitalOutPorts.Count()];

            digitalWriter = new DigitalSingleChannelWriter[NiDigitalOutPorts.Count()];
            for (Int32 i = 0; i < NiDigitalOutPorts.Count(); i++)
            {
                digitalWriteTask[i] = new NationalInstruments.DAQmx.Task();
                digitalWriteTask[i].DOChannels.CreateChannel(NiDigitalOutPorts[i], "portx", ChannelLineGrouping.OneChannelForAllLines);
                digitalWriter[i] = new DigitalSingleChannelWriter(digitalWriteTask[i].Stream);

                digitalWriteTask[i].Control(TaskAction.Verify);
                //digitalWriteTask[i].Control(TaskAction.Reserve);  // TODO
                //digitalWriteTask[i].Control(TaskAction.Commit);
                //digitalWriteTask[i].Control(TaskAction.Start);
            }
            #endregion

            #region "DIGITAL IN TASKS"
            NationalInstruments.DAQmx.Task[] digitalReadTask = new NationalInstruments.DAQmx.Task[NiDigitalInPorts.Count()];
            digitalReader = new DigitalSingleChannelReader[NiDigitalInPorts.Count()];
            for (Int32 i = 0; i < NiDigitalInPorts.Count(); i++)
            {
                digitalReadTask[i] = new NationalInstruments.DAQmx.Task();
                digitalReadTask[i].DIChannels.CreateChannel(NiDigitalInPorts[i], "portx", ChannelLineGrouping.OneChannelForAllLines);
                digitalReader[i] = new DigitalSingleChannelReader(digitalReadTask[i].Stream);

                digitalReadTask[i].Control(TaskAction.Verify);
                //digitalReadTask[i].Control(TaskAction.Reserve);
                //digitalReadTask[i].Control(TaskAction.Commit);
                //digitalReadTask[i].Control(TaskAction.Start);
            }
            #endregion

            #region "ANALOG OUT TASKS"
            NationalInstruments.DAQmx.Task[] analogOutTask = new NationalInstruments.DAQmx.Task[NiAllAnalogOut.Count()];
            analogWriter = new AnalogSingleChannelWriter[NiAllAnalogOut.Count()];

            Int32 analogOutCounter = 0;
            foreach (ChannelModel channel in NiAllAnalogOut)  //for (Int32 analogCounter = 0; analogCounter < NI_ProgramConfigInstance.NiAllAnalogOut.Count(); analogCounter++)
            {
                try
                {
                    analogOutTask[analogOutCounter] = new NationalInstruments.DAQmx.Task();
                    AOChannel myAOChannel;
                    myAOChannel = analogOutTask[analogOutCounter].AOChannels.CreateVoltageChannel(channel.NIName, "", Convert.ToDouble(channel.Min), Convert.ToDouble(channel.Max), AOVoltageUnits.Volts);
                    analogWriter[analogOutCounter] = new AnalogSingleChannelWriter(analogOutTask[analogOutCounter].Stream);
                    analogOutTask[analogOutCounter].Control(TaskAction.Verify);
                    //analogOutTask[analogOutCounter].Control(TaskAction.Reserve);
                    //analogOutTask[analogOutCounter].Control(TaskAction.Commit);
                    //analogOutTask[analogOutCounter].Control(TaskAction.Start);
                }
                catch (Exception ex)
                {
                    LogFiles.AddLogEntry(12, String.Format("Analog Out Build Task Error, Message: {1} {0}", ex.Message, ex.Source)); //channel.Value
                                                                                                                                                        //channel.ErrorCode = substring status code ex.Message;
                    channel.ErrorText = ex.Message;
                }
                analogOutCounter++;
            }
            #endregion

            #region "ANALOG IN TASKS"
            NationalInstruments.DAQmx.Task[] analogInTask = new NationalInstruments.DAQmx.Task[NiAllAnalogIn.Count()];
            analogReader = new AnalogSingleChannelReader[NiAllAnalogIn.Count()];

            Int32 analogInCounter = 0;
            foreach (ChannelModel channel in NiAllAnalogIn)
            {
                try
                {
                    analogInTask[analogInCounter] = new NationalInstruments.DAQmx.Task();
                    AIChannel myAIChannel;
                    if ("RSE" == channel.InMode) { myAIChannel = analogInTask[analogInCounter].AIChannels.CreateVoltageChannel(channel.NIName, "", AITerminalConfiguration.Rse, Convert.ToDouble(channel.Min), Convert.ToDouble(channel.Max), AIVoltageUnits.Volts); }
                    else if ("NRSE" == channel.InMode) { myAIChannel = analogInTask[analogInCounter].AIChannels.CreateVoltageChannel(channel.NIName, "", AITerminalConfiguration.Nrse, Convert.ToDouble(channel.Min), Convert.ToDouble(channel.Max), AIVoltageUnits.Volts); }
                    else if ("Diff" == channel.InMode) { myAIChannel = analogInTask[analogInCounter].AIChannels.CreateVoltageChannel(channel.NIName, "", AITerminalConfiguration.Differential, Convert.ToDouble(channel.Min), Convert.ToDouble(channel.Max), AIVoltageUnits.Volts); }
                    else if ("PDiff" == channel.InMode) { myAIChannel = analogInTask[analogInCounter].AIChannels.CreateVoltageChannel(channel.NIName, "", AITerminalConfiguration.Pseudodifferential, Convert.ToDouble(channel.Min), Convert.ToDouble(channel.Max), AIVoltageUnits.Volts); }
                    else
                    {
                        // channel.InMode error
                        myAIChannel = analogInTask[analogInCounter].AIChannels.CreateVoltageChannel(channel.NIName, "", AITerminalConfiguration.Rse, Convert.ToDouble(channel.Min), Convert.ToDouble(channel.Max), AIVoltageUnits.Volts);
                    }
                    analogInTask[analogInCounter].Control(TaskAction.Verify);
                    analogReader[analogInCounter] = new AnalogSingleChannelReader(analogInTask[analogInCounter].Stream);
                    //try { analogInTask[analogInCounter].Control(TaskAction.Reserve); } // TODO: check status
                    //catch { }
                    //try { analogInTask[analogInCounter].Control(TaskAction.Commit); }
                    //catch { }
                    //try { analogInTask[analogInCounter].Control(TaskAction.Start); } // By explicitly starting the task, these operations are performed once, not each time the read or write operation is performed.
                    //catch { }
                }
                catch (Exception ex)
                {
                    LogFiles.AddLogEntry(12, String.Format("Analog In Build Task Error, Message: {1} {0}", ex.Message, ex.Source)); //channel.Value
                                                                                                                                                       //channel.ErrorCode = substring status code ex.Message;
                    channel.ErrorText = ex.Message;
                }
                analogInCounter++;
            }
            #endregion
        }

    }
}
