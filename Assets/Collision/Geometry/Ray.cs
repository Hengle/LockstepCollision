using LockStepMath;
using Point = LockStepMath.LVector;

namespace LockStepCollision
{
    public partial class Ray : BaseShape
    {
        /// <summary>
        /// Collision Type
        /// </summary>
        public override EColType ColType
        {
            get { return EColType.Ray; }
        }

        /// <summary>
        /// orgin point
        /// </summary>
        public Point o;

        /// <summary>
        /// dir
        /// </summary>
        public LVector d;

        public Ray()
        {
        }

        public Ray(Point o, LVector d)
        {
            this.o = o;
            this.d = d;
        }

        public override bool TestWithShape(BaseShape shape)
        {
            return shape.TestWith(this);
        }

        public override bool TestWith(Sphere sphere)
        {
            return Collision.IntersectRaySphere(o, d, sphere, out LFloat t, out LVector p);
        }

        public override bool TestWith(AABB aabb)
        {
            return Collision.IntersectRayAABB(o, d, aabb, out LFloat t, out LVector p);
        }

        public override bool TestWith(Capsule capsule)
        {
            return Collision.IntersectRayCapsule(o, d, capsule.a, capsule.b, capsule.r, out LFloat t);
        }

        public override bool TestWith(OBB obb)
        {
            return Collision.IntersectRayOBB(o, d, obb, out LFloat t, out LVector p);
        }

        public override bool TestWith(Plane plane)
        {
            return Collision.IntersectRayPlane(o, d, plane, out LFloat t, out LVector p);
        }

        public override bool TestWith(Ray ray)
        {
            throw new System.NotImplementedException(GetType() + " not implement this TestWithRay");
        }
    }
}