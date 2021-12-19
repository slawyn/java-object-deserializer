using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/****************************
 * 
 *  Java Class
 * 
 ****************************/
namespace Loader.Application
{
    class JavaClass
    {
        string mClassName;
        byte mClassDescFlags;
        UInt64 mSerialVersionUID;
        Dictionary<string, string> mFields;
        List<JavaClass> mSuperClasses;


        public JavaClass(string classname, UInt64 serialversion)
        {

            mFields = new Dictionary<string, string>();
            mSuperClasses = new List<JavaClass>();
            mClassName = classname;
            mSerialVersionUID = serialversion;
        }

        public void AddField(string fieldname, string type)
        {
            if (!mFields.ContainsKey(fieldname))
                mFields.Add(fieldname, type);
            else
                throw new FormatException(String.Format("Field Repeated {0}", fieldname));
        }

        public void AddSuperClass(JavaClass javaclass)
        {
            if(javaclass != null)
            {
                mSuperClasses.Add(javaclass);
            }
        }

        public byte GetClassDescFlags()
        {
            return mClassDescFlags;
        }

        public void SetClassDescFlags(byte classsdescflags)
        {
            mClassDescFlags = classsdescflags;
        }

        public List<JavaClass> GetSuperClasses()
        {
            return mSuperClasses;
        }

        public Dictionary<string, string> GetFields()
        {
            return mFields;
        }

        public string GetClassName()
        {
            return mClassName;
        }
    }
}
