using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/****************************
 * 
 *  Helper functions
 *  
 *  Note: Java VM is Big-Endian
 * 
 ****************************/
namespace Loader
{
    class Helper
    {
        public static UInt16 ConvertToUint16(Span<byte> slice)
        {
            return (UInt16)(((UInt16)slice[0])<<8 | slice[1]);
        }

        public static UInt32 ConvertToUint32(Span<byte> slice)
        {
            return (UInt32)(((UInt32)slice[0]) << 24 | ((UInt32)slice[1]) << 16| ((UInt32)slice[2]) << 8|((UInt32)slice[3]));
        }

        public static UInt64 ConvertToUint64(Span<byte> slice)
        {
            return (UInt64)(((UInt64)slice[0]) << 56| ((UInt64)slice[1]) << 48 | ((UInt64)slice[2]) << 40| ((UInt64)slice[3]) << 32| ((UInt64)slice[4]) << 24| ((UInt64)slice[5]) << 16 | ((UInt64)slice[6]) << 8 | ((UInt64)slice[7]));
        }

        public static float ConvertToFloat(Span<byte> slice)
        {
            byte[] array = slice.ToArray();
            Array.Reverse(array, 0, array.Length);
            return System.BitConverter.ToSingle(slice.ToArray(), 0);

        }

        public static double ConvertToDouble(Span<byte> slice)
        {
            byte[] array = slice.ToArray();
            Array.Reverse(array, 0, array.Length);
            return System.BitConverter.ToDouble(slice.ToArray(), 0);
        }

        public static int ConvertToInt(Span<byte> slice)
        {
            byte[] array = slice.ToArray();
            Array.Reverse(array,0, array.Length);
            return System.BitConverter.ToInt32(array, 0);

        }
        public static UInt64 ConvertToLong(Span<byte> slice)
        {
            return ConvertToUint64(slice);

        }

        public static Int16 ConvertToShort(Span<byte> slice)
        {
            byte[] array = slice.ToArray();
            Array.Reverse(array, 0, array.Length);
            return System.BitConverter.ToInt16(slice.ToArray(), 0);

        }

        public static bool ConvertToBoolean(Span<byte> slice)
        {
            byte[] array = slice.ToArray();
            Array.Reverse(array, 0, array.Length);
            return System.BitConverter.ToBoolean(slice.ToArray(), 0);

        }

        public static string ConvertToHexByte(Span<byte> slice)
        {
            StringBuilder hex = new StringBuilder(slice.Length * 2);
            foreach (byte b in slice.ToArray())
                hex.AppendFormat("{0:X2}", b);
            return hex.ToString();
        }

        public static char ConvertToChar(byte singlebyte)
        {
            return (char)singlebyte;
        }

        public static string ConvertToUTF(Span<byte> slice)
        {   
            if(slice.Length>0)
                return Encoding.UTF8.GetString(slice.ToArray());
            return "";
        }

    }

}
