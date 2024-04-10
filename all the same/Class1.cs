using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using BepInEx.Configuration;


namespace ScrapsScrapsAllTheSame
{
    [BepInPlugin(modGUID, modName, modVersion)]
    // [BepInPlugin("nexor.MyFirstBepInExMod", "这是我的第2个BepIn插件", "1.0.0.0")]
    public class ScrapsAllTheSame : BaseUnityPlugin
    {
        private const string modGUID = "nexor.ScrapsAllTheSame";
        private const string modName = "ScrapsAllTheSame";
        private const string modVersion = "0.0.1";

        private readonly Harmony harmony = new Harmony(modGUID);
        public static ScrapsAllTheSame Instance;

        // 在插件启动时会直接调用Awake()方法
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            harmony.PatchAll();
            ((ScrapsAllTheSame)this).Logger.LogInfo((object)"ScrapsAllTheSame 0.0.1 loaded.");
        }
    }
}

namespace ScrapsAllTheSame.Patches.Items
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {
        private static List<SpawnableItemWithRarity> previousSpawnableScrap;

        [HarmonyPatch("SpawnScrapInLevel")]
        [HarmonyPrefix]
        [HarmonyPriority(int.MinValue)]
        private static void SpawnScrapInLevelPrefixPatch(ref SelectableLevel ___currentLevel)
        {
            Debug.Log("Setting spawnable scrap to be only one type of garbage randomly.");

            // Store the original list of spawnable scrap
            previousSpawnableScrap = ___currentLevel.spawnableScrap;

            /*for (int i = 0; i < previousSpawnableScrap.Count; i++)
            {
                string name = previousSpawnableScrap[i].spawnableItem.itemName.ToLower();
                Debug.Log(name);
            }*/
            // Randomly select one type of garbage
            SpawnableItemWithRarity selectedScrap = previousSpawnableScrap[UnityEngine.Random.Range(0, previousSpawnableScrap.Count)];

            // Create a new list with only the selected garbage
            List<SpawnableItemWithRarity> newSpawnableScrap = new List<SpawnableItemWithRarity>();
            newSpawnableScrap.Add(selectedScrap);

            // Set the current level's spawnable scrap to only contain the selected garbage
            ___currentLevel.spawnableScrap = newSpawnableScrap;
        }

        [HarmonyPatch("SpawnScrapInLevel")]
        [HarmonyPostfix]
        [HarmonyPriority(int.MaxValue)]
        private static void SpawnScrapInLevelPostfixPatch(ref SelectableLevel ___currentLevel)
        {
            // Restore the original list of spawnable scrap
            ___currentLevel.spawnableScrap = previousSpawnableScrap;

            Debug.Log("Restoring spawnable scrap to be normal.");
        }
    }
}
