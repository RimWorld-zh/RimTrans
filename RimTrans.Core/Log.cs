using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using log4net;
using log4net.Config;
using log4net.Repository;

namespace RimTrans.Core {
    /// <summary>
    /// 
    /// </summary>
    public static class Log {

        static Log() {
            ILoggerRepository repository = LogManager.CreateRepository("RimTransRepository");
            // 默认简单配置，输出至控制台
            BasicConfigurator.Configure(repository);
            logger = LogManager.GetLogger(repository.Name, "RimTrans");
            //logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        private static log4net.ILog logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static  void Debug(string message, Exception exception = null) {
            logger.Info(message, exception);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static  void Info(string message, Exception exception = null) {
            logger.Info(message, exception);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static  void Warn(string message, Exception exception = null) {
            logger.Info(message, exception);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static  void Error(string message, Exception exception = null) {
            logger.Info(message, exception);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static  void Fatal(string message, Exception exception = null) {
            logger.Info(message, exception);
        }
    }
}
