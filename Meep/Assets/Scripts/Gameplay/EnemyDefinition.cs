using UnityEngine;

namespace CircularBananas.Gameplay
{
    [CreateAssetMenu(menuName = "Circular Bananas/Enemy", fileName = "Enemy")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [SerializeField] private string enemyName;
        [SerializeField] private string biome;
        [SerializeField, TextArea] private string description;
        [SerializeField, Min(1)] private int baseHealth = 10;
        [SerializeField, Min(0)] private int experienceReward = 5;
        [SerializeField, Min(0)] private int attackPower = 2;
        [SerializeField] private MeepAffinity affinity;

        public string EnemyName => enemyName;
        public string Biome => biome;
        public string Description => description;
        public int BaseHealth => baseHealth;
        public int ExperienceReward => experienceReward;
        public int AttackPower => attackPower;
        public MeepAffinity Affinity => affinity;

        public bool MatchesBiome(string biomeName) => string.Equals(biome, biomeName, System.StringComparison.OrdinalIgnoreCase);
    }
}