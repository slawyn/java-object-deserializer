using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/****************************
 * 
 *  Logger classs for debugging
 * 
 ****************************/
namespace Loader
{
    class Logger
    {
        public const int LOGTYPE_ERROR = -1;
        public const int LOGTYPE_DEBUG = 0;
        public const int LOGTYPE_VERBOSE = 1;
        public const int LOGTYPE_NORMAL = 2;
        private static int mLoaderLogLevel;

        private static readonly string[] StatusText = { "Error:", "Debug:", "Verbose:", "Normal:" };

        public static void SetLoggingLevel(string arg)
        {
            mLoaderLogLevel = (int.Parse(arg) & 0x0F);
            mLoaderLogLevel = mLoaderLogLevel > LOGTYPE_NORMAL ? LOGTYPE_NORMAL : mLoaderLogLevel;
        }

        public static void Log(string text, int logtype)
        {
            if (logtype <= mLoaderLogLevel)
            {
                Console.Write(StatusText[mLoaderLogLevel + 1]);
                Console.WriteLine(text);
            }
        }
    }
}
