using System;
using System.Collections.Generic;
using System.Linq;

namespace Shmap.CommonServices
{
    public static class Require
    {
        public static void NotEmpty<T>(IEnumerable<T> collection, string arg, string msg = "")
        {
            NotNull(collection, arg, msg);

            if (!collection.Any())
            {
                throw new ArgumentException(msg, arg);
            }
        }

        public static void NotNull(object target, string arg, string msg = "")
        {
            if (target == null)
            {
                throw new ArgumentNullException(arg, msg);
            }
        }

        public static void NotNullOrEmpty(string target, string arg)
        {
            if (string.IsNullOrEmpty(target)) throw new ArgumentException(arg);
        }

        public static void Range(bool condition, string arg, string msg = "")
        {
            if (!condition)
            {
                throw new ArgumentOutOfRangeException(arg, msg);
            }
        }

        public static void That(bool condition, string arg, string msg = "")
        {
            if (!condition)
            {
                throw new ArgumentException(msg, arg);
            }
        }
    }
}