using System;
using System.IO;
using System.Text;

namespace Lycus.Satori
{
    public static class Loader
    {
        static bool IsOnChip(Machine machine, uint address)
        {
            var id = CoreId.FromAddress(address);

            return id.Row < machine.Rows && id.Column < machine.Columns;
        }

        public static void LoadCode(Core core, Stream stream)
        {
            if (core == null)
                throw new ArgumentNullException("core");

            if (stream == null)
                throw new ArgumentNullException("stream");

            core.Machine.Logger.VerboseCore(core, "Loading ELF executable");

            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                if (reader.ReadByte() != 0x7F ||
                    reader.ReadByte() != 'E' ||
                    reader.ReadByte() != 'L' ||
                    reader.ReadByte() != 'F')
                    throw new LoaderException("ELF magic number doesn't match EI_MAG{0..3}.");

                if (reader.ReadByte() != 1)
                    throw new LoaderException("ELF class is not ELFCLASS32.");

                if (reader.ReadByte() != 1)
                    throw new LoaderException("ELF data encoding is not ELFDATA2LSB.");

                if (reader.ReadByte() != 1)
                    throw new LoaderException("ELF version number is not EV_CURRENT.");

                if (reader.ReadByte() != 0)
                    throw new LoaderException("ELF ABI is not ELFOSABI_SYSV.");

                if (reader.ReadByte() != 0)
                    throw new LoaderException("ELF ABI version number is not zero.");

                reader.ReadBytes(7); // Padding bytes.

                if (reader.ReadUInt16() != 2)
                    throw new LoaderException("ELF object type is not ET_EXEC.");

                if (reader.ReadUInt16() != 0x1223)
                    throw new LoaderException("ELF machine type is not EM_ADAPTEVA_EPIPHANY.");

                if (reader.ReadUInt32() != 1)
                    throw new LoaderException("ELF file version number is not EV_CURRENT.");

                reader.ReadUInt32(); // The `e_entry` field.
                uint phOff = reader.ReadUInt32(); // The `e_phoff` field.
                var shOff = reader.ReadUInt32(); // The `e_shoff` field.

                if (reader.ReadUInt32() != 0x00000000)
                    throw new LoaderException("ELF machine flags are not 0x00000000.");

                reader.ReadUInt16(); // The `e_ehsize` field.
                reader.ReadUInt16(); // The `e_phentsize` field.

                var num = reader.ReadUInt16(); // The `e_phnum` field.

                // ELF has this wonderful feature where, if `e_phnum` is
                // `ushort.MaxValue`, we have to seek to the first section
                // header and read its `sh_info` field to get the real,
                // 32-bit `e_phnum` value.
                if (num == ushort.MaxValue)
                {
                    stream.Position = shOff;

                    // Skip all the crap we don't care about...
                    reader.ReadUInt32(); // The `sh_name` field.
                    reader.ReadUInt32(); // The `sh_type` field.
                    reader.ReadUInt32(); // The `sh_flags` field.
                    reader.ReadUInt32(); // The `sh_addr` field.
                    reader.ReadUInt32(); // The `sh_offset` field.
                    reader.ReadUInt32(); // The `sh_size` field.
                    reader.ReadUInt32(); // The `sh_info` field.

                    // And finally, the `sh_info` field that we want. At
                    // this point, we just stop reading the section header
                    // and skip right ahead to the program headers (below).
                    phOff = reader.ReadUInt32();
                }

                stream.Position = phOff;

                for (var i = 0; i < num; i++)
                {
                    reader.ReadUInt32(); // The `p_type` field.
                    var offset = reader.ReadUInt32(); // The `p_offset` field.
                    var vaddr = reader.ReadUInt32(); // The `p_vaddr` field.
                    reader.ReadUInt32(); // The `p_paddr` field.
                    var size = reader.ReadUInt32(); // The `p_filesz` field.
                    reader.ReadUInt32(); // The `p_memsz` field.
                    reader.ReadUInt32(); // The `p_flags` field.
                    reader.ReadUInt32(); // The `p_align` field.

                    bool local;
                    bool chip;

                    if ((vaddr & 0xFFF00000) != 0)
                    {
                        // Global address.
                        local = false;
                        chip = IsOnChip(core.Machine, vaddr);
                    }
                    else
                    {
                        // Local address.
                        local = true;
                        chip = true;
                    }

                    Core destCore;
                    uint dest;

                    if (local)
                    {
                        // It's on the current core.
                        destCore = core;
                        dest = vaddr;
                    }
                    else if (chip)
                    {
                        // It's on some other core.
                        uint raw;

                        var id = CoreId.FromAddress(vaddr, out raw);

                        destCore = core.Machine.Grid[id.Row][id.Column];
                        dest = raw;
                    }
                    else
                    {
                        // It's on external memory.
                        destCore = core;
                        dest = vaddr;

                        // If it's a physical address, translate it to a
                        // device-side virtual address that lies within the
                        // external memory region. `0x1E000000` is the start
                        // of the external memory region as seen by the host
                        // on the Parallella boards.
                        if (!(vaddr >= Memory.ExternalBaseAddress &&
                            vaddr < Memory.ExternalBaseAddress + core.Machine.Memory.Size))
                            dest += Memory.ExternalBaseAddress - 0x1E000000;
                    }

                    var pos = stream.Position;

                    stream.Position = offset;

                    // Now read in the machine code.
                    for (var j = 0; j < size; j++)
                        core.Machine.Memory.Write(destCore, (uint)(dest + j), reader.ReadByte());

                    // Jump back and continue reading program headers.
                    stream.Position = pos;
                }
            }
        }
    }
}
