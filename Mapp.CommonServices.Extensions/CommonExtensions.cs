using System;

namespace Shmap.CommonServices
{
    public static class CommonExtensions
    {
        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }


        public static bool MajorAndMinorEquals(this Version comparedVerison, Version currentVersion)
        {
            return comparedVerison.Major.Equals(currentVersion.Major)
                   && comparedVerison.Minor.Equals(currentVersion.Minor);
        }

    }
}
