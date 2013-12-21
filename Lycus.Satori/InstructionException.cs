using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Lycus.Satori
{
    [Serializable]
    public class InstructionException : Exception
    {
        [CLSCompliant(false)]
        public uint? Value { get; private set; }

        public bool Is16Bit { get; private set; }

        public InstructionException()
        {
        }

        public InstructionException(string message)
            : base(message)
        {
        }

        public InstructionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        [CLSCompliant(false)]
        public InstructionException(string message, uint value, bool is16Bit)
            : base(message)
        {
            Value = value;
            Is16Bit = is16Bit;
        }

        [CLSCompliant(false)]
        public InstructionException(string message, uint value, bool is16Bit, Exception innerException)
            : base(message, innerException)
        {
            Value = value;
            Is16Bit = is16Bit;
        }

        protected InstructionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Value = (uint?)info.GetValue("Value", typeof(uint?));
            Is16Bit = info.GetBoolean("Is16Bit");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            info.AddValue("Value", Value);
            info.AddValue("Is16Bit", Is16Bit);

            base.GetObjectData(info, context);
        }
    }
}
