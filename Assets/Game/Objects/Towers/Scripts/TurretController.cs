using System.Collections;
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
		//Тег, который присвоен объектам, являющимися потенциальными целям для турели
		public string enemyTag = "Enemy";
		//Префаб снаряда
		public GameObject projectile;
		//скорость снаряда
		public int projectileSpeed = 10;
		private bool allowFire = true;
		//Периодичность стрельбы
		public float fireRate = 1.0f;
		//Турель будет самостоятельно выбирать ближайшую цель для атаки,
		//поиск цели будет осуществлен с помощью корутина 1 раз в searchTimeDelay секунд
		private float searchTimeDelay = 1f;
		//Текущая цель турели
		private Transform target;
		//Предыдущее положение цели
		private Vector3 previousTargetPosition;
		//Скорость цели
		private Vector3 targetSpeed;
		//возможные состояния турели
		protected enum TurretState
		{
			Idle,		//состояние покоя
			Atack,		//цель найдена, атакуем
		}
		//начальное и текущее состояние
		protected TurretState state = TurretState.Idle;
		//Точка, по которой будет стрелять турель в случае обнаружения цели, по умолчанию это сама цель
		private Vector3 targetingPosition;
		private float sqrVisionRadius;

		private StateMachine<TurretState> stateMachine;
		private float lastShotTime;
		private float lastSearchTime;
		private List<Monster> monsters;

		public virtual void Start()
		{
			//радиус обзора в квадрате, используется в FindClosestTarget;
			sqrVisionRadius = visionRadius * visionRadius;			
			
			stateMachine = new StateMachine<TurretState>();
			stateMachine.AddState(TurretState.Idle);
			stateMachine.AddState(TurretState.Atack, null, AttackUpdate);
			stateMachine.CurrentState = TurretState.Idle;

			monsters = new List<Monster>();
		}

		public virtual void Update() {

//			if (Time.time - lastSearchTime >= searchTimeDelay)
//			{
//				SearchTarget();
//			}
			
			stateMachine.Update();
		}

		public virtual void LateUpdate() {
			if (stateMachine.CurrentState == TurretState.Atack)
			{
				//В конце каждого кадра высчитываем скорость цели
				targetSpeed = (target.position - previousTargetPosition) / Time.deltaTime;
			}
		}

		//состояние покоя
		protected virtual IEnumerator Idle()
		{
			//Перешли в состояние покоя, при необходимости можно сделать дополнительные действия
			//yield return null;

			//Пока цель не найдена, турель, например, может просто вращаться по сторонам - для красоты
			while(!target)
			{
				yield return null;
			}
			//Покидаем цикл как только была найдена цель, перед переходом в состояние атаки
			//можем выполнить еще какие-то действия и поставить нужную задержку
			//yield return null;

			//Переводим турель в состоянии атаки
			state = TurretState.Atack;
			//yield return null;
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

			Transform target = null;
			
			for (int i = 0; i < monsters.Count; i++)
			{
				Vector3 monsterPositionBeforeShoot = monsters[i].GetSpeed() * timeToNextShot + monsters[i].transform.position;
				Vector3 targetingPosition = CalculateAimNew(monsterPositionBeforeShoot, monsters[i].GetSpeed());

				if (Vector3.Distance(transform.position, targetingPosition) <= visionRadius)
				{
					target = monsters[i].transform;
					break;
				}
			}

			this.target = target;
			if (target != null && stateMachine.CurrentState != TurretState.Atack)
			{
				stateMachine.CurrentState = TurretState.Atack;
			}
		}

		private Vector3 CalculateAimNew(Vector3 monsterPosition, Vector3 monsterSpeed)
		{
			Vector3 targetingPosition = monsterPosition;
			
//			for (int i = 0; i < 10; i++) {
				float dist = (turretGun.position - targetingPosition).magnitude;
				float timeToTarget = dist / projectileSpeed;
				targetingPosition = targetingPosition + monsterSpeed * timeToTarget;
//			}
			
			return targetingPosition;
		}
		
		protected virtual Vector3 CalculateAim() {
			//По умолчанию турель стреляет прямо по цели, но, если цель движется, то нужно высчитать точку,
			//которая находится перед движущейся целью и по которой будет стрелять турель.
			//То есть турель должна стрелять на опережение
			targetingPosition = target.position;

			//Высчитываем точку, перед мишенью, по которой нужно произвести выстрел, чтобы попасть по движущейся мишени
			//по идее, чем больше итераций, тем точнее будет положение точки для упреждающего выстрела
			for (int i = 0; i < 10; i++) {
				float dist = (turretGun.position - targetingPosition).magnitude;
				float timeToTarget = dist / projectileSpeed;
				targetingPosition = target.position + targetSpeed * timeToTarget;
			}

			return targetingPosition;
		}

		private void SearchTarget()
		{
			//Ближайшая цель, попавшая в радиус обзора
			Transform closest = null;
			GameObject[] targets = GameObject.FindGameObjectsWithTag(enemyTag);
			//Квадрат радиуса обзора, это значение потребуется при поиске ближайшей цели
			float distance = sqrVisionRadius;
			foreach (GameObject go in targets)
			{
				//Находим расстояние между турелью и предполагаемой целью
				Vector3 diff = go.transform.position - transform.position;
				//С точки зрения производительности быстрее сравнить квадраты расстояний,
				//чем делать лишнюю операцию извлечения квадратного корня
				float curDistance = diff.sqrMagnitude;
				//если найдена цель в радиусе поражения, то запоминаем её
				if (curDistance < distance)
				{
					closest = go.transform;
					distance = curDistance;
				}
			}
			target = closest;

			if (target != null && stateMachine.CurrentState != TurretState.Atack)
			{
				stateMachine.CurrentState = TurretState.Atack;
			}
		}

		private void AttackUpdate()
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
				) as GameObject;

				BulletController projectileController = projectileItem.GetComponent<BulletController>();
				projectileController.speed = projectileSpeed;

				lastShotTime = Time.time;
			}
		}

		private void TurnTurret()
		{
			//Каждый кадр сохраняем текущее положение цели
			previousTargetPosition = target.position;
			
			//Получаем точку, по которой нужно произвести выстрел, чтобы попасть по движущейся цели
			targetingPosition = CalculateAim();

			//поворот башни к цели
			Vector3 directionTurretToTarget = targetingPosition - turretHead.position;
			//Вращение идет вокруг оси Y, поэтому вектор направления между целью и башней турели 
			//должен находится в горизонтальной плоскости
			directionTurretToTarget.y = 0;
			Quaternion rotateQuaternion = Quaternion.LookRotation(directionTurretToTarget);
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

			//наведение пушки на цель
			float d = Vector3.Distance(targetingPosition, turretGun.position);
			//Находим направление от точки вращения пушки к точке, на высоте которой находится цель
			//минус высота, на которой находится turretGun, иначе турель будет стрелять выше цели
			Vector3 directionToTarget = new Vector3(turretGun.forward.x, 0, turretGun.forward.z) * d
			                            + new Vector3(0, targetingPosition.y, 0) 
			                            - new Vector3(0, turretGun.position.y, 0);
			Quaternion rotateQuaternionGun = Quaternion.LookRotation(directionToTarget);
			float angleGun = Quaternion.Angle(turretGun.rotation, rotateQuaternionGun);
			turretGun.rotation = Quaternion.Slerp(
				turretGun.rotation,
				rotateQuaternionGun,
				Mathf.Min(1f, Time.deltaTime * rotationSpeed / angleGun)
			);
		}
		
		#endregion
	}
}