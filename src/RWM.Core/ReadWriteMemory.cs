using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RWM.Core
{
    public class ReadWriteMemory
    {
        //The function to read the memory of a process
        [DllImport("kernel32.dll")]
        private static extern Int32 ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, UInt32 dwSize, ref IntPtr lpNumberOfBytesRead);

        //The function to write into the memory of a process
        [DllImport("kernel32.dll")]
        private static extern Int32 WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, UInt32 dwSize, ref IntPtr lpNumberOfBytesRead);

        private readonly IntPtr _ProcessHandle;
        private ProcessModule _ProcessModule;
        private readonly IntPtr _BaseAddress;
        public IntPtr EndBaseAddress;
        public IntPtr BaseAddress => _BaseAddress;
        private Process[] _ProcessList;
        /// <summary>
        /// Initializing the class
        /// </summary>
        /// <param name="processName">The name of the process you want to read or write into the memory.</param>
        public ReadWriteMemory(string processName)
        {
            try
            {
                _ProcessList = Process.GetProcessesByName(processName);
                if (!_ProcessList.Any())
                {
                    //TODO: Throw exception that the process couldn't been found
                }
                else
                {
                    _ProcessHandle = _ProcessList.First().Handle;
                    _ProcessModule = _ProcessList.First().MainModule;
                    _BaseAddress = _ProcessModule.BaseAddress;
                    EndBaseAddress =  IntPtr.Add(_BaseAddress, _ProcessModule.ModuleMemorySize);
                }
            }
            catch (Exception ex)
            {
                //TODO: Throw a real exception which helps the user to find out what's wrong.
            }
        }

        #region "Reading from memory"
        /// <summary>
        /// Returns an byte array from the memory address to read from
        /// </summary>
        /// <param name="address">The memory address to read from</param>
        /// <param name="length">The length of the value</param>
        /// <returns></returns>
        private byte[] ReadBase(int address, int length = 4)
        {
            byte[] retBuffer = new byte[length];
            IntPtr zero = IntPtr.Zero;

            ReadProcessMemory(_ProcessHandle, new IntPtr(address), retBuffer, (UInt32)retBuffer.Length, ref zero);

            return retBuffer;
        }

        /// <summary>
        /// Returns an byte array from the memory address to read from
        /// </summary>
        /// <param name="address">The memory address to read from</param>
        /// <param name="length">The length of the value (Optional)</param>
        /// <returns>An byte array</returns>
        public byte[] ReadByte(int address, int length = 4)
        {
            return ReadBase(address, length);
        }

        /// <summary>
        /// Returns an integer from the memory address to read from
        /// </summary>
        /// <param name="address">The memory address to read from</param>
        /// <param name="length">The length of the value (Optional)</param>
        /// <returns>An integer value</returns>
        public int ReadInt(int address, int length = 4)
        {
            return BitConverter.ToInt32(ReadBase(address, length), 0);
        }

        /// <summary>
        /// Returns an integer from the memory address to read from
        /// </summary>
        /// <param name="address">The memory address to read from</param>
        /// <param name="offsets">The offsets that are pointing to the right address</param>
        /// <param name="length">The length of the value (Optional)</param>
        /// <returns>An integer value</returns>
        public int ReadInt(int address, int[] offsets, int length = 4)
        {
            var value = ReadInt(_BaseAddress.ToInt32() + address);
            foreach (int offset in offsets)
            {
                value = ReadInt(value + offset);
            }
            return value;
        }
        /// <summary>
        /// Returns an float from the memory address to read from
        /// </summary>
        /// <param name="address">The memory address to read from</param>
        /// <param name="length">The length of the value (Optional)</param>
        /// <returns>An integer value</returns>
        public float ReadFloat(int address, int length = 4)
        {
            return BitConverter.ToSingle(ReadBase(address, length), 0);
        }

        /// <summary>
        /// Returns an string from the memory address to read from
        /// </summary>
        /// <param name="address">The memory address to read from</param>
        /// <param name="length">The length of the value (Optional)</param>
        /// <returns>An integer value</returns>
        public string ReadString(int address, int length = 4)
        {
            return Encoding.ASCII.GetString(ReadBase(address, length));
        }
        #endregion

        #region "Writing to memory"
        /// <summary>
        /// Writes an integer value into the memory address.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="value">The value which will be written into the memory address.</param>
        public void Write(int address, int value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            IntPtr zero = IntPtr.Zero;

            WriteProcessMemory(_ProcessHandle, new IntPtr(address), buffer, (UInt32)buffer.Length, ref zero);
        }

        /// <summary>
        /// Writes an string value into the memory address.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="value">The value which will be written into the memory address.</param>
        public void Write(int address, string value)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(value);
            IntPtr textLength = new IntPtr(value.Length);

            WriteProcessMemory(_ProcessHandle, new IntPtr(address), buffer, (UInt32)buffer.Length, ref textLength);
        }

        /// <summary>
        /// Writes an float value into the memory address.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="value">The value which will be written into the memory address.</param>
        public void Write(int address, float value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            IntPtr zero = IntPtr.Zero;

            WriteProcessMemory(_ProcessHandle, new IntPtr(address), buffer, 4, ref zero);
        }

        /// <summary>
        /// Writes an float value into the memory address.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="offsets">The offsets that are pointing to the right address.</param>
        /// <param name="value">The value which will be written into the memory address.</param>
        public void Write(int address, int[] offsets, float value)
        {
            var targetPointer = IntPtr.Add(_BaseAddress, address);
            var intBaseValue = ReadInt(targetPointer.ToInt32());
            for (int i = 0; i < offsets.Length - 1; i++)
            {
                intBaseValue = ReadInt(IntPtr.Add(new IntPtr(intBaseValue), offsets[i]).ToInt32());
            }
            Write(intBaseValue + offsets[offsets.Length - 1], value);
        }

        /// <summary>
        /// Writes an byte array into the memory address.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="value">The value which will be written into the memory address.</param>
        public void Write(int address, byte[] value)
        {
            IntPtr zero = IntPtr.Zero;

            WriteProcessMemory(_ProcessHandle, new IntPtr(address), value, (UInt32)value.Length, ref zero);
        }

        /// <summary>
        /// Writes a NOP into the memory address.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="value">The value which will be written into the memory address.</param>
        public void Write(int address)
        {
            byte[] buffer = new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, };
            IntPtr zero = IntPtr.Zero;

            WriteProcessMemory(_ProcessHandle, new IntPtr(address), buffer, (UInt32)buffer.Length, ref zero);
        }
        #endregion

    }
}
