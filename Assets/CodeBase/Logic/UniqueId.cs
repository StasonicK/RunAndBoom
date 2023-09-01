using System;
using System.Linq;
using NTC.Global.Cache;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeBase.Logic
{
    public class UniqueId : MonoCache
    {
        [FormerlySerializedAs("Id")] [SerializeField]
        private string _id;

        private void Start() =>
            GenerateId();

        private void GenerateId()
        {
            UniqueId[] uniqueIds = FindObjectsOfType<UniqueId>();

            if (uniqueIds.Any(other => other != this && other._id == _id))
                _id = $"{GetComponent<UniqueId>().gameObject.scene.name}_{Guid.NewGuid().ToString()}";
        }
    }
}