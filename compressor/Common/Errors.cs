using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace compressor.Common
{
    class Errors
    {
        readonly LinkedList<ExceptionDispatchInfo> Exceptions = new LinkedList<ExceptionDispatchInfo>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(Exception e)
        {
            lock(this.Exceptions)
            {
                this.Exceptions.AddLast(ExceptionDispatchInfo.Capture(e));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Throw(string message)
        {
            Exception[] exceptions;
            lock(this.Exceptions)
            {
                exceptions = this.Exceptions.Select(x => x.SourceException).ToArray();
            }
            if(exceptions.Length > 0)
            {
                if(!string.IsNullOrEmpty(message))
                    throw new AggregateException(message, exceptions);
                else
                    throw new AggregateException(exceptions);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Throw()
        {
            Throw(null);
        }
    }
}