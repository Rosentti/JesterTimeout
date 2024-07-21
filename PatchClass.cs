using System.Collections.Concurrent;
using System.Collections.Generic;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace JesterTimeout;

public static class PatchClass
{
    // This is terrible, but since we don't have a way to define variables onto an existing class we'll just keep the data here and clear it when you leave the planet
    private static readonly Dictionary<JesterAI, float> timeoutDict = new();
    private static readonly object timeoutDictLock = new();

    [HarmonyPatch(typeof(JesterAI), nameof(JesterAI.Update))]
    [HarmonyPrefix]
    public static void JesterAI_Update(JesterAI __instance)
    {
        if (!__instance.IsHost) {
            return;
        }

        // This makes sure we only act when the jester is active and hunting, not when it's unwinding
        if (__instance.currentBehaviourStateIndex != 2) {
            //Debug.Log("Incorrect state for JesterTimeout " + __instance.currentBehaviourStateIndex);
            return;
        }

        //Debug.Log("JesterTimeout tick");

        lock (timeoutDictLock)
        {
            if (!timeoutDict.TryGetValue(__instance, out float timer)) {
                timeoutDict.Add(__instance, Plugin.TimeoutSecondsConfig.Value);
                Debug.Log("Jester added");

                // Don't count down on the first frame
                return;
            }

            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                Debug.Log("JesterTimeout time elapsed");
                __instance.previousState = 0;
                __instance.mainCollider.isTrigger = false;
                __instance.SetJesterInitialValues();
                __instance.SwitchToBehaviourState(0);

                // This allows the timeout to restart
                timeoutDict.Remove(__instance);
            } else {
                //Debug.Log("JesterTimeout time remaining " + timer);
                timeoutDict[__instance] = timer;
            }
        }
    }

    private static bool hasClearedList = true;

    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Update))]
    [HarmonyPostfix]
    public static void StartOfRound_Update(StartOfRound __instance)
    {
        if (!__instance.IsHost) {
            return;
        }
        
        // Only reset it once
        if (!__instance.inShipPhase) {
            hasClearedList = false;
            return;
        }

        if (hasClearedList) {
            return;
        }

        lock (timeoutDictLock)
        {
            timeoutDict.Clear();
        }

        hasClearedList = true;
        Debug.Log("Cleared jesters list");
    }

    // THIS SHIT DOESN'T WORK!!! WHYYYYYYYYY!
    // It spawns the jesters but they move next to the ship and refuse to do anything else
    // [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.LoadNewLevel))]
    // [HarmonyPostfix]
    // public static void RoundManager_LoadNewLevel(RoundManager __instance, int randomSeed, SelectableLevel newLevel)
    // {
    //     if (!Plugin.DebugSpawnJestersOutsideConfig.Value)
    //     {
    //         return;
    //     }

    //     if (!__instance.IsServer)
    //     {
    //         return;
    //     }

    //     var jesterEnemy = newLevel.Enemies.Find(e => e.enemyType.enemyName.Equals("jester", System.StringComparison.InvariantCultureIgnoreCase));
    //     foreach (var item in newLevel.Enemies)
    //     {
    //         Debug.Log("enemyType: " + item.enemyType.enemyName);
    //     }

    //     if (jesterEnemy == null) {
    //         Debug.Log("did not find enemy type Jester, can't spawn test jesters");
    //         return;
    //     }


    //     for (int i = 0; i < 5; i++)
    //     {
    //         GameObject obj = UnityEngine.Object.Instantiate(jesterEnemy.enemyType.enemyPrefab, GameObject.FindGameObjectsWithTag("OutsideAINode")[Random.Range(0, GameObject.FindGameObjectsWithTag("OutsideAINode").Length - 1)].transform.position, Quaternion.Euler(Vector3.zero));
    //         if (obj != null) {
    //             // EnemyAI ai = ai;
    //             // //RoundManager.Instance.SpawnedEnemies.Add(ai);
    //             // newLevel.Enemies.Add(enemy);
    //             // newLevel.OutsideEnemies.Add(enemy);
    //             // newLevel.DaytimeEnemies.Add(enemy);

    //             // newLevel.Enemies.Add(enemy2);
    //             // newLevel.OutsideEnemies.Add(enemy2);
    //             // newLevel.DaytimeEnemies.Add(enemy2);

    //             var ai = obj.GetComponent<JesterAI>();
    //             ai.allAINodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
    //             ai.isOutside = true;
    //             ai.isEnemyDead = false;

    //             if (GameNetworkManager.Instance.localPlayerController != null)
    //             {
    //                 ai.EnableEnemyMesh(!StartOfRound.Instance.hangarDoorsClosed || !GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom);
    //             }
    //             ai.SyncPositionToClients();

    //             obj.gameObject.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
    //             RoundManager.Instance.SpawnedEnemies.Add(ai);
    //             ai.enemyType.numberSpawned++;
    //             ai.SwitchToBehaviourState(0);
    //             obj.GetComponent<JesterAI>().updateDestinationInterval = 1;


    //             //RoundManager.Instance.SpawnEnemyOnServer(RoundManager.Instance.allEnemyVents[Random.Range(0, RoundManager.Instance.allEnemyVents.Length)].floorNode.position, RoundManager.Instance.allEnemyVents[i].floorNode.eulerAngles.y);

    //             // obj.gameObject.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);

    //             //obj.gameObject.GetComponentInChildren<JesterAI>().StartSearch();
    //             //obj.gameObject.GetComponentInChildren<JesterAI>().TargetClosestPlayer(50000, false, 360);

    //             //obj.gameObject.GetComponentInChildren<JesterAI>().StartCalculatingNextTargetNode();
    //             //obj.gameObject.GetComponentInChildren<JesterAI>().SwitchToBehaviourState(1);
    //         } else {
    //             Debug.Log("failed to spawn and we got null");
    //         }
    //     }        
    // }

    // [HarmonyPatch(typeof(Terminal), nameof(Terminal.Update))]
    // [HarmonyPostfix]
    // public static void Terminal_Update(Terminal __instance)
    // {
    //     if (!Plugin.DebugSpawnJestersOutsideConfig.Value)
    //     {
    //         return;
    //     }

    //     __instance.groupCredits = 10000;
    // }
}
