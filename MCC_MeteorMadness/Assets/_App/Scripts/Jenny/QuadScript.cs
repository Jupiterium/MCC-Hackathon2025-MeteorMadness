using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadScript : MonoBehaviour
{
    //Material mMaterial;
    //MeshRenderer mMeshRenderer;

    //float[] mPoints;
    //int mHitCount;

    //float mDelay;


    void Start()
    {
        //mDelay = 3;

        //mMeshRenderer = GetComponent<MeshRenderer>();
        //mMaterial = mMeshRenderer.material;

        //mPoints = new float[32 * 3]; //32 point 

    }

    //void Update()
    //{
    //    mDelay -= Time.deltaTime;
    //    if (mDelay <= 0)
    //    {
    //        GameObject go = Instantiate(Resources.Load<GameObject>("Projectile"));
    //        go.transform.position = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-3f, -1f));

    //        mDelay = 0.5f;
    //    }

    //}

    private void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint cp in collision.contacts)
        {
            Debug.Log("Contact with object " + cp.otherCollider.gameObject.name);

            // Get the local position of the hit point relative to the Earth sphere
            Vector3 localPoint = transform.InverseTransformPoint(cp.point).normalized;

            // Calculate latitude and longitude
            float longitude = Mathf.Atan2(localPoint.x, localPoint.z) * Mathf.Rad2Deg; // -180 to 180
            float latitude = Mathf.Asin(localPoint.y) * Mathf.Rad2Deg; // -90 to 90

            Debug.Log($"Impact Lat: {latitude}, Lon: {longitude}");

            // If you want to remap lat/long to UV space (0–1 range):
            float u = (longitude + 180f) / 360f;
            float v = (latitude + 90f) / 180f;
            Debug.Log($"Impact UV (simulated): {u}, {v}");

            // Destroy the projectile after hit
            Destroy(cp.otherCollider.gameObject);
        }
    }

    //public void addHitPoint(float xp, float yp)
    //{
    //    mPoints[mHitCount * 3] = xp;
    //    mPoints[mHitCount * 3 + 1] = yp;
    //    mPoints[mHitCount * 3 + 2] = Random.Range(1f, 3f);

    //    mHitCount++;
    //    mHitCount %= 32;

    //    mMaterial.SetFloatArray("_Hits", mPoints);
    //    mMaterial.SetInt("_HitCount", mHitCount);

    //}

}