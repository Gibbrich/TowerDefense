﻿using System.Collections;
using System.Collections.Generic;
using Gamelogic.Extensions;
using UnityEngine;

namespace Game
{
    public class CannonTower : BaseTower
    {
        #region Editor tweakable fields
        
        [Tooltip("Tower head, rotates around Y-axis")] 
        [SerializeField]
        private Transform turretTower;

        [Tooltip("Tower cannon, rotates around X-axis")]
        [SerializeField]
        private Transform turretGun;

        [Tooltip("Head and cannon rotation speed, degree/sec")] 
        [SerializeField]
        private int rotationSpeed;

        [SerializeField] private CannonProjectile projectile;
        
        #endregion
        
        #region Private fields
        
        private Monster target;

        // Shooting point. By default equals target 
        private Vector3 targetingPosition;
        private StateMachine<TurretState> stateMachine;
        
        #endregion

        #region Unity callbacks
        
        protected override void Start()
        {
            base.Start();

            stateMachine = new StateMachine<TurretState>();
            stateMachine.AddState(TurretState.IDLE, null, IdleOnUpdate);
            stateMachine.AddState(TurretState.ATTACK, null, AttackOnUpdate);
            stateMachine.CurrentState = TurretState.IDLE;

            monsters = new List<Monster>();
        }

        private void Update()
        {
            stateMachine.Update();
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, shootRange);
            //Current cannon direction
            Debug.DrawRay(turretGun.position, turretGun.forward * shootRange, Color.blue);
            //Target cannon direction
            Debug.DrawRay(turretGun.position, (targetingPosition - turretGun.position), Color.yellow);
            //Current tower direction
            Debug.DrawRay(turretTower.position, turretTower.forward * shootRange, Color.red);
        }

        protected override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);

            Monster monster = other.GetComponent<Monster>();
            if (monster != null)
            {
                SearchTarget();
            }
        }

        protected override void OnTriggerExit(Collider other)
        {
            base.OnTriggerExit(other);

            Monster monster = other.GetComponent<Monster>();
            if (monster != null)
            {
                SearchTarget();
            }
        }

        #endregion

        #region Private methods

        private void Shoot()
        {
            //5 degrees gap - for inaccurace calculations 
            bool isCannonRotatedToTarget =
                Vector3.Angle(turretGun.forward, targetingPosition - turretGun.position) <= 5;

            if (Time.time - lastShotTime >= shootInterval && isCannonRotatedToTarget)
            {
                CannonProjectile cannonProjectile = pool.GetNewObjectSilently() as CannonProjectile;
                cannonProjectile.transform.position = turretGun.position;
                cannonProjectile.transform.rotation =
                    Quaternion.FromToRotation(projectile.transform.forward, turretGun.forward);

                lastShotTime = Time.time;
            }
        }

        protected override BaseProjectile GetProjectilePrefab()
        {
            return projectile;
        }

        private void SearchTarget()
        {
            // find, whether cannon is ready to shoot
            float timeToNextShot = 0f;

            if (Time.time - lastShotTime < shootInterval)
            {
                timeToNextShot = shootInterval - (Time.time - lastShotTime);
            }

            Monster target = null;

            for (int i = 0; i < monsters.Count; i++)
            {
                Vector3 monsterPositionBeforeShoot =
                    monsters[i].GetSpeed() * timeToNextShot + monsters[i].transform.position;
                Vector3 advance = CalculateAdvance(monsterPositionBeforeShoot, monsters[i].GetSpeed());

                if (Vector3.Distance(transform.position, advance) <= shootRange)
                {
                    target = monsters[i];
                    break;
                }
            }

            this.target = target;
            if (target != null)
            {
                if (stateMachine.CurrentState != TurretState.ATTACK)
                {
                    stateMachine.CurrentState = TurretState.ATTACK;
                }
            }
            else
            {
                stateMachine.CurrentState = TurretState.IDLE;
            }
        }

        private Vector3 CalculateAdvance(Vector3 monsterPosition, Vector3 monsterSpeed)
        {
            float dist = (turretGun.position - monsterPosition).magnitude;
            float timeToTarget = dist / projectile.Speed;
            return monsterPosition + monsterSpeed * timeToTarget;
        }

        protected virtual Vector3 CalculateAim()
        {
            //По умолчанию турель стреляет прямо по цели, но, если цель движется, то нужно высчитать точку,
            //которая находится перед движущейся целью и по которой будет стрелять турель.
            //То есть турель должна стрелять на опережение
            targetingPosition = target.transform.position;

            //Высчитываем точку, перед мишенью, по которой нужно произвести выстрел, чтобы попасть по движущейся мишени
            //по идее, чем больше итераций, тем точнее будет положение точки для упреждающего выстрела
            for (int i = 0; i < 10; i++)
            {
                float dist = (turretGun.position - targetingPosition).magnitude;
                float timeToTarget = dist / projectile.Speed;
                targetingPosition = target.transform.position + target.GetSpeed() * timeToTarget;
            }

            return targetingPosition;
        }

        private void IdleOnUpdate()
        {
            if (turretTower.rotation != Quaternion.identity)
            {
                RotateTower(Vector3.zero);
            }

            if (turretGun.rotation != Quaternion.identity)
            {
                RotateGun(Vector3.zero);
            }
        }

        private void AttackOnUpdate()
        {
            if (target == null || !target.gameObject.activeSelf)
            {
                stateMachine.CurrentState = TurretState.IDLE;
            }
            else
            {
                TurnTurret();
                Shoot();
            }
        }

        private void TurnTurret()
        {
            //Получаем точку, по которой нужно произвести выстрел, чтобы попасть по движущейся цели
            targetingPosition = CalculateAim();

            //поворот башни к цели
            Vector3 directionTurretToTarget = targetingPosition - turretTower.position;
            //Вращение идет вокруг оси Y, поэтому вектор направления между целью и башней турели 
            //должен находится в горизонтальной плоскости
            directionTurretToTarget.y = 0;
            RotateTower(directionTurretToTarget);

            //наведение пушки на цель
            float d = Vector3.Distance(targetingPosition, turretGun.position);
            //Находим направление от точки вращения пушки к точке, на высоте которой находится цель
            //минус высота, на которой находится turretGun, иначе турель будет стрелять выше цели
            Vector3 directionToTarget = new Vector3(turretGun.forward.x, 0, turretGun.forward.z) * d
                                        + new Vector3(0, targetingPosition.y, 0)
                                        - new Vector3(0, turretGun.position.y, 0);
            RotateGun(directionToTarget);
        }

        private void RotateGun(Vector3 directionToTarget)
        {
            Quaternion rotateQuaternionGun = Quaternion.LookRotation(directionToTarget);
            float angleGun = Quaternion.Angle(turretGun.rotation, rotateQuaternionGun);
            turretGun.rotation = Quaternion.Slerp(
                turretGun.rotation,
                rotateQuaternionGun,
                Mathf.Min(1f, Time.deltaTime * rotationSpeed / angleGun)
            );
        }

        private void RotateTower(Vector3 directionToTarget)
        {
            Quaternion rotateQuaternion = Quaternion.LookRotation(directionToTarget);
            //Для вращения используется Quaternion.Slerp, 3-ий параметр, которой лежит в промежутке [0,1] включительно.
            //Чтобы вращение происходило с одинайковой скоростью, нужно расчитать значение, 
            //на которое надо поворачивать турель каждый кадр.
            //Получаем угол, на который должна повернуться башня
            float angle = Quaternion.Angle(turretTower.localRotation, rotateQuaternion);
            turretTower.localRotation = Quaternion.Slerp(
                turretTower.localRotation,
                rotateQuaternion,
                //высчитываем на сколько должна провернуться башня в течение одного кадра
                Mathf.Min(1f, Time.deltaTime * rotationSpeed / angle)
            );
        }

        #endregion
    }

    public enum TurretState
    {
        IDLE,
        ATTACK
    }
}