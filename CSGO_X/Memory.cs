using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace CSGO_X
{
    class Memory
    {
        #region DLL方法引入
        /// <summary>
        /// 从内存中读取字节集
        /// </summary>
        /// <param name="hProcess">进程句柄</param>
        /// <param name="lpBaseAddress">内存基质</param>
        /// <param name="lpBuffer">读取到缓存区的指针</param>
        /// <param name="nSize">缓存区大小</param>
        /// <param name="lpNumberOfBytesRead">读取长度</param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, int nSize, IntPtr lpNumberOfBytesRead);

        /// <summary>
        /// 从内存中写入字节集
        /// </summary>
        /// <param name="hProcess">进程句柄</param>
        /// <param name="lpBaseAddress">内存地址</param>
        /// <param name="lpBuffer">需要写入的数据</param>
        /// <param name="nSize">写入字节大小,比如int32是4个字节</param>
        /// <param name="lpNumberOfBytesWritten">写入长度</param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, int[] lpBuffer, int nSize, IntPtr lpNumberOfBytesWritten);

        //以现有进程获取句柄
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        //关闭句柄
        [DllImport("Kernel32.dll")]
        private static extern void CloseHandle(IntPtr hObject);
        #endregion
        IntPtr Process_Handle;
        int PID;
        static byte[] buffer = new byte[4];
        static float[] Singles = new float[1];
        static IntPtr byteADDR = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
        public Memory(string PName) {
            PID = Get_PID(PName);
            Process_Handle = OpenProcess(0x001F0FFF, false, PID);
        }

        /// <summary>
        /// 得到模块基质
        /// </summary>
        /// <param name="PName">进程名</param>
        /// <param name="DllName">动态链接库名</param>
        /// <returns></returns>
        public IntPtr GetModuleHandle(string DllName)
        {
            try
            {
                var P = Process.GetProcessById(PID);
                for (int i = 0; i < P.Modules.Count; i++)
                {
                    if (P.Modules[i].ModuleName == DllName)
                    {
                        return P.Modules[i].BaseAddress;
                    }
                }
            }
            catch (Exception e)
            {

            }
            return (IntPtr)0;
        }

        //以进程名得到进程ID
        static int Get_PID(string PName)
        {
            try
            {
                var process = Process.GetProcessesByName(PName);
                return process[0].Id;

            }
            catch
            {
                return 0;
            }
        }


        /// <summary>
        /// 读取内存内容(整数型)
        /// </summary>
        /// <param name="BaseADDR">内存地址</param>
        /// <param name="PName">进程名</param>
        /// <returns>内存值</returns>
        public int Read_MemoryInt32(int ADDR)
        {         
            //将基质内存中的值读入缓冲区
            var Rs = ReadProcessMemory(Process_Handle, (IntPtr)ADDR, byteADDR, 4, IntPtr.Zero);
            return Rs ? Marshal.ReadInt32(byteADDR):-1;
        }

        /// <summary>
        /// 读取内存内容(浮点型)
        /// </summary>
        /// <param name="BaseADDR">内存地址</param>
        /// <param name="PName">进程名</param>
        /// <returns>内存值</returns>
        public float Read_MemoryFloat(int ADDR)
        {
            //将基质内存中的值读入缓冲区
            var Rs = ReadProcessMemory(Process_Handle, (IntPtr)ADDR, byteADDR, 4, IntPtr.Zero);
            
            Marshal.Copy(byteADDR, Singles, 0, 1);
            return Rs ?  Singles[0]: -1;
        }

        /// <summary>
        /// 将数值写入内存地址
        /// </summary>
        /// <param name="PName">进程名</param>
        /// <param name="ADDR">内存地址</param>
        /// <param name="Value">待写入数值</param>
        /// <returns></returns>
        public bool Write_MemoryValue(int ADDR, int Value)
        {
            var Rs = WriteProcessMemory(Process_Handle, (IntPtr)ADDR, new int[] { Value }, 4, IntPtr.Zero);
            return Rs;
        }

        /// <summary>
        /// 得到实际操作地址
        /// </summary>
        /// <param name="PName">进程名</param>
        /// <param name="BaseADDR">基质</param>
        /// <param name="Deviations">偏移量</param>
        /// <returns></returns>
        public int Get_RealityADDR(int BaseADDR, string[] Deviations)
        {
            var RealityADDR = Read_MemoryInt32(BaseADDR);
            for (int i = 0; i < Deviations.Length - 1; i++)
            {
                RealityADDR = Read_MemoryInt32(RealityADDR + Convert.ToInt32(Deviations[i], 16));
            }
            RealityADDR = RealityADDR + Convert.ToInt32(Deviations.Last(), 16);
            return RealityADDR;
        }

        public void Close() {
            CloseHandle(Process_Handle);
        }
    }
}
