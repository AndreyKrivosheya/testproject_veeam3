using System;
using System.Collections.Generic;
using System.Linq;

namespace compressor.Common
{
    static class ArrayExtensionsForIndexOf
    {
        public static IEnumerable<int> IndexOf<T>(this T[] x, T[] y, int start)
        {
            if(start > x.Length - y.Length - 1)
            {
                return Enumerable.Empty<int>();
            }
            else
            {
                var index = Enumerable.Range(start, (x.Length - y.Length) - start);
                for (int i_tmp = 0; i_tmp < y.Length; ++i_tmp)
                {
                    var i = i_tmp;
                    index = index.Where(n => {
                        if(x[n + i] == null && y[i] == null)
                        {
                            return true;
                        }
                        else if(x[n + i] != null && y[i] != null)
                        {
                            return x[n + i].Equals(y[i]);
                        }
                        else
                        {
                            return false;
                        }
                    });
                }
                return index;
            }
        }

        public static IEnumerable<int> IndexOf<T>(this T[] x, T[] y)
        {
            return x.IndexOf(y, 0);
        }
    }
}
