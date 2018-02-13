using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CannonTower : MonoBehaviour
{
    #region Editor tweakable fields
    
    [SerializeField] private float shootInterval = 0.5f;
    [SerializeField] private float shootRange = 4f;
    [SerializeField] private GameObject m_projectilePrefab;
    
    #endregion
    
    #region Fields
    
    private float lastShotTime;
    private GameObject shootPoint;
    
    
    /* todo    - think on better collection
     * @author - Dvurechenskiyi
     * @date   - 13.02.2018
     * @time   - 17:33
    */    
    private List<Monster> monsters;
    
    #endregion

    #region Unity callbacks

    private void Start()
    {
        shootPoint = new GameObject();
        shootPoint.transform.parent = gameObject.transform;
        shootPoint.transform.localPosition = new Vector3(0, 1.4f, 0);

        monsters = new List<Monster>();

        SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
        sphereCollider.radius = shootRange;
    }

    void Update()
    {
        if (m_projectilePrefab == null)
        {
            return;
        }

        if (Time.time - lastShotTime >= shootInterval)
        {
            foreach (var monster in monsters)
            {
                /* todo    - use FirstOrderIntercept
                 * @author - Dvurechenskiyi
                 * @date   - 13.02.2018
                 * @time   - 17:34
                */
                if (Vector3.Distance(transform.position, monster.transform.position) > shootRange)
                {
                    continue;
                }    

                // shot
                Instantiate(m_projectilePrefab, shootPoint.transform.position, shootPoint.transform.rotation);

                lastShotTime = Time.time;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Monster monster = other.GetComponent<Monster>();
        if (monster != null)
        {
            monsters.Add(monster);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Monster monster = other.GetComponent<Monster>();
        if (monster != null)
        {
            monsters.Remove(monster);
        }
    }

    #endregion
    


    //first-order intercept using absolute target position
    public static Vector3 FirstOrderIntercept(Vector3 shooterPosition, Vector3 shooterVelocity, float shotSpeed,
        Vector3 targetPosition, Vector3 targetVelocity)
    {
        Vector3 targetRelativePosition = targetPosition - shooterPosition;
        Vector3 targetRelativeVelocity = targetVelocity - shooterVelocity;
        float t = FirstOrderInterceptTime(shotSpeed, targetRelativePosition, targetRelativeVelocity);
        return targetPosition + t * targetRelativeVelocity;
    }

    //first-order intercept using relative target position
    public static float FirstOrderInterceptTime(float shotSpeed, Vector3 targetRelativePosition, Vector3 targetRelativeVelocity)
    {
        float velocitySquared = targetRelativeVelocity.sqrMagnitude;
        if (velocitySquared < 0.001f)
        {
            return 0f;
        }

        float a = velocitySquared - shotSpeed * shotSpeed;

        //handle similar velocities
        if (Mathf.Abs(a) < 0.001f)
        {
            float doubleProduct = 2f * Vector3.Dot(targetRelativeVelocity, targetRelativePosition);
            float t = -targetRelativePosition.sqrMagnitude / doubleProduct;
            return Mathf.Max(t, 0f); //don't shoot back in time
        }

        float b = 2f * Vector3.Dot(targetRelativeVelocity, targetRelativePosition);
        float c = targetRelativePosition.sqrMagnitude;
        float determinant = b * b - 4f * a * c;

        if (determinant > 0f)
        {
            //determinant > 0; two intercept paths (most common)
            float t1 = (-b + Mathf.Sqrt(determinant)) / (2f * a),
                t2 = (-b - Mathf.Sqrt(determinant)) / (2f * a);
            if (t1 > 0f)
            {
                if (t2 > 0f)
                {
                    return Mathf.Min(t1, t2); //both are positive
                }
                else
                {
                    return t1; //only t1 is positive
                }

            }
            else
            {
                return Mathf.Max(t2, 0f); //don't shoot back in time
            }
        }
        else if (determinant < 0f)
        {
            //determinant < 0; no intercept path
            return 0f;
        }
        else
        {
            //determinant = 0; one intercept path, pretty much never happens
            return Mathf.Max(-b / (2f * a), 0f); //don't shoot back in time
        }
    }
}