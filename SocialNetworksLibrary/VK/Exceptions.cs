using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VK
{
    class VKException : Exception
    {
        public VKException() : base()
        {

        }

        public VKException(string message) : base(message)
        {

        }
        public override string ToString()
        {
            return base.ToString();
        }
    }
}
