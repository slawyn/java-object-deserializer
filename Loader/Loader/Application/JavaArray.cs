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
    class JavaArray: JavaVariable
    {

        Dictionary<string, JavaVariable> mClassData;
        JavaClass mBaseClass;

        public JavaArray(JavaClass baseclass)
        {
            mClassData = new Dictionary<string, JavaVariable>();
            mBaseClass = baseclass;
        }

        public void AddValue(string fieldname, JavaVariable value)
        {
            mClassData.Add(fieldname, value);
        }

    }
}
