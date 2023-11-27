using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SprayPaintApp
{
    public static class ErrorLogger
    {
        private static readonly string LogFilePath = "error_log.txt";

        // LogError method to log an error message to a file
        public static void LogError(string ErrorMessage)
        {
            try
            {
                using (StreamWriter sm = new StreamWriter(LogFilePath, true))
                {
                    sm.WriteLine($"{DateTime.Now} - {ErrorMessage}");
                }
            }
            catch( Exception ex )
            {
                Console.WriteLine($"Error Logged to file: {ex.Message}");
            }
        }
    }
}
