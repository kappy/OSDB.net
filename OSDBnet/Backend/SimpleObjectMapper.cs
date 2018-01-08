using System;
using System.Collections.Generic;
using CookComputing.XmlRpc;

namespace OSDBnet.Backend
{
    public static class SimpleObjectMapper
    {
        public static T MapToObject<T>(XmlRpcStruct obj) where T : class
        {
            T instance = Activator.CreateInstance<T>();

            var destinationType = typeof(T);
            var members = destinationType.GetFields();

            foreach (var member in members)
            {
                if (obj.ContainsKey(member.Name))
                {
                    member.SetValue(instance, obj[member.Name]);
                }
            }

            return instance;
        }

        public static IDictionary<string, string> MapToDictionary(XmlRpcStruct obj)
        {
            if (obj == null)
                return null;

            IDictionary<string, string> instance = new Dictionary<string, string>();

            foreach (string key in obj.Keys)
            {
                instance.Add(key, obj[key].ToString());
            }

            return instance;
        }
    }
}