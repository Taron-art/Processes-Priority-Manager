using System.Diagnostics;
using Cocona;
using Cpu_affinity;

#if DEBUG
Debugger.Launch();
#endif

CoconaLiteApp.Run<ApplicationRunner>(args);
