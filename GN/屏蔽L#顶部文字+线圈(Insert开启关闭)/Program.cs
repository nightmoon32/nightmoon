using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;
using LeagueSharp;

namespace LeaguesharpStreamingMode
{
    class Program
    {
        static Assembly lib = Assembly.Load(LeaguesharpStreamingMode.Properties.Resources.LeaguesharpStreamingModelib);
        static string version = Game.Version[3] == '.' ? Game.Version.Substring(0, 3) : Game.Version.Substring(0, 4);
        static Int32 LeaguesharpCore = GetModuleAddress("Leaguesharp.Core.dll");
        static Dictionary<string, Int32[]> offsets;

        static Int32 GetModuleAddress(String ModuleName)
        {
            Process P = Process.GetCurrentProcess();
            for (int i = 0; i < P.Modules.Count; i++)
                if (P.Modules[i].ModuleName == ModuleName)
                    return (Int32)(P.Modules[i].BaseAddress);
            return 0;
        }

        static byte[] ReadMemory(Int32 address, Int32 length)
        {
            MethodInfo _ReadMemory = lib.GetType("LeaguesharpStreamingModelib.MemoryModule").GetMethods()[2];
            return (byte[])_ReadMemory.Invoke(null, new object[] { address, length });
        }

        static void WriteMemory(Int32 address, byte value)
        {
            MethodInfo _WriteMemory = lib.GetType("LeaguesharpStreamingModelib.MemoryModule").GetMethods()[4];
            _WriteMemory.Invoke(null, new object[] { address, value });
        }

        static void WriteMemory(Int32 address, byte[] array)
        {
            for (int i = 0; i < array.Length; i++)
                WriteMemory(address + i, array[i]);
        }

        static int SignatureScan(int start, int length, int[] pattern)
        {
            var buffer = ReadMemory(start, length);
            for (int i = 0; i < buffer.Length - pattern.Length; i++)
            {
                if ((int)buffer[i] == pattern[0])
                {
                    for (int i2 = 1; i2 < pattern.Length; i2++)
                    {
                        if (pattern[i2] >= 0 && (int)buffer[i + i2] != pattern[i2])
                            break;
                        if (i2 == pattern.Length - 1)
                            return i;
                    }
                }
            }
            return -1;
        }

        enum functionOffset : int
        {
            drawEvent = 0,
            printChat = 1,
            loadingScreenWatermark = 2
        }

        enum asm : byte
        {
            ret = 0xC3,
            push_ebp = 0x55,
            nop = 0x90
        }

        static void SetUpOffsets()
        {
            offsets = new Dictionary<string, Int32[]>();
            int[] pattern1 = { 0x55, 0x8B, 0xEC, 0x6A, 0xFF, 0x68, -1, -1, -1, -1, 0x64, 0xA1, 0, 0, 0, 0, 0x50, 0x83, 0xEC, 0x0C, 0x56, 0xA1, -1, -1, -1, -1, 0x33, 0xC5 };
            int[] pattern2 = { 0x55, 0x8B, 0xEC, 0x8D, 0x45, 0x14, 0x50 };
            int length = 0x50000;
            int result1 = SignatureScan(LeaguesharpCore, length, pattern1);
            int result2 = SignatureScan(LeaguesharpCore, length, pattern2);
            offsets.Add(version, new Int32[] { result1, result2, result2 - 0x7B });

            //offsets.Add("4.19", new Int32[] { 0x5F40, 0x9B60, 0x9B40 });
            //offsets.Add("4.20", new Int32[] { 0x6040, 0x9C00, 0x9BE0 });
            //offsets.Add("4.21", new Int32[] { 0x6420, 0xA320, 0xA1B5 });
            //offsets.Add("5.1", new Int32[] { 0x6440, 0xA290, 0x0 });
            //offsets.Add("5.2", new Int32[] { 0x1A800, 0x21D20, 0x21CA0 });
            //offsets.Add("5.3", new Int32[] { 0x1A900, 0x21E90, 0x21E15 });
        }

        static void Enable()
        {
            WriteMemory(LeaguesharpCore + offsets[version][(int)functionOffset.drawEvent], (byte)asm.ret);
            WriteMemory(LeaguesharpCore + offsets[version][(int)functionOffset.printChat], (byte)asm.ret);
            WriteMemory(LeaguesharpCore + offsets[version][(int)functionOffset.loadingScreenWatermark], new byte[] { (byte)asm.nop, (byte)asm.nop, (byte)asm.nop, 
                                                                                                         (byte)asm.nop, (byte)asm.nop, (byte)asm.nop });
        }

        static void Disable()
        {
            WriteMemory(LeaguesharpCore + offsets[version][(int)functionOffset.drawEvent], (byte)asm.push_ebp);
            WriteMemory(LeaguesharpCore + offsets[version][(int)functionOffset.printChat], (byte)asm.push_ebp);
        }

        static bool IsEnabled() { return ReadMemory(LeaguesharpCore + offsets[version][(int)functionOffset.printChat], 1)[0] == (byte)asm.ret; }

        static uint[] hotkeys = { 0x24, 0x2D };  //home key, insert key
        static void OnWndProc(LeagueSharp.WndEventArgs args)
        {
            if (args.Msg == 0x100) //WM_KEYDOWN
            {
                if (hotkeys.Contains(args.WParam))
                {
                    if (IsEnabled())
                        Disable();
                    else
                        Enable();
                }
            }
        }

        static void Main(string[] args)
        {
            SetUpOffsets();
            Enable();

            LeagueSharp.Game.OnWndProc += OnWndProc;
            AppDomain.CurrentDomain.DomainUnload += delegate
            {
                Disable();
            };
        }

    }
}
