using CodeBase.Projectiles.Movement;
using NTC.Global.Cache;
using UnityEngine;

namespace CodeBase.Projectiles.Hit
{
    public abstract class BaseProjectileHit : MonoCache
    {
        [SerializeField] protected ProjectileTrail Trail;
        [SerializeField] protected ProjectileMovement Movement;

        protected string[] Tags;

        protected bool IsTargetTag(string targetTag)
        {
            foreach (string tag in Tags)
                if (targetTag.Equals(tag))
                    return true;

            return false;
        }
    }
}