using System.Numerics;

namespace Theatre.Utils
{
    internal class Colour
    {
        public readonly float R, G, B, A;

        public Colour(float r, float g, float b, float a = 1)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public Colour(Vector4 v)
        {
            new Colour(v.X, v.Y, v.Z, v.W);
        }

        public Colour(Vector3 v)
        {
            new Colour(new Vector4(v, 1));
        }

        private Vector3 ToVector3()
        {
            return new Vector3(R, G, B);
        }

        public Vector4 ToVector4()
        {
            return new Vector4(ToVector3(), A);
        }
    }
}
