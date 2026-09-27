using UnityEngine;

namespace CircularBananas.Gameplay
{
    [CreateAssetMenu(menuName = "Circular Bananas/Adventure", fileName = "Adventure")]
    public sealed class AdventureDefinition : ScriptableObject
    {
        [SerializeField] private string biome;
        [SerializeField] private AdventureEventDefinition[] events;

        public string Biome => biome;
        public AdventureEventDefinition[] Events => events;
    }
}