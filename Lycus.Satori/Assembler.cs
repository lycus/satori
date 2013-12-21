using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Lycus.Satori.Instructions;

namespace Lycus.Satori
{
    public class Assembler : IDisposable
    {
        readonly BinaryWriter _writer;

        bool _disposed;

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public Assembler()
        {
            _writer = new BinaryWriter(new MemoryStream());
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _disposed = true;

            _writer.Dispose();
        }

        ~Assembler()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        public byte[] Copy()
        {
            return ((MemoryStream)_writer.BaseStream).ToArray();
        }

        protected void Emit(Instruction instruction)
        {
            if (instruction == null)
                throw new ArgumentNullException("instruction");

            instruction.Encode();

            if (instruction.Is16Bit)
                _writer.Write((ushort)instruction.Value);
            else
                _writer.Write(instruction.Value);
        }

        public void Branch(int immediate, bool is16Bit = false)
        {
            Branch(ConditionCode.Unconditional, immediate, is16Bit);
        }

        public void Branch(ConditionCode condition, int immediate, bool is16Bit = false)
        {
            Emit(new BranchInstruction(is16Bit)
            {
                Condition = condition,
                Immediate = immediate,
            });
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        public void Move(int rd, int rn, bool is16Bit = false)
        {
            Move(ConditionCode.Unconditional, rd, rn, is16Bit);
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        public void Move(ConditionCode condition, int rd, int rn, bool is16Bit = false)
        {
            Emit(new MoveInstruction(is16Bit)
            {
                Condition = condition,
                SourceRegister = rn,
                DestinationRegister = rd,
            });
        }
    }
}
