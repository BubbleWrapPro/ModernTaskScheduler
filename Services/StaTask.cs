using System;
using System.Threading;
using System.Threading.Tasks;

namespace ModernTaskScheduler.Services
{
    /// <summary>
    /// Helper to execute TaskScheduler COM operations in an isolated Single-Threaded Apartment (STA) thread.
    /// This prevents REGDB_E_IIDNOTREG (0x80040155) caused by cross-apartment COM interface marshalling in .NET 8.
    /// </summary>
    public static class StaTask
    {
        public static Task<T> Run<T>(Func<T> func)
        {
            var tcs = new TaskCompletionSource<T>();
            var thread = new Thread(() =>
            {
                try
                {
                    var result = func();
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            })
            {
                IsBackground = true
            };

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        public static Task Run(Action action)
        {
            var tcs = new TaskCompletionSource<bool>();
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            })
            {
                IsBackground = true
            };

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }
    }
}
