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
        private static extern IntPtr ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref IntPtr lpNumberOfBytesRead);

        //The function to write into the memory of a process
        [DllImport("kernel32.dll")]
        private static extern IntPtr WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref IntPtr lpNumberOfBytesRead);

        private IntPtr _ProcessHandle;
        private ProcessModule _ProcessModule;
        private IntPtr _BaseAddress;
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
                if (_ProcessList.Count() == 0)
                {
                    //TODO: Throw exception that the process couldn't been found
                }
                else
                {
                    _ProcessHandle = _ProcessList.First().Handle;
                    _ProcessModule = _ProcessList.First().MainModule;
                    _BaseAddress = _ProcessModule.BaseAddress;
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
            byte[] retBuffer = new byte[length - 1];
            IntPtr zero = IntPtr.Zero;

            ReadProcessMemory(_ProcessHandle, new IntPtr(address), retBuffer, retBuffer.Length, ref zero);

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

            WriteProcessMemory(_ProcessHandle, new IntPtr(address), buffer, buffer.Length, ref zero);
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

            WriteProcessMemory(_ProcessHandle, new IntPtr(address), buffer, buffer.Length, ref textLength);
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
        /// Writes an byte array into the memory address.
        /// </summary>
        /// <param name="address">The memory address to write to.</param>
        /// <param name="value">The value which will be written into the memory address.</param>
        public void Write(int address, byte[] value)
        {
            IntPtr zero = IntPtr.Zero;

            WriteProcessMemory(_ProcessHandle, new IntPtr(address), value, value.Length, ref zero);
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

            WriteProcessMemory(_ProcessHandle, new IntPtr(address), buffer, buffer.Length, ref zero);
        }
        #endregion

    }
}
