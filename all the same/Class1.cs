using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using BepInEx.Configuration;


namespace ScrapsAllTheSame
{
    [BepInPlugin(modGUID, modName, modVersion)]
    // [BepInPlugin("nexor.MyFirstBepInExMod", "这是我的第2个BepIn插件", "1.0.0.0")]
    public class ScrapsAllTheSame : BaseUnityPlugin
    {
        private const string modGUID = "nexor.ScrapsAllTheSame";
        private const string modName = "ScrapsAllTheSame";
        private const string modVersion = "0.0.3";
        public ConfigEntry<float> random_scrap_day_chance;
        public ConfigEntry<int> random_scrap_type_num;
        private readonly Harmony harmony = new Harmony(modGUID);
        public static ScrapsAllTheSame Instance;

        // 在插件启动时会直接调用Awake()方法
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            random_scrap_day_chance = ((BaseUnityPlugin)this).Config.Bind<float>("Scraps All The Same Config",
                "Same Scraps Day Chance (%)",
                100,
                "你可以在这里修改'单种垃圾日'出现的概率\n" +
                "You can modify the probability of Same Scraps Day here");
            random_scrap_type_num = ((BaseUnityPlugin)this).Config.Bind<int>("Scraps All The Same Config",
                "Random Scrap Type Number",
                1,
                "你可以在这里修改'单种垃圾日'随机出现多少种垃圾，默认只有一种\n" +
                "Here you can change how many types of garbage appear randomly in the 'Single Garbage Day', and there is only one type by default");
            harmony.PatchAll();
            ((ScrapsAllTheSame)this).Logger.LogInfo((object)"ScrapsAllTheSame 0.0.3 loaded.");
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
            if (ScrapsAllTheSame.Instance.random_scrap_day_chance.Value > (float) UnityEngine.Random.Range(0, 100)) { 

                Debug.Log("Setting spawnable scrap to be only one type of garbage randomly.");

                // Store the original list of spawnable scrap
                previousSpawnableScrap = ___currentLevel.spawnableScrap;

                /*for (int i = 0; i < previousSpawnableScrap.Count; i++)
                {
                    string name = previousSpawnableScrap[i].spawnableItem.itemName.ToLower();
                    Debug.Log(name);
                }*/
                // Create a new list with only the selected garbage
                List<SpawnableItemWithRarity> newSpawnableScrap = new List<SpawnableItemWithRarity>();

                for (int i = 0; i < ScrapsAllTheSame.Instance.random_scrap_type_num.Value; i++)
                {
                    SpawnableItemWithRarity item = previousSpawnableScrap[Random.Range(0, previousSpawnableScrap.Count)];
                    newSpawnableScrap.Add(item);
                }

                // Set the current level's spawnable scrap to only contain the selected garbage
                ___currentLevel.spawnableScrap = newSpawnableScrap;
            }
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
