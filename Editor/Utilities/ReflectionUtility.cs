using System;
using System.Reflection;
using UnityEngine;

namespace NPTP.ReferenceableScriptables.Editor.Utilities
{
    public class ReflectionUtility : MonoBehaviour
    {
        public static bool TryInvokeStaticMethod(Type classType, string methodName, params object[] parameters)
        {
            MethodInfo methodInfo = classType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            if (methodInfo == null)
            {
                return false;
            }

            try
            {
                methodInfo.Invoke(null, parameters);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}