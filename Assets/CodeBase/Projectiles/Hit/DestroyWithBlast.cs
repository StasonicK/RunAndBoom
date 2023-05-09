﻿using System.Collections.Generic;
using CodeBase.Enemy;
using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Projectiles.Hit
{
    public class DestroyWithBlast : MonoBehaviour
    {
        [SerializeField] private LayerMask _objectLayerMask;

        private const string EnemyTag = "Enemy";
        private const string DestructableTag = "Destructable";

        private int _objectsHitsCount = 16;
        private float _sphereDistance = 0f;
        private List<EnemyHealth> _enemies = new List<EnemyHealth>();

        public void HitAllAround(float sphereRadius, float damage)
        {
            _enemies.Clear();
            RaycastHit[] objectsHits = new RaycastHit[_objectsHitsCount];
            int objectsHitsCount = GetObjectsHits(objectsHits, sphereRadius);

            for (int i = 0; i < objectsHitsCount; i++)
            {
                string objectTag = objectsHits[i].transform.gameObject.tag;

                if (objectTag == EnemyTag)
                {
                    objectsHits[i].transform.gameObject.GetComponent<IHealth>().TakeDamage(damage);
                    objectsHits[i].transform.gameObject.GetComponent<IDeath>()?.Die();
                }
                else if (objectTag == DestructableTag)
                {
                    // objectsHits[i].transform.gameObject
                    objectsHits[i].transform.parent.gameObject.transform.parent.gameObject
                        .GetComponent<IDeath>()?.Die();
                }
            }
        }

        private int GetObjectsHits(RaycastHit[] hits, float sphereRadius)
        {
            PhysicsDebug.DrawDebug(transform.position, sphereRadius, 10f);
            return Physics.SphereCastNonAlloc(transform.position, sphereRadius, transform.forward, hits,
                _sphereDistance, _objectLayerMask, QueryTriggerInteraction.Ignore);
        }
    }
}