using System;

namespace Lycus.Satori
{
    /// <summary>
    /// Represents the register file of an Epiphany eCore.
    /// 
    /// This class doesn't actually hold any state by itself. It
    /// simply forwards register accesses to the appropriate memory
    /// locations.
    ///
    /// Generally, writing to system registers is a bad idea as it
    /// can destabilize the state of a core. Instead, prefer public
    /// APIs that deal with various system registers. Writing to
    /// some registers, such as the general-purpose registers and
    /// the program counter, is safe as long as the core is idle or
    /// halted when the writing happens.
    /// </summary>
    public sealed class RegisterFile
    {
        /// <summary>
        /// The lowest general-purpose register.
        /// </summary>
        public const int MinRegister = 0;

        /// <summary>
        /// The highest general-purpose register.
        /// </summary>
        public const int MaxRegister = 63;

        [CLSCompliant(false)]
        public const uint CoreConfigAddress = 0x000F0400;

        [CLSCompliant(false)]
        public const uint CoreStatusAddress = 0x000F0404;

        [CLSCompliant(false)]
        public const uint ProgramCounterAddress = 0x000F0408;

        [CLSCompliant(false)]
        public const uint DebugStatusAddress = 0x000F040C;

        [CLSCompliant(false)]
        public const uint LoopCounterAddress = 0x000F0414;

        [CLSCompliant(false)]
        public const uint LoopStartAddress = 0x000F0418;

        [CLSCompliant(false)]
        public const uint LoopEndAddress = 0x000F041C;

        [CLSCompliant(false)]
        public const uint InterruptReturnAddress = 0x000F0420;

        [CLSCompliant(false)]
        public const uint InterruptMaskAddress = 0x000F0424;

        [CLSCompliant(false)]
        public const uint InterruptLatchAddress = 0x000F0428;

        [CLSCompliant(false)]
        public const uint InterruptLatchStoreAddress = 0x000F042C;

        [CLSCompliant(false)]
        public const uint InterruptLatchClearAddress = 0x000F0430;

        [CLSCompliant(false)]
        public const uint InterruptsPendingAddress = 0x000F0434;

        [CLSCompliant(false)]
        public const uint CoreStatusStoreAddress = 0x000F0440;

        [CLSCompliant(false)]
        public const uint DebugCommandAddress = 0x000F0448;

        [CLSCompliant(false)]
        public const uint ResetCoreAddress = 0x000F070C;

        [CLSCompliant(false)]
        public const uint CoreTimer0Address = 0x000F0438;

        [CLSCompliant(false)]
        public const uint CoreTimer1Address = 0x000F043C;

        [CLSCompliant(false)]
        public const uint MemoryStatusAddress = 0x000F0604;

        [CLSCompliant(false)]
        public const uint MemoryProtectAddress = 0x000F0608;

        [CLSCompliant(false)]
        public const uint DirectAccess0ConfigAddress = 0x000F0500;

        [CLSCompliant(false)]
        public const uint DirectAccess0StrideAddress = 0x000F0504;

        [CLSCompliant(false)]
        public const uint DirectAccess0CountAddress = 0x000F0508;

        [CLSCompliant(false)]
        public const uint DirectAccess0SourceAddress = 0x000F050C;

        [CLSCompliant(false)]
        public const uint DirectAccess0TargetAddress = 0x000F0510;

        [CLSCompliant(false)]
        public const uint DirectAccess0Auto0Address = 0x000F0514;

        [CLSCompliant(false)]
        public const uint DirectAccess0Auto1Address = 0x000F0518;

        [CLSCompliant(false)]
        public const uint DirectAccess0StatusAddress = 0x000F051C;

        [CLSCompliant(false)]
        public const uint DirectAccess1ConfigAddress = 0x000F0520;

        [CLSCompliant(false)]
        public const uint DirectAccess1StrideAddress = 0x000F0524;

        [CLSCompliant(false)]
        public const uint DirectAccess1CountAddress = 0x000F0528;

        [CLSCompliant(false)]
        public const uint DirectAccess1SourceAddress = 0x000F052C;

        [CLSCompliant(false)]
        public const uint DirectAccess1TargetAddress = 0x000F0530;

        [CLSCompliant(false)]
        public const uint DirectAccess1Auto0Address = 0x000F0534;

        [CLSCompliant(false)]
        public const uint DirectAccess1Auto1Address = 0x000F0538;

        [CLSCompliant(false)]
        public const uint DirectAccess1StatusAddress = 0x000F053C;

        [CLSCompliant(false)]
        public const uint MeshConfigAddress = 0x000F0700;

        [CLSCompliant(false)]
        public const uint CoreIdAddress = 0x000F0704;

        [CLSCompliant(false)]
        public const uint MulticastConfigAddress = 0x000F0708;

        [CLSCompliant(false)]
        public const uint CMeshRouteAddress = 0x000F0710;

        [CLSCompliant(false)]
        public const uint XMeshRouteAddress = 0x000F0714;

        [CLSCompliant(false)]
        public const uint RMeshRouteAddress = 0x000F0718;

        public Core Core { get; private set; }

        internal RegisterFile(Core core)
        {
            Core = core;
        }

        public int this[int number]
        {
            get
            {
                if (number < MinRegister || number > MaxRegister)
                    throw new ArgumentOutOfRangeException("number", number,
                        "General-purpose register number is out of range.");

                return Core.Machine.Memory.ReadInt32(Core,
                    (uint)(Memory.RegisterFileAddress + number * sizeof(uint)));
            }
            set
            {
                if (number < MinRegister || number > MaxRegister)
                    throw new ArgumentOutOfRangeException("number", number,
                        "General-purpose register number is out of range.");

                Core.Machine.Memory.Write(Core,
                    (uint)(Memory.RegisterFileAddress + number * sizeof(uint)), value);
            }
        }

        [CLSCompliant(false)]
        public uint CoreConfig
        {
            get { return Core.Machine.Memory.ReadUInt32(Core, CoreConfigAddress); }
            set { Core.Machine.Memory.Write(Core, CoreConfigAddress, value); }
        }

        [CLSCompliant(false)]
        public uint CoreStatus
        {
            get { return Core.Machine.Memory.ReadUInt32(Core, CoreStatusAddress); }
            set { Core.Machine.Memory.Write(Core, CoreStatusAddress, value); }
        }

        [CLSCompliant(false)]
        public uint ProgramCounter
        {
            get { return Core.Machine.Memory.ReadUInt32(Core, ProgramCounterAddress); }
            set { Core.Machine.Memory.Write(Core, ProgramCounterAddress, value); }
        }

        [CLSCompliant(false)]
        public uint DebugStatus
        {
            get { return Core.Machine.Memory.ReadUInt32(Core, DebugStatusAddress); }
            set { Core.Machine.Memory.Write(Core, DebugStatusAddress, value); }
        }

        [CLSCompliant(false)]
        public uint LoopCounter
        {
            get { return Core.Machine.Memory.ReadUInt32(Core, LoopCounterAddress); }
            set { Core.Machine.Memory.Write(Core, LoopCounterAddress, value); }
        }

        [CLSCompliant(false)]
        public uint LoopStart
        {
            get { return Core.Machine.Memory.ReadUInt32(Core, LoopStartAddress); }
            set { Core.Machine.Memory.Write(Core, LoopStartAddress, value); }
        }

        [CLSCompliant(false)]
        public uint LoopEnd
        {
            get { return Core.Machine.Memory.ReadUInt32(Core, LoopEndAddress); }
            set { Core.Machine.Memory.Write(Core, LoopEndAddress, value); }
        }

        [CLSCompliant(false)]
        public uint InterruptReturn
        {
            get { return Core.Machine.Memory.ReadUInt32(Core, InterruptReturnAddress); }
            set { Core.Machine.Memory.Write(Core, InterruptReturnAddress, value); }
        }

        [CLSCompliant(false)]
        public uint InterruptMask
        {
            get { return Core.Machine.Memory.ReadUInt32(Core, InterruptMaskAddress); }
            set { Core.Machine.Memory.Write(Core, InterruptMaskAddress, value); }
        }

        [CLSCompliant(false)]
        public uint InterruptLatch
        {
            get { return Core.Machine.Memory.ReadUInt32(Core, InterruptLatchAddress); }
            set { Core.Machine.Memory.Write(Core, InterruptLatchAddress, value); }
        }

        [CLSCompliant(false)]
        public uint InterruptLatchStore
        {
            get { return Core.Machine.Memory.ReadUInt32(Core, InterruptLatchStoreAddress); }
            set { Core.Machine.Memory.Write(Core, InterruptLatchStoreAddress, value); }
        }

        [CLSCompliant(false)]
        public uint InterruptLatchClear
        {
            get { return Core.Machine.Memory.ReadUInt32(Core, InterruptLatchClearAddress); }
            set { Core.Machine.Memory.Write(Core, InterruptLatchClearAddress, value); }
        }

        [CLSCompliant(false)]
        public uint InterruptsPending
        {
            get { return Core.Machine.Memory.ReadUInt32(Core, InterruptsPendingAddress); }
            set { Core.Machine.Memory.Write(Core, InterruptsPendingAddress, value); }
        }

        [CLSCompliant(false)]
        public uint CoreStatusStore
        {
            get { return Core.Machine.Memory.ReadUInt32(Core, CoreStatusStoreAddress); }
            set { Core.Machine.Memory.Write(Core, CoreStatusStoreAddress, value); }
        }

        [CLSCompliant(false)]
        public uint DebugCommand
        {
            get { return Core.Machine.Memory.ReadUInt32(Core, DebugCommandAddress); }
            set { Core.Machine.Memory.Write(Core, DebugCommandAddress, value); }
        }

        [CLSCompliant(false)]
        public uint ResetCore
        {
            get { return Core.Machine.Memory.ReadUInt32(Core, ResetCoreAddress); }
            set { Core.Machine.Memory.Write(Core, ResetCoreAddress, value); }
        }

        [CLSCompliant(false)]
        public uint CoreTimer0
        {
            get { return Core.Machine.Memory.ReadUInt32(Core, CoreTimer0Address); }
            set { Core.Machine.Memory.Write(Core, CoreTimer0Address, value); }
        }

        [CLSCompliant(false)]
        public uint CoreTimer1
        {
            get { return Core.Machine.Memory.ReadUInt32(Core, CoreTimer1Address); }
            set { Core.Machine.Memory.Write(Core, CoreTimer1Address, value); }
        }

        [CLSCompliant(false)]
        public uint MemoryStatus
        {
            get { return Core.Machine.Memory.ReadUInt32(Core, MemoryStatusAddress); }
            set { Core.Machine.Memory.Write(Core, MemoryStatusAddress, value); }
        }

        [CLSCompliant(false)]
        public uint MemoryProtect
        {
            get { return Core.Machine.Memory.ReadUInt32(Core, MemoryProtectAddress); }
            set { Core.Machine.Memory.Write(Core, MemoryProtectAddress, value); }
        }

        [CLSCompliant(false)]
        public uint MeshConfig
        {
            get { return Core.Machine.Memory.ReadUInt32(Core, MeshConfigAddress); }
            set { Core.Machine.Memory.Write(Core, MeshConfigAddress, value); }
        }

        [CLSCompliant(false)]
        public uint CoreId
        {
            get { return Core.Machine.Memory.ReadUInt32(Core, CoreIdAddress); }
            set { Core.Machine.Memory.Write(Core, CoreIdAddress, value); }
        }

        [CLSCompliant(false)]
        public uint MulticastConfig
        {
            get { return Core.Machine.Memory.ReadUInt32(Core, MulticastConfigAddress); }
            set { Core.Machine.Memory.Write(Core, MulticastConfigAddress, value); }
        }

        [CLSCompliant(false)]
        public uint CMeshRoute
        {
            get { return Core.Machine.Memory.ReadUInt32(Core, CMeshRouteAddress); }
            set { Core.Machine.Memory.Write(Core, CMeshRouteAddress, value); }
        }

        [CLSCompliant(false)]
        public uint XMeshRoute
        {
            get { return Core.Machine.Memory.ReadUInt32(Core, XMeshRouteAddress); }
            set { Core.Machine.Memory.Write(Core, XMeshRouteAddress, value); }
        }

        [CLSCompliant(false)]
        public uint RMeshRoute
        {
            get { return Core.Machine.Memory.ReadUInt32(Core, RMeshRouteAddress); }
            set { Core.Machine.Memory.Write(Core, RMeshRouteAddress, value); }
        }
    }
}
