using System;
using System.ComponentModel;
using System.Runtime.ExceptionServices;

namespace Affinity_manager.Exceptions
{
    public class ServiceNotInstalledException : Exception
    {
        private const int ERROR_SERVICE_DOES_NOT_EXIST = 1060;

        public ServiceNotInstalledException(string serviceName)
            : base(string.Format("Service {0} is not installed", serviceName))
        {
        }

        public ServiceNotInstalledException(string? serviceName, Exception? innerException)
            : base(string.Format("Service {0} is not installed", serviceName), innerException)
        {
        }


        public static void ThrowFromInvalidOperationException(InvalidOperationException e, string serviceName)
        {
            if (e.InnerException is Win32Exception inner && inner.NativeErrorCode == ERROR_SERVICE_DOES_NOT_EXIST)
            {
                throw new ServiceNotInstalledException(serviceName, e);
            }

            ExceptionDispatchInfo.Throw(e);
        }
    }
}
