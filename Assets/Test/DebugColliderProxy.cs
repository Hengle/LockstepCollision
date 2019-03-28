using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

using LockStepCollision;
using LockStepMath;
using Collision = LockStepCollision.Collision;

namespace Test
{
    public struct ColliderLocalInfo
    {
        public LVector offset;
        public LVector rotation;
        public static readonly ColliderLocalInfo identity = new ColliderLocalInfo(LVector.zero, LVector.zero); 

        public ColliderLocalInfo(LVector offset,LVector rotation)
        {
            this.offset = offset;
            this.rotation = rotation;
        }
    }

    public class DebugColliderProxy : MonoBehaviour
    {
        public static List<DebugColliderProxy> allProxys = new List<DebugColliderProxy>();
        public List<BaseShape> allColliders = new List<BaseShape>();
        public List<ColliderLocalInfo> allColliderOffset = new List<ColliderLocalInfo>();
        public Sphere boundSphere;
        public Material mat;
        private void Start()
        {
            allProxys.Add(this);
            mat =new Material(GetComponent<Renderer>().material);
            GetComponent<Renderer>().material = mat;
            lastPosition = transform.position;
            lastRotation = transform.rotation;
        }
        
        /// <summary>
        /// 获取所有的Shape 的总boundSphere
        /// </summary>
        /// <returns></returns>
        public virtual Sphere GetBoundSphere()
        {
            Sphere retS = null;
            foreach (var col in allColliders)
            {
                var tempS = col.GetBoundSphere();
                if (retS == null)
                {
                    retS = tempS;
                }
                else
                {
                    var rt = (tempS.c - retS.c);
                    var sqrCenterDist = rt.sqrMagnitude;
                    var rDist =  retS.r + tempS.r;
                    if (rDist * rDist <=sqrCenterDist)//separatie
                    {
                        var centerDist= LMath.Sqrt(sqrCenterDist);
                        var r =  (centerDist + rDist)* LFloat.half ;
                        var c = retS.c + rt.normalized * (r - retS.r);
                        retS.c = c;
                        retS.r = r;
                    }
                    else
                    {
                        var rdiff = LMath.Abs(retS.r - tempS.r);
                        if (rdiff <= sqrCenterDist) //one contains another
                        {
                            if (retS.r < tempS.r)
                            {
                                retS.c = tempS.c;
                                retS.r = tempS.r;
                            }
                        }
                        else //intersect
                        {
                            var centerDist= LMath.Sqrt(sqrCenterDist);
                            var r =  (centerDist + rDist)* LFloat.half ;
                            var c = retS.c + rt.normalized * (r - retS.r);
                            retS.c = c;
                            retS.r = r;
                        }
                    }
                }
            }

            return retS;
        } 
        private void OnDrawGizmos()
        {
            foreach (var col in allColliders)
            {
                col.OnDrawGizmos(true,Color.green);
            }

            if (allColliders[0] is Capsule)
            {
                int i = 0;
            }

            boundSphere?.OnDrawGizmos(true,Color.red);
        }

        private void Update()
        {
       
            bool hasCollidedOthers = false;
            //TODO 改为更加 性能友好的判定方式
            foreach (var col in allProxys)
            {
                if(col != this)
                {
                    if (TestColliderProxy(this, col))
                    {
                        hasCollidedOthers = true;
                        break;
                    }
                }
            }
            mat.color = hasCollidedOthers ? Color.red : Color.white;
            //check pos or rotation changed
            var diffPos = lastPosition != transform.position;
            var diffRot = lastRotation != transform.rotation;
            if (diffPos|| diffRot)
            {
                UpdateBoundBox(diffPos, diffRot,transform.position.ToLVector(),transform.rotation.eulerAngles.ToLVector());
                lastPosition = transform.position;
                lastRotation = transform.rotation;
            }
        }
        public  static bool TestColliderProxy(DebugColliderProxy a, DebugColliderProxy b)
        {
            bool hasCollidedOthers = false;
            var isCollided = Collision.TestSphereSphere(a.boundSphere, b.boundSphere);
            if (isCollided)
            {
                foreach (var cCola in a.allColliders)
                {
                    foreach (var cColb in b.allColliders)
                    {
                        if (BaseShape.TestShapeWithShape(cCola, cColb))
                        {
                            hasCollidedOthers = true;
                            break;
                        }
                    }
                }
                       
            }
            return hasCollidedOthers;
        }

        public Vector3 lastPosition;
        public Quaternion lastRotation;
        public void UpdatePosition(Vector3 position)
        {
        }

        public void UpdateRotation(Quaternion rotation)
        {
        }

        public void UpdateBoundBox(bool isDiffPos,bool isDiffRot,LVector targetPos,LVector targetRot)
        {
            if (isDiffPos)
            {
                boundSphere.c = targetPos;    
            }

            foreach (var col in allColliders)
            {
                col.UpdateCollider(isDiffPos,isDiffRot,targetPos,targetRot);
            }
        }


        public void AddTestCollider(GameObject obj,PrimitiveType type)
        {
            switch (type)
            {
                case PrimitiveType.Cube:
                {
                    var _col = new AABB();
                    _col.min = (obj.transform.position - Vector3.one * 0.5f).ToLVector();
                    _col.max = (obj.transform.position + Vector3.one * 0.5f).ToLVector();
                    AddCollider(_col,ColliderLocalInfo.identity); break;
                }
                case PrimitiveType.Sphere:
                {
                    var _col = new Sphere(obj.transform.position.ToLVector(),0.5f.ToLFloat());
                    AddCollider(_col,ColliderLocalInfo.identity); break;
                }
                case PrimitiveType.Capsule:
                {
                    var _col = new Capsule();
                    _col.a = (obj.transform.position - Vector3.up * 0.5f).ToLVector();
                    _col.b = (obj.transform.position + Vector3.up * 0.5f).ToLVector();
                    _col.r = 0.5f.ToLFloat();
                    AddCollider(_col,ColliderLocalInfo.identity); break;
                }
            }
        }

        public void AddCollider(BaseShape shape,ColliderLocalInfo localInfo)
        {
            Debug.Assert(shape!= null);
            allColliders.Add(shape);
            allColliderOffset.Add(localInfo);
            boundSphere = GetBoundSphere();
        }
    }
}