using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/****************************
 * 
 *  Data container for controlled parsing of bytes
 * 
 ****************************/
namespace Loader
{
    class DataContainer
    {
        private int Offset;
        private byte[] Bytes;

        public DataContainer(byte[] Bytes) 
        {
            this.Bytes = Bytes;
            Offset = 0;
        }

        public int GetAvailable() {
            if (Offset < Bytes.Length)
                return Bytes.Length - Offset;
            else
                return 0;
        }

        public byte PopByte()
        {
            byte Out = this.Bytes[Offset];
            Offset += 1;
            return Out;
        }

        public byte PopNextControl() {
            return PopByte();
        }


        public byte PeekByte()
        {
            return this.Bytes[Offset];
        }

        public Span<byte> PeekBytes(int Count) 
        {
            var slice = new Span<byte>(this.Bytes, this.Offset, Count);
            return slice;
        }

        public Span<byte> PopAlotOfBytes(UInt64 Count) 
        {
            // TODO: Change this in the future to be able to pop amount 
            // of bytes defined by 64-bit Value
            if (Count >= 0x8000000000000000)
                return null;
            
            return PopBytes((int)Count);
        }

        public Span<byte> PopBytes(int Count)
        {
            var slice = new Span<byte>(this.Bytes, Offset, Count);
            Offset += Count;
            return slice;
        }

    }
}
