using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;
using Zenject;

/// <summary>
/// This class defines game rules for current scene. Provides required dependencies for other classes.
/// </summary>
public class GameInstaller : MonoInstaller
{
    #region Editor tweakable fields

    [SerializeField] private Monster monsterPrefab;

    #endregion

    public override void InstallBindings()
    {
        Container
            .BindFactory<Monster, MonsterFactory>()
            .FromComponentInNewPrefab(monsterPrefab)
            .WithGameObjectName("Monster")
            .UnderTransformGroup("Monsters");
    }
}