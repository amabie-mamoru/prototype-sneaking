using System;
using System.Reflection;
using NUnit.Framework;

namespace PrototypeSneaking.Editor.Tests
{
    public class TestBase
    {
        public TestBase()
        {
        }

        /// <examples>
        /// var method = GetMethod<Sight>(sight, "Include");
        /// </examples>
        protected MethodInfo GetMethod<T>(T instance, string methodName)
        {
            var methodInfo = instance.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (methodInfo == null) {
                Assert.Fail($"Not Found Method: {instance.GetType()}#{methodName}");
            }
            return methodInfo;
        }

        /// <examples>
        /// var method = GetOverloadMethod<Sight>(sight, "FindOrLoseWithRay", new System.Type[] { typeof(Vector3), typeof(GameObject) });
        /// </examples>
        protected MethodInfo GetOverloadMethod<T>(T instance, string methodName, Type[] types)
        {
            var methodInfo = instance.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, types, null);
            if (methodInfo == null)
            {
                Assert.Fail($"Not Found Method: {instance.GetType()}#{methodName}");
            }
            return methodInfo;
        }

        /// <examples>
        /// var field = GetField<Sight>(sight, "ObjectsInSight");
        /// </examples>
        protected FieldInfo GetField<T>(T instance, string fieldName)
        {
            var fieldInfo = instance.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo == null) {
                Assert.Fail($"Not Found Field: {instance.GetType()}#{fieldName}");
            }
            return fieldInfo;
        }
    }
}