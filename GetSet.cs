using System;
using System.Linq;

namespace NI_Interface
{
    public class GetSet
    {
        private static NI_Interface.NIConfig NI_ProgramConfigInstance = NI_Interface.NIConfig.ProgramConfigInstance;

        public GetSet()
        {
            //empty
        }

        public Int32 UpdateDOUTChannels(Int32 digitalPortReference)
        {
            Int32 calculatedInt = -1;
            Random r = new Random();
            calculatedInt = (r.Next(10, 1000));
            return calculatedInt;
        }

        public void UpdateDINChannels(string port, UInt32 readValue)
        {
            LogFiles.AddLogEntry(97, String.Format("Update DIN Channel: {0}", readValue));
            return;
        }

        public double UpdateAOUTChannel(string thisNIName)
        {
            // using RAW/native Int32 is more performance efficient, but maybe device dependent ?
            try
            {
                ChannelModel match = new ChannelModel();
                match = NI_ProgramConfigInstance.NiAllAnalogOut.First(item => item.NIName == thisNIName);

                match.ErrorCode = 0;
                match.ErrorText = "Success";
                LogFiles.AddLogEntry(97, String.Format("Update AOUT Channel: {0}, Value: {1}", match.NIName, match.Value));
                return match.Value;
            }
            catch
            {
                LogFiles.AddLogEntry(14, String.Format("Update AOUT Channel Error: No Match found for {0}", thisNIName));
                return 0;
            }
        }

        public void UpdateAINChannel(string thisNIName, double readValue)
        {
            try
            {
                // using RAW/native Int32 is more performance efficient, but maybe device dependent ?
                ChannelModel match = new ChannelModel();
                match = NI_ProgramConfigInstance.NiAllAnalogIn.First(item => item.NIName == thisNIName);
                // TODO add MovingAverage
                match.Value = readValue;
                LogFiles.AddLogEntry(97, String.Format("Update AIN Channel: {0}, Value: {1}", match.NIName, match.Value));
                return;
            }
            catch
            {
                LogFiles.AddLogEntry(14, String.Format("Update AIN Channel Error: No Match found for {0}", thisNIName));
                return;
            }
        }


        #region "Sync"
        public void UpdateMSIAOUTChannel(ChannelModel channel)
        {
        }

        public void UpdateMSIAINChannel(ChannelModel channel)
        {
        }
        #endregion


        public static void ReportError(string error)
        {
            // TODO report relevant errors to UI
        }


    }
}
