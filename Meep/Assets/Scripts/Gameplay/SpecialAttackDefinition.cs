using UnityEngine;

namespace CircularBananas.Gameplay
{
    [CreateAssetMenu(menuName = "Circular Bananas/Special Attack", fileName = "SpecialAttack")]
    public sealed class SpecialAttackDefinition : ScriptableObject
    {
        [SerializeField] private int id;
        [SerializeField] private string attackName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private MeepAffinity affinity;
        [SerializeField] private MeepStat powerStat = MeepStat.Fitness;
        [SerializeField, Min(0)] private int basePower = 5;
        [SerializeField, Min(0)] private int cooldown = 3;

        public int Id => id;
        public string AttackName => attackName;
        public string Description => description;
        public MeepAffinity Affinity => affinity;
        public MeepStat PowerStat => powerStat;
        public int BasePower => basePower;
        public int Cooldown => cooldown;
    }
}