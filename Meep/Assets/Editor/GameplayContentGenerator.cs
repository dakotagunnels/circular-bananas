using System.IO;
using System.Collections.Generic;
using CircularBananas.Gameplay;
using UnityEditor;
using UnityEngine;

namespace CircularBananas.Editor
{
    public static class GameplayContentGenerator
    {
        private static readonly HashSet<Object> CreatedThisRun = new HashSet<Object>();
        private const string Root = "Assets/GameData";
        private const string EnemiesPath = Root + "/Enemies";
        private const string SpecialsPath = Root + "/SpecialAttacks";
        private const string AdventuresPath = Root + "/Adventures";
        private const string EventsPath = Root + "/Events";
        private const string PrefabsPath = "Assets/Prefabs/Gameplay";
        private const string ArtPath = "Assets/Art/Placeholders";

        [MenuItem("Circular Bananas/Create Starter Gameplay Content")]
        public static void CreateStarterContent()
        {
            CreatedThisRun.Clear();
            EnsureFolder("Assets", "GameData");
            EnsureFolder(Root, "Enemies");
            EnsureFolder(Root, "SpecialAttacks");
            EnsureFolder(Root, "Adventures");
            EnsureFolder(Root, "Events");
            EnsureFolder("Assets", "Prefabs");
            EnsureFolder("Assets/Prefabs", "Gameplay");
            EnsureFolder("Assets", "Art");
            EnsureFolder("Assets/Art", "Placeholders");
            MigrateMisnamedEventAssets();

            EnemyDefinition[] enemies = CreateEnemies();
            SpecialAttackDefinition[] specials = CreateSpecials();
            SkillCheckDefinition skillCheck = CreateSkillCheck();
            CreateAdventures(enemies, skillCheck);
            CreatePrefabs(enemies, specials);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Gameplay content ready", "Created or kept the starter enemies, special attacks, biome adventures, and 2D prefabs under Assets.", "OK");
        }

        private static EnemyDefinition[] CreateEnemies()
        {
            string[,] rows =
            {
                { "Slime", "Beach", "10", "5", "2", "Chill" }, { "Crab", "Beach", "7", "6", "5", "Chill" },
                { "Sand Elemental", "Beach", "3", "5", "7", "Dense" }, { "Seagull", "Beach", "4", "4", "5", "Jittery" },
                { "Beached Whale", "Beach", "15", "8", "6", "Chill" }, { "Slime", "Mountain", "10", "5", "2", "Chill" },
                { "Rock", "Mountain", "8", "2", "1", "Dense" }, { "Bat", "Mountain", "3", "4", "6", "Jittery" },
                { "Harpy", "Mountain", "6", "6", "6", "Jittery" }, { "Slime", "Forest", "10", "5", "2", "Chill" },
                { "Mouse", "Forest", "5", "4", "3", "Boring" }, { "Pixie", "Forest", "4", "5", "7", "Jittery" },
                { "Pumpkin", "Forest", "3", "5", "7", "Firey" }, { "Ent", "Forest", "13", "9", "7", "Dense" },
                { "Slime", "City", "10", "5", "2", "Chill" }, { "Thug", "City", "6", "7", "7", "Boring" },
                { "Stray", "City", "5", "2", "4", "Boring" }, { "Smog", "City", "3", "5", "7", "Jittery" },
                { "Slime", "Dungeon", "10", "5", "2", "Chill" }, { "Goblin", "Dungeon", "5", "5", "5", "Boring" },
                { "Mimic", "Dungeon", "10", "10", "10", "Boring" }
            };

            EnemyDefinition[] result = new EnemyDefinition[rows.GetLength(0)];
            for (int i = 0; i < result.Length; i++)
            {
                string name = rows[i, 0];
                string biome = rows[i, 1];
                EnemyDefinition asset = GetOrCreate<EnemyDefinition>(EnemiesPath + "/" + SafeName(name) + "_" + biome + ".asset");
                if (CreatedThisRun.Contains(asset))
                {
                SerializedObject data = new SerializedObject(asset);
                Set(data, "enemyName", name);
                Set(data, "biome", biome);
                Set(data, "description", "A " + name.ToLowerInvariant() + " found in the " + biome.ToLowerInvariant() + ".");
                Set(data, "baseHealth", int.Parse(rows[i, 2]));
                Set(data, "experienceReward", int.Parse(rows[i, 3]));
                Set(data, "attackPower", int.Parse(rows[i, 4]));
                Set(data, "affinity", (int)ParseAffinity(rows[i, 5]));
                data.ApplyModifiedPropertiesWithoutUndo();
                }
                result[i] = asset;
            }
            return result;
        }

        private static SpecialAttackDefinition[] CreateSpecials()
        {
            string[,] rows =
            {
                { "Palm Strike", "Targeted strike to a vital point", "Boring", "Fitness", "5", "3" },
                { "Bright Finger", "Focus energy to ignite a fist for an explosive blow", "Firey", "Fitness", "6", "3" },
                { "Cool It!", "Mock the enemy into a self-harming rage", "Chill", "Cleverness", "10", "5" },
                { "Blow You Away!", "Wind-powered punch", "Jittery", "Cleverness", "7", "4" },
                { "Grit Those Teeth!", "Put your full weight behind a punch", "Dense", "Chonk", "10", "10" }
            };
            SpecialAttackDefinition[] result = new SpecialAttackDefinition[rows.GetLength(0)];
            for (int i = 0; i < result.Length; i++)
            {
                SpecialAttackDefinition asset = GetOrCreate<SpecialAttackDefinition>(SpecialsPath + "/" + SafeName(rows[i, 0]) + ".asset");
                if (CreatedThisRun.Contains(asset))
                {
                SerializedObject data = new SerializedObject(asset);
                Set(data, "id", i + 1);
                Set(data, "attackName", rows[i, 0]);
                Set(data, "description", rows[i, 1]);
                Set(data, "affinity", (int)ParseAffinity(rows[i, 2]));
                Set(data, "powerStat", (int)ParseStat(rows[i, 3]));
                Set(data, "basePower", int.Parse(rows[i, 4]));
                Set(data, "cooldown", int.Parse(rows[i, 5]));
                data.ApplyModifiedPropertiesWithoutUndo();
                }
                result[i] = asset;
            }
            return result;
        }

        private static SkillCheckDefinition CreateSkillCheck()
        {
            SkillCheckDefinition asset = GetOrCreate<SkillCheckDefinition>(EventsPath + "/Dungeon_Spike_Pit_Check.asset");
            if (CreatedThisRun.Contains(asset))
            {
            SerializedObject data = new SerializedObject(asset);
            Set(data, "skill", (int)MeepStat.Fitness);
            Set(data, "baseDifficulty", 5);
            Set(data, "rewardStat", (int)MeepStat.Fitness);
            Set(data, "reward", 2);
            Set(data, "rewardText", "Made it across the spike pit safely!");
            Set(data, "punishmentStat", (int)MeepStat.Fitness);
            Set(data, "punishment", -2);
            Set(data, "punishmentText", "Fell into the pit and lost some fitness.");
            data.ApplyModifiedPropertiesWithoutUndo();
            }
            return asset;
        }

        private static void CreateAdventures(EnemyDefinition[] enemies, SkillCheckDefinition skillCheck)
        {
            EnemyDefinition FindEnemy(string name, string biome)
            {
                for (int i = 0; i < enemies.Length; i++)
                    if (enemies[i].EnemyName == name && enemies[i].Biome == biome) return enemies[i];
                return null;
            }

            CreateAdventure("Beach", new[]
            {
                Event("Coconut", "A coconut fell on {name}'s head! At least they get to eat it!", MeepStat.Chonk, -2),
                Event("Sandcastle", "{name} built a sandcastle! They had a lot of fun!", MeepStat.Tiredness, -5),
                Event("SandcastleStudy", "{name} builds an impressive sandcastle and learns from it.", MeepStat.Cleverness, 2),
                Event("Seashell", "{name} contemplates a seashell and gains a deep understanding.", MeepStat.Chonk, 2),
                Event("Sunburn", "{name} thinks they're starting to get a sunburn...", MeepStat.Health, -5)
            });
            CreateAdventure("Mountain", new[]
            {
                Event("Mountain", "Chase", "{name} drops something down the mountain and chases it!", MeepStat.Fitness, 2),
                Event("Mountain", "CliffView", "{name} reaches the edge of a cliff. What a beautiful view!", MeepStat.Tiredness, 10),
                Event("Mountain", "Avalanche", "{name} accidentally starts an avalanche. Uh oh!", MeepStat.Health, -10),
                Event("Mountain", "StopAvalanche", "{name} stops an avalanche and feels stronger.", MeepStat.Chonk, 2),
                Event("Mountain", "Eggs", "{name} stumbles upon some eggs. Dinner time!", MeepStat.Health, 10)
            });
            CreateAdventure("Forest", new[]
            {
                Event("Forest", "Berries", "{name} found berries in the woods!", MeepStat.Health, 10),
                Event("Forest", "Snake", "A snake startles {name} and leaves a small bite.", MeepStat.Health, -5),
                Event("Forest", "TreeRoot", "{name} trips over a tree root. How embarrassing...", MeepStat.Health, -2),
                Event("Forest", "Flowers", "{name} finds flowers and takes a couple home.", MeepStat.Tiredness, -5),
                Event("Forest", "Poppies", "{name} finds a field of poppies. They're getting sleepy...", MeepStat.Tiredness, 10)
            });
            CreateAdventure("City", new[]
            {
                Event("City", "Alley", "{name} passes a dark alley... Did something move?", MeepStat.Tiredness, 5),
                Event("City", "Manhole", "{name} finds a manhole cover and tries to get in, but it will not budge.", MeepStat.Tiredness, 5),
                Event("City", "Gym", "{name} joins a local gym. Time to get ripped!", MeepStat.Fitness, 2),
                Event("City", "GarageSale", "{name} stumbles upon a garage sale but finds nothing they like.", MeepStat.Health, 0),
                Event("City", "Museum", "{name} visits a museum and learns a lot.", MeepStat.Cleverness, 2)
            });

            AdventureEventDefinition mimicEvent = GetOrCreate<AdventureEventDefinition>(EventsPath + "/Dungeon_Living_Chest.asset");
            SetEvent(mimicEvent, "{name} finds a chest! Oh no, it's alive!", AdventureEffect.Encounter, MeepStat.Health, 0, FindEnemy("Mimic", "Dungeon"), null);
            AdventureEventDefinition shrine = Event("Dungeon", "ExperienceShrine", "After searching for a while, {name} stumbles upon an experience shrine!", MeepStat.Experience, 50);
            AdventureEventDefinition spikes = Event("Dungeon", "SpikeTrap", "{name} fell into a spike trap!", MeepStat.Fitness, -2);
            AdventureEventDefinition rocks = Event("Dungeon", "FallingRocks", "{name} got hit by falling rocks!", MeepStat.Cleverness, -2);
            AdventureEventDefinition gas = Event("Dungeon", "PoisonGas", "{name} ran into poison gas!", MeepStat.Chonk, -2);
            AdventureEventDefinition skill = GetOrCreate<AdventureEventDefinition>(EventsPath + "/Dungeon_Spike_Pit.asset");
            SetEvent(skill, "{name} comes across a spike pit!", AdventureEffect.SkillCheck, MeepStat.Fitness, 0, null, skillCheck);
            CreateAdventure("Dungeon", new[] { mimicEvent, shrine, spikes, rocks, gas, skill });
        }

        private static AdventureEventDefinition Event(string biome, string id, string text, MeepStat stat, int value)
        {
            AdventureEventDefinition asset = GetOrCreate<AdventureEventDefinition>(EventsPath + "/" + biome + "_" + id + ".asset");
            SetEvent(asset, text, AdventureEffect.StatChange, stat, value, null, null);
            return asset;
        }

        private static AdventureEventDefinition Event(string id, string text, MeepStat stat, int value)
        {
            return Event("Beach", id, text, stat, value);
        }

        private static void MigrateMisnamedEventAssets()
        {
            string[,] moves =
            {
                { "Mountain", "Chase" }, { "Mountain", "CliffView" }, { "Mountain", "Avalanche" },
                { "Mountain", "StopAvalanche" }, { "Mountain", "Eggs" },
                { "Forest", "Berries" }, { "Forest", "Snake" }, { "Forest", "TreeRoot" },
                { "Forest", "Flowers" }, { "Forest", "Poppies" },
                { "City", "Alley" }, { "City", "Manhole" }, { "City", "Gym" },
                { "City", "GarageSale" }, { "City", "Museum" }
            };

            for (int i = 0; i < moves.GetLength(0); i++)
            {
                string oldPath = EventsPath + "/Beach_" + moves[i, 1] + ".asset";
                string newPath = EventsPath + "/" + moves[i, 0] + "_" + moves[i, 1] + ".asset";
                if (AssetDatabase.LoadAssetAtPath<AdventureEventDefinition>(oldPath) == null ||
                    AssetDatabase.LoadAssetAtPath<AdventureEventDefinition>(newPath) != null) continue;

                string error = AssetDatabase.MoveAsset(oldPath, newPath);
                if (!string.IsNullOrEmpty(error)) Debug.LogWarning("Could not rename " + oldPath + ": " + error);
            }
        }

        private static void CreateAdventure(string biome, AdventureEventDefinition[] events)
        {
            AdventureDefinition asset = GetOrCreate<AdventureDefinition>(AdventuresPath + "/" + biome + ".asset");
            SerializedObject data = new SerializedObject(asset);
            if (!CreatedThisRun.Contains(asset)) return;
            Set(data, "biome", biome);
            SerializedProperty list = data.FindProperty("events");
            list.arraySize = events.Length;
            for (int i = 0; i < events.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = events[i];
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetEvent(AdventureEventDefinition asset, string text, AdventureEffect effect, MeepStat stat, int value, EnemyDefinition encounter, SkillCheckDefinition skill)
        {
            if (!CreatedThisRun.Contains(asset)) return;
            SerializedObject data = new SerializedObject(asset);
            Set(data, "eventText", text);
            Set(data, "effect", (int)effect);
            Set(data, "affectedStat", (int)stat);
            Set(data, "value", value);
            Set(data, "encounter", encounter);
            Set(data, "skillCheck", skill);
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreatePrefabs(EnemyDefinition[] enemies, SpecialAttackDefinition[] specials)
        {
            Sprite meepSprite = CreatePlaceholderSprite("MeepPlaceholder", new Color32(242, 189, 70, 255));
            Sprite enemySprite = CreatePlaceholderSprite("EnemyPlaceholder", new Color32(220, 95, 75, 255));
            CreatePrefab("Meep", meepSprite, true);
            CreatePrefab("Enemy", enemySprite, false);
            CreateAdventureControllerPrefab(enemies, specials);
        }

        private static void CreateAdventureControllerPrefab(EnemyDefinition[] enemies, SpecialAttackDefinition[] specials)
        {
            string path = PrefabsPath + "/AdventureController.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;
            GameObject instance = new GameObject("AdventureController");
            AdventureController controller = instance.AddComponent<AdventureController>();
            SerializedObject data = new SerializedObject(controller);
            Set(data, "enemyPrefab", AssetDatabase.LoadAssetAtPath<EnemyCharacter>(PrefabsPath + "/Enemy.prefab"));
            SerializedProperty enemyList = data.FindProperty("enemies");
            enemyList.arraySize = enemies.Length;
            for (int i = 0; i < enemies.Length; i++) enemyList.GetArrayElementAtIndex(i).objectReferenceValue = enemies[i];
            SerializedProperty specialList = data.FindProperty("specialAttacks");
            specialList.arraySize = specials.Length;
            for (int i = 0; i < specials.Length; i++) specialList.GetArrayElementAtIndex(i).objectReferenceValue = specials[i];
            data.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
        }

        private static Sprite CreatePlaceholderSprite(string name, Color32 color)
        {
            string path = ArtPath + "/" + name + ".png";
            if (File.Exists(path))
            {
                AssetDatabase.ImportAsset(path);
                return AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
            Texture2D texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            Color32 clear = new Color32(0, 0, 0, 0);
            for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
            {
                float dx = x - 31.5f;
                float dy = y - 31.5f;
                bool inside = dx * dx + dy * dy < 29 * 29;
                texture.SetPixel(x, y, inside ? (Color)color : (Color)clear);
            }
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void CreatePrefab(string prefabName, Sprite sprite, bool isMeep)
        {
            string path = PrefabsPath + "/" + prefabName + ".prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;
            GameObject instance = new GameObject(prefabName);
            instance.AddComponent<SpriteRenderer>().sprite = sprite;
            instance.AddComponent<CircleCollider2D>();
            if (isMeep) instance.AddComponent<MeepCharacter>();
            else instance.AddComponent<EnemyCharacter>();
            PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
        }

        private static T GetOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            CreatedThisRun.Add(asset);
            return asset;
        }

        private static void Set(SerializedObject data, string property, string value) => data.FindProperty(property).stringValue = value;
        private static void Set(SerializedObject data, string property, int value) => data.FindProperty(property).intValue = value;
        private static void Set(SerializedObject data, string property, Object value) => data.FindProperty(property).objectReferenceValue = value;

        private static MeepAffinity ParseAffinity(string value)
        {
            switch (value)
            {
                case "Firey": return MeepAffinity.Firey;
                case "Chill": return MeepAffinity.Chill;
                case "Jittery": return MeepAffinity.Jittery;
                case "Dense": return MeepAffinity.Dense;
                default: return MeepAffinity.Boring;
            }
        }

        private static MeepStat ParseStat(string value)
        {
            switch (value)
            {
                case "Cleverness": return MeepStat.Cleverness;
                case "Chonk": return MeepStat.Chonk;
                default: return MeepStat.Fitness;
            }
        }

        private static string SafeName(string value)
        {
            foreach (char invalid in Path.GetInvalidFileNameChars()) value = value.Replace(invalid, '_');
            return value.Replace(' ', '_').Replace('!', '_');
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
        }
    }
}