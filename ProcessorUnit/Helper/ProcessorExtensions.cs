using ModelCore.Schema.TXN;
using ProcessorUnit.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessorUnit.Helper
{
    public static class ProcessorExtensions
    {
        public static  Root CreateMessageToken(this ExecutorForeverBase executor)
        {
            Root result = new Root
            {
                UXB2B = "電子發票系統",
                Result = new RootResult
                {
                    timeStamp = DateTime.Now,
                    value = 0
                }
            };
            return result;
        }
    }
}
