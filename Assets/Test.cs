using System;
using System.Collections.Generic;
using LibSugar.Unity;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace DefaultNamespace
{

    public class Test : MonoBehaviour
    {
        public BurstUlid testSer; 
        
        private void Start()
        {
            using var ulids = new NativeArray<Ulid>(100, Allocator.TempJob);
            var job = new GenJob { ulids = ulids };
            job.Schedule(100, 1).Complete();
            foreach (var ulid in ulids)
            {
                Debug.Log(ulid);
            }
        }

        private struct GenJob : IJobParallelFor
        {
            public NativeArray<Ulid> ulids;

            public void Execute(int index)
            {
                ulids[index] = Ulid.NewUlid();
            }
        }
    }

}
