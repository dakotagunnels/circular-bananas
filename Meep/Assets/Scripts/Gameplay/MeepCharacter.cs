using System;
using UnityEngine;

namespace CircularBananas.Gameplay
{
    public sealed class MeepCharacter : MonoBehaviour
    {
        [SerializeField] private string meepName = "Tomo";
        [SerializeField] private MeepAffinity affinity;
        [SerializeField] private int health = 30;
        [SerializeField] private int maxHealth = 30;
        [SerializeField] private int tiredness;
        [SerializeField] private int cleverness = 5;
        [SerializeField] private int chonk = 5;
        [SerializeField] private int fitness = 5;
        [SerializeField] private int selfAwareness;
        [SerializeField] private int level = 1;
        [SerializeField] private int gold;
        [SerializeField] private int experience;
        [SerializeField] private SpecialAttackDefinition specialAttack;
        [SerializeField] private int specialCooldown;

        public string Name { get => meepName; set => meepName = string.IsNullOrWhiteSpace(value) ? "Tomo" : value.Trim(); }
        public MeepAffinity Affinity { get => affinity; set => affinity = value; }
        public int Health => health;
        public int MaxHealth => maxHealth;
        public int Tiredness => tiredness;
        public int Cleverness => cleverness;
        public int Chonk => chonk;
        public int Fitness => fitness;
        public int SelfAwareness => selfAwareness;
        public int Level => level;
        public int Gold => gold;
        public int Experience => experience;
        public SpecialAttackDefinition SpecialAttack => specialAttack;
        public int SpecialCooldown => specialCooldown;
        public bool IsTired => tiredness > 50 * level;
        public event Action<int> LeveledUp;

        public static MeepCharacter Create(GameObject target, string characterName = "Tomo")
        {
            MeepCharacter meep = target.GetComponent<MeepCharacter>();
            if (meep == null) meep = target.AddComponent<MeepCharacter>();
            meep.InitializeRandom(characterName);
            return meep;
        }

        public void InitializeRandom(string characterName = "Tomo")
        {
            Name = characterName;
            affinity = (MeepAffinity)UnityEngine.Random.Range(0, 5);
            cleverness = UnityEngine.Random.Range(1, 10);
            chonk = UnityEngine.Random.Range(1, 10);
            fitness = UnityEngine.Random.Range(1, 10);
            selfAwareness = UnityEngine.Random.Range(1000, 10000);
            tiredness = 0;
            maxHealth = UnityEngine.Random.Range(20, 51);
            health = maxHealth;
            level = 1;
            gold = 0;
            experience = 0;
            specialAttack = null;
            specialCooldown = 0;
        }

        public void SetSpecialAttack(SpecialAttackDefinition definition)
        {
            specialAttack = definition;
            specialCooldown = 0;
        }

        public int ApplyDamage(int amount)
        {
            int applied = Mathf.Max(0, amount);
            health = Mathf.Max(0, health - applied);
            return applied;
        }

        public void RestoreHealth(int amount)
        {
            health = Mathf.Clamp(health + Mathf.Max(0, amount), 0, maxHealth);
        }

        public void AddTiredness(int amount)
        {
            tiredness = Mathf.Max(0, tiredness + amount);
        }

        public void Rest()
        {
            tiredness = 0;
            health = maxHealth;
        }

        public void ChangeStat(MeepStat stat, int amount)
        {
            switch (stat)
            {
                case MeepStat.Health:
                    if (amount < 0) ApplyDamage(-amount); else RestoreHealth(amount);
                    break;
                case MeepStat.Tiredness: AddTiredness(amount); break;
                case MeepStat.Cleverness: cleverness = Mathf.Max(0, cleverness + amount); break;
                case MeepStat.Chonk: chonk = Mathf.Max(0, chonk + amount); break;
                case MeepStat.Fitness: fitness = Mathf.Max(0, fitness + amount); break;
                case MeepStat.Gold: gold = Mathf.Max(0, gold + amount); break;
                case MeepStat.Experience: AddExperience(amount); break;
            }
        }

        public void AddExperience(int amount, SpecialAttackDefinition[] availableSpecials = null)
        {
            experience = Mathf.Max(0, experience + amount);
            while (experience >= level * 200)
            {
                experience -= level * 200;
                level++;
                ApplyAffinityGrowth();
                maxHealth += chonk / 2;
                health = maxHealth;
                if (level >= 3 && specialAttack == null)
                    specialAttack = FindAffinitySpecial(availableSpecials);
                LeveledUp?.Invoke(level);
            }
        }

        public int Attack(EnemyCharacter target)
        {
            if (target == null) return 0;
            int damage = fitness * 2;
            target.ApplyDamage(damage);
            AdvanceSpecialCooldown();
            return damage;
        }

        public int UseSpecial(EnemyCharacter target)
        {
            if (target == null || specialAttack == null || specialCooldown > 0) return 0;
            int stat = specialAttack.PowerStat == MeepStat.Cleverness ? cleverness
                : specialAttack.PowerStat == MeepStat.Chonk ? chonk : fitness;
            int damage = Mathf.Max(0, stat * specialAttack.BasePower);
            target.ApplyDamage(damage);
            specialCooldown = specialAttack.Cooldown;
            return damage;
        }

        public void AdvanceSpecialCooldown()
        {
            if (specialCooldown > 0) specialCooldown--;
        }

        private void ApplyAffinityGrowth()
        {
            switch (affinity)
            {
                case MeepAffinity.Boring:
                    chonk += UnityEngine.Random.Range(0, 5); cleverness += UnityEngine.Random.Range(0, 5); fitness += UnityEngine.Random.Range(0, 5); break;
                case MeepAffinity.Firey:
                    chonk += UnityEngine.Random.Range(0, 2); cleverness += UnityEngine.Random.Range(0, 3); fitness += UnityEngine.Random.Range(2, 6); break;
                case MeepAffinity.Chill:
                    chonk += UnityEngine.Random.Range(0, 4); cleverness += UnityEngine.Random.Range(3, 9); fitness += UnityEngine.Random.Range(0, 3); break;
                case MeepAffinity.Jittery:
                    chonk += UnityEngine.Random.Range(0, 2); cleverness += UnityEngine.Random.Range(0, 8); fitness += UnityEngine.Random.Range(0, 8); break;
                case MeepAffinity.Dense:
                    chonk += UnityEngine.Random.Range(3, 8); cleverness += UnityEngine.Random.Range(0, 2); fitness += UnityEngine.Random.Range(0, 3); break;
            }
        }

        private SpecialAttackDefinition FindAffinitySpecial(SpecialAttackDefinition[] available)
        {
            if (available == null) return null;
            for (int i = 0; i < available.Length; i++)
                if (available[i] != null && available[i].Affinity == affinity) return available[i];
            return null;
        }
    }
}