using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game;
using Gamelogic.Extensions;

public class CannonTower : BaseTower
{
    #region Editor tweakable fields

    [SerializeField] protected CannonProjectile projectilePrefab;

    #endregion

    #region Fields

    private GameObject shootPoint;
    private Pool<CannonProjectile> pool;

    #endregion

    #region Unity callbacks

    protected override void Start()
    {
        base.Start();

        shootPoint = new GameObject();
        shootPoint.transform.parent = gameObject.transform;
        shootPoint.transform.localPosition = new Vector3(0, 1.4f, 0);

        pool = new Pool<CannonProjectile>(10,
                                          () => BaseProjectile.Create(projectilePrefab, shootPoint.transform.position, this),
                                          projectile => Destroy(projectile.gameObject),
                                          projectile => projectile.gameObject.SetActive(true),
                                          projectile => projectile.gameObject.SetActive(false));
    }

    #endregion

    #region Private methods

    public override void ReleaseProjectile(BaseProjectile projectile)
    {
        pool.Release(projectile as CannonProjectile);
    }

    protected override void Shoot(Monster monster)
    {
        pool.GetNewObjectSilently(10);
    }

    protected override bool CheckDistance(Monster monster)
    {
        /* todo    - use FirstOrderIntercept
         * @author - Dvurechenskiyi
         * @date   - 13.02.2018
         * @time   - 17:34
        */
        return Vector3.Distance(transform.position, monster.transform.position) <= shootRange;
    }

    protected override BaseProjectile GetProjectilePrefab()
    {
        return projectilePrefab;
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
    public static float FirstOrderInterceptTime(float shotSpeed, Vector3 targetRelativePosition,
                                                Vector3 targetRelativeVelocity)
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