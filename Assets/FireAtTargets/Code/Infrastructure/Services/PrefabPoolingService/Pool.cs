using System.Collections.Generic;
using UnityEngine;

namespace FreedLOW.FireAtTargets.Code.Infrastructure.Services.PrefabPoolingService
{
    public class Pool
    {
        public readonly Queue<GameObject> Objects;
        public Transform Container { get; private set; }

        public Pool(Transform container)
        {
            Container = container;
            Objects = new Queue<GameObject>();
        }
    }
}