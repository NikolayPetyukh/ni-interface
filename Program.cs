using System;
using System.Linq;
using System.Threading;

namespace NI_Interface
{
    public static class NI_InterfaceClass
    {
        private static NI_Interface.NIConfig NI_ProgramConfigInstance = NI_Interface.NIConfig.ProgramConfigInstance;

        private static readonly System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("en-US");
		private static System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
		private static Thread DigitalReaderWriterThread = null;
        private static Thread AnalogReaderThread = null;
        private static Thread AnalogWriterThread = null;

        public static void Main()
        {
			stopWatch.Start();


			LogFiles.AddLogEntry(1, String.Format("NI_Interface Started"));

            NI_Interface.GetSet UpdateNIChannels = new NI_Interface.GetSet();

            NI_ProgramConfigInstance.RebuildChannelSets();
            NI_ProgramConfigInstance.RebuildTasks();

			stopWatch.Stop();

			TimeSpan ts = stopWatch.Elapsed;
			TimeSpan ts1;

			Console.WriteLine("1    " + ts);
            while (true)
            {
                Thread.Sleep(100);

                if (true)
				{
					stopWatch.Start();


					LogFiles.AddLogEntry(99, String.Format("Start digital:"));
                    #region "Digital OUT WRITING"

                    DigitalReaderWriterThread = Thread.CurrentThread;

                    try
                    {
                        Int32 digitalCounter = 0;
                        while (digitalCounter < NI_ProgramConfigInstance.NiDigitalOutPorts.Count())
                        {
                            NI_ProgramConfigInstance.CalculatedValueToWrite[digitalCounter] = UpdateNIChannels.UpdateDOUTChannels(digitalCounter);
                            LogFiles.AddLogEntry(99, String.Format("DOUT: CalculatedValueToWrite {0}", NI_ProgramConfigInstance.CalculatedValueToWrite[digitalCounter]));
                            if (-1 != NI_ProgramConfigInstance.CalculatedValueToWrite[digitalCounter])
                            {
                                NI_ProgramConfigInstance.digitalWriter[digitalCounter].WriteSingleSamplePort(true, (UInt32)NI_ProgramConfigInstance.CalculatedValueToWrite[digitalCounter]);
                            }
                            else
                            {
                                LogFiles.AddLogEntry(97, String.Format("DigitalWriteTask: No Update needed"));
                            }
                            digitalCounter++;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogFiles.AddLogEntry(12, String.Format("DigitalWriteTask Exception: {0}", ex.Message));
                    }
                    #endregion

                    #region "Digital IN READING"
                    try
                    {
                        Int32 digitalCounter = 0;
                        while (digitalCounter < NI_ProgramConfigInstance.NiDigitalInPorts.Count())
                        {
                            UInt32 data = NI_ProgramConfigInstance.digitalReader[digitalCounter].ReadSingleSamplePortUInt32();
                            UpdateNIChannels.UpdateDINChannels(NI_ProgramConfigInstance.NiDigitalInPorts[digitalCounter], data);
                            digitalCounter++;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogFiles.AddLogEntry(12, String.Format("DigitalReadTask Exception: {0}", ex.Message));
                    }
                    #endregion
                    LogFiles.AddLogEntry(99, String.Format("End digital:"));




                    AnalogWriterThread = Thread.CurrentThread;
                    #region "ANALOG OUT WRITING"

                    //sync to latest setpoint before writing
                    foreach (ChannelModel channel in NI_ProgramConfigInstance.NiAllAnalogOut)
                    {
                        UpdateNIChannels.UpdateMSIAOUTChannel(channel);
                    }

                    Int32 analogOutCounter = 0;
                    foreach (ChannelModel channel in NI_ProgramConfigInstance.NiAllAnalogOut)
                    {
                        try
                        {
                            NI_ProgramConfigInstance.analogWriter[analogOutCounter].WriteSingleSample(true, UpdateNIChannels.UpdateAOUTChannel(channel.NIName));
                            if (61 != channel.ErrorCode && 62 != channel.ErrorCode)
                            {
                                channel.ErrorCode = 0;
                                channel.ErrorText = "Success";
                            }
                        }
                        catch (Exception ex)
                        {
                            LogFiles.AddLogEntry(12, String.Format("Write Error Message: {1} {0}", ex.Message, ex.Source)); //channel.Value
                                                                                                                            //channel.ErrorCode = substring status code ex.Message;
                            channel.ErrorText = ex.Message;
                        }
                        analogOutCounter++;
                    }
                    #endregion
                    LogFiles.AddLogEntry(99, String.Format("END AOUT"));


                    #region "ANALOG IN READING working with 3seconds"

                    AnalogReaderThread = Thread.CurrentThread;
                    #region "ANALOG IN READING"
                    Int32 analogInCounter = 0;
                    LogFiles.AddLogEntry(99, String.Format("START AIN:"));
                    foreach (ChannelModel channel in NI_ProgramConfigInstance.NiAllAnalogIn)
                    {
                        try
                        {
                            //Prepare the table for Data
                            double sum = 0;
                            double[] data = NI_ProgramConfigInstance.analogReader[analogInCounter].ReadMultiSample(NI_ProgramConfigInstance.AverageWindowWidth);
                            foreach (double dat in data) { sum += dat; }
                            double newValue = sum / NI_ProgramConfigInstance.AverageWindowWidth;
                            UpdateNIChannels.UpdateAINChannel(channel.NIName, newValue);
                            channel.ErrorCode = 0;
                            channel.ErrorText = "Success";
                        }
                        catch (Exception ex)
                        {
                            LogFiles.AddLogEntry(12, String.Format("Read Error Message: {1} {0}", ex.Message, ex.Source));
                            // TODO channel.ErrorCode = substring status code ex.Message;
                            channel.ErrorText = ex.Message;
                        }
                        analogInCounter++;
                    }

                    // Sync after reading values
                    foreach (ChannelModel channel in NI_ProgramConfigInstance.NiAllAnalogIn)
                    {
                        UpdateNIChannels.UpdateMSIAINChannel(channel);
                    }
                    #endregion
                    LogFiles.AddLogEntry(99, String.Format("End AIN"));
                    #endregion

                }
				stopWatch.Stop();
				ts1 = stopWatch.Elapsed;
				Console.WriteLine("2    " + ts1);
			}
            LogFiles.AddLogEntry(1, String.Format("NI_Interface Ended"));
        }

    }
}