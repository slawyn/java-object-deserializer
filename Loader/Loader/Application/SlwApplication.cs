using Loader.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/****************************
 * 
 *  Container for Java Classes, References and Objects
 *  - Base Objects are saved in mJavaObjects
 * 
 ****************************/
namespace Loader
{
    class SlwApplication
    {
        SortedDictionary<UInt32, JavaClass> mJavaClasses;
        Dictionary<UInt32, JavaVariable> mJavaVariables;
        Dictionary<UInt32, string> mReferencedStrings;
        List<JavaObject> mJavaObjects;

        public SlwApplication() 
        {
            mJavaClasses = new SortedDictionary<UInt32, JavaClass>();
            mJavaVariables = new Dictionary<UInt32, JavaVariable>();
            mReferencedStrings = new Dictionary<UInt32,string>();
            mJavaObjects = new List<JavaObject>();

        }

        public void PrintClasses()
        {
            Console.WriteLine("# Classes");
            int index = 0;
            foreach(var item in mJavaClasses)
            {
                Console.WriteLine(String.Format("[{0}] {1}", index++, item.Value.GetClassName()));
            }
        }

        public void AddJavaObject(JavaObject javaobject)
        {
            if (javaobject != null)
            {
                mJavaObjects.Add(javaobject);
            }
        }


        public void CreateStringReference(UInt32 handle, string referredstring)
        {
            mReferencedStrings.Add(handle, referredstring);
        }

        public string GetStringByHandle(UInt32 handle) 
        {
            string type;
            mReferencedStrings.TryGetValue(handle, out type);
            return type;
        }

        public JavaClass GetClassByHandle(UInt32 handle)
        {
            JavaClass jc;
            mJavaClasses.TryGetValue(handle, out jc);

            if (jc == null)
            {
                throw new FormatException(String.Format("Class Handle Not Found {0}", handle));
            }
            return jc;
        }

        public JavaVariable GetVariableByHandle(UInt32 handle)
        {
            JavaVariable jo;
            mJavaVariables.TryGetValue(handle, out jo);

            if (jo == null)
            {
                throw new FormatException(String.Format("Object Handle Not Found {0}", handle));
            }
            return jo;
        }

        public JavaObject CreateObject(UInt32 handle, JavaClass baseclass)
        {
            JavaObject jo = null;
            if (!mJavaVariables.ContainsKey(handle))
            {
                jo = new JavaObject(baseclass);
                mJavaVariables.Add(handle, jo);
            }
            else
            {
                throw new FormatException(String.Format("Object Handle Repeated {0}", handle));
            }
            return jo;
        }
        public JavaArray CreateArray(UInt32 handle, JavaClass baseclass)
        {
            JavaArray jo = null;
            if (!mJavaVariables.ContainsKey(handle))
            {
                jo = new JavaArray(baseclass);
                mJavaVariables.Add(handle, jo);
            }
            else
            {
                throw new FormatException(String.Format("Object Handle Repeated {0}", handle));
            }
            return jo;
        }

        public JavaPrimitive CreatePrimitive(UInt32 handle, string value)
        {
            JavaPrimitive jp = null;
            if (!mJavaVariables.ContainsKey(handle))
            {
                jp = new JavaPrimitive(value);
                mJavaVariables.Add(handle, jp);
            }
            else
            {
                throw new FormatException(String.Format("Object Handle Repeated {0}", handle));
            }
            return jp;
        }



        public JavaClass CreateClass(UInt32 handle, string classname, UInt64 serialversion)
        {
            JavaClass jc = null;
            if (!mJavaClasses.ContainsKey(handle)) { 
                jc = new JavaClass(classname, serialversion);


                mJavaClasses.Add(handle, jc);
                CreateStringReference(handle, classname);
            }
            else
            {
                throw new FormatException(String.Format("Class Handle Repeated {0}", handle));
            }

            return jc;
        }

    }
}
