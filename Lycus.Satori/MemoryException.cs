using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Lycus.Satori
{
    [Serializable]
    public class MemoryException : Exception
    {
        [CLSCompliant(false)]
        public uint? Address { get; private set; }

        public bool Write { get; private set; }

        public MemoryException()
        {
        }

        public MemoryException(string message)
            : base(message)
        {
        }

        public MemoryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        [CLSCompliant(false)]
        public MemoryException(string message, uint address, bool write)
            : base(message)
        {
            Address = address;
            Write = write;
        }

        [CLSCompliant(false)]
        public MemoryException(string message, uint address, bool write, Exception innerException)
            : base(message, innerException)
        {
            Address = address;
            Write = write;
        }

        protected MemoryException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Address = (uint?)info.GetValue("Address", typeof(uint?));
            Write = info.GetBoolean("Write");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            info.AddValue("Address", Address);
            info.AddValue("Write", Write);

            base.GetObjectData(info, context);
        }
    }
}
