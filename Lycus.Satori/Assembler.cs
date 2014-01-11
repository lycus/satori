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

        public MemoryStream Stream
        {
            get { return (MemoryStream)_writer.BaseStream; }
        }

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

        protected T Emit<T>(T instruction)
            where T : Instruction
        {
            if (instruction == null)
                throw new ArgumentNullException("instruction");

            instruction.Encode();
            instruction.Check();

            if (instruction.Is16Bit)
                _writer.Write((ushort)instruction.Value);
            else
                _writer.Write(instruction.Value);

            return instruction;
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        public AddImmediateInstruction AddImmediate(int rd, int rn, int immediate, bool is16Bit = false)
        {
            return Emit(new AddImmediateInstruction(is16Bit)
            {
                SourceRegister = rn,
                DestinationRegister = rd,
                Immediate = immediate,
            });
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rm")]
        public AddInstruction Add(int rd, int rn, int rm, bool is16Bit = false)
        {
            return Emit(new AddInstruction(is16Bit)
            {
                SourceRegister = rn,
                OperandRegister = rm,
                DestinationRegister = rd,
            });
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rm")]
        public AndInstruction And(int rd, int rn, int rm, bool is16Bit = false)
        {
            return Emit(new AndInstruction(is16Bit)
            {
                SourceRegister = rn,
                OperandRegister = rm,
                DestinationRegister = rd,
            });
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        public ArithmeticShiftRightImmediateInstruction ArithmeticShiftRightImmediate(
            int rd, int rn, int immediate, bool is16Bit = false)
        {
            return Emit(new ArithmeticShiftRightImmediateInstruction(is16Bit)
            {
                SourceRegister = rn,
                DestinationRegister = rd,
                Immediate = immediate,
            });
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rm")]
        public ArithmeticShiftRightInstruction ArithmeticShiftRight(int rd, int rn, int rm, bool is16Bit = false)
        {
            return Emit(new ArithmeticShiftRightInstruction(is16Bit)
            {
                SourceRegister = rn,
                OperandRegister = rm,
                DestinationRegister = rd,
            });
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        public BitReverseInstruction BitReverse(int rd, int rn, int immediate, bool is16Bit = false)
        {
            return Emit(new BitReverseInstruction(is16Bit)
            {
                SourceRegister = rn,
                DestinationRegister = rd,
                Immediate = immediate,
            });
        }

        public BranchInstruction Branch(int immediate, bool is16Bit = false)
        {
            return Branch(ConditionCode.Unconditional, immediate, is16Bit);
        }

        public BranchInstruction Branch(ConditionCode condition, int immediate, bool is16Bit = false)
        {
            return Emit(new BranchInstruction(is16Bit)
            {
                Condition = condition,
                Immediate = immediate,
            });
        }

        public BreakpointInstruction Breakpoint()
        {
            return Emit(new BreakpointInstruction());
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rm")]
        public ExclusiveOrInstruction ExclusiveOr(int rd, int rn, int rm, bool is16Bit = false)
        {
            return Emit(new ExclusiveOrInstruction(is16Bit)
            {
                SourceRegister = rn,
                OperandRegister = rm,
                DestinationRegister = rd,
            });
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        public FixInstruction Fix(int rd, int rn, bool is16Bit = false)
        {
            return Emit(new FixInstruction(is16Bit)
            {
                SourceRegister = rn,
                OperandRegister = rn,
                DestinationRegister = rd,
            });
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        public FloatAbsoluteInstruction FloatAbsolute(int rd, int rn, bool is16Bit = false)
        {
            return Emit(new FloatAbsoluteInstruction(is16Bit)
            {
                SourceRegister = rn,
                OperandRegister = rn,
                DestinationRegister = rd,
            });
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rm")]
        public FloatAddInstruction FloatAdd(int rd, int rn, int rm, bool is16Bit = false)
        {
            return Emit(new FloatAddInstruction(is16Bit)
            {
                SourceRegister = rn,
                OperandRegister = rm,
                DestinationRegister = rd,
            });
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rm")]
        public FloatMultiplyAddInstruction FloatMultiplyAdd(int rd, int rn, int rm, bool is16Bit = false)
        {
            return Emit(new FloatMultiplyAddInstruction(is16Bit)
            {
                SourceRegister = rn,
                OperandRegister = rm,
                DestinationRegister = rd,
            });
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rm")]
        public FloatMultiplyInstruction FloatMultiply(int rd, int rn, int rm, bool is16Bit = false)
        {
            return Emit(new FloatMultiplyInstruction(is16Bit)
            {
                SourceRegister = rn,
                OperandRegister = rm,
                DestinationRegister = rd,
            });
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rm")]
        public FloatMultiplySubtractInstruction FloatMultiplySubtract(int rd, int rn, int rm, bool is16Bit = false)
        {
            return Emit(new FloatMultiplySubtractInstruction(is16Bit)
            {
                SourceRegister = rn,
                OperandRegister = rm,
                DestinationRegister = rd,
            });
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rm")]
        public FloatSubtractInstruction FloatSubtract(int rd, int rn, int rm, bool is16Bit = false)
        {
            return Emit(new FloatSubtractInstruction(is16Bit)
            {
                SourceRegister = rn,
                OperandRegister = rm,
                DestinationRegister = rd,
            });
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        public FloatInstruction Float(int rd, int rn, bool is16Bit = false)
        {
            return Emit(new FloatInstruction(is16Bit)
            {
                SourceRegister = rn,
                OperandRegister = rn,
                DestinationRegister = rd,
            });
        }

        public GlobalInterruptDisableInstruction GlobalInterruptDisable()
        {
            return Emit(new GlobalInterruptDisableInstruction());
        }

        public GlobalInterruptEnableInstruction GlobalInterruptEnable()
        {
            return Emit(new GlobalInterruptEnableInstruction());
        }

        public IdleInstruction Idle()
        {
            return Emit(new IdleInstruction());
        }

        public JumpInstruction Jump(int rd, bool is16Bit = false)
        {
            return Emit(new JumpInstruction(is16Bit)
            {
                SourceRegister = rd,
            });
        }

        public JumpLinkInstruction JumpLink(int rd, bool is16Bit = false)
        {
            return Emit(new JumpLinkInstruction(is16Bit)
            {
                SourceRegister = rd,
            });
        }

        [CLSCompliant(false)]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        public LoadDisplacementInstruction LoadDisplacement(Size size, int rd,
            int rn, uint immediate, bool subtract, bool is16Bit = false)
        {
            return Emit(new LoadDisplacementInstruction(is16Bit)
            {
                Size = size,
                DestinationRegister = rd,
                SourceRegister = rn,
                Immediate = immediate,
                Subtract = subtract,
            });
        }

        [CLSCompliant(false)]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rm")]
        public LoadIndexInstruction LoadIndex(Size size, int rd,
            int rn, int rm, bool subtract, bool is16Bit = false)
        {
            return Emit(new LoadIndexInstruction(is16Bit)
            {
                Size = size,
                DestinationRegister = rd,
                SourceRegister = rn,
                OperandRegister = rm,
                Subtract = subtract,
            });
        }

        [CLSCompliant(false)]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        public LoadPostModifyImmediateInstruction LoadPostModifyImmediate(Size size, int rd,
            int rn, uint immediate, bool subtract, bool is16Bit = false)
        {
            return Emit(new LoadPostModifyImmediateInstruction(is16Bit)
            {
                Size = size,
                DestinationRegister = rd,
                SourceRegister = rn,
                Immediate = immediate,
                Subtract = subtract,
            });
        }

        [CLSCompliant(false)]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rm")]
        public LoadPostModifyInstruction LoadPostModify(Size size, int rd, int rn, int rm, bool subtract)
        {
            return Emit(new LoadPostModifyInstruction
            {
                Size = size,
                DestinationRegister = rd,
                SourceRegister = rn,
                OperandRegister = rm,
                Subtract = subtract,
            });
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        public LogicalShiftLeftImmediateInstruction LogicalShiftLeftImmediate(
            int rd, int rn, int immediate, bool is16Bit = false)
        {
            return Emit(new LogicalShiftLeftImmediateInstruction(is16Bit)
            {
                SourceRegister = rn,
                DestinationRegister = rd,
                Immediate = immediate,
            });
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rm")]
        public LogicalShiftLeftInstruction LogicalShiftLeft(int rd, int rn, int rm, bool is16Bit = false)
        {
            return Emit(new LogicalShiftLeftInstruction(is16Bit)
            {
                SourceRegister = rn,
                OperandRegister = rm,
                DestinationRegister = rd,
            });
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        public LogicalShiftRightImmediateInstruction LogicalShiftRightImmediate(
            int rd, int rn, int immediate, bool is16Bit = false)
        {
            return Emit(new LogicalShiftRightImmediateInstruction(is16Bit)
            {
                SourceRegister = rn,
                DestinationRegister = rd,
                Immediate = immediate,
            });
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rm")]
        public LogicalShiftRightInstruction LogicalShiftRight(int rd, int rn, int rm, bool is16Bit = false)
        {
            return Emit(new LogicalShiftRightInstruction(is16Bit)
            {
                SourceRegister = rn,
                OperandRegister = rm,
                DestinationRegister = rd,
            });
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        public MoveFromSystemInstruction MoveFromSystem(RegisterGroup group, int rd, int rn, bool is16Bit = false)
        {
            return Emit(new MoveFromSystemInstruction(is16Bit)
            {
                RegisterGroup = group,
                SourceRegister = rn,
                DestinationRegister = rd,
            });
        }

        [CLSCompliant(false)]
        public MoveHighImmediateInstruction MoveHighImmediate(int rd, uint immediate)
        {
            return Emit(new MoveHighImmediateInstruction
            {
                DestinationRegister = rd,
                Immediate = immediate,
            });
        }

        [CLSCompliant(false)]
        public MoveImmediateInstruction MoveImmediate(int rd, uint immediate, bool is16Bit = false)
        {
            return Emit(new MoveImmediateInstruction(is16Bit)
            {
                DestinationRegister = rd,
                Immediate = immediate,
            });
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        public MoveInstruction Move(ConditionCode condition, int rd, int rn, bool is16Bit = false)
        {
            return Emit(new MoveInstruction(is16Bit)
            {
                Condition = condition,
                SourceRegister = rn,
                DestinationRegister = rd,
            });
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        public MoveInstruction Move(int rd, int rn, bool is16Bit = false)
        {
            return Move(ConditionCode.Unconditional, rd, rn, is16Bit);
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        public MoveToSystemInstruction MoveToSystem(RegisterGroup group, int rd, int rn, bool is16Bit = false)
        {
            return Emit(new MoveToSystemInstruction(is16Bit)
            {
                RegisterGroup = group,
                SourceRegister = rn,
                DestinationRegister = rd,
            });
        }

        public MulticoreBreakpointInstruction MulticoreBreakpoint()
        {
            return Emit(new MulticoreBreakpointInstruction());
        }

        public NoOperationInstruction NoOperation()
        {
            return Emit(new NoOperationInstruction());
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rm")]
        public OrInstruction Or(int rd, int rn, int rm, bool is16Bit = false)
        {
            return Emit(new OrInstruction(is16Bit)
            {
                SourceRegister = rn,
                OperandRegister = rm,
                DestinationRegister = rd,
            });
        }

        public ReturnInterruptInstruction ReturnInterrupt()
        {
            return Emit(new ReturnInterruptInstruction());
        }

        public SoftwareInterruptInstruction SoftwareInterrupt()
        {
            return Emit(new SoftwareInterruptInstruction());
        }

        [CLSCompliant(false)]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        public StoreDisplacementInstruction StoreDisplacement(Size size, int rd,
            int rn, uint immediate, bool subtract, bool is16Bit = false)
        {
            return Emit(new StoreDisplacementInstruction(is16Bit)
            {
                Size = size,
                DestinationRegister = rn,
                SourceRegister = rd,
                Immediate = immediate,
                Subtract = subtract,
            });
        }

        [CLSCompliant(false)]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rm")]
        public StoreIndexInstruction StoreIndex(Size size, int rd, int rn, int rm, bool subtract, bool is16Bit = false)
        {
            return Emit(new StoreIndexInstruction(is16Bit)
            {
                Size = size,
                DestinationRegister = rn,
                SourceRegister = rd,
                OperandRegister = rm,
                Subtract = subtract,
            });
        }

        [CLSCompliant(false)]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        public StorePostModifyImmediateInstruction StorePostModifyImmediate(Size size, int rd,
            int rn, uint immediate, bool subtract, bool is16Bit = false)
        {
            return Emit(new StorePostModifyImmediateInstruction(is16Bit)
            {
                Size = size,
                DestinationRegister = rn,
                SourceRegister = rd,
                Immediate = immediate,
                Subtract = subtract,
            });
        }

        [CLSCompliant(false)]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rm")]
        public StorePostModifyInstruction StorePostModify(Size size, int rd, int rn, int rm, bool subtract)
        {
            return Emit(new StorePostModifyInstruction
            {
                Size = size,
                DestinationRegister = rn,
                SourceRegister = rd,
                OperandRegister = rm,
                Subtract = subtract,
            });
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        public SubtractImmediateInstruction SubtractImmediate(int rd, int rn, int immediate, bool is16Bit = false)
        {
            return Emit(new SubtractImmediateInstruction(is16Bit)
            {
                SourceRegister = rn,
                DestinationRegister = rd,
                Immediate = immediate,
            });
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rm")]
        public SubtractInstruction Subtract(int rd, int rn, int rm, bool is16Bit = false)
        {
            return Emit(new SubtractInstruction(is16Bit)
            {
                SourceRegister = rn,
                OperandRegister = rm,
                DestinationRegister = rd,
            });
        }

        public SynchronizeInstruction Synchronize()
        {
            return Emit(new SynchronizeInstruction());
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rn")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rm")]
        public TestSetInstruction TestSet(int rd, int rn, int rm, bool subtract)
        {
            return Emit(new TestSetInstruction
            {
                Size = Size.Int32,
                SourceRegister = rn,
                OperandRegister = rm,
                DestinationRegister = rd,
                Subtract = subtract,
            });
        }

        public TrapInstruction Trap(int code)
        {
            return Emit(new TrapInstruction
            {
                Code = code,
            });
        }

        public UnimplementedInstruction Unimplemented()
        {
            return Emit(new UnimplementedInstruction());
        }

        public WiredAndInstruction WiredAnd()
        {
            return Emit(new WiredAndInstruction());
        }
    }
}
