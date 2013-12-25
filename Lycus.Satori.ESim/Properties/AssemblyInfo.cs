using System;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("e-sim")]
[assembly: AssemblyDescription("An Epiphany processor architecture simulator with true concurrency.")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany("The Lycus Foundation")]
[assembly: AssemblyProduct("Satori")]
[assembly: AssemblyCopyright("Copyright © 2013")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
