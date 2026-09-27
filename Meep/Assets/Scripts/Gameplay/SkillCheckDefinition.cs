using UnityEngine;

namespace CircularBananas.Gameplay
{
    [CreateAssetMenu(menuName = "Circular Bananas/Skill Check", fileName = "SkillCheck")]
    public sealed class SkillCheckDefinition : ScriptableObject
    {
        [SerializeField] private MeepStat skill = MeepStat.Fitness;
        [SerializeField, Min(0)] private int baseDifficulty = 5;
        [SerializeField] private MeepStat rewardStat = MeepStat.Fitness;
        [SerializeField] private int reward = 2;
        [SerializeField, TextArea] private string rewardText;
        [SerializeField] private MeepStat punishmentStat = MeepStat.Fitness;
        [SerializeField] private int punishment = -2;
        [SerializeField, TextArea] private string punishmentText;

        public MeepStat Skill => skill;
        public int BaseDifficulty => baseDifficulty;
        public MeepStat RewardStat => rewardStat;
        public int Reward => reward;
        public string RewardText => rewardText;
        public MeepStat PunishmentStat => punishmentStat;
        public int Punishment => punishment;
        public string PunishmentText => punishmentText;
    }
}