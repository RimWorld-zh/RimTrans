using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimTrans.Core {
    /// <summary>
    /// 
    /// </summary>
    public static class Log {

        /// <summary>
        /// Initialize Log, add Console.WriteLine to each handlers.
        /// </summary>
        public static void Initialize() {
        }

        #region Debug

        /// <summary>
        /// 
        /// </summary>
        public static readonly List<Func<string, Exception, Task>> DebugHandlers = new List<Func<string, Exception, Task>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static async void Debug(string message, Exception exception = null) {
            await Task.WhenAll(DebugHandlers.Select(h => h(message, exception)));
        }

        #endregion

        #region Info

        /// <summary>
        /// 
        /// </summary>
        public static readonly List<Func<string, Exception, Task>> InfoHandlers = new List<Func<string, Exception, Task>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static async void Info(string message, Exception exception = null) {
            await Task.WhenAll(InfoHandlers.Select(h => h(message, exception)));
        }

        #endregion

        #region Warning

        /// <summary>
        /// 
        /// </summary>
        public static readonly List<Func<string, Exception, Task>> WarnHandlers = new List<Func<string, Exception, Task>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static async void Warn(string message, Exception exception = null) {
            await Task.WhenAll(WarnHandlers.Select(h => h(message, exception)));
        }

        #endregion

        #region Error

        /// <summary>
        /// 
        /// </summary>
        public static readonly List<Func<string, Exception, Task>> ErrorHandlers = new List<Func<string, Exception, Task>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static async void Error(string message, Exception exception = null) {
            await Task.WhenAll(ErrorHandlers.Select(h => h(message, exception)));
        }

        #endregion

        #region Error

        /// <summary>
        /// 
        /// </summary>
        public static readonly List<Func<string, Exception, Task>> FatalHandlers = new List<Func<string, Exception, Task>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static async void Fatal(string message, Exception exception = null) {
            await Task.WhenAll(FatalHandlers.Select(h => h(message, exception)));
        }

        #endregion
    }
}
