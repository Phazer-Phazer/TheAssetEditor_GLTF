using Microsoft.Xna.Framework;

namespace Editors.ImportExport.Geometry
{
    public class Handedness
    {
        public static Vector3 TranslationSwizzle(Vector3 t) => new Vector3(t.X, t.Y, t.Z);
        public static System.Numerics.Vector3 PositionSwizzle(System.Numerics.Vector3 pos) => new System.Numerics.Vector3(pos.X, pos.Y, pos.Z);
        public static Quaternion RotationSwizzle(Quaternion q) => new Quaternion(q.X, q.Y, q.Z, q.W);
    }
}
