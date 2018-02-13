using UnityEngine;
using System.Collections;
using Game;
using Gamelogic.Extensions;
using Zenject;

public class Spawner : MonoBehaviour
{
    #region Editor tweakable fields

    [SerializeField] private float interval = 3;

    #endregion

    #region Fields

    private float lastSpawnTime;
    private Pool<Monster> pool;

    #endregion

    #region Unity callbacks

    void Update()
    {
        if (Time.time - lastSpawnTime < interval)
        {
            if (!pool.IsObjectAvailable)
            {
                pool.IncCapacity(10);
            }
            pool.GetNewObject();
            
            lastSpawnTime = Time.time;
        }
    }

    #endregion
    
    #region Public methods

    public void Release(Monster monster)
    {
        pool.Release(monster);
    }
    
    #endregion

    #region Private methods

    [Inject]
    private void Init(MonsterFactory factory)
    {
        pool = new Pool<Monster>(10,
                                 factory.Create,
                                 monster => Destroy(monster.gameObject),
                                 MonsterWakeUp,
                                 MonsterSetToSleep);
    }

    private void MonsterWakeUp(Monster monster)
    {
        monster.gameObject.transform.position = transform.position;
        monster.gameObject.SetActive(true);
    }

    private void MonsterSetToSleep(Monster monster)
    {
        monster.gameObject.SetActive(false);
    }

    #endregion
}