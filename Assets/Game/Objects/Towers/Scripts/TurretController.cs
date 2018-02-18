﻿using System.Collections;
using System.Collections.Generic;
using Gamelogic.Extensions;
using UnityEngine;

namespace Game
{
	public class TurretController : MonoBehaviour
	{
		//Голова турели, которая вращается вокруг оси Y
		public Transform turretHead;
		//Пушка турели, которая является дочерним элементом turretHead и может вращаться вокруг оси X,
		//дуло пушки должно быть расположено соотвествующим образом, 
		//то есть точка вращения (центр координат этого объекта) не должна находится в центре объекта
		public Transform turretGun;
		//Радиус обзора турели, объект попавший в этот радиус будет атакован
		public float visionRadius = 10;
		//Скорость поворота турели к цели
		public int rotationSpeed;
		//Префаб снаряда
		public GameObject projectile;
		//скорость снаряда
		public int projectileSpeed = 10;
		//Периодичность стрельбы
		public float fireRate = 1.0f;

		//Текущая цель турели
		private Monster target;
		
		//Точка, по которой будет стрелять турель в случае обнаружения цели, по умолчанию это сама цель
		private Vector3 targetingPosition;
		
		private StateMachine<TurretState> stateMachine;
		private float lastShotTime;
		private float lastSearchTime;
		private List<Monster> monsters;

		public virtual void Start()
		{			
			stateMachine = new StateMachine<TurretState>();
			stateMachine.AddState(TurretState.IDLE, null, IdleOnUpdate);
			stateMachine.AddState(TurretState.ATTACK, null, AttackOnUpdate);
			stateMachine.CurrentState = TurretState.IDLE;

			monsters = new List<Monster>();
		}

		public virtual void Update() {

			stateMachine.Update();
		}

		//Для удобства - в окне редактора покажем радиус поражения турели и некоторые дополнительные данные
		void OnDrawGizmos() 
		{
			Gizmos.DrawWireSphere(transform.position, visionRadius);
			//Текущее направление пушки
			Debug.DrawRay(turretGun.position, turretGun.forward * visionRadius, Color.blue);
			//Направление от центра вращения пушки к цели, которое в итоге должна принять пушка турели
			Debug.DrawRay(turretGun.position, (targetingPosition - turretGun.position), Color.yellow);
			//Направление, в которое "смотрит" турель
			Debug.DrawRay(turretHead.position, turretHead.forward * visionRadius, Color.red);
		}
		
		#region Unity callbacks

		private void OnTriggerEnter(Collider other)
		{
			Monster monster = other.GetComponent<Monster>();
			if (monster != null)
			{
				monsters.Add(monster);
				monster.Death += OnMonsterDeath;
				SearchTargetNew();
			}
		}

		private void OnTriggerExit(Collider other)
		{
			Monster monster = other.GetComponent<Monster>();
			if (monster != null)
			{
				monsters.Remove(monster);
				monster.Death -= OnMonsterDeath;
				SearchTargetNew();
			}
		}

		#endregion
				
		#region Private methods
		
		private void OnMonsterDeath(Monster monster)
		{
			monsters.Remove(monster);
			SearchTargetNew();
		}

		private void SearchTargetNew()
		{
			// find, whether cannon is ready to shoot
			float timeToNextShot = 0f;
        
			if (Time.time - lastShotTime < fireRate)
			{
				timeToNextShot = fireRate - (Time.time - lastShotTime);
			}

			Monster target = null;
			
			for (int i = 0; i < monsters.Count; i++)
			{
				Vector3 monsterPositionBeforeShoot = monsters[i].GetSpeed() * timeToNextShot + monsters[i].transform.position;
				Vector3 advance = CalculateAdvance(monsterPositionBeforeShoot, monsters[i].GetSpeed());

				if (Vector3.Distance(transform.position, advance) <= visionRadius)
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
			float timeToTarget = dist / projectileSpeed;
			return monsterPosition + monsterSpeed * timeToTarget;
		}
		
		protected virtual Vector3 CalculateAim() {
			//По умолчанию турель стреляет прямо по цели, но, если цель движется, то нужно высчитать точку,
			//которая находится перед движущейся целью и по которой будет стрелять турель.
			//То есть турель должна стрелять на опережение
			targetingPosition = target.transform.position;

			//Высчитываем точку, перед мишенью, по которой нужно произвести выстрел, чтобы попасть по движущейся мишени
			//по идее, чем больше итераций, тем точнее будет положение точки для упреждающего выстрела
			for (int i = 0; i < 10; i++) {
				float dist = (turretGun.position - targetingPosition).magnitude;
				float timeToTarget = dist / projectileSpeed;
				targetingPosition = target.transform.position + target.GetSpeed() * timeToTarget;
			}

			return targetingPosition;
		}

		private void IdleOnUpdate()
		{
			RotateTower(Vector3.zero);
			RotateGun(Vector3.zero);
		}

		private void AttackOnUpdate()
		{
			TurnTurret();

			//5 degrees gap - for inaccurace calculations 
			bool isCannonRotatedToTarget = Vector3.Angle(turretGun.forward, targetingPosition - turretGun.position) <= 5;

			//если можем сделать выстрел, то стреляем
			if (Time.time - lastShotTime >= fireRate && isCannonRotatedToTarget) {

				GameObject projectileItem = Instantiate(
					projectile,
					turretGun.position,
					Quaternion.FromToRotation (projectile.transform.forward, turretGun.forward)
				);

				BulletController projectileController = projectileItem.GetComponent<BulletController>();
				projectileController.speed = projectileSpeed;

				lastShotTime = Time.time;
			}
		}

		private void TurnTurret()
		{
			//Получаем точку, по которой нужно произвести выстрел, чтобы попасть по движущейся цели
			targetingPosition = CalculateAim();

			//поворот башни к цели
			Vector3 directionTurretToTarget = targetingPosition - turretHead.position;
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
			float angle = Quaternion.Angle(turretHead.localRotation, rotateQuaternion);
			turretHead.localRotation = Quaternion.Slerp(
				turretHead.localRotation,
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