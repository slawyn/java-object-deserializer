using System;
using System.Collections.Generic;
using System.Linq;

/****************************
 * 
 *  Java Object
 * 
 ****************************/
namespace Loader.Application
{
    class JavaPrimitive: JavaVariable
    {
        string mValue;
        public JavaPrimitive(string value)
        {
            mValue = value;
        }

    }
}
