﻿using System;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace SCFramework
{
    public class SerializeHelper
    {
        public static bool SerializeBinary(string path, object obj)
        {
            if (string.IsNullOrEmpty(path))
            {
                Log.w("SerializeBinary Without Valid Path.");
                return false;
            }

            if (obj == null)
            {
                Log.w("SerializeBinary obj is Null.");
                return false;
            }

            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                bf.Serialize(fs, obj);
                return true;
            }
        }

        public static object DeserializeBinary(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Log.w("DeserializeBinary Without Valid Path.");
                return null;
            }

            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                object data = bf.Deserialize(fs);

                if (data != null)
                {
                    return data;
                }
            }

            Log.w("DeserializeBinary Failed:" + path);
            return null;
        }

        public static bool SerializeXML(string path, object obj)
        {
            if (string.IsNullOrEmpty(path))
            {
                Log.w("SerializeBinary Without Valid Path.");
                return false;
            }

            if (obj == null)
            {
                Log.w("SerializeBinary obj is Null.");
                return false;
            }

            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                XmlSerializer xmlserializer = new XmlSerializer(obj.GetType());
                xmlserializer.Serialize(fs, obj);
                return true;
            }
        }

        public static object DeserializeXML<T>(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Log.w("DeserializeBinary Without Valid Path.");
                return null;
            }

            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                XmlSerializer xmlserializer = new XmlSerializer(typeof(T));
                object data = xmlserializer.Deserialize(fs);

                if (data != null)
                {
                    return data;
                }
            }

            Log.w("DeserializeBinary Failed:" + path);
            return null;
        }
    }
}
