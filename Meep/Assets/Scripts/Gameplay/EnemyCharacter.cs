using UnityEngine;

namespace CircularBananas.Gameplay
{
    public sealed class EnemyCharacter : MonoBehaviour
    {
        [SerializeField] private EnemyDefinition definition;
        [SerializeField] private int level = 1;
        [SerializeField] private int health;

        public EnemyDefinition Definition => definition;
        public string Name => definition == null ? gameObject.name : definition.EnemyName;
        public int Level => level;
        public int Health => health;
        public int ExperienceReward => definition == null ? 0 : definition.ExperienceReward * level;
        public int AttackPower => definition == null ? 0 : definition.AttackPower * level;
        public bool IsDefeated => health <= 0;

        public void Initialize(EnemyDefinition enemyDefinition, int enemyLevel)
        {
            definition = enemyDefinition;
            level = Mathf.Max(1, enemyLevel);
            health = definition == null ? 1 : definition.BaseHealth * level;
            gameObject.name = Name;
        }

        public int ApplyDamage(int amount)
        {
            int applied = Mathf.Max(0, amount);
            health = Mathf.Max(0, health - applied);
            return applied;
        }

        public int Attack(MeepCharacter target)
        {
            if (target == null) return 0;
            int damage = Mathf.Max(1, AttackPower - target.Chonk / 5);
            target.ApplyDamage(damage);
            return damage;
        }
    }
}