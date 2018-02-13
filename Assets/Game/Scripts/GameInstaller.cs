using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    #region Editor tweakable fields

    [SerializeField] private Vector3 target;
    [SerializeField] private Monster monsterPrefab;

    #endregion

    public override void InstallBindings()
    {
        Container
            .BindInstance(target)
            .WithId("target")
            .AsSingle();

        Container
            .BindFactory<Monster, MonsterFactory>()
            .FromComponentInNewPrefab(monsterPrefab)
            .WithGameObjectName("Monster")
            .UnderTransformGroup("Monsters");
    }
}