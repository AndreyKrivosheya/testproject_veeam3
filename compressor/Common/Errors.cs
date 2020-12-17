using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace compressor.Common
{
    class Errors
    {
        readonly LinkedList<ExceptionDispatchInfo> ExceptionInfos = new LinkedList<ExceptionDispatchInfo>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(Exception e)
        {
            lock(this.ExceptionInfos)
            {
                this.ExceptionInfos.AddLast(ExceptionDispatchInfo.Capture(e));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Throw(string message)
        {
            ExceptionDispatchInfo[] errors;
            lock(this.ExceptionInfos)
            {
                errors = this.ExceptionInfos.ToArray();
            }
            if(errors.Length > 0)
            {
                if(errors.Length > 1)
                {
                    var exceptions = errors.Select(e => e.SourceException);
                    if(!string.IsNullOrEmpty(message))
                        throw new AggregateException(message, exceptions);
                    else
                        throw new AggregateException(exceptions);
                }
                else
                {
                    errors[0].Throw();
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Throw()
        {
            Throw(null);
        }
    }
}