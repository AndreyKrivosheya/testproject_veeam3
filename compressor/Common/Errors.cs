using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace compressor.Common
{
    class Errors
    {
        readonly List<ExceptionDispatchInfo> ExceptionInfos = new List<ExceptionDispatchInfo>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(Exception e)
        {
            lock(this.ExceptionInfos)
            {
                this.ExceptionInfos.Add(ExceptionDispatchInfo.Capture(e));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Throw(string message)
        {
            List<ExceptionDispatchInfo> errors;
            lock(this.ExceptionInfos)
            {
                errors = new List<ExceptionDispatchInfo>(this.ExceptionInfos);
                this.ExceptionInfos.Clear();
            }
            if(errors.Count > 0)
            {
                if(errors.Count > 1)
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