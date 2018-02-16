using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game;
using Gamelogic.Extensions;

public class CannonTower : BaseTower
{
    #region Editor tweakable fields

    [SerializeField] private CannonProjectile projectilePrefab;

    #endregion
    
    #region Fields

    private Vector3 advance;
    private Monster target;
    
    #endregion
    
    #region Unity callbacks

    protected override void Update()
    {
        base.Update();

        if (target)
        {
            /* todo    - aim to target + advance
             * @author - Артур
             * @date   - 15.02.2018
             * @time   - 22:20
            */
            
            transform.LookAt(target.transform.position + advance);
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        if (other.GetComponent<Monster>())
        {
            SetTarget();
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        
        if (other.GetComponent<Monster>())
        {
            SetTarget();
        }
    }

    private void OnDrawGizmos()
    {
        Debug.DrawRay(shootSocket.transform.position, transform.forward * 10, Color.blue);
    }

    #endregion
    
    #region Private methods

    private void SetTarget()
    {
        /* todo    - target must be future target position instead of monster
         * @author - Артур
         * @date   - 15.02.2018
         * @time   - 22:44
        */
        Vector3 interceptPoint = GetInterceptPoint();
        advance = interceptPoint - target.transform.position;
    }

    private Vector3 GetInterceptPoint()
    {
        for (int i = 0; i < monsters.Count; i++)
        {
            // find, whether cannon is ready to shoot
            float remainedCooldown = 0f;
        
            if (Time.time - lastShotTime < shootInterval)
            {
                remainedCooldown = shootInterval - (Time.time - lastShotTime);
            }
        
            Monster monster = monsters[i];

            // if cannon is ready to shoot, calculate intercept position. 
            // If intercept position is in shoot range, return it, otherwise return Vector3.zero
            if (Math.Abs(remainedCooldown) < 0.01f)
            {
                Vector3 interceptPosition = FirstOrderIntercept(shootSocket.transform.position, Vector3.zero, projectilePrefab.Speed, monster.transform.position, monster.GetVelocity());

                // If intercept position is in shoot range, return it, otherwise return Vector3.zero
                if (Vector3.Distance(transform.position, interceptPosition) <= shootRange)
                {
                    target = monster;
                    return interceptPosition;
                }
            }
            else
            {
                // otherwise, cannon is on cooldown, so we need calculate monster future position.
                Vector3 futurePosition = monster.transform.position + monster.GetVelocity() * remainedCooldown;
            
                /* todo    - in future shootSocket position will change, as cannon rotates. Fix it
                 * @author - Dvurechenskiyi
                 * @date   - 15.02.2018
                 * @time   - 16:28
                */
                Vector3 interceptPosition = FirstOrderIntercept(shootSocket.transform.position, Vector3.zero,
                    projectilePrefab.Speed, futurePosition, monster.GetVelocity());

                // If intercept position is in shoot range, return it, otherwise return Vector3.zero
                if (Vector3.Distance(transform.position, interceptPosition) <= shootRange)
                {
                    target = monster;
                    return interceptPosition;
                }
            }
        }

        return Vector3.zero;
    }

    protected override void Shoot(Monster monster)
    {
        CannonProjectile projectile = pool.GetNewObjectSilently() as CannonProjectile;
        projectile.SetDirection(shootSocket.transform.forward);
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
        float t = CalculateInterceptTime(shooterPosition, shooterVelocity, shotSpeed, targetPosition,
            targetVelocity);
        Vector3 targetRelativeVelocity = targetVelocity - shooterVelocity;
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

    public static float CalculateInterceptTime(Vector3 shooterPosition, Vector3 shooterVelocity, float shotSpeed,
        Vector3 targetPosition, Vector3 targetVelocity)
    {
        Vector3 targetRelativePosition = targetPosition - shooterPosition;
        Vector3 targetRelativeVelocity = targetVelocity - shooterVelocity;
        return FirstOrderInterceptTime(shotSpeed, targetRelativePosition, targetRelativeVelocity);
    }
}