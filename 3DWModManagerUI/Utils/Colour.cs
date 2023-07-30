using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace _3DWModManagerUI.Utils
{
    internal class Colour
    {
        private float r;
        private float g;
        private float b;
        private float a;

        public Colour(float r, float g, float b, float a = 1)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(r, g, b);
        }

        public Vector4 ToVector4()
        {
            return new Vector4(r, g, b, a);
        }
    }
}
