using Loader.Application;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

/************************************************************ 
 * 
 * Reference:
 * https://docs.oracle.com/javase/6/docs/platform/serialization/spec/protocol.html
 * 
 * Description:
 * - Loader is stateless and recursive.  
 * 
 * Changes: 2021-07-14  First version that can parse .slw3
 *          
 ************************************************************/

namespace Loader
{
    static class Loader
    {
        const UInt16 STREAM_MAGIC = (UInt16)0xaced;
        const UInt16 STREAM_VERSION = 5;

        const byte TC_NULL = (byte)0x70;
        const byte TC_REFERENCE = (byte)0x71;
        const byte TC_CLASSDESC = (byte)0x72;
        const byte TC_OBJECT = (byte)0x73;
        const byte TC_STRING = (byte)0x74;
        const byte TC_ARRAY = (byte)0x75;
        const byte TC_CLASS = (byte)0x76;
        const byte TC_BLOCKDATA = (byte)0x77;
        const byte TC_ENDBLOCKDATA = (byte)0x78;
        const byte TC_RESET = (byte)0x79;
        const byte TC_BLOCKDATALONG = (byte)0x7A;
        const byte TC_EXCEPTION = (byte)0x7B;
        const byte TC_LONGSTRING = (byte)0x7C;
        const byte TC_PROXYCLASSDESC = (byte)0x7D;
        const byte TC_ENUM = (byte)0x7E;

        const byte SC_WRITE_METHOD = 0x01;  // if SC_SERIALIZABLE
        const byte SC_BLOCK_DATA = 0x08;    // if SC_EXTERNALIZABLE
        const byte SC_SERIALIZABLE = 0x02;
        const byte SC_EXTERNALIZABLE = 0x04;
        const byte SC_ENUM = 0x10;

        const char PRIM_TYPECODE_BYTE = 'B';
        const char PRIM_TYPECODE_CHAR = 'C';
        const char PRIM_TYPECODE_DOUBLE = 'D';
        const char PRIM_TYPECODE_FLOAT = 'F';
        const char PRIM_TYPECODE_INTEGER = 'I';
        const char PRIM_TYPECODE_LONG = 'J';
        const char PRIM_TYPECODE_SHORT = 'S';
        const char PRIM_TYPECODE_BOOLEAN = 'Z';

        const char OBJ_TYPECODE_ARRAY = '[';
        const char OBJ_TYPECODE_OBJECT = 'L';

        const int BASE_WIRE_HANDLE = 0x7E0000;
        static UInt32 mCurrentHandle;


        /****************************
        * 
        *  Loader main
        * 
        ****************************/
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                try
                {
                    Logger.SetLoggingLevel(args[0]);
                    Loader.Load(new DataContainer(File.ReadAllBytes(args[1])));
                }
                catch (FileNotFoundException ex)
                {
                    Logger.Log(ex.Message, Logger.LOGTYPE_ERROR);
                }
                catch (ArgumentException ex)
                {
                    Logger.Log(ex.Message, Logger.LOGTYPE_ERROR);
                }
                catch (FormatException ex)
                {
                    Logger.Log(ex.Message, Logger.LOGTYPE_ERROR);
                }
                catch (EndOfStreamException ex)
                {
                    Logger.Log(ex.Message, Logger.LOGTYPE_ERROR);
                }
            }
        }


        /*****************************************
         * 
         *  Generate a new handle for the Resource
         * 
         *****************************************/
        static UInt32 GenerateNewHandle()
        {
            return mCurrentHandle++;
        }

        /*****************************************
         * 
         *  Starts the Stream Parser. Non-Reentrant
         * 
         *****************************************/
        static SlwApplication Load(DataContainer container)
        {

            UInt16 streamMagic = Helper.ConvertToUint16(container.PopBytes(2));
            UInt16 streamVersion = Helper.ConvertToUint16(container.PopBytes(2));

            SlwApplication slwApplication = new SlwApplication();
            if (STREAM_MAGIC == streamMagic)
            {
                if (STREAM_VERSION == streamVersion)
                {
                    // Init handle and execute the state machine
                    mCurrentHandle = BASE_WIRE_HANDLE;
                    while (container.GetAvailable() > 0)
                    {
                        JavaObject jo;
                        jo = ParseContent(container.PopNextControl(), slwApplication, container);
   
                        slwApplication.AddJavaObject(jo);
                     
                    }
                }
                else
                {
                    Logger.Log(String.Format("Invalid STREAM_VERSION {0}", streamVersion), Logger.LOGTYPE_ERROR);
                }
            }
            else
            {
                Logger.Log(String.Format("Invalid STREAM_MAGIC {0}", streamMagic), Logger.LOGTYPE_ERROR);
            }

            slwApplication.PrintClasses();
            return slwApplication;
        }

        /*****************************************
         * 
         *  Content Parser. Re-entrant
         * 
         *****************************************/
        static JavaObject ParseContent(byte streamcontrol, SlwApplication slwapp, DataContainer container) 
        {
            JavaArray ja;
            JavaClass jc;
            JavaObject jo = null;
            string elementString;

            switch (streamcontrol)
            {
                case TC_OBJECT:
                    jo = ParseObject(streamcontrol, slwapp, container);
                    break;
                case TC_CLASS:
                    Logger.Log("NOT IMPLEMENTED: TC_CLASS", Logger.LOGTYPE_DEBUG);
                    break;
                case TC_ARRAY:
                    ParseNewArray(streamcontrol, slwapp, container, out ja);
                    break;
                case TC_STRING:
                case TC_LONGSTRING:
                    ParseNewString(streamcontrol, slwapp, container, out elementString);
                    break;
                case TC_ENUM:
                    Logger.Log("NOT IMPLEMENTED: TC_ENUM", Logger.LOGTYPE_DEBUG);
                    break;
                case TC_PROXYCLASSDESC:
                case TC_CLASSDESC:
                    ParseNewClassDesc(streamcontrol, slwapp, container, out jc);
                    break;
                case TC_REFERENCE:
                    Logger.Log("NOT IMPLEMENTED: TC_REFERENCE", Logger.LOGTYPE_DEBUG);
                    break;
                case TC_NULL:
                    ParseNull(slwapp, container);
                    break;
                case TC_EXCEPTION:
                    Logger.Log("NOT IMPLEMENTED: TC_EXCEPTION", Logger.LOGTYPE_DEBUG);
                    break;
                case TC_RESET:
                    Logger.Log("NOT IMPLEMENTED: TC_RESET", Logger.LOGTYPE_DEBUG);
                    break;
                case TC_BLOCKDATA:
                    Logger.Log("NOT IMPLEMENTED: TC_BLOCKDATA", Logger.LOGTYPE_DEBUG);
                    break;
                case TC_BLOCKDATALONG:
                    Logger.Log("NOT IMPLEMENTED: TC_BLOCKDATALONG", Logger.LOGTYPE_DEBUG);
                    break;
                default:
                    throw new FormatException(String.Format("ParseContent: Unknown streamcontrol {0}", streamcontrol));

            }
            return jo;

        }

        /*****************************************
         * 
         *  Class Descriptor Parser. Re-entrant
         * 
         *****************************************/
        static void ParseNewClassDesc(byte streamcontrol, SlwApplication slwapp, DataContainer container, out JavaClass jc)
        {

            UInt64 serialVersionUID;
            UInt16 classNameLength;
            string className;

            switch (streamcontrol)
            {
                case TC_PROXYCLASSDESC:
                    Logger.Log("NOT IMPLEMENTED: TC_PROXYCLASSDESC", Logger.LOGTYPE_DEBUG);                    // Not Required for .slw3
                    jc = null;
                    break;

                // TC_CLASSDESC className serialVersionUID newHandle classDescInfo
                case TC_CLASSDESC:
                    classNameLength = Helper.ConvertToUint16(container.PopBytes(2));          // ClassDesc:ClassnameLength
                    className = Helper.ConvertToUTF(container.PopBytes(classNameLength));     // ClassDesc:Classname
                    serialVersionUID = Helper.ConvertToUint64(container.PopBytes(8));         // ClassDesc:SerialUid
                    jc = slwapp.CreateClass(GenerateNewHandle(), className, serialVersionUID);

                    // ClassDesc:ClassInfo
                    ParseClassDescInfo(jc, slwapp, container);
                    break;
                case TC_REFERENCE:
                    UInt32 handle = Helper.ConvertToUint32(container.PopBytes(4));
                    jc = slwapp.GetClassByHandle(handle);
                    break;
                case TC_NULL:
                    // No Class
                    Logger.Log("NewClassDesc: Null", Logger.LOGTYPE_DEBUG);
                    jc = null;
                    break;
                default:
                    throw new FormatException(String.Format("NewClassDesc: Unknown streamcontrol {0}", streamcontrol));
                  
            }
        }


        /*****************************************
         * 
         *  Class Descriptor Info. Re-entrant
         * 
         *****************************************/
        static void ParseClassDescInfo(JavaClass jc, SlwApplication slwapp, DataContainer container)
        {
            JavaClass jcsuper;
            UInt16 fieldCount;


            // classDescInfo: classDescFlags fields classAnnotation superClassDesc
            jc.SetClassDescFlags(container.PopByte());                              // ClassDesc:ClassInfo:ClassDescFlags
            fieldCount = Helper.ConvertToUint16(container.PopBytes(2));             // ClassDesc:ClassInfo:Fields:Count
            for (int fieldIndex = 0; fieldIndex < fieldCount; fieldIndex++)
            {
                // Class:ClassInfo:Fields:FieldDesc
                byte fieldType = container.PopByte();
                UInt16 fieldNameLength = Helper.ConvertToUint16(container.PopBytes(2));
                string fieldName = Helper.ConvertToUTF(container.PopBytes(fieldNameLength));
                string className1;
                switch ((char)fieldType)
                {
                    case OBJ_TYPECODE_ARRAY:
                    case OBJ_TYPECODE_OBJECT:
                        ParseNewString(container.PopNextControl(), slwapp, container, out className1);
                        jc.AddField(fieldName, className1);
                        break;
                    default:
                        jc.AddField(fieldName, (char)fieldType + "");
                        break;
                }
            }

            // Required elements
            ParseClassAnnotation(container.PopNextControl(), ref jc, slwapp, container);
            ParseSuperClassDesc(container.PopNextControl(), slwapp, container, out jcsuper);

            jc.AddSuperClass(jcsuper);
        }


        /*****************************************
        * 
        *  Read Field. Re-entrant
        * 
        *****************************************/
        static void ReadField(string complextype, SlwApplication slwapp, DataContainer container, out JavaVariable value)
        {
            char primitiveType = complextype[0];
            JavaPrimitive jp;
            switch (primitiveType)
            {
                case PRIM_TYPECODE_BYTE:
                    value = new JavaPrimitive(Helper.ConvertToHexByte(container.PopBytes(1)));
                    break;
                case PRIM_TYPECODE_CHAR:
                    value = new JavaPrimitive(Helper.ConvertToChar(container.PopByte()).ToString());
                    break;
                case PRIM_TYPECODE_DOUBLE:
                    value = new JavaPrimitive(Helper.ConvertToDouble(container.PopBytes(8)).ToString());
                    break;
                case PRIM_TYPECODE_FLOAT:
                    value = new JavaPrimitive(Helper.ConvertToFloat(container.PopBytes(4)).ToString());
                    break;
                case PRIM_TYPECODE_INTEGER:
                    value = new JavaPrimitive(Helper.ConvertToInt(container.PopBytes(4)).ToString());
                    break;
                case PRIM_TYPECODE_LONG:
                    value = new JavaPrimitive(Helper.ConvertToLong(container.PopBytes(8)).ToString());
                    break;
                case PRIM_TYPECODE_SHORT:
                    value = new JavaPrimitive(Helper.ConvertToShort(container.PopBytes(2)).ToString());
                    break;
                case PRIM_TYPECODE_BOOLEAN:
                    value = new JavaPrimitive(Helper.ConvertToBoolean(container.PopBytes(1)).ToString());
                    break;
                case OBJ_TYPECODE_ARRAY:
                case OBJ_TYPECODE_OBJECT:

                    // Find Depth of Array
                    // int ArrayDepth = 0;
                    //while (primitiveType == OBJ_TYPECODE_ARRAY)
                    //{
                    //    primitiveType = complexType[++ArrayDepth];
                    //}

                    // Check if it's an Object or a primitive
                    // primitiveType = complexType[ArrayDepth];
                    byte objectType = container.PopNextControl();
                    if ((objectType == TC_STRING) || (objectType == TC_LONGSTRING))
                    {
                        UInt32 elementNameLength = Helper.ConvertToUint16(container.PopBytes(2));
                        string elementName = Helper.ConvertToUTF(container.PopBytes((int)elementNameLength));

                        value = slwapp.CreatePrimitive(GenerateNewHandle(), elementName);
                    }
                    else if ( (objectType == TC_REFERENCE))
                    {   
                        UInt32 handle = Helper.ConvertToUint32(container.PopBytes(4));
                        value = slwapp.GetVariableByHandle(handle);
                    }
                    else if (objectType == TC_ARRAY)
                    {
                        JavaArray jo;
                        ParseNewArray(objectType, slwapp, container, out jo);
                        value = jo;
                    }
                    else if (objectType == TC_NULL)
                    {
                        value = null;
                    }
                    else if(objectType == TC_OBJECT)
                    {
                        value = ParseObject(objectType, slwapp, container);
                    }
                    else
                    {
                        throw new FormatException(String.Format("ReadField: Unknown objectType {0}", objectType));
                    }
                    break;
                default:
                    throw new FormatException(String.Format("ReadField: Unknown primitiveType {0}", primitiveType));
            }
        }

        /*****************************************
         * 
         *  Class Data. Re-entrant
         * 
         *****************************************/
        static void ParseClassData(JavaClass jc, SlwApplication slwapp, DataContainer container, out JavaObject jo)
        {
            byte classDescFlags;


            // If we made it this war there should be some classdata otherwise this won't execute
            // Fields from Superclasses are loaded first
            jo = slwapp.CreateObject(GenerateNewHandle(), jc);
            foreach (var superclass in jc.GetSuperClasses())
            {
                classDescFlags = superclass.GetClassDescFlags();
                if ((classDescFlags & SC_SERIALIZABLE) == SC_SERIALIZABLE)
                {
                    foreach (var field in superclass.GetFields())
                    {
                        // Class:ClassInfo:ClassData:Fields
                        string Name = field.Key;
                        string complexType = field.Value;
                        JavaVariable value;

                        ReadField(complexType, slwapp, container, out value);
                        jo.AddValue(Name, value);
                    }
                }

                // Parse Object Annotations if there are any
                if ((((classDescFlags & SC_SERIALIZABLE) == SC_SERIALIZABLE) && ((classDescFlags & SC_WRITE_METHOD) == SC_WRITE_METHOD)) ||((((classDescFlags & SC_EXTERNALIZABLE) == SC_EXTERNALIZABLE) && ((classDescFlags & SC_BLOCK_DATA) == SC_BLOCK_DATA))))
                {
                    ParseObjectAnnotation(container.PopNextControl(), ref jo, slwapp, container);
                }
            }

            // Fields from base class are loaded last
            // Class:ClassInfo:ClassData:Fields
            classDescFlags = jc.GetClassDescFlags();
            if ((classDescFlags & SC_SERIALIZABLE) == SC_SERIALIZABLE)
            {
                foreach (var field in jc.GetFields())
                {
                        string Name = field.Key;
                        string complexType = field.Value;
                    JavaVariable value;

                        ReadField(complexType, slwapp, container, out value);
                        jo.AddValue(Name, value);
                }
            }

            if ((((classDescFlags & SC_SERIALIZABLE) == SC_SERIALIZABLE) && ((classDescFlags & SC_WRITE_METHOD) == SC_WRITE_METHOD)) || ((((classDescFlags & SC_EXTERNALIZABLE) == SC_EXTERNALIZABLE) && ((classDescFlags & SC_BLOCK_DATA) == SC_BLOCK_DATA))))
            {
                ParseObjectAnnotation(container.PopNextControl(), ref jo, slwapp, container);
            }
        }


        /*****************************************
         * 
         *  Array Descriptor. Re-entrant
         * 
         *****************************************/
        static void ParseNewArray(byte streamcontrol, SlwApplication slwapp, DataContainer container, out JavaArray jo)
        {
            JavaClass jc;
            switch (streamcontrol)
            {
                case TC_ARRAY:
                    ParseNewClassDesc(container.PopNextControl(), slwapp, container, out jc);

                    // Fill Array
                    UInt32 arraySize = Helper.ConvertToUint32(container.PopBytes(4));
                    jo = slwapp.CreateArray(GenerateNewHandle(), jc);
                    for (UInt32 arrayIndex = 0; arrayIndex < arraySize; arrayIndex++)
                    {
                        JavaVariable value;

                        ReadField(jc.GetClassName().Substring(1), slwapp, container, out value);
                        jo.AddValue(arrayIndex + "", value);
                    }
                    break;
                default:
                    throw new FormatException(String.Format("ParseNewArray: Unknown streamcontrol {0}", streamcontrol));

            }
        }


        /*****************************************
         * 
         *  String/Reference Descriptor. Re-entrant
         * 
         *****************************************/
        static void ParseNewString(byte streamcontrol, SlwApplication slwapp, DataContainer container, out string elementName) 
        {
            UInt64 elementNameLength;
            UInt32 handle;

            // Strings and References
            switch (streamcontrol)
            { 
                case TC_STRING:
                    elementNameLength = Helper.ConvertToUint16(container.PopBytes(2));
                    elementName = Helper.ConvertToUTF(container.PopBytes((int)elementNameLength));
                    slwapp.CreateStringReference(GenerateNewHandle(), elementName);
                    break;
                case TC_LONGSTRING:
                    elementNameLength = Helper.ConvertToUint64(container.PopAlotOfBytes(8));
                    elementName = Helper.ConvertToUTF(container.PopAlotOfBytes(elementNameLength));
                    slwapp.CreateStringReference(GenerateNewHandle(), elementName);
                    break;
                case TC_REFERENCE:
                    handle = Helper.ConvertToUint32(container.PopBytes(4));
                    elementName = slwapp.GetStringByHandle(handle);
                    break;
                default:
                    throw new FormatException(String.Format("ParseNewString: Unknown string type {0}", streamcontrol));
            
            }
        }


        /*****************************************
         * 
         *  Class Annotation. Re-entrant
         * 
         *****************************************/

        static void ParseClassAnnotation(byte streamcontrol, ref JavaClass jc, SlwApplication slwapp, DataContainer container)
        {
            // ClassDesc:ClassInfo:Annotation Recursion happens here
            if (streamcontrol != TC_ENDBLOCKDATA) 
            {
                ParseContent(container.PopNextControl(), slwapp, container);
            }

        }

        static void ParseObjectAnnotation(byte streamcontrol, ref JavaObject jo, SlwApplication slwapp, DataContainer container)
        {
            // ClassData:ObjectAnnotation Recursion happens here
            if (streamcontrol != TC_ENDBLOCKDATA)
            {
                ParseContent(container.PopNextControl(), slwapp, container);
            }
        }

        /*****************************************
         * 
         *  Super Class Descriptor. Re-entrant
         * 
         *****************************************/
        static void ParseSuperClassDesc(byte streamcontrol, SlwApplication slwapp, DataContainer container, out JavaClass javaclasscreated)
        {
            // ClassDesc:ClassInfo:SuperClass
            ParseNewClassDesc(streamcontrol, slwapp, container, out javaclasscreated);
        }

        static void ParseNull(SlwApplication slwapp, DataContainer container)
        {
            container.PopByte();
            container.PopByte();
        }


        /*****************************************
         * 
         *  Object Parser. Re-entrant
         * 
         *****************************************/
        static JavaObject ParseObject(byte streamcontrol, SlwApplication slwapp, DataContainer container)
        {
            // TC_OBJECT
            JavaObject jo = null;
            JavaClass jc;

            switch (streamcontrol)
            {
                case TC_OBJECT:
                    ParseNewClassDesc(container.PopNextControl(), slwapp, container, out jc);
                    if (jc != null)
                    {
                        ParseClassData(jc, slwapp, container, out jo);
                    }
                    break;
                default:
                    throw new FormatException(String.Format("ParseObject: Unknown streamcontrol {0}", streamcontrol));
            }
            return jo;
        }
    }
}
