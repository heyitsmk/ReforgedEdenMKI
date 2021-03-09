using Eleon.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReforgedEdenMKI
{
    internal static class Utility
    {
        internal static float Magnitude(this PVector3 vec)
        {
            return (float)Math.Sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z);
        }

        internal static PVector3 Minus(this PVector3 vecA, PVector3 vecB)
        {
            return new PVector3(vecA.x - vecB.x, vecA.y - vecB.y, vecA.z - vecB.z);
        }
    }
}
