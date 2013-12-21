namespace Lycus.Satori.Kernels
{
    /// <summary>
    /// A kernel that supports nothing.
    /// </summary>
    public class NullKernel : Kernel
    {
        public override Capabilities Capabilities
        {
            get { return Capabilities.None; }
        }
    }
}
