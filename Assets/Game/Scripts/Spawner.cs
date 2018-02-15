using UnityEngine;
using System.Collections;
using Game;
using Gamelogic.Extensions;
using Zenject;

/// <summary>
/// Spawner does not contains Monster prefab for extension purposes, i.e. gamedesigner may set different rules/monster types. Thus, Monster prefab should be set up in SceneContext.
/// </summary>
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
        if (Time.time - lastSpawnTime >= interval)
        {
            pool.GetNewObjectSilently(10);
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
        monster.Refresh();
        monster.gameObject.SetActive(true);
    }

    private void MonsterSetToSleep(Monster monster)
    {
        monster.gameObject.SetActive(false);
    }

    #endregion
}