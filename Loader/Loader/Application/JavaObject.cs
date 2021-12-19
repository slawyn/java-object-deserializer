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
    class JavaObject:JavaVariable
    {
        Dictionary<string, JavaVariable> mClassData;
        JavaClass mBaseClass;

        public JavaObject(JavaClass baseclass)
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
