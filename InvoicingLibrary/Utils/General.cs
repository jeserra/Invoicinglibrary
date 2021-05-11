using System;
using System.Reflection;


namespace ProcessCFDI.Utils
{
    public class General
    {
        public void Invoke<T>(string methodName) where T : new()
        {
            T instance = new T();
            MethodInfo method = typeof(T).GetMethod(methodName);
            method.Invoke(instance, null);
        }
    }
}
