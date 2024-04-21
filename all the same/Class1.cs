using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using BepInEx.Configuration;
using System.Xml.Linq;


namespace ScrapsAllTheSame
{
    [BepInPlugin(modGUID, modName, modVersion)]
    // [BepInPlugin("nexor.MyFirstBepInExMod", "这是我的第2个BepIn插件", "1.0.0.0")]
    public class ScrapsAllTheSame : BaseUnityPlugin
    {
        private const string modGUID = "nexor.ScrapsAllTheSame";
        private const string modName = "ScrapsAllTheSame";
        private const string modVersion = "0.0.4";
        public ConfigEntry<float> random_scrap_day_chance;
        public ConfigEntry<int> random_scrap_type_num;
        public ConfigEntry<string> item_filter_list;
        private readonly Harmony harmony = new Harmony(modGUID);
        public static ScrapsAllTheSame Instance;
        public static BepInEx.Logging.ManualLogSource Logger;

        // 在插件启动时会直接调用Awake()方法
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            random_scrap_day_chance = Config.Bind<float>("Scraps All The Same Config",
                "Same Scraps Day Chance (%)",
                100,
                "你可以在这里修改'单种垃圾日'出现的概率\n" +
                "You can modify the probability of Same Scraps Day here");
            random_scrap_type_num = Config.Bind<int>("Scraps All The Same Config",
                "Random Scrap Type Number",
                1,
                "你可以在这里修改'单种垃圾日'随机出现多少种垃圾，默认只有一种\n" +
                "Here you can change how many types of garbage appear randomly in the 'Single Garbage Day', and there is only one type by default");
            item_filter_list = Config.Bind<string>("Scraps All The Same Config",
                "Item Filter List",
                "",
                "你可以在这里更改随机的垃圾会从哪些垃圾里面随机，如果是空的则默认从地图会生成的所有垃圾中随机，如果非空则只会从您填写的垃圾列表中随机，垃圾的名字和名字间用 英文逗号 隔开！\n" +
                "You can change which trash the randomized trash will be randomized from here, if it's empty it will be randomized by default from all the trash the map will generate, " +
                "if it's non-empty it will only be randomized from the list of trash you fill in, with English commas separating the name of the trash from the name of the trash!\n" +
                "这里附上不完全的物品名称表:[airhorn, bell, big bolt, bottles, brush, candy, cash register, chemical jug, clown horn, comedy, cookie mold pan, dust pan, easter egg, egg beater, fancy lamp, flask, gift, gold bar, golden cup, hairdryer, homemade flashbang, jar of pickles, large axle, laser pointer, magic 7 ball, magnifying glass, metal sheet, mug, old phone, painting, perfume bottle, pill bottle, plastic fish, red soda, remote, ring, rubber ducky, steering wheel, stop sign, tea kettle, teeth, toothpaste, toy cube, toy robot, tragedy, v-type engine, whoopie cushion, yield sign]");
            harmony.PatchAll();
            Logger = base.Logger;
            Logger.LogInfo("ScrapsAllTheSame 0.0.4 loaded.");
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

                // ScrapsAllTheSame.Logger.LogInfo("Setting spawnable scrap to be only one type of garbage randomly.");


                // Store the original list of spawnable scrap
                previousSpawnableScrap = ___currentLevel.spawnableScrap;
                List<SpawnableItemWithRarity> filteredSpawnableScrap = new List<SpawnableItemWithRarity>();

                // item filter
                string filterList = ScrapsAllTheSame.Instance.item_filter_list.Value.Trim();
                // ScrapsAllTheSame.Logger.LogInfo("Get string "+filterList);
                if (string.IsNullOrWhiteSpace(filterList))
                {
                    // 如果过滤列表为空，则不进行过滤，保留所有垃圾类型
                    filteredSpawnableScrap = previousSpawnableScrap;
                }
                else
                {
                    // ScrapsAllTheSame.Logger.LogInfo("Using customized scrap list");
                    string[] filtered_item_names = filterList.Split(',');
                    foreach (string filter_item_name in filtered_item_names)
                    {
                        string name = filter_item_name.Trim().ToLower();
                        // ScrapsAllTheSame.Logger.LogInfo("寻找目标"+name);
                        // 遍历之前的可生成垃圾列表，将符合过滤条件的垃圾类型加入到 filteredSpawnableScrap 中
                        for (int i = 0; i < previousSpawnableScrap.Count;i++)
                        {
                            SpawnableItemWithRarity item = previousSpawnableScrap[i];
                            if (item.spawnableItem.itemName.ToLower().Contains(name))
                            {
                                filteredSpawnableScrap.Add(item);
                                // ScrapsAllTheSame.Logger.LogInfo("找到一个");
                            }
                        }
                    }
                }


                // Create a new list with only the selected garbage
                List<SpawnableItemWithRarity> newSpawnableScrap = new List<SpawnableItemWithRarity>();

                for (int i = 0; i < ScrapsAllTheSame.Instance.random_scrap_type_num.Value; i++)
                {
                    SpawnableItemWithRarity item = filteredSpawnableScrap[Random.Range(0, filteredSpawnableScrap.Count)];
                    newSpawnableScrap.Add(item);
                }

                HUDManager.Instance.DisplayTip("Random type scrap day!", "Setting spawnable scraps to be only " + ScrapsAllTheSame.Instance.random_scrap_type_num.Value.ToString() + " type of scraps randomly.");

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

            // ScrapsAllTheSame.Logger.LogInfo("Restoring spawnable scrap to be normal.");
        }
    }
}
