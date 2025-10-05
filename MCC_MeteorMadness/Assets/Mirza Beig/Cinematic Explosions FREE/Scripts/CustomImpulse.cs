using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirzaBeig.CinematicExplosionsFree
{
    public class CustomImpulse : MonoBehaviour
    {
        // Removed CinemachineImpulseSource to avoid errors

        void Start()
        {
            // Optional: put any initialization code here
        }

        void OnEnable()
        {
            // No camera shake here
            // Explosion effect will still play normally
        }

        void Update()
        {
            // Optional: put any per-frame logic here
        }
    }
}
