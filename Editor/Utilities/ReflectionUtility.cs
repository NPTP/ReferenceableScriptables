using System;
using System.Reflection;
using UnityEngine;

namespace NPTP.ReferenceableScriptables.Editor.Utilities
{
    public class ReflectionUtility : MonoBehaviour
    {
        public static void SetSerializedField(object instance, string fieldName, object value)
        {
            if (instance == null)
            {
                return;
            }
            
            Type type = instance.GetType();
            FieldInfo fieldInfo = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (fieldInfo == null)
            {
                Type baseType = type.BaseType;
                while (baseType != null)
                {
                    fieldInfo = baseType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
                    if (fieldInfo != null)
                    {
                        fieldInfo.SetValue(instance, value);
                        return;
                    }

                    baseType = baseType.BaseType;
                }
            }
            else
            {
                fieldInfo.SetValue(instance, value);
            }
        }
        
        public static void InvokeStaticMethod<T>(string methodName, params object[] parameters)
        {
            MethodInfo methodInfo = typeof(T).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            if (methodInfo == null)
            {
                return;
            }

            try
            {
                methodInfo.Invoke(null, parameters);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
    }
}