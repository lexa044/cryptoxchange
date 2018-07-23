using System;

namespace CryptoXchange.Models
{
    public class ServiceResponse<T> : ServiceResponse
    {
        public T Result { get; internal set; }
    }

    public class ServiceResponse
    {
        public bool HasError { get; internal set; }
        public Exception Exception { get; internal set; }
        public string ReferenceNumber { get; set; }
    }
}
