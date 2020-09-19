using System;
namespace LWNet
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    [Obsolete]
    public class LWRPC : Attribute { }

    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    [Obsolete]
    public class FastLWRPC : Attribute { }

    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class LWFastRPC : Attribute
    {
        public string MethodCode { get; private set; }

        /// <summary>
        /// code representing this rpc method
        /// </summary>
        /// <param name="methodCode">Method code for representing this RPC method</param>
        public LWFastRPC(string methodCode)
        {
            this.MethodCode = methodCode;
        }
    }
}
