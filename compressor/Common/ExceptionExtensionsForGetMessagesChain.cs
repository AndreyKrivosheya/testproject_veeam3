using System;
using System.IO;
using System.Linq;

namespace compressor.Common
{
    static class ExceptionExtensionsForGetMessagesChain
    {
        static string GetMessagesChainFromAggregate(this AggregateException e)
        {
            if(e.InnerExceptions.Count > 0)
            {
                return string.Format("[{0}]", string.Join(", ", e.InnerExceptions.Select(x => string.Format("{{{0}}}", x.GetMessagesChain()))));
            }
            else
            {
                return e.Message;
            }
        }
        static string GetMessagesChainFromOrdinary(this Exception e)
        {
            if(e.InnerException != null)
            {
                if(!string.IsNullOrEmpty(e.Message))
                {
                    return string.Format("{0} => {1}", e.Message, e.InnerException.GetMessagesChain());
                }
                else
                {
                    return e.InnerException.GetMessagesChain();
                }
            }
            else
            {
                return e.Message;
            }
        }

        public static string GetMessagesChain(this Exception e)
        {
            var eAsAggregate = e as AggregateException;
            if(eAsAggregate != null)
            {
                return eAsAggregate.GetMessagesChainFromAggregate();
            }
            else
            {
                return e.GetMessagesChainFromOrdinary();
            }
        }
    }
}
