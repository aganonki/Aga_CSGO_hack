using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Aganonki
{        
    class Program
    {

        #region OFFSETS
        public static int offsetMiscEntityList = 0x00;
        public static int offsetMiscLocalPlayer = 0x00;
        public static int offsetMiscJump = 0x00;
        public static int offsetMiscClientState = 0x00;
        public static int offsetMiscSetViewAngles = 0x00;
        public static int offsetMiscGlowManager = 0x00;
        public static int offsetMiscSignOnState = 0xE8;
        public static int offsetMiscWeaponTable = 0x04A5DC4C;
        public static int offsetvMatrix = 0x00;
        private static int offsetEntityCrosshair;
        public static int offsetEntityID = 0x00;
        public static int offsetEntityHealth = 0x00;
        public static int offsetEntityVecOrigin = 0x00;

        public static int offsetPlayerTeamNum = 0x00;
        public static int offsetPlayerBoneMatrix = 0x00;

        public static int offsetPlayerWeaponHandle = 0x12C0;   // m_hActiveWeapon
        public static int offsetWeaponId = 0x1690;   // Search for weaponid
        #endregion

        public static int[] entityAddresses;
        private static Vector3 vecPunch = Vector3.Zero;
        private static KeyUtils keyUtils;
        private static MemUtils memUtils;

        private static void Main(string[] args)
        {
            EntryPoint("Not sure if you wan to load like this");
        }

        public static int EntryPoint(string startez)
        {
            keyUtils = new KeyUtils();
            memUtils = new MemUtils {UseUnsafeReadWrite = false};
            SystemSounds.Beep.Play();
            //jinvoke
            MessageBox.Show("Sucessfully injected to " + Process.GetCurrentProcess().ProcessName + "\n" + startez);
            Thread thread = new Thread(new ThreadStart(Loop));
            thread.IsBackground = true;
            thread.Start();
            thread.Join();
            return 1;
        }
        public static CSGOLocalPlayer localPlayer;
        private static bool bWork = true;
        private static bool TGbot = true;
        private static bool bHop = true;
        private static bool glowEnabled = false;
        //jMustMethod
        private static int TGdelay = 50;
        private static void Loop()
        {
			try{
            ProcUtils proc;                                             //swap1    
            //jinvoke
            ProcessModule clientDll = null;                            //swap1
            ProcessModule engineDll = null;                            //swap1
//            CSGOPlayer[] players = new CSGOPlayer[1024 * 2];
//            CSGOEntity[] entities = new CSGOEntity[1024 * 2];
            entityAddresses = new int[1024 * 2];                      //swap1
            int entityListAddress;                                    //swap1
            int localPlayerAddress;                                   //swap1
            int clientStateAddress;                                   //swap1
            int glowAddress;                                          //swap1
            int glowCount;                                            //swap1
            //jinvoke
            SignOnState signOnState;                                   //swap1
            byte[] data;                                               //swap1

            CSGOPlayer nullPlayer = new CSGOPlayer() { m_iID = 0, m_iHealth = 0, m_iTeam = 0 };                 //swap1
            CSGOEntity nullEntity = new CSGOEntity() { m_iID = 0, m_iDormant = 1, m_iVirtualTable = 0 };        //swap1

            //jinvoke
            GlowObjectDefinition[] glowObjects = new GlowObjectDefinition[128];        //swap1
            CSGOPlayer[] players = new CSGOPlayer[1024 * 2];                           //swap1
            CSGOEntity[] entities = new CSGOEntity[1024 * 2];                          //swap1
            //jinvoke
            while (!ProcUtils.ProcessIsRunning("csgo")) { Thread.Sleep(500);}

            proc = new ProcUtils("csgo", WinAPI.ProcessAccessFlags.VirtualMemoryRead | WinAPI.ProcessAccessFlags.VirtualMemoryWrite | WinAPI.ProcessAccessFlags.VirtualMemoryOperation);
            memUtils.Handle = proc.Handle;
            while (clientDll == null) { clientDll = proc.GetModuleByName(@"bin\client.dll"); }
            while (engineDll == null) { engineDll = proc.GetModuleByName("engine.dll"); }

            int clientDllBase = clientDll.BaseAddress.ToInt32();
            int engineDllBase = engineDll.BaseAddress.ToInt32();

            CSGOScanner.ScanOffsets(memUtils, clientDll, engineDll);		
			var startus=true;
                        var startus1=true;
            while (proc.IsRunning && bWork)
            {
                Thread.Sleep((int) (1000f/120f));

                //Don't do anything if game is not in foreground
                if (WinAPI.GetForegroundWindow() != proc.Process.MainWindowHandle)
                    continue;
                keyUtils.Update();
                if (keyUtils.KeyWentUp(WinAPI.VirtualKeyShort.END)) bWork = false;  //swap2
                    
                //jinvoke
                if (keyUtils.KeyWentUp(WinAPI.VirtualKeyShort.NUMPAD0)) { TGbot = !TGbot; SystemSounds.Beep.Play(); }                //swap2
                if (keyUtils.KeyWentUp(WinAPI.VirtualKeyShort.NUMPAD1)) { bHop = !bHop; SystemSounds.Beep.Play(); }                  //swap2
                if (keyUtils.KeyWentUp(WinAPI.VirtualKeyShort.NUMPAD2)) { glowEnabled = !glowEnabled; SystemSounds.Beep.Play(); }    //swap2
                if (keyUtils.KeyWentUp(WinAPI.VirtualKeyShort.NUMPAD3)) { trollmod = !trollmod; SystemSounds.Beep.Play(); }             //swap2
                if (keyUtils.KeyWentUp(WinAPI.VirtualKeyShort.OEM_MINUS)) { TGdelay = TGdelay > 5 ? TGdelay - 5 : TGdelay; SystemSounds.Beep.Play(); } //swap2
                if (keyUtils.KeyWentUp(WinAPI.VirtualKeyShort.OEM_PLUS)) { TGdelay += 5; SystemSounds.Beep.Play(); } //swap2
                if (startus) {startus=false; MessageBox.Show("Working!1"); }
                //jinvoke
                entityListAddress = clientDll.BaseAddress.ToInt32() + offsetMiscEntityList;                           //swap2
                localPlayerAddress = memUtils.Read<int>((IntPtr)(offsetMiscLocalPlayer + clientDllBase));             //swap2
                //jinvoke
                clientStateAddress = memUtils.Read<int>((IntPtr)(engineDllBase + offsetMiscClientState));             //swap2
                signOnState = (SignOnState)memUtils.Read<int>((IntPtr)(clientStateAddress + offsetMiscSignOnState));  
                //Sanity checks
                if (signOnState != SignOnState.SIGNONSTATE_FULL)
                    continue;
                localPlayer = memUtils.Read<CSGOLocalPlayer>((IntPtr)(localPlayerAddress));
                //jinvoke
                if(!localPlayer.IsValid())
                    continue;
                memUtils.Read((IntPtr)(clientDllBase + offsetMiscEntityList), out data, 16 * entityAddresses.Length);
                //jinvoke
                //Read entities (players)
                for (int i = 0; i < data.Length / 16; i++)
                {
                    int address = BitConverter.ToInt32(data, i * 16);
                    entityAddresses[i] = address;
                    if (address != 0)
                    {
                        entities[i] = memUtils.Read<CSGOEntity>((IntPtr)address);
                        if (entities[i].GetClassID(memUtils) == 34)
                            players[i] = memUtils.Read<CSGOPlayer>((IntPtr)address);
                        else
                            players[i] = nullPlayer;
                        //jinvoke
                    }
                    else
                    {
                        entities[i] = nullEntity;                            //swap3
                        players[i] = nullPlayer;                             //swap3
                    }
                }
                try{
                TriggerBot(players);      //swapmethods
                //jinvoke
                #region Bunnyhop
                Bhop(clientDllBase);    //swapmethods
                #endregion
                //jinvoke
                #region Glow
                Glow(clientDllBase, entities, players);  //swapmethods
                #endregion
				if(startus1){startus1=false; MessageBox.Show("Working!2");}
                            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }    
            }
		}
			catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }  
            
        }

        private static void Glow(int clientDllBase, CSGOEntity[] entities, CSGOPlayer[] players)
        {
            try
            {
                GlowObjectDefinition[] glowObjects;
                byte[] data;
                if (glowEnabled)
                {
                    var glowAddress = memUtils.Read<int>((IntPtr)(clientDllBase + offsetMiscGlowManager));
                    //jinvoke
                    var glowCount = memUtils.Read<int>((IntPtr)(clientDllBase + offsetMiscGlowManager + 0x04));
                    glowObjects = new GlowObjectDefinition[glowCount];
                    int size = Marshal.SizeOf(typeof(GlowObjectDefinition));
                    //jinvoke
                    memUtils.Read((IntPtr)(glowAddress), out data, size * glowCount);
                    for (int i = 0; i < glowObjects.Length; i++)
                    {
                        var clr = Color.Black;
                        glowObjects[i] = data.GetStructure<GlowObjectDefinition>(i * size, size);
                        //jinvoke
                        for (int idx = 0; idx < entities.Length; idx++)
                        {
                            if (glowObjects[i].pEntity != 0 && entityAddresses[idx] == glowObjects[i].pEntity)
                            {
                                if (!entities[idx].IsValid(memUtils))
                                    break;
                                if (Aganonki.Glow.GlowCheck((ClassID)entities[idx].GetClassID(memUtils), players[idx], ref clr))
                                    break;
                                //jinvoke
                                glowObjects[i].a = 1f;                               //swap4
                                glowObjects[i].r = (float)(clr.R / 255f);             //swap4
                                glowObjects[i].g = (float)(clr.G / 255f);             //swap4
                                glowObjects[i].b = (float)(clr.B / 255f);             //swap4
                                //jinvoke
                                glowObjects[i].m_bRenderWhenOccluded = true;         //swap4
                                glowObjects[i].m_bRenderWhenUnoccluded = false;      //swap4
                                glowObjects[i].m_bFullBloom = false;                 //swap4
                                memUtils.Write<GlowObjectDefinition>((IntPtr)(glowAddress + size * i), glowObjects[i], 4, size - 14);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }      
        }

        private static void Bhop(int clientDllBase)
        {
            if (bHop)
            {
                if (keyUtils.KeyIsDown(WinAPI.VirtualKeyShort.SPACE))
                {
                    //jinvoke
                    if ((localPlayer.m_iFlags & 1) == 1) //Stands (FL_ONGROUND)
                    {
                        memUtils.Write<int>((IntPtr) (clientDllBase + offsetMiscJump), 5);
                        //jinvoke
                    }
                    else
                    {
                        memUtils.Write<int>((IntPtr) (clientDllBase + offsetMiscJump), 4);
                    }
                    //jinvoke
                }
            }
        }

        private static void TriggerBot(CSGOPlayer[] players)
        {
            #region Triggerbot

            if (TGbot || keyUtils.KeyIsDown(WinAPI.VirtualKeyShort.KEY_C) || keyUtils.KeyIsDown(WinAPI.VirtualKeyShort.MBUTTON))
            {
                if (localPlayer.m_iCrosshairIdx > 0 && localPlayer.m_iCrosshairIdx <= players.Length)
                {
                    CSGOPlayer target = players[localPlayer.m_iCrosshairIdx - 1];
                    if (!target.IsValid(memUtils)) return;
                    //jinvoke
                    if (target.m_iTeam == localPlayer.m_iTeam) return;
                    CSGOWeapon weapon = localPlayer.GetActiveWeapon(memUtils);
                    //jinvoke
                    if (!weapon.IsValid() || weapon.IsNonAim()) return;
                    Thread.Sleep(TGdelay);
                    //jinvoke
                    KeyUtils.LMouseClick(10);
                    Thread.Sleep(10);
                }
            }

            #endregion
        }

        //jmethod
        public static Random lol = new Random();
        public static bool trollmod = false;

    }

    public static class CSGOScanner
    {
        static ScanResult scan;

        static ProcessModule clientDll;
        static int clientDllBase;
        static ProcessModule engineDll;
        static int engineDllBase;
        public static void ScanOffsets(MemUtils memUtils, ProcessModule client, ProcessModule engine)
        {
            clientDll = client;
            engineDll = engine;
            clientDllBase = clientDll.BaseAddress.ToInt32();
            engineDllBase = engineDll.BaseAddress.ToInt32();
            EntityOff(memUtils);                                //swap5
            LocalPlayer(memUtils);                              //swap5
            Jump(memUtils);
            ClientState(memUtils);                              //swap5
//            SetViewAngles(memUtils);
            SignOnState(memUtils);                              //swap5
            GlowManager(memUtils);
//            WeaponTable(memUtils);
//            EntityID(memUtils);
            EntityHealth(memUtils);                              //swap5
//            EntityVecOrigin(memUtils);
            PlayerTeamNum(memUtils);                             //swap5
//            PlayerBoneMatrix(memUtils);
            PlayerWeaponHandle(memUtils);                        //swap5
//            vMatrix(memUtils);
            clientDll = null;
            engineDll = null;
            clientDllBase = 0;
            engineDllBase = 0;
        }
        #region MISC
        static void vMatrix(MemUtils memUtils)
        {
            scan = memUtils.PerformSignatureScan(new byte[] {                                 
                0x53, 0x8B, 0xDC, 0x83, 0xEC, 0x08, 0x83, 0xE4,
                0xF0, 0x83, 0xC4, 0x04, 0x55, 0x8B, 0x6B, 0x04,
                0x89, 0x6C, 0x24, 0x04, 0x8B, 0xEC, 0xA1, 0x00,
                0x00, 0x00, 0x00, 0x81, 0xEC, 0x98, 0x03, 0x00,
                0x00 }, "xxxxxxxxxxxxxxxxxxxxxxx????xxxxxx", clientDll);
            if (scan.Success)
            {
                int address = memUtils.Read<int>((IntPtr)(scan.Address.ToInt32() + +0x4EE));
                address -= clientDllBase;
                address += 0x80;
                Program.offsetvMatrix = address;
            }
        }
        static void EntityOff(MemUtils memUtils)
        {
            scan = memUtils.PerformSignatureScan(new byte[] { 0x05, 0x00, 0x00, 0x00, 0x00, 0xC1, 0xe9, 0x00, 0x39, 0x48, 0x04 }, "x????xx?xxx", clientDll);
            if (scan.Success)
            {
                int tmp = memUtils.Read<int>((IntPtr)(scan.Address.ToInt32() + 1));
                byte tmp2 = memUtils.Read<byte>((IntPtr)(scan.Address.ToInt32() + 7));
                Program.offsetMiscEntityList = tmp + tmp2 - clientDllBase;
            }
        }
        static void LocalPlayer(MemUtils memUtils)
        {
            scan = memUtils.PerformSignatureScan(new byte[] { 0x8D, 0x34, 0x85, 0x00, 0x00, 0x00, 0x00, 0x89, 0x15, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x41, 0x08, 0x8B, 0x48 }, "xxx????xx????xxxxx", clientDll);
            if (scan.Success)
            {
                int tmp = memUtils.Read<int>((IntPtr)(scan.Address.ToInt32() + 3));
                byte tmp2 = memUtils.Read<byte>((IntPtr)(scan.Address.ToInt32() + 18));
                Program.offsetMiscLocalPlayer = tmp + tmp2 - clientDllBase;
            }
        }
        static void Jump(MemUtils memUtils)
        {
            scan = memUtils.PerformSignatureScan(new byte[] { 0x89, 0x15, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x15, 0x00, 0x00, 0x00, 0x00, 0xF6, 0xC2, 0x03, 0x74, 0x03, 0x83, 0xCE, 0x08, 0xA8, 0x08, 0xBF }, "xx????xx????xxxxxxxxxxx", clientDll);
            if (scan.Success)
            {
                int tmp = memUtils.Read<int>((IntPtr)(scan.Address.ToInt32() + 2));
                Program.offsetMiscJump = tmp - clientDllBase;
            }
        }
        static void ClientState(MemUtils memUtils)
        {
            scan = memUtils.PerformSignatureScan(new byte[] { 0xC2, 0x00, 0x00, 0xCC, 0xCC, 0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x33, 0xC0, 0x83, 0xB9 }, "x??xxxx????xxxx", engineDll);
            if (scan.Success)
            {
                int tmp = memUtils.Read<int>((IntPtr)(scan.Address.ToInt32() + 7));
                Program.offsetMiscClientState = tmp - engineDllBase;
            }
        }
        static void SetViewAngles(MemUtils memUtils)
        {
            scan = memUtils.PerformSignatureScan(new byte[] { 0x8B, 0x15, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x4D, 0x08, 0x8B, 0x82, 0x00, 0x00, 0x00, 0x00, 0x89, 0x01, 0x8B, 0x82, 0x00, 0x00, 0x00, 0x00, 0x89, 0x41, 0x04 }, "xx????xxxxx????xxxx????xxx", engineDll);
            if (scan.Success)
            {
                int tmp = memUtils.Read<int>((IntPtr)(scan.Address.ToInt32() + 11));
                Program.offsetMiscSetViewAngles = tmp;
            }
        }
        static void SignOnState(MemUtils memUtils)
        {
            scan = memUtils.PerformSignatureScan(
                new byte[] { 0x51, 0xA1, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x51, 0x00, 0x83, 0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7C, 0x40, 0x3B, 0xD1 },
                "xx????xx?xx?????xxxx", engineDll);
            if (scan.Success)
            {
                int tmp = memUtils.Read<int>((IntPtr)(scan.Address.ToInt32() + 11));
                Program.offsetMiscSignOnState = tmp;
            }
        }
        static void GlowManager(MemUtils memUtils)
        {
            scan = memUtils.PerformSignatureScan(new byte[] { 0x8D, 0x8F, 0x00, 0x00, 0x00, 0x00, 0xA1, 0x00, 0x00, 0x00, 0x00, 0xC7, 0x04, 0x02, 0x00, 0x00, 0x00, 0x00, 0x89, 0x35, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x51 }, "xx????x????xxx????xx????xx", clientDll);
            if (scan.Success)
            {
                int tmp = memUtils.Read<int>((IntPtr)(scan.Address.ToInt32() + 7));
                Program.offsetMiscGlowManager = tmp - clientDllBase;
            }
        }
        static void WeaponTable(MemUtils memUtils)
        {
            scan = memUtils.PerformSignatureScan(new byte[] { 0xA1, 0x00, 0x00, 0x00, 0x00, 0x0F, 0xB7, 0xC9, 0x03, 0xC9, 0x8B, 0x44, 0x00, 0x0C, 0xC3 }, "x????xxxxxxx?xx", clientDll);
            if (scan.Success)
            {
                int tmp = memUtils.Read<int>((IntPtr)(scan.Address.ToInt32() + 1));
                Program.offsetMiscWeaponTable = tmp - clientDllBase;
            }
        }
        #endregion
        #region ENTITY
        static void EntityID(MemUtils memUtils)
        {
            scan = memUtils.PerformSignatureScan(
                new byte[] { 0x74, 0x72, 0x80, 0x79, 0x00, 0x00, 0x8B, 0x56, 0x00, 0x89, 0x55, 0x00, 0x74, 0x17 },
                "xxxx??xx?xx?xx", clientDll);
            if (scan.Success)
            {
                byte tmp = memUtils.Read<byte>((IntPtr)(scan.Address.ToInt32() + 8));
                Program.offsetEntityID = tmp;
            }
        }
        static void EntityHealth(MemUtils memUtils)
        {
            scan = memUtils.PerformSignatureScan(
                new byte[] { 0x8B, 0x41, 0x00, 0x89, 0x41, 0x00, 0x8B, 0x41, 0x00, 0x89, 0x41, 0x00, 0x8B, 0x41, 0x00, 0x89, 0x41, 0x00, 0x8B, 0x4F, 0x00, 0x83, 0xB9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7F, 0x2E },
                "xx?xx?xx?xx?xx?xx?xx?xx????xxx", clientDll);
            if (scan.Success)
            {
                int tmp = memUtils.Read<int>((IntPtr)(scan.Address.ToInt32() + 23));
                Program.offsetEntityHealth = tmp;
            }
        }
        static void EntityVecOrigin(MemUtils memUtils)
        {
            scan = memUtils.PerformSignatureScan(
                new byte[] { 0x8A, 0x0E, 0x80, 0xE1, 0xFC, 0x0A, 0xC8, 0x88, 0x0E, 0xF3, 0x00, 0x00, 0x87, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x87, 0x00, 0x00, 0x00, 0x00, 0x9F },
                "xxxxxxxxxx??x??????x????x", clientDll);
            if (scan.Success)
            {
                int tmp = memUtils.Read<int>((IntPtr)(scan.Address.ToInt32() + 13));
                Program.offsetEntityVecOrigin = tmp;
            }
        }
        #endregion
        #region PLAYER
        static void PlayerTeamNum(MemUtils memUtils)
        {
            scan = memUtils.PerformSignatureScan(
                new byte[] { 0xCC, 0xCC, 0xCC, 0x8B, 0x89, 0x00, 0x00, 0x00, 0x00, 0xE9, 0x00, 0x00, 0x00, 0x00, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0x8B, 0x81, 0x00, 0x00, 0x00, 0x00, 0xC3, 0xCC, 0xCC },
                "xxxxx????x????xxxxxxx????xxx", clientDll);
            if (scan.Success)
            {
                int tmp = memUtils.Read<int>((IntPtr)(scan.Address.ToInt32() + 5));
                Program.offsetPlayerTeamNum = tmp;
            }
        }
        static void PlayerBoneMatrix(MemUtils memUtils)
        {
            scan = memUtils.PerformSignatureScan(
                new byte[] { 0x83, 0x3C, 0xB0, 0xFF, 0x75, 0x15, 0x8B, 0x87, 0x00, 0x00, 0x00, 0x00, 0x8B, 0xCF, 0x8B, 0x17, 0x03, 0x44, 0x24, 0x0C, 0x50 },
                "xxxxxxxx????xxxxxxxxx", clientDll);
            if (scan.Success)
            {
                int tmp = memUtils.Read<int>((IntPtr)(scan.Address.ToInt32() + 8));
                Program.offsetPlayerBoneMatrix = tmp;
            }
        }
        static void PlayerWeaponHandle(MemUtils memUtils)
        {
            scan = memUtils.PerformSignatureScan(
                new byte[] { 0x0F, 0x45, 0xF7, 0x5F, 0x8B, 0x8E, 0x00, 0x00, 0x00, 0x00, 0x5E, 0x83, 0xF9, 0xFF },
                "xxxxxx????xxxx", clientDll);
            if (scan.Success)
            {
                int tmp = memUtils.Read<int>((IntPtr)(scan.Address.ToInt32() + 6));
                Program.offsetPlayerWeaponHandle = tmp;
            }
        }
        #endregion

    }

    [StructLayout(LayoutKind.Explicit)]
    public struct GlowObjectDefinition
    {
        [FieldOffset(0x00)]
        public int pEntity;
        [FieldOffset(0x04)]
        public float r;
        [FieldOffset(0x08)]
        public float g;
        [FieldOffset(0x0C)]
        public float b;
        [FieldOffset(0x10)]
        public float a;

        //16 bytes junk
        [FieldOffset(0x14)]
        public int junk01;
        [FieldOffset(0x18)]
        public int junk02;
        [FieldOffset(0x1C)]
        public int junk03;
        [FieldOffset(0x20)]
        public int junk04;

        [FieldOffset(0x24)]
        public bool m_bRenderWhenOccluded;
        [FieldOffset(0x25)]
        public bool m_bRenderWhenUnoccluded;
        [FieldOffset(0x26)]
        public bool m_bFullBloom;

        //10 bytes junk
        [FieldOffset(0x2A)]
        public int junk05;
        [FieldOffset(0x2E)]
        public int junk06;
        [FieldOffset(0x32)]
        public short junk07;
    }

    public enum ClassID
    {
        AK47 = 1,
        BaseAnimating = 2,
        BaseDoor = 10,
        BaseEntity = 11,
        BaseTrigger = 20,
        C4 = 28,
        CSGameRulesProxy = 33,
        CSPlayer = 34,
        CSPlayerResource = 35,
        CSRagdoll = 36,
        CSTeam = 37,
        CascadeLight = 29,
        Chicken = 30,
        ColorCorrection = 31,
        DEagle = 38,
        DecoyGrenade = 39,
        DynamicProp = 42,
        EnvDetailController = 50,
        EnvTonemapController = 57,
        EnvWind = 58,
        Flashbang = 63,
        FogController = 64,
        FuncBrush = 69,
        FuncOccluder = 74,
        FuncRotating = 76,
        Func_Dust = 66,
        HEGrenade = 81,
        Hostage = 82,
        IncendiaryGrenade = 84,
        Inferno = 85,
        Knife = 88,
        KnifeGG = 88,
        LightGlow = 90,
        MolotovGrenade = 92,
        ParticleSystem = 97,
        PhysicsProp = 100,
        PhysicsPropMultiplayer = 101,
        PlantedC4 = 103,
        PostProcessController = 109,
        PredictedViewModel = 112,
        PropDoorRotating = 114,
        RopeKeyframe = 120,
        ShadowControl = 123,
        SmokeGrenade = 125,
        SmokeStack = 126,
        Sprite = 129,
        Sun = 134,
        VGuiScreen = 190,
        VoteController = 191,
        WeaponAUG = 194,
        WeaponAWP = 195,
        WeaponBizon = 196,
        WeaponElite = 200,
        WeaponFiveSeven = 202,
        WeaponG3SG1 = 203,
        WeaponGalilAR = 205,
        WeaponGlock = 206,
        WeaponHKP2000 = 207,
        WeaponM249 = 208,
        WeaponM4A1 = 210,
        WeaponMP7 = 214,
        WeaponMP9 = 215,
        WeaponMag7 = 212,
        WeaponNOVA = 217,
        WeaponNegev = 216,
        WeaponP250 = 219,
        WeaponP90 = 220,
        WeaponSCAR20 = 222,
        WeaponSG556 = 226,
        WeaponSSG08 = 227,
        WeaponTaser = 228,
        WeaponTec9 = 229,
        WeaponUMP45 = 231,
        WeaponUMP45x = 232,
        WeaponXM1014 = 233,
        WeaponM4 = 211,
        WeaponNova = 218,
        WeaponMAG = 213,
        ParticleSmokeGrenade = 237,
        Weapon = 0xBEEF,
        Unknown = 0xDEAD,
        ParticleDecoy = 40,
        ParticleFlash = 9,
        ParticleIncendiaryGrenade = 93,
        WeaponG3SG1x = 204,
        WeaponDualBerettas = 201,
        WeaponTec9x = 230,
        WeaponPPBizon = 197,
        WeaponP90x = 221,
        WeaponSCAR20x = 223,
        WeaponXM1014x = 234,
        WeaponM249x = 209,
    }

    public static class Glow
    {
        public static bool GlowCheck(ClassID id, CSGOPlayer entity, ref Color clr)
        {
            switch (id)
            {            
                case ClassID.CSPlayer:
                    {
                        if (entity.m_iTeam == Program.localPlayer.m_iTeam)
                            clr = Color.Blue;
                        else if (entity.m_bSpotted && (entity.m_iTeam == 3 || entity.m_iTeam == 2))
                            clr = Color.Green;
                        else
                            clr = Program.trollmod?Color.FromArgb(Program.lol.Next(1,254),Program.lol.Next(1,254),Program.lol.Next(1,254),Program.lol.Next(1,254)):Color.Red;
                        break;
                    }
                case ClassID.AK47:
                case ClassID.DEagle:
                case ClassID.WeaponAUG:
                case ClassID.WeaponAWP:
                case ClassID.WeaponBizon:
                case ClassID.WeaponElite:
                case ClassID.WeaponFiveSeven:
                case ClassID.WeaponG3SG1:
                case ClassID.WeaponGalilAR:
                case ClassID.WeaponGlock:
                case ClassID.WeaponHKP2000:
                case ClassID.WeaponM249:
                case ClassID.WeaponM249x:
                case ClassID.WeaponM4A1:
                case ClassID.WeaponMP7:
                case ClassID.WeaponMP9:
                case ClassID.WeaponMag7:
                case ClassID.WeaponNOVA:
                case ClassID.WeaponNegev:
                case ClassID.WeaponP250:
                case ClassID.WeaponP90:
                case ClassID.WeaponP90x:
                case ClassID.WeaponSCAR20:
                case ClassID.WeaponSG556:
                case ClassID.WeaponSSG08:
                case ClassID.WeaponTaser:
                case ClassID.WeaponTec9:
                case ClassID.WeaponTec9x:
                case ClassID.WeaponUMP45:
                case ClassID.WeaponXM1014:
                case ClassID.Weapon:
                case ClassID.WeaponNova:
                case ClassID.WeaponM4:
                case ClassID.WeaponUMP45x:
                case ClassID.WeaponXM1014x:
                case ClassID.WeaponMAG:
                case ClassID.WeaponG3SG1x:
                case ClassID.WeaponDualBerettas:
                case ClassID.WeaponPPBizon:
                case ClassID.WeaponSCAR20x:
                    {
                        clr = Color.Violet;
                        break;
                    }
                case ClassID.HEGrenade:
                case ClassID.SmokeGrenade:
                case ClassID.MolotovGrenade:
                case ClassID.IncendiaryGrenade:
                case ClassID.Flashbang:
                case ClassID.DecoyGrenade:
                case ClassID.ParticleDecoy:
                case ClassID.ParticleSmokeGrenade:
                case ClassID.SmokeStack:
                case ClassID.ParticleIncendiaryGrenade:
                case ClassID.ParticleFlash:
                    {
                        clr = Color.DarkRed;
                        break;
                    }
                case ClassID.Hostage:
                case ClassID.Chicken:
                    {
                        clr = Color.HotPink;
                        break;
                    }
                case ClassID.C4:
                case ClassID.PlantedC4:
                    {
                        clr = Color.DarkViolet;
                        break;
                    }
                //case ClassID.DynamicProp:
                //case ClassID.Inferno:
                //case ClassID.BaseAnimating:
                //case ClassID.BaseDoor:
                //case ClassID.BaseEntity:
                //case ClassID.BaseTrigger:
                //case ClassID.CSGameRulesProxy:
                //case ClassID.CSPlayerResource:
                //case ClassID.CSRagdoll:
                //case ClassID.CascadeLight:
                //case ClassID.ColorCorrection:
                //case ClassID.EnvDetailController:
                //case ClassID.EnvTonemapController:
                //case ClassID.EnvWind:
                //case ClassID.FogController:
                //case ClassID.FuncBrush:
                //case ClassID.FuncOccluder:
                //case ClassID.FuncRotating:
                //case ClassID.Func_Dust:
                //case ClassID.LightGlow:
                //case ClassID.ParticleSystem:
                //case ClassID.PhysicsProp:
                //case ClassID.PhysicsPropMultiplayer:
                //case ClassID.PostProcessController:
                //case ClassID.PredictedViewModel:
                //case ClassID.PropDoorRotating:
                //case ClassID.RopeKeyframe:
                //case ClassID.ShadowControl:
                //case ClassID.Sprite:
                //case ClassID.Sun:
                //case ClassID.VGuiScreen:
                //case ClassID.VoteController:
                //case ClassID.Knife:
                default:
                    break;
            }
            return clr == Color.Black;
        }

    }

    [StructLayout(LayoutKind.Explicit)]
    public struct CSGOPlayer
    {
        [FieldOffset(0x8)]
        public int m_iVirtualTable;

        [FieldOffset(0x64)]
        public int m_iID;

        [FieldOffset(0xE9)]
        public byte m_iDormant;

        [FieldOffset(0xF0)]
        public int m_iTeam;

        [FieldOffset(0xFC)]
        public int m_iHealth;

        [FieldOffset(0x134)]
        public Vector3 m_vecOrigin;

        [FieldOffset(0x935)]
        public bool m_bSpotted;

        [FieldOffset(0xA78)]
        public int m_pBoneMatrix;

        [FieldOffset(0x12C0)]
        public uint m_hActiveWeapon;

        public bool IsValid(MemUtils memUtils)
        {
            return this.m_iID != 0 && this.m_iDormant != 1 && this.m_iHealth > 0 && (m_iTeam == 2 || m_iTeam == 3);
        }

        public int GetBoneAddress(int boneIndex)
        {
            return m_pBoneMatrix + boneIndex * 0x30;
        }
        public int GetClientClass(MemUtils memUtils)
        {
            int function = memUtils.Read<int>((IntPtr)(m_iVirtualTable + 2 * 0x04));
            return memUtils.Read<int>((IntPtr)(function + 0x01));
        }
        public int GetClassID(MemUtils memUtils)
        {
            return memUtils.Read<int>((IntPtr)(GetClientClass(memUtils) + 20));
        }
        public String GetName(MemUtils memUtils)
        {
            int ptr = memUtils.Read<int>((IntPtr)(GetClassID(memUtils) + 8));
            return memUtils.ReadString((IntPtr)(ptr + 8), 32, Encoding.ASCII);
        }
        public CSGOWeapon GetActiveWeapon(MemUtils memUtils)
        {
            if (this.m_hActiveWeapon == 0xFFFFFFFF)
                return new CSGOWeapon() { m_iItemDefinitionIndex = 0, m_iWeaponID = 0 };

            uint handle = this.m_hActiveWeapon & 0xFFF;
            int weapAddress = memUtils.Read<int>((IntPtr)(Program.entityAddresses[handle - 1]));
            return memUtils.Read<CSGOWeapon>((IntPtr)weapAddress);
        }
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct CSGOEntity
    {
        [FieldOffset(0x8)]
        public uint m_iVirtualTable;

        [FieldOffset(0x64)]
        public int m_iID;

        [FieldOffset(0xE9)]
        public byte m_iDormant;

        [FieldOffset(0x148)]
        public short m_hOwner;

        public bool IsValid(MemUtils memUtils)
        {
            return this.m_iID != 0 && this.m_iDormant != 1 && this.m_iVirtualTable != 0 && this.m_iVirtualTable != 0xFFFFFFFF;
        }
        public int GetClientClass(MemUtils memUtils)
        {
            uint function = memUtils.Read<uint>((IntPtr)(m_iVirtualTable + 2 * 0x04));
            if (function != 0xFFFFFFFF)
                return memUtils.Read<int>((IntPtr)(function + 0x01));
            else
                return -1;
        }
        public int GetClassID(MemUtils memUtils)
        {
            try
            {
                int clientClass = GetClientClass(memUtils);
                if (clientClass != -1)
                    return memUtils.Read<int>((IntPtr) (clientClass + 20));
                return clientClass;
            }
            catch
            {
                return 0;
            }
        }
        public string GetName(MemUtils memUtils)
        {
            int clientClass = GetClientClass(memUtils);
            if (clientClass != -1)
            {
                int ptr = memUtils.Read<int>((IntPtr)(GetClassID(memUtils) + 8));
                return memUtils.ReadString((IntPtr)(ptr + 8), 32, Encoding.ASCII);
            }
            return "none";
        }
    }

    public enum SignOnState
    {
        SIGNONSTATE_NONE = 0,
        SIGNONSTATE_CHALLENGE = 1,
        SIGNONSTATE_CONNECTED = 2,
        SIGNONSTATE_NEW = 3,
        SIGNONSTATE_PRESPAWN = 4,
        SIGNONSTATE_SPAWN = 5,
        SIGNONSTATE_FULL = 6,
        SIGNONSTATE_CHANGELEVEL = 7
    }
    [StructLayout(LayoutKind.Explicit)]
    struct CSGOLocalPlayer
    {
        [FieldOffset(0x64)]
        public int m_iID;

        [FieldOffset(0xF0)]
        public int m_iTeam;

        [FieldOffset(0xFC)]
        public int m_iHealth;

        [FieldOffset(0x100)]
        public int m_iFlags;

        [FieldOffset(0x104)]
        public Vector3 m_vecViewOffset;

        [FieldOffset(0x110)]
        public Vector3 m_vecVelocity;

        [FieldOffset(0x134)]
        public Vector3 m_vecOrigin;

        [FieldOffset(0x12C0)]
        public uint m_hActiveWeapon;

        [FieldOffset(0x13E8)]
        public Vector3 m_vecPunch;

        [FieldOffset(0x1d6C)]
        public int m_iShotsFired;

        [FieldOffset(0x2410)]
        public int m_iCrosshairIdx;

        public bool IsValid()
        {
            return this.m_iID != 0 && this.m_iHealth > 0 && (m_iTeam == 2 || m_iTeam == 3);
        }
        public CSGOWeapon GetActiveWeapon(MemUtils memUtils)
        {
            try{
            if (this.m_hActiveWeapon == 0xFFFFFFFF)
                return new CSGOWeapon() { m_iItemDefinitionIndex = 0, m_iWeaponID = 0 };

            uint handle = this.m_hActiveWeapon & 0xFFF;
            int weapAddress = Program.entityAddresses[handle - 1];
            return memUtils.Read<CSGOWeapon>((IntPtr)weapAddress);
            }
            catch
            {
                return new CSGOWeapon{m_iWeaponID = 0};
            }
        }
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct CSGOWeapon
    {
        public enum CSGO_Weapon_ID
        {
            weapon_none = 0,
            weapon_deagle,
            weapon_elite,
            weapon_fiveseven,
            weapon_glock,
            weapon_p228,
            weapon_usp,
            weapon_ak47,
            weapon_aug,
            weapon_awp,
            weapon_famas,
            weapon_g3sg1,
            weapon_galil,
            weapon_galilar,
            weapon_m249,
            weapon_m3,
            weapon_m4a1,
            weapon_mac10,
            weapon_mp5navy,
            weapon_p90,
            weapon_scout,
            weapon_sg550,
            weapon_sg552,
            weapon_tmp,
            weapon_ump45,
            weapon_xm1014,
            weapon_bizon,
            weapon_mag7,
            weapon_negev,
            weapon_sawedoff,
            weapon_tec9,
            weapon_taser,
            weapon_hkp2000,
            weapon_mp7,
            weapon_mp9,
            weapon_nova,
            weapon_p250,
            weapon_scar17,
            weapon_scar20,
            weapon_sg556,
            weapon_ssg08,
            weapon_knifegg,
            weapon_knife,
            weapon_flashbang,
            weapon_hegrenade,
            weapon_smokegrenade,
            weapon_molotov,
            weapon_decoy,
            weapon_incgrenade,
            weapon_c4
        };

        [FieldOffset(0x131C)]
        public int m_iItemDefinitionIndex;

        [FieldOffset(0x15B4)]
        public int m_iState;

        [FieldOffset(0x15c0)]
        public int m_iClip1;

        [FieldOffset(0x159C)]
        public float m_flNextPrimaryAttack;

        [FieldOffset(0x15F9)]
        public bool m_bCanReload;

        [FieldOffset(0x162C)]
        public int m_iWeaponTableIndex;

        [FieldOffset(0x1670)]
        public float m_fAccuracyPenalty;

        [FieldOffset(0x1690)]
        public int m_iWeaponID;

        public bool IsValid()
        {
            return this.m_iWeaponID > 0 && this.m_iItemDefinitionIndex > 0;
        }

        public bool IsFullAuto()
        {
            return this.IsAssaultRifle() || this.IsMachinePistol() || this.IsMachineGun();
        }
        public bool IsNonAim()
        {
            return
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_knifegg ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_knife ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_flashbang ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_hegrenade ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_smokegrenade ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_molotov ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_decoy ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_incgrenade ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_c4;
        }
        public bool IsPistol()
        {
            return
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_deagle ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_elite ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_fiveseven ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_glock ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_p228 ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_usp ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_tec9 ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_taser ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_hkp2000;
        }
        public bool IsMachinePistol()
        {
            //jinvoke
            return
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_bizon ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_mac10 ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_mp5navy ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_mp7 ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_mp9 ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_p90 ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_tec9 ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_tmp ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_ump45;
        }
        public bool IsAssaultRifle()
        {
            //jinvoke
            return
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_ak47 ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_aug ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_famas ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_galil ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_galilar ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_m4a1 ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_sg556;
        }
        public bool IsMachineGun()
        {
            return
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_negev ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_m249;
        }
        public bool IsPumpGun()
        {
            return
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_m3 ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_mag7 ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_nova ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_sawedoff ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_xm1014;
        }
        public bool IsSniper()
        {
            return
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_awp ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_scout ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_scar20 ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_ssg08 ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_sg550 ||
                this.m_iWeaponID == (int)CSGO_Weapon_ID.weapon_g3sg1;
        }
    }

    public class KeyUtils
    {
        #region VARIABLES
        private Hashtable keys, prevKeys;
        private short[] allKeys;
        #endregion
        #region STATIC METHODS
        public static bool GetKeyDown(WinAPI.VirtualKeyShort key)
        {
            return GetKeyDown((Int32)key);
        }
        public static void LMouseClick(int sleeptime)
        {
            WinAPI.mouse_event(WinAPI.MOUSEEVENTF.LEFTDOWN, 0, 0, 0, 0);
            Thread.Sleep(sleeptime);
            WinAPI.mouse_event(WinAPI.MOUSEEVENTF.LEFTUP, 0, 0, 0, 0);
        }
        public static bool GetKeyDown(Int32 key)
        {
            return Convert.ToBoolean(WinAPI.GetKeyState(key) & WinAPI.KEY_PRESSED);
        }
        public static bool GetKeyDownAsync(Int32 key)
        {
            return GetKeyDownAsync((WinAPI.VirtualKeyShort)key);
        }
        public static bool GetKeyDownAsync(WinAPI.VirtualKeyShort key)
        {
            return Convert.ToBoolean(WinAPI.GetAsyncKeyState(key) & WinAPI.KEY_PRESSED);
        }
        #endregion
        #region CONSTRUCTOR/DESTRUCTOR
        public KeyUtils()
        {
            keys = new Hashtable();
            prevKeys = new Hashtable();
            WinAPI.VirtualKeyShort[] _keys = (WinAPI.VirtualKeyShort[])Enum.GetValues(typeof(WinAPI.VirtualKeyShort));
            allKeys = new short[_keys.Length];
            for (int i = 0; i < allKeys.Length; i++)
                allKeys[i] = (short)_keys[i];
            //jinvoke
            Init();
        }
        ~KeyUtils()
        {
            keys.Clear();
            prevKeys.Clear();
        }
        #endregion
        #region METHODS
        /// <summary>
        /// Initializes and fills the hashtables
        /// </summary>
        private void Init()
        {
            foreach (Int32 key in allKeys)
            {
                if (!prevKeys.ContainsKey(key))
                {
                    prevKeys.Add(key, false);
                    keys.Add(key, false);
                }
            }
        }
        /// <summary>
        /// Updates the key-states
        /// </summary>
        public void Update()
        {
            prevKeys = (Hashtable)keys.Clone();
            foreach (Int32 key in allKeys)
            {
                keys[key] = GetKeyDown(key);
            }
            //jinvoke
        }
        /// <summary>
        /// Returns an array of all keys that went up since the last Update-call
        /// </summary>
        /// <returns></returns>
        public WinAPI.VirtualKeyShort[] KeysThatWentUp()
        {
            List<WinAPI.VirtualKeyShort> keys = new List<WinAPI.VirtualKeyShort>();
            foreach (WinAPI.VirtualKeyShort key in allKeys)
            {
                if (KeyWentUp(key))
                    keys.Add(key);
            }
            return keys.ToArray();
        }
        /// <summary>
        /// Returns an array of all keys that went down since the last Update-call
        /// </summary>
        /// <returns></returns>
        public WinAPI.VirtualKeyShort[] KeysThatWentDown()
        {
            List<WinAPI.VirtualKeyShort> keys = new List<WinAPI.VirtualKeyShort>();
            foreach (WinAPI.VirtualKeyShort key in allKeys)
            {
                if (KeyWentDown(key))
                    keys.Add(key);
            }
            return keys.ToArray();
        }
        /// <summary>
        /// Returns an array of all keys that went are down since the last Update-call
        /// </summary>
        /// <returns></returns>
        public WinAPI.VirtualKeyShort[] KeysThatAreDown()
        {
            List<WinAPI.VirtualKeyShort> keys = new List<WinAPI.VirtualKeyShort>();
            foreach (WinAPI.VirtualKeyShort key in allKeys)
            {
                if (KeyIsDown(key))
                    keys.Add(key);
            }
            return keys.ToArray();
        }
        /// <summary>
        /// Returns whether the given key went up since the last Update-call
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns></returns>
        public bool KeyWentUp(WinAPI.VirtualKeyShort key)
        {
            return KeyWentUp((Int32)key);
        }
        /// <summary>
        /// Returns whether the given key went up since the last Update-call
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns></returns>
        public bool KeyWentUp(Int32 key)
        {
            if (!KeyExists(key))
                return false;
            return (bool)prevKeys[key] && !(bool)keys[key];
        }
        /// <summary>
        /// Returns whether the given key went down since the last Update-call
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns></returns>
        public bool KeyWentDown(WinAPI.VirtualKeyShort key)
        {
            return KeyWentDown((Int32)key);
        }
        /// <summary>
        /// Returns whether the given key went down since the last Update-call
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns></returns>
        public bool KeyWentDown(Int32 key)
        {
            if (!KeyExists(key))
                return false;
            return !(bool)prevKeys[key] && (bool)keys[key];
        }
        /// <summary>
        /// Returns whether the given key was down at time of the last Update-call
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns></returns>
        public bool KeyIsDown(WinAPI.VirtualKeyShort key)
        {
            return KeyIsDown((Int32)key);
        }
        /// <summary>
        /// Returns whether the given key was down at time of the last Update-call
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns></returns>
        public bool KeyIsDown(Int32 key)
        {
            if (!KeyExists(key))
                return false;
            return (bool)prevKeys[key] || (bool)keys[key];
        }
        /// <summary>
        /// Returns whether the given key is contained in the used hashtables
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns></returns>
        private bool KeyExists(Int32 key)
        {
            return (prevKeys.ContainsKey(key) && keys.ContainsKey(key));
        }
        #endregion
    }
    public class WinAPI
    {
        #region OpenProcess/CloseProcess
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(
             ProcessAccessFlags processAccess,
             bool bInheritHandle,
             int processId
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }
        #endregion
        #region RPM/WPM
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesWritten);
        #endregion
        #region Window-functions
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        public static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ClientToScreen(IntPtr hWnd, out POINT lpPoint);

        [DllImport("gdi32.dll")]
        public static extern bool GetWindowOrgEx(IntPtr hdc, out POINT lpPoint);
        [DllImport("user32.dll")]
        public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);
        [DllImport("dwmapi.dll", PreserveSig = true)]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;        // x position of upper-left corner
            public int Y;         // y position of upper-left corner
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWINFO
        {
            public uint cbSize;
            public RECT rcWindow;
            public RECT rcClient;
            public uint dwStyle;
            public uint dwExStyle;
            public uint dwWindowStatus;
            public uint cxWindowBorders;
            public uint cyWindowBorders;
            public ushort atomWindowType;
            public ushort wCreatorVersion;

            public WINDOWINFO(Boolean? filler)
                : this()   // Allows automatic initialization of "cbSize" with "new WINDOWINFO(null/true/false)".
            {
                cbSize = (UInt32)(Marshal.SizeOf(typeof(WINDOWINFO)));
            }

        }
        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }
        public enum WindowMessage : uint
        {
            WM_ACTIVATE = 0x0006,
            WM_ACTIVATEAPP = 0x001C,
            WM_AFXFIRST = 0x0360,
            WM_AFXLAST = 0x037F,
            WM_APP = 0x8000,
            WM_ASKCBFORMATNAME = 0x030C,
            WM_CANCELJOURNAL = 0x004B,
            WM_CANCELMODE = 0x001F,
            WM_CAPTURECHANGED = 0x0215,
            WM_CHANGECBCHAIN = 0x030D,
            WM_CHANGEUISTATE = 0x0127,
            WM_CHAR = 0x0102,
            WM_CHARTOITEM = 0x002F,
            WM_CHILDACTIVATE = 0x0022,
            WM_CLEAR = 0x0303,
            WM_CLOSE = 0x0010,
            WM_COMMAND = 0x0111,
            WM_COMPACTING = 0x0041,
            WM_COMPAREITEM = 0x0039,
            WM_CONTEXTMENU = 0x007B,
            WM_COPY = 0x0301,
            WM_COPYDATA = 0x004A,
            WM_CREATE = 0x0001,
            WM_CTLCOLORBTN = 0x0135,
            WM_CTLCOLORDLG = 0x0136,
            WM_CTLCOLOREDIT = 0x0133,
            WM_CTLCOLORLISTBOX = 0x0134,
            WM_CTLCOLORMSGBOX = 0x0132,
            WM_CTLCOLORSCROLLBAR = 0x0137,
            WM_CTLCOLORSTATIC = 0x0138,
            WM_CUT = 0x0300,
            WM_DEADCHAR = 0x0103,
            WM_DELETEITEM = 0x002D,
            WM_DESTROY = 0x0002,
            WM_DESTROYCLIPBOARD = 0x0307,
            WM_DEVICECHANGE = 0x0219,
            WM_DEVMODECHANGE = 0x001B,
            WM_DISPLAYCHANGE = 0x007E,
            WM_DRAWCLIPBOARD = 0x0308,
            WM_DRAWITEM = 0x002B,
            WM_DROPFILES = 0x0233,
            WM_ENABLE = 0x000A,
            WM_ENDSESSION = 0x0016,
            WM_ENTERIDLE = 0x0121,
            WM_ENTERMENULOOP = 0x0211,
            WM_ENTERSIZEMOVE = 0x0231,
            WM_ERASEBKGND = 0x0014,
            WM_EXITMENULOOP = 0x0212,
            WM_EXITSIZEMOVE = 0x0232,
            WM_FONTCHANGE = 0x001D,
            WM_GETDLGCODE = 0x0087,
            WM_GETFONT = 0x0031,
            WM_GETHOTKEY = 0x0033,
            WM_GETICON = 0x007F,
            WM_GETMINMAXINFO = 0x0024,
            WM_GETOBJECT = 0x003D,
            WM_GETTEXT = 0x000D,
            WM_GETTEXTLENGTH = 0x000E,
            WM_HANDHELDFIRST = 0x0358,
            WM_HANDHELDLAST = 0x035F,
            WM_HELP = 0x0053,
            WM_HOTKEY = 0x0312,
            WM_HSCROLL = 0x0114,
            WM_HSCROLLCLIPBOARD = 0x030E,
            WM_ICONERASEBKGND = 0x0027,
            WM_IME_CHAR = 0x0286,
            WM_IME_COMPOSITION = 0x010F,
            WM_IME_COMPOSITIONFULL = 0x0284,
            WM_IME_CONTROL = 0x0283,
            WM_IME_ENDCOMPOSITION = 0x010E,
            WM_IME_KEYDOWN = 0x0290,
            WM_IME_KEYLAST = 0x010F,
            WM_IME_KEYUP = 0x0291,
            WM_IME_NOTIFY = 0x0282,
            WM_IME_REQUEST = 0x0288,
            WM_IME_SELECT = 0x0285,
            WM_IME_SETCONTEXT = 0x0281,
            WM_IME_STARTCOMPOSITION = 0x010D,
            WM_INITDIALOG = 0x0110,
            WM_INITMENU = 0x0116,
            WM_INITMENUPOPUP = 0x0117,
            WM_INPUTLANGCHANGE = 0x0051,
            WM_INPUTLANGCHANGEREQUEST = 0x0050,
            WM_KEYDOWN = 0x0100,
            WM_KEYFIRST = 0x0100,
            WM_KEYLAST = 0x0108,
            WM_KEYUP = 0x0101,
            WM_KILLFOCUS = 0x0008,
            WM_LBUTTONDBLCLK = 0x0203,
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MBUTTONDBLCLK = 0x0209,
            WM_MBUTTONDOWN = 0x0207,
            WM_MBUTTONUP = 0x0208,
            WM_MDIACTIVATE = 0x0222,
            WM_MDICASCADE = 0x0227,
            WM_MDICREATE = 0x0220,
            WM_MDIDESTROY = 0x0221,
            WM_MDIGETACTIVE = 0x0229,
            WM_MDIICONARRANGE = 0x0228,
            WM_MDIMAXIMIZE = 0x0225,
            WM_MDINEXT = 0x0224,
            WM_MDIREFRESHMENU = 0x0234,
            WM_MDIRESTORE = 0x0223,
            WM_MDISETMENU = 0x0230,
            WM_MDITILE = 0x0226,
            WM_MEASUREITEM = 0x002C,
            WM_MENUCHAR = 0x0120,
            WM_MENUCOMMAND = 0x0126,
            WM_MENUDRAG = 0x0123,
            WM_MENUGETOBJECT = 0x0124,
            WM_MENURBUTTONUP = 0x0122,
            WM_MENUSELECT = 0x011F,
            WM_MOUSEACTIVATE = 0x0021,
            WM_MOUSEFIRST = 0x0200,
            WM_MOUSEHOVER = 0x02A1,
            WM_MOUSELAST = 0x020D,
            WM_MOUSELEAVE = 0x02A3,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_MOUSEHWHEEL = 0x020E,
            WM_MOVE = 0x0003,
            WM_MOVING = 0x0216,
            WM_NCACTIVATE = 0x0086,
            WM_NCCALCSIZE = 0x0083,
            WM_NCCREATE = 0x0081,
            WM_NCDESTROY = 0x0082,
            WM_NCHITTEST = 0x0084,
            WM_NCLBUTTONDBLCLK = 0x00A3,
            WM_NCLBUTTONDOWN = 0x00A1,
            WM_NCLBUTTONUP = 0x00A2,
            WM_NCMBUTTONDBLCLK = 0x00A9,
            WM_NCMBUTTONDOWN = 0x00A7,
            WM_NCMBUTTONUP = 0x00A8,
            WM_NCMOUSEHOVER = 0x02A0,
            WM_NCMOUSELEAVE = 0x02A2,
            WM_NCMOUSEMOVE = 0x00A0,
            WM_NCPAINT = 0x0085,
            WM_NCRBUTTONDBLCLK = 0x00A6,
            WM_NCRBUTTONDOWN = 0x00A4,
            WM_NCRBUTTONUP = 0x00A5,
            WM_NCXBUTTONDBLCLK = 0x00AD,
            WM_NCXBUTTONDOWN = 0x00AB,
            WM_NCXBUTTONUP = 0x00AC,
            WM_NCUAHDRAWCAPTION = 0x00AE,
            WM_NCUAHDRAWFRAME = 0x00AF,
            WM_NEXTDLGCTL = 0x0028,
            WM_NEXTMENU = 0x0213,
            WM_NOTIFY = 0x004E,
            WM_NOTIFYFORMAT = 0x0055,
            WM_NULL = 0x0000,
            WM_PAINT = 0x000F,
            WM_PAINTCLIPBOARD = 0x0309,
            WM_PAINTICON = 0x0026,
            WM_PALETTECHANGED = 0x0311,
            WM_PALETTEISCHANGING = 0x0310,
            WM_PARENTNOTIFY = 0x0210,
            WM_PASTE = 0x0302,
            WM_PENWINFIRST = 0x0380,
            WM_PENWINLAST = 0x038F,
            WM_POWER = 0x0048,
            WM_POWERBROADCAST = 0x0218,
            WM_PRINT = 0x0317,
            WM_PRINTCLIENT = 0x0318,
            WM_QUERYDRAGICON = 0x0037,
            WM_QUERYENDSESSION = 0x0011,
            WM_QUERYNEWPALETTE = 0x030F,
            WM_QUERYOPEN = 0x0013,
            WM_QUEUESYNC = 0x0023,
            WM_QUIT = 0x0012,
            WM_RBUTTONDBLCLK = 0x0206,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205,
            WM_RENDERALLFORMATS = 0x0306,
            WM_RENDERFORMAT = 0x0305,
            WM_SETCURSOR = 0x0020,
            WM_SETFOCUS = 0x0007,
            WM_SETFONT = 0x0030,
            WM_SETHOTKEY = 0x0032,
            WM_SETICON = 0x0080,
            WM_SETREDRAW = 0x000B,
            WM_SETTEXT = 0x000C,
            WM_SETTINGCHANGE = 0x001A,
            WM_SHOWWINDOW = 0x0018,
            WM_SIZE = 0x0005,
            WM_SIZECLIPBOARD = 0x030B,
            WM_SIZING = 0x0214,
            WM_SPOOLERSTATUS = 0x002A,
            WM_STYLECHANGED = 0x007D,
            WM_STYLECHANGING = 0x007C,
            WM_SYNCPAINT = 0x0088,
            WM_SYSCHAR = 0x0106,
            WM_SYSCOLORCHANGE = 0x0015,
            WM_SYSCOMMAND = 0x0112,
            WM_SYSDEADCHAR = 0x0107,
            WM_SYSKEYDOWN = 0x0104,
            WM_SYSKEYUP = 0x0105,
            WM_TCARD = 0x0052,
            WM_TIMECHANGE = 0x001E,
            WM_TIMER = 0x0113,
            WM_UNDO = 0x0304,
            WM_UNINITMENUPOPUP = 0x0125,
            WM_USER = 0x0400,
            WM_USERCHANGED = 0x0054,
            WM_VKEYTOITEM = 0x002E,
            WM_VSCROLL = 0x0115,
            WM_VSCROLLCLIPBOARD = 0x030A,
            WM_WINDOWPOSCHANGED = 0x0047,
            WM_WINDOWPOSCHANGING = 0x0046,
            WM_WININICHANGE = 0x001A,
            WM_XBUTTONDBLCLK = 0x020D,
            WM_XBUTTONDOWN = 0x020B,
            WM_XBUTTONUP = 0x020C
        }
        public enum SetWindowPosFlags : ushort
        {
            NOSIZE = 0x0001,
            NOMOVE = 0x0002,
            NOZORDER = 0x0004,
            NOREDRAW = 0x0008,
            NOACTIVATE = 0x0010,
            DRAWFRAME = 0x0020,
            FRAMECHANGED = 0x0020,
            SHOWWINDOW = 0x0040,
            HIDEWINDOW = 0x0080,
            NOCOPYBITS = 0x0100,
            NOOWNERZORDER = 0x0200,
            NOREPOSITION = 0x0200,
            NOSENDCHANGING = 0x0400,
            DEFERERASE = 0x2000,
            ASYNCWINDOWPOS = 0x4000
        }
        public enum SetWindpwPosHWNDFlags
        {
            NoTopMost = -2,
            TopMost = -1,
            Top = 0,
            Bottom = 1
        }
        public enum LayeredWindowAttributesFlags
        {
            LWA_ALPHA = 0x2,
            LWA_COLORKEY = 0x1
        }
        public enum GetWindowLongFlags
        {
            GWL_EXSTYLE = -20,
            GWL_HINSTANCE = -6,
            GWL_HWNDPARENT = -8,
            GWL_ID = -12,
            GWL_STYLE = -16,
            GWL_USERDATA = -21,
            GWL_WNDPROC = -4
        }
        public enum ExtendedWindowStyles : int
        {
            WS_EX_ACCEPTFILES = 0x00000010,
            WS_EX_APPWINDOW = 0x00040000,
            WS_EX_CLIENTEDGE = 0x00000200,
            WS_EX_COMPOSITED = 0x02000000,
            WS_EX_CONTEXTHELP = 0x00000400,
            WS_EX_CONTROLPARENT = 0x00010000,
            WS_EX_DLGMODALFRAME = 0x00000001,
            WS_EX_LAYERED = 0x00080000,
            WS_EX_LAYOUTRTL = 0x00400000,
            WS_EX_LEFT = 0x00000000,
            WS_EX_LEFTSCROLLBAR = 0x00004000,
            WS_EX_LTRREADING = 0x00000000,
            WS_EX_MDICHILD = 0x00000040,
            WS_EX_NOACTIVATE = 0x08000000,
            WS_EX_NOINHERITLAYOUT = 0x00100000,
            WS_EX_NOPARENTNOTIFY = 0x00000004,
            WS_EX_NOREDIRECTIONBITMAP = 0x00200000,
            WS_EX_OVERLAPPEDWINDOW = 0x00000300, //WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,
            WS_EX_PALETTEWINDOW = 0x00000188, //WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST,
            WS_EX_RIGHT = 0x00001000,
            WS_EX_RIGHTSCROLLBAR = 0x00000000,
            WS_EX_RTLREADING = 0x00002000,
            WS_EX_STATICEDGE = 0x00020000,
            WS_EX_TOOLWINDOW = 0x00000080,
            WS_EX_TOPMOST = 0x00000008,
            WS_EX_TRANSPARENT = 0x00000020,
            WS_EX_WINDOWEDGE = 0x00000100
        }
        #endregion
        #region Input-functions
        [DllImport("User32.dll")]
        public static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern uint MapVirtualKey(uint uCode, uint uMapType);
        [DllImport("User32.dll")]
        public static extern short GetAsyncKeyState(VirtualKeyShort vKey);
        public const int KEY_PRESSED = 0x8000;

        [DllImport("user32.dll")]
        public static extern int GetKeyState(Int32 vKey);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(MOUSEEVENTF dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        [DllImport("user32.dll")]
        static extern bool keybd_event(byte bVk, byte bScan, uint dwFlags,
           int dwExtraInfo);

        [DllImport("user32.dll")]
        static extern bool keybd_event(byte bVk, byte bScan, uint dwFlags,
           UIntPtr dwExtraInfo);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        static extern UInt32 SendInput(UInt32 nInputs, [MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] INPUT[] pInputs, Int32 cbSize);

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public uint type;
            public InputUnion U;
            public static int Size
            {
                get { return Marshal.SizeOf(typeof(INPUT)); }
            }
        }
        #region [Structs+Enums]
        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public MOUSEEVENTF dwFlags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public VirtualKeyShort wVk;
            public ScanCodeShort wScan;
            public KEYEVENTF dwFlags;
            public int time;
            public UIntPtr dwExtraInfo;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }
        [Flags]
        public enum KEYEVENTF : uint
        {
            EXTENDEDKEY = 0x0001,
            KEYUP = 0x0002,
            SCANCODE = 0x0008,
            UNICODE = 0x0004
        }
        [Flags]
        public enum MOUSEEVENTF : uint
        {
            ABSOLUTE = 0x8000,
            HWHEEL = 0x01000,
            MOVE = 0x0001,
            MOVE_NOCOALESCE = 0x2000,
            LEFTDOWN = 0x0002,
            LEFTUP = 0x0004,
            RIGHTDOWN = 0x0008,
            RIGHTUP = 0x0010,
            MIDDLEDOWN = 0x0020,
            MIDDLEUP = 0x0040,
            VIRTUALDESK = 0x4000,
            WHEEL = 0x0800,
            XDOWN = 0x0080,
            XUP = 0x0100
        }
        public enum VirtualKeyShort : short
        {
            ///<summary>
            ///Left mouse button
            ///</summary>
            LBUTTON = 0x01,
            ///<summary>
            ///Right mouse button
            ///</summary>
            RBUTTON = 0x02,
            ///<summary>
            ///Control-break processing
            ///</summary>
            CANCEL = 0x03,
            ///<summary>
            ///Middle mouse button (three-button mouse)
            ///</summary>
            MBUTTON = 0x04,
            ///<summary>
            ///Windows 2000/XP: X1 mouse button
            ///</summary>
            XBUTTON1 = 0x05,
            ///<summary>
            ///Windows 2000/XP: X2 mouse button
            ///</summary>
            XBUTTON2 = 0x06,
            ///<summary>
            ///BACKSPACE key
            ///</summary>
            BACK = 0x08,
            ///<summary>
            ///TAB key
            ///</summary>
            TAB = 0x09,
            ///<summary>
            ///CLEAR key
            ///</summary>
            CLEAR = 0x0C,
            ///<summary>
            ///ENTER key
            ///</summary>
            RETURN = 0x0D,
            ///<summary>
            ///SHIFT key
            ///</summary>
            SHIFT = 0x10,
            ///<summary>
            ///CTRL key
            ///</summary>
            CONTROL = 0x11,
            ///<summary>
            ///ALT key
            ///</summary>
            MENU = 0x12,
            ///<summary>
            ///PAUSE key
            ///</summary>
            PAUSE = 0x13,
            ///<summary>
            ///CAPS LOCK key
            ///</summary>
            CAPITAL = 0x14,
            ///<summary>
            ///Input Method Editor (IME) Kana mode
            ///</summary>
            KANA = 0x15,
            ///<summary>
            ///IME Hangul mode
            ///</summary>
            HANGUL = 0x15,
            ///<summary>
            ///IME Junja mode
            ///</summary>
            JUNJA = 0x17,
            ///<summary>
            ///IME final mode
            ///</summary>
            FINAL = 0x18,
            ///<summary>
            ///IME Hanja mode
            ///</summary>
            HANJA = 0x19,
            ///<summary>
            ///IME Kanji mode
            ///</summary>
            KANJI = 0x19,
            ///<summary>
            ///ESC key
            ///</summary>
            ESCAPE = 0x1B,
            ///<summary>
            ///IME convert
            ///</summary>
            CONVERT = 0x1C,
            ///<summary>
            ///IME nonconvert
            ///</summary>
            NONCONVERT = 0x1D,
            ///<summary>
            ///IME accept
            ///</summary>
            ACCEPT = 0x1E,
            ///<summary>
            ///IME mode change request
            ///</summary>
            MODECHANGE = 0x1F,
            ///<summary>
            ///SPACEBAR
            ///</summary>
            SPACE = 0x20,
            ///<summary>
            ///PAGE UP key
            ///</summary>
            PRIOR = 0x21,
            ///<summary>
            ///PAGE DOWN key
            ///</summary>
            NEXT = 0x22,
            ///<summary>
            ///END key
            ///</summary>
            END = 0x23,
            ///<summary>
            ///HOME key
            ///</summary>
            HOME = 0x24,
            ///<summary>
            ///LEFT ARROW key
            ///</summary>
            LEFT = 0x25,
            ///<summary>
            ///UP ARROW key
            ///</summary>
            UP = 0x26,
            ///<summary>
            ///RIGHT ARROW key
            ///</summary>
            RIGHT = 0x27,
            ///<summary>
            ///DOWN ARROW key
            ///</summary>
            DOWN = 0x28,
            ///<summary>
            ///SELECT key
            ///</summary>
            SELECT = 0x29,
            ///<summary>
            ///PRINT key
            ///</summary>
            PRINT = 0x2A,
            ///<summary>
            ///EXECUTE key
            ///</summary>
            EXECUTE = 0x2B,
            ///<summary>
            ///PRINT SCREEN key
            ///</summary>
            SNAPSHOT = 0x2C,
            ///<summary>
            ///INS key
            ///</summary>
            INSERT = 0x2D,
            ///<summary>
            ///DEL key
            ///</summary>
            DELETE = 0x2E,
            ///<summary>
            ///HELP key
            ///</summary>
            HELP = 0x2F,
            ///<summary>
            ///0 key
            ///</summary>
            KEY_0 = 0x30,
            ///<summary>
            ///1 key
            ///</summary>
            KEY_1 = 0x31,
            ///<summary>
            ///2 key
            ///</summary>
            KEY_2 = 0x32,
            ///<summary>
            ///3 key
            ///</summary>
            KEY_3 = 0x33,
            ///<summary>
            ///4 key
            ///</summary>
            KEY_4 = 0x34,
            ///<summary>
            ///5 key
            ///</summary>
            KEY_5 = 0x35,
            ///<summary>
            ///6 key
            ///</summary>
            KEY_6 = 0x36,
            ///<summary>
            ///7 key
            ///</summary>
            KEY_7 = 0x37,
            ///<summary>
            ///8 key
            ///</summary>
            KEY_8 = 0x38,
            ///<summary>
            ///9 key
            ///</summary>
            KEY_9 = 0x39,
            ///<summary>
            ///A key
            ///</summary>
            KEY_A = 0x41,
            ///<summary>
            ///B key
            ///</summary>
            KEY_B = 0x42,
            ///<summary>
            ///C key
            ///</summary>
            KEY_C = 0x43,
            ///<summary>
            ///D key
            ///</summary>
            KEY_D = 0x44,
            ///<summary>
            ///E key
            ///</summary>
            KEY_E = 0x45,
            ///<summary>
            ///F key
            ///</summary>
            KEY_F = 0x46,
            ///<summary>
            ///G key
            ///</summary>
            KEY_G = 0x47,
            ///<summary>
            ///H key
            ///</summary>
            KEY_H = 0x48,
            ///<summary>
            ///I key
            ///</summary>
            KEY_I = 0x49,
            ///<summary>
            ///J key
            ///</summary>
            KEY_J = 0x4A,
            ///<summary>
            ///K key
            ///</summary>
            KEY_K = 0x4B,
            ///<summary>
            ///L key
            ///</summary>
            KEY_L = 0x4C,
            ///<summary>
            ///M key
            ///</summary>
            KEY_M = 0x4D,
            ///<summary>
            ///N key
            ///</summary>
            KEY_N = 0x4E,
            ///<summary>
            ///O key
            ///</summary>
            KEY_O = 0x4F,
            ///<summary>
            ///P key
            ///</summary>
            KEY_P = 0x50,
            ///<summary>
            ///Q key
            ///</summary>
            KEY_Q = 0x51,
            ///<summary>
            ///R key
            ///</summary>
            KEY_R = 0x52,
            ///<summary>
            ///S key
            ///</summary>
            KEY_S = 0x53,
            ///<summary>
            ///T key
            ///</summary>
            KEY_T = 0x54,
            ///<summary>
            ///U key
            ///</summary>
            KEY_U = 0x55,
            ///<summary>
            ///V key
            ///</summary>
            KEY_V = 0x56,
            ///<summary>
            ///W key
            ///</summary>
            KEY_W = 0x57,
            ///<summary>
            ///X key
            ///</summary>
            KEY_X = 0x58,
            ///<summary>
            ///Y key
            ///</summary>
            KEY_Y = 0x59,
            ///<summary>
            ///Z key
            ///</summary>
            KEY_Z = 0x5A,
            ///<summary>
            ///Left Windows key (Microsoft Natural keyboard) 
            ///</summary>
            LWIN = 0x5B,
            ///<summary>
            ///Right Windows key (Natural keyboard)
            ///</summary>
            RWIN = 0x5C,
            ///<summary>
            ///Applications key (Natural keyboard)
            ///</summary>
            APPS = 0x5D,
            ///<summary>
            ///Computer Sleep key
            ///</summary>
            SLEEP = 0x5F,
            ///<summary>
            ///Numeric keypad 0 key
            ///</summary>
            NUMPAD0 = 0x60,
            ///<summary>
            ///Numeric keypad 1 key
            ///</summary>
            NUMPAD1 = 0x61,
            ///<summary>
            ///Numeric keypad 2 key
            ///</summary>
            NUMPAD2 = 0x62,
            ///<summary>
            ///Numeric keypad 3 key
            ///</summary>
            NUMPAD3 = 0x63,
            ///<summary>
            ///Numeric keypad 4 key
            ///</summary>
            NUMPAD4 = 0x64,
            ///<summary>
            ///Numeric keypad 5 key
            ///</summary>
            NUMPAD5 = 0x65,
            ///<summary>
            ///Numeric keypad 6 key
            ///</summary>
            NUMPAD6 = 0x66,
            ///<summary>
            ///Numeric keypad 7 key
            ///</summary>
            NUMPAD7 = 0x67,
            ///<summary>
            ///Numeric keypad 8 key
            ///</summary>
            NUMPAD8 = 0x68,
            ///<summary>
            ///Numeric keypad 9 key
            ///</summary>
            NUMPAD9 = 0x69,
            ///<summary>
            ///Multiply key
            ///</summary>
            MULTIPLY = 0x6A,
            ///<summary>
            ///Add key
            ///</summary>
            ADD = 0x6B,
            ///<summary>
            ///Separator key
            ///</summary>
            SEPARATOR = 0x6C,
            ///<summary>
            ///Subtract key
            ///</summary>
            SUBTRACT = 0x6D,
            ///<summary>
            ///Decimal key
            ///</summary>
            DECIMAL = 0x6E,
            ///<summary>
            ///Divide key
            ///</summary>
            DIVIDE = 0x6F,
            ///<summary>
            ///F1 key
            ///</summary>
            F1 = 0x70,
            ///<summary>
            ///F2 key
            ///</summary>
            F2 = 0x71,
            ///<summary>
            ///F3 key
            ///</summary>
            F3 = 0x72,
            ///<summary>
            ///F4 key
            ///</summary>
            F4 = 0x73,
            ///<summary>
            ///F5 key
            ///</summary>
            F5 = 0x74,
            ///<summary>
            ///F6 key
            ///</summary>
            F6 = 0x75,
            ///<summary>
            ///F7 key
            ///</summary>
            F7 = 0x76,
            ///<summary>
            ///F8 key
            ///</summary>
            F8 = 0x77,
            ///<summary>
            ///F9 key
            ///</summary>
            F9 = 0x78,
            ///<summary>
            ///F10 key
            ///</summary>
            F10 = 0x79,
            ///<summary>
            ///F11 key
            ///</summary>
            F11 = 0x7A,
            ///<summary>
            ///F12 key
            ///</summary>
            F12 = 0x7B,
            ///<summary>
            ///F13 key
            ///</summary>
            F13 = 0x7C,
            ///<summary>
            ///F14 key
            ///</summary>
            F14 = 0x7D,
            ///<summary>
            ///F15 key
            ///</summary>
            F15 = 0x7E,
            ///<summary>
            ///F16 key
            ///</summary>
            F16 = 0x7F,
            ///<summary>
            ///F17 key  
            ///</summary>
            F17 = 0x80,
            ///<summary>
            ///F18 key  
            ///</summary>
            F18 = 0x81,
            ///<summary>
            ///F19 key  
            ///</summary>
            F19 = 0x82,
            ///<summary>
            ///F20 key  
            ///</summary>
            F20 = 0x83,
            ///<summary>
            ///F21 key  
            ///</summary>
            F21 = 0x84,
            ///<summary>
            ///F22 key, (PPC only) Key used to lock device.
            ///</summary>
            F22 = 0x85,
            ///<summary>
            ///F23 key  
            ///</summary>
            F23 = 0x86,
            ///<summary>
            ///F24 key  
            ///</summary>
            F24 = 0x87,
            ///<summary>
            ///NUM LOCK key
            ///</summary>
            NUMLOCK = 0x90,
            ///<summary>
            ///SCROLL LOCK key
            ///</summary>
            SCROLL = 0x91,
            ///<summary>
            ///Left SHIFT key
            ///</summary>
            LSHIFT = 0xA0,
            ///<summary>
            ///Right SHIFT key
            ///</summary>
            RSHIFT = 0xA1,
            ///<summary>
            ///Left CONTROL key
            ///</summary>
            LCONTROL = 0xA2,
            ///<summary>
            ///Right CONTROL key
            ///</summary>
            RCONTROL = 0xA3,
            ///<summary>
            ///Left MENU key
            ///</summary>
            LMENU = 0xA4,
            ///<summary>
            ///Right MENU key
            ///</summary>
            RMENU = 0xA5,
            ///<summary>
            ///Windows 2000/XP: Browser Back key
            ///</summary>
            BROWSER_BACK = 0xA6,
            ///<summary>
            ///Windows 2000/XP: Browser Forward key
            ///</summary>
            BROWSER_FORWARD = 0xA7,
            ///<summary>
            ///Windows 2000/XP: Browser Refresh key
            ///</summary>
            BROWSER_REFRESH = 0xA8,
            ///<summary>
            ///Windows 2000/XP: Browser Stop key
            ///</summary>
            BROWSER_STOP = 0xA9,
            ///<summary>
            ///Windows 2000/XP: Browser Search key 
            ///</summary>
            BROWSER_SEARCH = 0xAA,
            ///<summary>
            ///Windows 2000/XP: Browser Favorites key
            ///</summary>
            BROWSER_FAVORITES = 0xAB,
            ///<summary>
            ///Windows 2000/XP: Browser Start and Home key
            ///</summary>
            BROWSER_HOME = 0xAC,
            ///<summary>
            ///Windows 2000/XP: Volume Mute key
            ///</summary>
            VOLUME_MUTE = 0xAD,
            ///<summary>
            ///Windows 2000/XP: Volume Down key
            ///</summary>
            VOLUME_DOWN = 0xAE,
            ///<summary>
            ///Windows 2000/XP: Volume Up key
            ///</summary>
            VOLUME_UP = 0xAF,
            ///<summary>
            ///Windows 2000/XP: Next Track key
            ///</summary>
            MEDIA_NEXT_TRACK = 0xB0,
            ///<summary>
            ///Windows 2000/XP: Previous Track key
            ///</summary>
            MEDIA_PREV_TRACK = 0xB1,
            ///<summary>
            ///Windows 2000/XP: Stop Media key
            ///</summary>
            MEDIA_STOP = 0xB2,
            ///<summary>
            ///Windows 2000/XP: Play/Pause Media key
            ///</summary>
            MEDIA_PLAY_PAUSE = 0xB3,
            ///<summary>
            ///Windows 2000/XP: Start Mail key
            ///</summary>
            LAUNCH_MAIL = 0xB4,
            ///<summary>
            ///Windows 2000/XP: Select Media key
            ///</summary>
            LAUNCH_MEDIA_SELECT = 0xB5,
            ///<summary>
            ///Windows 2000/XP: Start Application 1 key
            ///</summary>
            LAUNCH_APP1 = 0xB6,
            ///<summary>
            ///Windows 2000/XP: Start Application 2 key
            ///</summary>
            LAUNCH_APP2 = 0xB7,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard.
            ///</summary>
            OEM_1 = 0xBA,
            ///<summary>
            ///Windows 2000/XP: For any country/region, the '+' key
            ///</summary>
            OEM_PLUS = 0xBB,
            ///<summary>
            ///Windows 2000/XP: For any country/region, the ',' key
            ///</summary>
            OEM_COMMA = 0xBC,
            ///<summary>
            ///Windows 2000/XP: For any country/region, the '-' key
            ///</summary>
            OEM_MINUS = 0xBD,
            ///<summary>
            ///Windows 2000/XP: For any country/region, the '.' key
            ///</summary>
            OEM_PERIOD = 0xBE,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard.
            ///</summary>
            OEM_2 = 0xBF,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard. 
            ///</summary>
            OEM_3 = 0xC0,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard. 
            ///</summary>
            OEM_4 = 0xDB,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard. 
            ///</summary>
            OEM_5 = 0xDC,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard. 
            ///</summary>
            OEM_6 = 0xDD,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard. 
            ///</summary>
            OEM_7 = 0xDE,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard.
            ///</summary>
            OEM_8 = 0xDF,
            ///<summary>
            ///Windows 2000/XP: Either the angle bracket key or the backslash key on the RT 102-key keyboard
            ///</summary>
            OEM_102 = 0xE2,
            ///<summary>
            ///Windows 95/98/Me, Windows NT 4.0, Windows 2000/XP: IME PROCESS key
            ///</summary>
            PROCESSKEY = 0xE5,
            ///<summary>
            ///Windows 2000/XP: Used to pass Unicode characters as if they were keystrokes.
            ///The VK_PACKET key is the low word of a 32-bit Virtual Key value used for non-keyboard input methods. For more information,
            ///see Remark in KEYBDINPUT, SendInput, WM_KEYDOWN, and WM_KEYUP
            ///</summary>
            PACKET = 0xE7,
            ///<summary>
            ///Attn key
            ///</summary>
            ATTN = 0xF6,
            ///<summary>
            ///CrSel key
            ///</summary>
            CRSEL = 0xF7,
            ///<summary>
            ///ExSel key
            ///</summary>
            EXSEL = 0xF8,
            ///<summary>
            ///Erase EOF key
            ///</summary>
            EREOF = 0xF9,
            ///<summary>
            ///Play key
            ///</summary>
            PLAY = 0xFA,
            ///<summary>
            ///Zoom key
            ///</summary>
            ZOOM = 0xFB,
            ///<summary>
            ///Reserved 
            ///</summary>
            NONAME = 0xFC,
            ///<summary>
            ///PA1 key
            ///</summary>
            PA1 = 0xFD,
            ///<summary>
            ///Clear key
            ///</summary>
            OEM_CLEAR = 0xFE
        }
        public enum ScanCodeShort : short
        {
            LBUTTON = 0,
            RBUTTON = 0,
            CANCEL = 70,
            MBUTTON = 0,
            XBUTTON1 = 0,
            XBUTTON2 = 0,
            BACK = 14,
            TAB = 15,
            CLEAR = 76,
            RETURN = 28,
            SHIFT = 42,
            CONTROL = 29,
            MENU = 56,
            PAUSE = 0,
            CAPITAL = 58,
            KANA = 0,
            HANGUL = 0,
            JUNJA = 0,
            FINAL = 0,
            HANJA = 0,
            KANJI = 0,
            ESCAPE = 1,
            CONVERT = 0,
            NONCONVERT = 0,
            ACCEPT = 0,
            MODECHANGE = 0,
            SPACE = 57,
            PRIOR = 73,
            NEXT = 81,
            END = 79,
            HOME = 71,
            LEFT = 75,
            UP = 72,
            RIGHT = 77,
            DOWN = 80,
            SELECT = 0,
            PRINT = 0,
            EXECUTE = 0,
            SNAPSHOT = 84,
            INSERT = 82,
            DELETE = 83,
            HELP = 99,
            KEY_0 = 11,
            KEY_1 = 2,
            KEY_2 = 3,
            KEY_3 = 4,
            KEY_4 = 5,
            KEY_5 = 6,
            KEY_6 = 7,
            KEY_7 = 8,
            KEY_8 = 9,
            KEY_9 = 10,
            KEY_A = 30,
            KEY_B = 48,
            KEY_C = 46,
            KEY_D = 32,
            KEY_E = 18,
            KEY_F = 33,
            KEY_G = 34,
            KEY_H = 35,
            KEY_I = 23,
            KEY_J = 36,
            KEY_K = 37,
            KEY_L = 38,
            KEY_M = 50,
            KEY_N = 49,
            KEY_O = 24,
            KEY_P = 25,
            KEY_Q = 16,
            KEY_R = 19,
            KEY_S = 31,
            KEY_T = 20,
            KEY_U = 22,
            KEY_V = 47,
            KEY_W = 17,
            KEY_X = 45,
            KEY_Y = 21,
            KEY_Z = 44,
            LWIN = 91,
            RWIN = 92,
            APPS = 93,
            SLEEP = 95,
            NUMPAD0 = 82,
            NUMPAD1 = 79,
            NUMPAD2 = 80,
            NUMPAD3 = 81,
            NUMPAD4 = 75,
            NUMPAD5 = 76,
            NUMPAD6 = 77,
            NUMPAD7 = 71,
            NUMPAD8 = 72,
            NUMPAD9 = 73,
            MULTIPLY = 55,
            ADD = 78,
            SEPARATOR = 0,
            SUBTRACT = 74,
            DECIMAL = 83,
            DIVIDE = 53,
            F1 = 59,
            F2 = 60,
            F3 = 61,
            F4 = 62,
            F5 = 63,
            F6 = 64,
            F7 = 65,
            F8 = 66,
            F9 = 67,
            F10 = 68,
            F11 = 87,
            F12 = 88,
            F13 = 100,
            F14 = 101,
            F15 = 102,
            F16 = 103,
            F17 = 104,
            F18 = 105,
            F19 = 106,
            F20 = 107,
            F21 = 108,
            F22 = 109,
            F23 = 110,
            F24 = 118,
            NUMLOCK = 69,
            SCROLL = 70,
            LSHIFT = 42,
            RSHIFT = 54,
            LCONTROL = 29,
            RCONTROL = 29,
            LMENU = 56,
            RMENU = 56,
            BROWSER_BACK = 106,
            BROWSER_FORWARD = 105,
            BROWSER_REFRESH = 103,
            BROWSER_STOP = 104,
            BROWSER_SEARCH = 101,
            BROWSER_FAVORITES = 102,
            BROWSER_HOME = 50,
            VOLUME_MUTE = 32,
            VOLUME_DOWN = 46,
            VOLUME_UP = 48,
            MEDIA_NEXT_TRACK = 25,
            MEDIA_PREV_TRACK = 16,
            MEDIA_STOP = 36,
            MEDIA_PLAY_PAUSE = 34,
            LAUNCH_MAIL = 108,
            LAUNCH_MEDIA_SELECT = 109,
            LAUNCH_APP1 = 107,
            LAUNCH_APP2 = 33,
            OEM_1 = 39,
            OEM_PLUS = 13,
            OEM_COMMA = 51,
            OEM_MINUS = 12,
            OEM_PERIOD = 52,
            OEM_2 = 53,
            OEM_3 = 41,
            OEM_4 = 26,
            OEM_5 = 43,
            OEM_6 = 27,
            OEM_7 = 40,
            OEM_8 = 0,
            OEM_102 = 86,
            PROCESSKEY = 0,
            PACKET = 0,
            ATTN = 0,
            CRSEL = 0,
            EXSEL = 0,
            EREOF = 93,
            PLAY = 0,
            ZOOM = 98,
            NONAME = 0,
            PA1 = 0,
            OEM_CLEAR = 0,
        }
        #endregion
        #endregion
        #region Console
        [DllImport("kernel32")]
        public static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool FreeConsole();
        #endregion

        public static int MakeLParam(int LoWord, int HiWord)
        {
            return ((HiWord << 16) | (LoWord & 0xffff));
        }
    }
    /// <summary>
    /// A class that simplifies read- and write-operations to processes
    /// Includes signature-scanning
    /// </summary>
    public class MemUtils
    {
        #region CONSTANTS
        private const int SIZE_FLOAT = sizeof(float);
        private const int MAX_DUMP_SIZE = 0xFFFF;
        #endregion
        #region PROPERTIES
        /// <summary>
        /// The handle to the process this class reads memory from/writes memory to
        /// </summary>
        public IntPtr Handle { get; set; }
        /// <summary>
        /// Determines whether data will be read/written using unsafe code or not.
        /// Implementation of unsafe code comes from:
        /// https://github.com/Aevitas/bluerain/blob/master/src/BlueRain/ExternalProcessMemory.cs
        /// </summary>
        public bool UseUnsafeReadWrite { get; set; }
        public long BytesRead { get; private set; }
        public long BytesWritten { get; private set; }
        #endregion
        #region METHODS
        #region PRIMITIVE WRAPPERS
        /// <summary>
        /// Reads a chunk of memory
        /// </summary>
        /// <param name="address">The address of the chunk of memory</param>
        /// <param name="data">The byte-array to write the read data to</param>
        /// <param name="length">The number (in bytes) of bytes to read</param>
        public void Read(IntPtr address, out byte[] data, int length)
        {
            IntPtr numBytes = IntPtr.Zero;
            data = new byte[length];
            bool result = WinAPI.ReadProcessMemory(Handle, address, data, length, out numBytes);
            //jinvoke
            BytesRead += numBytes.ToInt32();
            
            if (!result)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }
        /// <summary>
        /// Writes a chunk of memory
        /// </summary>
        /// <param name="address">The address to write to</param>
        /// <param name="data">A byte-array of data to write</param>
        public void Write(IntPtr address, byte[] data)
        {
            IntPtr numBytes = IntPtr.Zero;
            bool result = WinAPI.WriteProcessMemory(Handle, address, data, data.Length, out numBytes);
            BytesWritten += numBytes.ToInt32();
            //jinvoke
            if (!result)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }
        /// <summary>
        /// Writes a chunk of memory using the given offset and length of data
        /// It will apply the offset to the address as well as to the data, length defines the number of bytes to write (beginning at offset)
        /// </summary>
        /// <param name="address">The address to write to</param>
        /// <param name="data">A byte-array of data to write</param>
        /// <param name="offset">Skips the given number of bytes (applies to address and data)</param>
        /// <param name="length">Number of bytes to write (beginning at offset)</param>
        public void Write(IntPtr address, byte[] data, int offset, int length)
        {
            byte[] writeData = new byte[length];
            Array.Copy(data, offset, writeData, 0, writeData.Length);
            Write((IntPtr)(address.ToInt32() + offset), writeData);
        }
        #endregion
        #region SPECIALIZED FUNCTIONS
        #region READ
        /// <summary>
        /// Reads a string from memory using the given encoding
        /// </summary>
        /// <param name="address">The address of the string to read</param>
        /// <param name="length">The length of the string</param>
        /// <param name="encoding">The encoding of the string</param>
        /// <returns>The string read from memory</returns>
        public String ReadString(IntPtr address, int length, Encoding encoding)
        {
            byte[] data;
            Read(address, out data, length);
            string text = encoding.GetString(data);
            if (text.Contains("\0"))
                text = text.Substring(0, text.IndexOf('\0'));
            return text;
            //return encoding.GetString(data);
        }
        /// <summary>
        /// Generic function to read data from memory using the given type
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="address">The address to read data at</param>
        /// <param name="defVal">The default value of this operation (which is returned in case the Read-operation fails)</param>
        /// <returns>The value read from memory</returns>
        public T Read<T>(IntPtr address, T defVal = default(T)) where T : struct
        {
            byte[] data;
            int size = Marshal.SizeOf(typeof(T));

            Read(address, out data, size);
            return BytesToT<T>(data, defVal);
        }
        /// <summary>
        /// Generic function to read an array of data from memory using the given type
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="address">The address to read data at</param>
        /// <param name="length">The number of elements to read</param>
        /// <returns></returns>
        public T[] ReadArray<T>(IntPtr address, int length) where T : struct
        {
            byte[] data;
            int size = Marshal.SizeOf(typeof(T));

            Read(address, out data, size * length);
            T[] result = new T[length];
            for (int i = 0; i < length; i++)
                result[i] = BytesToT<T>(data, i * size);

            return result;
        }
        /// <summary>
        /// Generic function to read data from memory using the given type
        /// Applies the given offsets to read multilevel-pointers
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="address">The address to read data at</param>
        /// <param name="offsets">Array of offsets to apply</param>
        /// <returns></returns>
        public T ReadMultilevelPointer<T>(IntPtr address, params int[] offsets) where T : struct
        {
            for (int i = 0; i < offsets.Length - 1; i++)
                address = Read<IntPtr>((IntPtr)(address.ToInt64() + offsets[i]));
            return Read<T>((IntPtr)(address.ToInt64() + offsets[offsets.Length - 1]), default(T));
        }
        /// <summary>
        /// Reads a matrix from memory
        /// </summary>
        /// <param name="address">The address of the matrix in memory</param>
        /// <param name="rows">The number of rows of this matrix</param>
        /// <param name="columns">The number of columns of this matrix</param>
        /// <returns>The matrix read from memory</returns>
        //public Matrix ReadMatrix(IntPtr address, int rows, int columns)
        //{
        //    Matrix matrix = new Matrix(rows, columns);
        //    byte[] data;
        //    Read(address, out data, SIZE_FLOAT * rows * columns);
        //    matrix.Read(data);

        //    return matrix;
        //}
        /// <summary>
        /// Generic function to read an array from memory using the given type and offsets.
        /// Offsets will be added to the address. (They will not be summed up but rather applied individually)
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="address">The address to read data at</param>
        /// <param name="offsets">Offsets that will be applied to the address</param>
        /// <returns></returns>
        public T[] Read<T>(IntPtr address, params int[] offsets) where T : struct
        {
            T[] values = new T[offsets.Length];
            for (int i = 0; i < offsets.Length; i++)
                values[i] = Read<T>((IntPtr)(address.ToInt32() + offsets[i]));
            return values;
        }
        #endregion
        #region WRITE
        /// <summary>
        /// Writes a string to memory using the given encoding
        /// </summary>
        /// <param name="address">The address to write the string to</param>
        /// <param name="text">The text to write</param>
        /// <param name="encoding">The encoding of the string</param>
        public void WriteString(IntPtr address, string text, Encoding encoding)
        {
            Write(address, encoding.GetBytes(text));
        }
        /// <summary>
        /// Generic function to write data to memory using the given type
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="address">The address to write data to</param>
        /// <param name="value">The value to write to memory</param>
        public void Write<T>(IntPtr address, T value) where T : struct
        {
            Write(address, TToBytes<T>(value));
        }
        /// <summary>
        /// Writes a value using the given offset and length of data
        /// It will apply the offset to the address as well as to the data, length defines the number of bytes to write (beginning at offset)
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="address">The address to write to</param>
        /// <param name="value">The value to write</param>
        /// <param name="offset">Skips the given number of bytes (applies to address and data)</param>
        /// <param name="length">Number of bytes to write (beginning at offset)</param>
        /// <returns></returns>
        public void Write<T>(IntPtr address, T value, int offset, int length) where T : struct
        {
            byte[] data = TToBytes<T>(value);
            Write(address, data, offset, length);
        }
        /// <summary>
        /// Writes a matrix to memory
        /// </summary>
        /// <param name="address">The address to write the matrix to</param>
        /// <param name="matrix">The matrix to write to memory</param>
        //public void WriteMatrix(IntPtr address, Matrix matrix)
        //{
        //    Write(address, matrix.ToByteArray());
        //}
        #endregion
        #endregion
        #region MARSHALLING
        /// <summary>
        /// Converts the given array of bytes to the specified type.
        /// Uses either marshalling or unsafe code, depending on UseUnsafeReadWrite
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="data">Array of bytes</param>
        /// <param name="defVal">The default value of this operation (which is returned in case the Read-operation fails)</param>
        /// <returns></returns>
        public T BytesToT<T>(byte[] data, T defVal = default(T)) where T : struct
        {
            T structure = defVal;

            //if (UseUnsafeReadWrite)
            //{
            //    fixed (byte* b = data)
            //        structure = (T)Marshal.PtrToStructure((IntPtr)b, typeof(T));
            //}
            //else
            {
                GCHandle gcHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                structure = (T)Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(T));
                gcHandle.Free();
            }
            return structure;
        }
        /// <summary>
        /// Converts the given array of bytes to the specified type.
        /// Uses either marshalling or unsafe code, depending on UseUnsafeReadWrite
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="data">Array of bytes</param>
        /// <param name="index">Index of the data to convert</param>
        /// <param name="defVal">The default value of this operation (which is returned in case the Read-operation fails)</param>
        /// <returns></returns>
        public T BytesToT<T>(byte[] data, int index, T defVal = default(T)) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] tmp = new byte[size];
            Array.Copy(data, index, tmp, 0, size);
            return BytesToT<T>(tmp, defVal);
        }
        /// <summary>
        /// Converts the given struct to a byte-array
        /// </summary>
        /// <typeparam name="T">The type of the struct</typeparam>
        /// <param name="value">Value to conver to bytes</param>
        /// <returns></returns>
        public  byte[] TToBytes<T>(T value) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] data = new byte[size];

            //if (UseUnsafeReadWrite)
            //{
            //    fixed (byte* b = data)
            //        Marshal.StructureToPtr(value, (IntPtr)b, true);
            //}
            //else
            {
                IntPtr ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(value, ptr, true);
                Marshal.Copy(ptr, data, 0, size);
                Marshal.FreeHGlobal(ptr);
            }

            return data;
        }
        #endregion
        #region SIGSCANNING
        /// <summary>
        /// Performs a signature-scan using for the given pattern and mask in the given range of the process' address space
        /// </summary>
        /// <param name="pattern">Byte-pattern to scan for</param>
        /// <param name="mask">Mask to scan for ('?' is the wildcard)</param>
        /// <param name="module">Module to scan</param>
        /// <param name="codeSectionOnly">If true, MemUtils will parse the module's headers and scan the .code-section only</param>
        /// <param name="wildcard">Char that is used as wildcard in the mask</param>
        /// <returns></returns>
        public ScanResult PerformSignatureScan(byte[] pattern, string mask, ProcessModule module, bool codeSectionOnly = true, char wildcard = '?')
        {
            if (codeSectionOnly)
            {
                PEInfo info = new PEInfo(module, this);
                return PerformSignatureScan(
                    pattern,
                    mask,
                    (IntPtr)(info.PEOptHeaderAddress.ToInt64() + info.PEOptHeader.BaseOfCode),
                    info.PEOptHeader.SizeOfCode,
                    wildcard);
            }
            else
            {
                return PerformSignatureScan(
                    pattern,
                    mask,
                    module.BaseAddress,
                    module.ModuleMemorySize,
                    wildcard);
            }
        }
        /// <summary>
        /// Performs a signature-scan using for the given pattern and mask in the given range of the process' address space
        /// </summary>
        /// <param name="pattern">Byte-pattern to scan for</param>
        /// <param name="mask">Mask to scan for ('?' is the wildcard)</param>
        /// <param name="from">Where to start scanning from</param>
        /// <param name="length">The length of the range to scan in</param>
        /// <param name="wildcard">Char that is used as wildcard in the mask</param>
        /// <returns></returns>
        public ScanResult PerformSignatureScan(byte[] pattern, string mask, IntPtr from, int length, char wildcard = '?')
        {
            return PerformSignatureScan(pattern, mask, from, (IntPtr)(from.ToInt64() + length), wildcard);
        }
        /// <summary>
        /// Performs a signature-scan using for the given pattern and mask in the given range of the process' address space
        /// Returns the address of the beginning of the pattern if found, returns IntPtr.Zero if not found
        /// </summary>
        /// <param name="pattern">Byte-pattern to scan for</param>
        /// <param name="mask">Mask to scan for</param>
        /// <param name="from">Where to start scanning from</param>
        /// <param name="to">Where to stop scanning at</param>
        /// <param name="wildcard">Char that is used as wildcard in the mask</param>
        /// <returns></returns>
        public ScanResult PerformSignatureScan(byte[] pattern, string mask, IntPtr from, IntPtr to, char wildcard = '?')
        {
            if (from.ToInt64() >= to.ToInt64())
                throw new ArgumentException();
            if (pattern == null)
                throw new ArgumentNullException();
            if (mask.Length != pattern.Length)
                throw new ArgumentException();

            long totalLength = to.ToInt64() - from.ToInt64();
            int dumps = (int)Math.Ceiling((double)(totalLength) / (double)MAX_DUMP_SIZE);
            int length = 0;
            byte[] data;

            for (int dmp = 0; dmp < dumps; dmp++)
            {
                if (totalLength - (dmp * MAX_DUMP_SIZE) < MAX_DUMP_SIZE)
                    length = (int)(totalLength - (dmp * MAX_DUMP_SIZE));
                else
                    length = MAX_DUMP_SIZE;

                Read((IntPtr)(from.ToInt64() + dmp * MAX_DUMP_SIZE), out data, length);
                int idx = ScanDump(data, pattern, mask, wildcard);
                if (idx != -1)
                {
                    return new ScanResult()
                    {
                        Success = true,
                        Base = from,
                        Offset = (IntPtr)(dmp * MAX_DUMP_SIZE + idx),
                        Address = (IntPtr)(from + dmp * MAX_DUMP_SIZE + idx)
                    };
                }
            }

            return new ScanResult() { Address = IntPtr.Zero, Base = IntPtr.Zero, Offset = IntPtr.Zero, Success = false };
        }
        /// <summary>
        /// Scans a dumped chunk of memory and returns the index of the pattern if found
        /// </summary>
        /// <param name="data">Chunk of memory</param>
        /// <param name="pattern">Byte-pattern to scan for</param>
        /// <param name="mask">Mask to scan for</param>
        /// <param name="wildcard">Char that is used as wildcard in the mask</param>
        /// <returns>Index of pattern if found, -1 if not found</returns>
        private int ScanDump(byte[] data, byte[] pattern, string mask, char wildcard)
        {
            bool found = false;
            for (int idx = 0; idx < data.Length - pattern.Length; idx++)
            {
                found = true;
                for (int chr = 0; chr < mask.Length; chr++)
                {
                    if (mask[chr] != wildcard)
                    {
                        if (data[idx + chr] != pattern[chr])
                        {
                            found = false;
                            break;
                        }
                    }
                }
                if (found)
                    return idx;
            }
            return -1;
        }
        /// <summary>
        /// Creates a mask from a given pattern, using the given chars
        /// </summary>
        /// <param name="pattern">The pattern this functions designs a mask for</param>
        /// <param name="wildcardByte">Byte that is interpreted as a wildcard</param>
        /// <param name="wildcardChar">Char that is used as wildcard</param>
        /// <param name="matchChar">Char that is no wildcard</param>
        /// <returns></returns>
        public string MaskFromPattern(byte[] pattern, byte wildcardByte, char wildcardChar = '?', char matchChar = 'x')
        {
            char[] chr = new char[pattern.Length];
            for (int i = 0; i < chr.Length; i++)
                chr[i] = pattern[i] == wildcardByte ? wildcardChar : matchChar;
            return new string(chr);
        }
        #endregion
        #endregion
    }
    /// <summary>
    /// Generic object methods
    /// </summary>
    public static class MemStatic
    {
        public static T GetStructure<T>(byte[] data)
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            T structure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return structure;
        }
        public static T GetStructure<T>(this byte[] data, int offset, int length)
        {
            byte[] dt = new byte[length];
            Array.Copy(data, offset, dt, 0, length);
            return GetStructure<T>(dt);
        }
        /// <summary>
        /// Gets size of T object
        /// </summary>
        /// <returns>Size of object</returns>
        public static int SizeOf<T>(this T obj)
        {
            return Marshal.SizeOf(typeof(T));
        }
        public static string GetMemberName<T, TValue>(Expression<Func<T, TValue>> memberAccess)
        {
            return ((MemberExpression)memberAccess.Body).Member.Name;
        }
    }
    public struct ScanResult
    {
        public bool Success;
        public IntPtr Address;
        public IntPtr Base;
        public IntPtr Offset;
    }
    /// <summary>
    /// Parses information about a module
    /// </summary>
    public struct PEInfo
    {
        private MemUtils MemUtils;
        /// <summary>
        /// DOS-header of the module
        /// </summary>
        public DOSHeader DOSHeader;

        /// <summary>
        /// COFF-header of the module
        /// </summary>
        public COFFHeader COFFHeader;

        /// <summary>
        /// PE optional header of the module
        /// </summary>
        public PEOptHeader PEOptHeader;

        /// <summary>
        /// Address of the COFF header
        /// </summary>
        public IntPtr COFFHeaderAddress;

        /// <summary>
        /// Address of the PE optional header
        /// </summary>
        public IntPtr PEOptHeaderAddress;

        /// <summary>
        /// Initializes a new PEInfo using the given module
        /// </summary>
        /// <param name="module"></param>
        /// <param name="memUtils">Instance of MemUtils to use in order to read data</param>
        public PEInfo(ProcessModule module, MemUtils memUtils) : this(module.BaseAddress, memUtils) { }

        /// <summary>
        /// Initializes a new PEInfo using the given baseaddress of a module
        /// </summary>
        /// <param name="baseAddress"></param>
        /// <param name="memUtils">Instance of MemUtils to use in order to read data</param>
        public PEInfo(IntPtr baseAddress, MemUtils memUtils)
        {
            MemUtils = memUtils;

            DOSHeader = MemUtils.Read<DOSHeader>(baseAddress);

            COFFHeaderAddress = new IntPtr(baseAddress.ToInt64() + DOSHeader.e_lfanew + 4);
            COFFHeader = MemUtils.Read<COFFHeader>(COFFHeaderAddress);

            PEOptHeaderAddress = new IntPtr(COFFHeaderAddress.ToInt64() + Marshal.SizeOf(typeof(COFFHeader)));
            PEOptHeader = MemUtils.Read<PEOptHeader>(PEOptHeaderAddress);
        }
    }
    /// <summary>
    /// Source: https://en.wikibooks.org/wiki/X86_Disassembly/Windows_Executable_Files#Code_Sections
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public  struct DOSHeader
    {
        public short signature;
        public short lastsize;
        public short nblocks;
        public short nreloc;
        public short hdrsize;
        public short minalloc;
        public short maxalloc;
        public short ss;
        public short sp;
        public short checksum;
        public short ip;
        public short cs;
        public short relocpos;
        public short noverlay;

        public short reserved1;
        public short reserved2;
        public short reserved3;
        public short reserved4;

        public short oem_id;
        public short oem_info;

        public short reserved5;
        public short reserved6;
        public short reserved7;
        public short reserved8;
        public short reserved9;
        public short reserved10;
        public short reserved11;
        public short reserved12;
        public short reserved13;
        public short reserved14;

        public int e_lfanew;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct COFFHeader
    {
        short Machine;
        short NumberOfSections;
        int TimeDateStamp;
        int PointerToSymbolTable;
        int NumberOfSymbols;
        short SizeOfOptionalHeader;
        short Characteristics;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct PEOptHeader
    {
        public short signature; //decimal number 267 for 32 bit, and 523 for 64 bit.
        public char MajorLinkerVersion;
        public char MinorLinkerVersion;
        public int SizeOfCode;
        public int SizeOfInitializedData;
        public int SizeOfUninitializedData;
        public int AddressOfEntryPoint;  //The RVA of the code entry popublic int
        public int BaseOfCode;
        public int BaseOfData;
        public int ImageBase;
        public int SectionAlignment;
        public int FileAlignment;
        public short MajorOSVersion;
        public short MinorOSVersion;
        public short MajorImageVersion;
        public short MinorImageVersion;
        public short MajorSubsystemVersion;
        public short MinorSubsystemVersion;
        public int Reserved;
        public int SizeOfImage;
        public int SizeOfHeaders;
        public int Checksum;
        public short Subsystem;
        public short DLLCharacteristics;
        public int SizeOfStackReserve;
        public int SizeOfStackCommit;
        public int SizeOfHeapReserve;
        public int SizeOfHeapCommit;
        public int LoaderFlags;
        public int NumberOfRvaAndSizes;
        public data_directory DataDirectory1;
        public data_directory DataDirectory2;
        public data_directory DataDirectory3;
        public data_directory DataDirectory4;
        public data_directory DataDirectory5;
        public data_directory DataDirectory6;
        public data_directory DataDirectory7;
        public data_directory DataDirectory8;
        public data_directory DataDirectory9;
        public data_directory DataDirectory10;
        public data_directory DataDirectory11;
        public data_directory DataDirectory12;
        public data_directory DataDirectory13;
        public data_directory DataDirectory14;
        public data_directory DataDirectory15;
        public data_directory DataDirectory16;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct data_directory
    {
        public int VirtualAddress;
        public int Size;
    }
    public struct Vector3
    {
        #region VARIABLES
        public float X;
        public float Y;
        public float Z;
        #endregion

        #region PROPERTIES
        /// <summary>
        /// Returns a new Vector3 at (0,0,0)
        /// </summary>
        public static Vector3 Zero
        {
            get { return new Vector3(0, 0, 0); }
        }
        /// <summary>
        /// Returns a new Vector3 at (1,0,0)
        /// </summary>
        public static Vector3 UnitX
        {
            get { return new Vector3(1, 0, 0); }
        }
        /// <summary>
        /// Returns a new Vector3 at (0,1,0)
        /// </summary>
        public static Vector3 UnitY
        {
            get { return new Vector3(0, 1, 0); }
        }
        /// <summary>
        /// Returns a new Vector3 at (0,0,1)
        /// </summary>
        public static Vector3 UnitZ
        {
            get { return new Vector3(0, 0, 1); }
        }
        #endregion

        #region CONSTRUCTOR
        /// <summary>
        /// Initializes a new Vector3 using the given values
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public Vector3(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
        /// <summary>
        /// Initializes a new Vector3 by copying the values of the given Vector3
        /// </summary>
        /// <param name="vec"></param>
        public Vector3(Vector3 vec) : this(vec.X, vec.Y, vec.Z) { }
        /// <summary>
        /// Initializes a new Vector3 using the given float-array
        /// </summary>
        /// <param name="values"></param>
        public Vector3(float[] values) : this(values[0], values[1], values[2]) { }
        #endregion

        #region METHODS
        /// <summary>
        /// Returns the length of this Vector3
        /// </summary>
        /// <returns></returns>
        public float Length()
        {
            return (float)System.Math.Abs(System.Math.Sqrt(System.Math.Pow(X, 2) + System.Math.Pow(Y, 2) + System.Math.Pow(Z, 2)));
        }
        /// <summary>
        /// Returns the distance from this Vector3 to the given Vector3
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public float DistanceTo(Vector3 other)
        {
            return (this - other).Length();
        }

        public override bool Equals(object obj)
        {
            Vector3 vec = (Vector3)obj;
            return this.GetHashCode() == vec.GetHashCode();
        }

        public override int GetHashCode()
        {
            return this.X.GetHashCode() ^ this.Y.GetHashCode() ^ this.Z.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[X={0}, Y={1}, Z={2}]", this.X.ToString(), this.Y.ToString(), this.Z.ToString());
        }
        #endregion

        #region OPERATORS
        public static Vector3 operator +(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }
        public static Vector3 operator -(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }
        public static Vector3 operator *(Vector3 v1, float scalar)
        {
            return new Vector3(v1.X * scalar, v1.Y * scalar, v1.Z * scalar);
        }
        public static bool operator ==(Vector3 v1, Vector3 v2)
        {
            return v1.X == v2.X && v1.Y == v2.Y && v1.Z == v2.Z;
        }
        public static bool operator !=(Vector3 v1, Vector3 v2)
        {
            return !(v1 == v2);
        }
        public float this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return this.X;
                    case 1:
                        return this.Y;
                    case 2:
                        return this.Z;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (i)
                {
                    case 0:
                        this.X = value;
                        break;
                    case 1:
                        this.Y = value;
                        break;
                    case 2:
                        this.Z = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }
        #endregion
    }
    public class ProcUtils
    {
        #region PROPERTIES
        /// <summary>
        /// The process this ProcUtils wraps
        /// </summary>
        public Process Process { get; private set; }
        /// <summary>
        /// The (opened) handle to the process of this ProcUtils
        /// </summary>
        public IntPtr Handle { get; private set; }
        public bool IsRunning
        {
            get
            {
                if (Process == null)
                    return false;
                if (Process.HasExited)
                {
                    Process.Dispose();
                    Process = null;
                    CloseHandleToProcess(Handle);
                    return false;
                }
                return true;
            }
        }
        #endregion
        #region STATIC METHODS
        /// <summary>
        /// Returns whether a specific process is running
        /// </summary>
        /// <param name="name">The name of the process</param>
        /// <returns></returns>
        public static bool ProcessIsRunning(string name)
        {
            return Process.GetProcessesByName(name).Length > 0;
        }
        /// <summary>
        /// Returns whether a specific process is running
        /// </summary>
        /// <param name="id">The ID of the process</param>
        /// <returns></returns>
        public static bool ProcessIsRunning(int id)
        {
            return Process.GetProcessById(id) != null;
        }
        /// <summary>
        /// Opens a handle to a process
        /// </summary>
        /// <param name="id">ID of the process</param>
        /// <param name="flags">ProcessAccessFlags to use</param>
        /// <returns>A handle to the process</returns>
        public static IntPtr OpenHandleByProcessID(int id, WinAPI.ProcessAccessFlags flags)
        {
            return WinAPI.OpenProcess(flags, false, id);
        }
        /// <summary>
        /// Opens a handle to a process
        /// </summary>
        /// <param name="name">Name of the process</param>
        /// <param name="flags">ProcessAccessFlags to use</param>
        /// <returns>A handle to the process</returns>
        public static IntPtr OpenHandleByProcessName(string name, WinAPI.ProcessAccessFlags flags)
        {
            return OpenHandleByProcessID(Process.GetProcessesByName(name)[0].Id, flags);
        }
        /// <summary>
        /// Opens a handle to a process
        /// </summary>
        /// <param name="process">The process-object of the process</param>
        /// <param name="flags">ProcessAccessFlags to use</param>
        /// <returns>A handle to the process</returns>
        public static IntPtr OpenHandleByProcess(Process process, WinAPI.ProcessAccessFlags flags)
        {
            return OpenHandleByProcessID(process.Id, flags);
        }
        /// <summary>
        /// Closes the given handle to the process
        /// </summary>
        /// <param name="handle">Handle to the process</param>
        public static void CloseHandleToProcess(IntPtr handle)
        {
            WinAPI.CloseHandle(handle);
        }
        #endregion
        #region CONSTRUCTOR/DESTRUCTOR
        /// <summary>
        /// Initializes a new ProcUtils
        /// </summary>
        /// <param name="processName">Name of the process</param>
        /// <param name="handleFlags">ProcessAccessFlags to use</param>
        public ProcUtils(string processName, WinAPI.ProcessAccessFlags handleFlags)
            : this(Process.GetProcessesByName(processName)[0], handleFlags)
        { }
        /// <summary>
        /// Initializes a new ProcUtils
        /// </summary>
        /// <param name="id">ID of the process</param>
        /// <param name="handleFlags">ProcessAccessFlags to use</param>
        public ProcUtils(int id, WinAPI.ProcessAccessFlags handleFlags)
            : this(Process.GetProcessById(id), handleFlags)
        { }
        /// <summary>
        /// Initializes a new ProcUtils
        /// </summary>
        /// <param name="process">Process-object of the process</param>
        /// <param name="handleFlags">ProcessAccessFlags to use</param>
        public ProcUtils(Process process, WinAPI.ProcessAccessFlags handleFlags)
        {
            this.Process = process;
            this.Handle = ProcUtils.OpenHandleByProcess(process, handleFlags);
        }
        ~ProcUtils()
        {
            CloseHandleToProcess(Handle);
        }
        #endregion
        #region METHODS
        /// <summary>
        /// Retrieves the process-module with the given name, returns null if not found
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ProcessModule GetModuleByName(string name)
        {
            try
            {
                foreach (ProcessModule module in Process.Modules)
                    if (module.FileName.EndsWith(name))
                        return module;
            }
            catch { }
            return null;
        }
        #endregion
    }
}
