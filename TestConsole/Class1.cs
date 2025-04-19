using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
{
    public class Class1
    {
        protected ValueSet _ValueSet = new ValueSet { };
        protected  class ValueSet
        {
            public int A01 { get; internal set; } = 0;
            public int A02 { get; internal set; } = 1;
        }

        public virtual void Test()
        {
            Console.WriteLine($"{_ValueSet.A01} = {(int)_ValueSet.A01}");
            DateTime date = DateTime.Today;
        }
    }
}
