using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace KaizerWaldCode.Utils
{
    // See : https://stackoverflow.com/questions/36239705/serialize-and-deserialize-json-and-json-array-in-unity
    public static class JsonHelper
    {
        public static T[] FromJson<T>(in string path) where T : struct
        {
            using (StreamReader stream = new StreamReader(path))
            {
                string json = stream.ReadToEnd();
                Wrapper<T> w = JsonUtility.FromJson<Wrapper<T>>(json);
                return w.A;
            }
        }

        public static void ToJson<T>(T[] array, in string path, bool prettyPrint = false) where T : struct
        {
            using (StreamWriter stream = new StreamWriter(path))
            {
                Wrapper<T> wrapper = new Wrapper<T>();
                wrapper.A = array;
                stream.Write(JsonUtility.ToJson(wrapper, prettyPrint));
            }
        }
        
        public static void ToJson<T>(T obj, in string path, bool prettyPrint = false) //where T : class
        {
            using StreamWriter stream = new StreamWriter(path);
            stream.Write(JsonUtility.ToJson(obj, prettyPrint));
        }
        //Doesn't seems to works
        public static void ToJson<T>(NativeArray<T> array, in string path, bool prettyPrint = false) where T : struct
        {
            using (StreamWriter stream = new StreamWriter(path))
            {
                Wrapper<T> wrapper = new Wrapper<T>();
                wrapper.A = array.ToArray();
                stream.Write(JsonUtility.ToJson(wrapper, prettyPrint));
            }
        }
        
        //ASYNC METHOD
        public static async Task<T[]> FromJsonAsync<T>(string path) where T : struct
        {
            using (StreamReader stream = new StreamReader(path))
            {
                string json = await stream.ReadToEndAsync(); 
                Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
                return wrapper.A;
            } ;
        }
        
        [Serializable]
        private class Wrapper<T> where T : struct
        {
            public T[] A;
        }
    }
}
