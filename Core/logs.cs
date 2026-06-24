using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication_and_Authorization_Api.Core
{
    public class logs
    {
        public static void LogRequest(object Details, string errorfile)
        {
            string directorypath = @"D:\Minimart Logs";

            if (!Directory.Exists(directorypath))
            {
                Directory.CreateDirectory(directorypath);
            }

            //string filePath = Path.Combine(directorypath, errorfile);

            string filePath = Path.Combine(directorypath, errorfile);


            if (!File.Exists(filePath)) 
            {
                string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");

                filePath = Path.Combine(directorypath, $"{Path.GetFileNameWithoutExtension(errorfile)}_{timeStamp}{Path.GetExtension(errorfile)}");
                File.Create(filePath).Close();
            }


            //Write Request Details to the logfile

            using (StreamWriter writer = File.AppendText(filePath))
            {
                writer.WriteLine($"[{DateTime.Now}] Request {Details}");
            }

        }
    }
}
