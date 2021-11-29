using LateCat.PoseidonEngine.Core;
using System;
using System.Diagnostics;

namespace LateCat.Core
{
    public class ProcessSuspend
    {
        public static void SuspendAllThreads(ExtPrograms obj)
        {
            try
            {
                foreach (ProcessThread thread in obj.Proc.Threads)
                {
                    var pOpenThread = Win32.OpenThread(Win32.ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
                    if (pOpenThread == IntPtr.Zero)
                    {
                        break;
                        // continue;
                    }

                    if (obj.SuspendCnt == 0)
                        obj.SuspendCnt = Win32.SuspendThread(pOpenThread);

                    Win32.CloseHandle(pOpenThread);
                }
            }
            catch
            {
                //pgm unexpected ended etc, ignore; setupdesktop class will dispose it once ready.
            }
        }

        public static void ResumeAllThreads(ExtPrograms obj)
        {
            try
            {
                foreach (ProcessThread thread in obj.Proc.Threads)
                {
                    var pOpenThread = Win32.OpenThread(Win32.ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
                    if (pOpenThread == IntPtr.Zero)
                    {
                        break;
                        //  continue;
                    }

                    do
                    {
                        obj.SuspendCnt = (uint)Win32.ResumeThread(pOpenThread);
                    } while (obj.SuspendCnt > 0);

                    Win32.CloseHandle(pOpenThread);
                }
            }
            catch
            {

            }
        }

    }
}
