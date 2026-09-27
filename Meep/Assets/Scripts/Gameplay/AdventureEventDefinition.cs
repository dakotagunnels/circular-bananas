using UnityEngine;

namespace CircularBananas.Gameplay
{
    [CreateAssetMenu(menuName = "Circular Bananas/Adventure Event", fileName = "AdventureEvent")]
    public sealed class AdventureEventDefinition : ScriptableObject
    {
        [SerializeField, TextArea] private string eventText;
        [SerializeField] private AdventureEffect effect;
        [SerializeField] private MeepStat affectedStat;
        [SerializeField] private int value;
        [SerializeField] private EnemyDefinition encounter;
        [SerializeField] private SkillCheckDefinition skillCheck;

        public string EventText => eventText;
        public AdventureEffect Effect => effect;
        public MeepStat AffectedStat => affectedStat;
        public int Value => value;
        public EnemyDefinition Encounter => encounter;
        public SkillCheckDefinition SkillCheck => skillCheck;
    }
}