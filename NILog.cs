using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace NI_Interface
{
    public class LogFiles
    {
        private static readonly object writeLock = new object();

        public static void AddLogEntry(int debugLevel, String logMessage)
        {
            if (Convert.ToInt16(99) >= debugLevel)
            {
                System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        CultureInfo ci = new CultureInfo("en-US");
                        StringBuilder sb = new StringBuilder();
                        sb.Append(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff", ci));
                        sb.Append("; - ");
                        sb.Append(logMessage);
                        sb.Append(System.Environment.NewLine);
                        lock (writeLock)
                        {
                            using (StreamWriter outfile = new StreamWriter(@"NI_Interface.log", true))
                            {
                                outfile.Write(sb.ToString());
                                outfile.Flush();
                                outfile.Close();
                                outfile.Dispose();
                            }
                        }
                    }
                    catch (System.Exception) { }
                });
            }
        }

		internal static void AddLogEntry(int v, object p)
		{
			throw new NotImplementedException();
		}
	}
}

