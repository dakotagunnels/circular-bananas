using System;
using UnityEngine;

namespace CircularBananas.Gameplay
{
    public sealed class AdventureController : MonoBehaviour
    {
        [SerializeField] private MeepCharacter player;
        [SerializeField] private EnemyCharacter enemyPrefab;
        [SerializeField] private EnemyDefinition[] enemies;
        [SerializeField] private SpecialAttackDefinition[] specialAttacks;
        [SerializeField] private Transform enemySpawnPoint;

        private AdventureDefinition activeAdventure;
        private EnemyCharacter activeEnemy;
        private bool activeEncounterFromEvent;

        public MeepCharacter Player => player;
        public EnemyCharacter ActiveEnemy => activeEnemy;
        public bool IsInCombat => activeEnemy != null;
        public bool IsAdventureActive => activeAdventure != null;

        public event Action<string> Message;
        public event Action<AdventureEventDefinition> EventResolved;
        public event Action<EnemyCharacter> EncounterStarted;
        public event Action<EnemyCharacter, bool> EncounterEnded;
        public event Action AdventureEnded;

        public void BeginAdventure(AdventureDefinition adventure, MeepCharacter activePlayer)
        {
            if (adventure == null || activePlayer == null || activePlayer.IsTired || IsInCombat)
            {
                Message?.Invoke("An adventure cannot start right now.");
                return;
            }

            activeAdventure = adventure;
            player = activePlayer;
            Message?.Invoke(player.Name + " set out for " + adventure.Biome + ".");
        }

        public void Explore()
        {
            if (activeAdventure == null || IsInCombat) return;
            if (player.IsTired)
            {
                FinishAdventure();
                return;
            }

            if (UnityEngine.Random.Range(0, 2) == 0 && TryStartEncounter(activeAdventure.Biome, null)) return;

            AdventureEventDefinition[] events = activeAdventure.Events;
            if (events == null || events.Length == 0)
            {
                Message?.Invoke("There are no events configured for this adventure.");
                FinishAdventure();
                return;
            }

            AdventureEventDefinition adventureEvent = events[UnityEngine.Random.Range(0, events.Length)];
            ResolveEvent(adventureEvent);
            player.AddTiredness(5);
            EventResolved?.Invoke(adventureEvent);
            if (IsInCombat) return;
            if (player.IsTired) FinishAdventure();
        }

        public void ResolveCombatAction(CombatAction action)
        {
            if (!IsInCombat || player == null) return;
            if (action == CombatAction.Escape)
            {
                Message?.Invoke(player.Name + " escaped from " + activeEnemy.Name + ".");
                EndEncounter(false);
                if (player.IsTired) FinishAdventure();
                return;
            }

            int damage = action == CombatAction.Special
                ? player.UseSpecial(activeEnemy)
                : player.Attack(activeEnemy);

            if (damage <= 0)
            {
                Message?.Invoke(action == CombatAction.Special
                    ? "The special attack is unavailable or still cooling down."
                    : "No damage was dealt.");
                return;
            }

            Message?.Invoke(player.Name + " dealt " + damage + " damage to " + activeEnemy.Name + ".");
            if (activeEnemy.IsDefeated)
            {
                player.AddTiredness(10);
                player.AddExperience(activeEnemy.ExperienceReward, specialAttacks);
                EndEncounter(true);
                if (player.IsTired) FinishAdventure();
                return;
            }

            activeEnemy.Attack(player);
            if (player.Health <= 0)
            {
                Message?.Invoke(player.Name + " was defeated by " + activeEnemy.Name + ".");
                EndEncounter(false);
                FinishAdventure();
            }
        }

        public void EndAdventure()
        {
            if (IsInCombat) EndEncounter(false);
            FinishAdventure();
        }

        private bool TryStartEncounter(string biome, EnemyDefinition specificEnemy, bool fromEvent = false)
        {
            EnemyDefinition definition = specificEnemy;
            if (definition == null)
            {
                EnemyDefinition[] availableEnemies = enemies ?? Array.Empty<EnemyDefinition>();
                int matchCount = 0;
                for (int i = 0; i < availableEnemies.Length; i++)
                    if (availableEnemies[i] != null && availableEnemies[i].MatchesBiome(biome)) matchCount++;
                if (matchCount == 0) return false;
                int selected = UnityEngine.Random.Range(0, matchCount);
                for (int i = 0; i < availableEnemies.Length; i++)
                {
                    if (availableEnemies[i] == null || !availableEnemies[i].MatchesBiome(biome)) continue;
                    if (selected-- == 0) { definition = availableEnemies[i]; break; }
                }
            }

            if (definition == null || enemyPrefab == null)
            {
                Message?.Invoke("Assign an enemy prefab and enemy definitions to the Adventure Controller.");
                return false;
            }

            int adjustedPlayerLevel = Mathf.Max(3, player.Level);
            int enemyLevel = UnityEngine.Random.Range(adjustedPlayerLevel - 2, adjustedPlayerLevel + 3);
            Vector3 position = enemySpawnPoint == null ? transform.position : enemySpawnPoint.position;
            activeEnemy = Instantiate(enemyPrefab, position, Quaternion.identity);
            activeEnemy.Initialize(definition, enemyLevel);
            activeEncounterFromEvent = fromEvent;
            EncounterStarted?.Invoke(activeEnemy);
            Message?.Invoke(activeEnemy.Name + " appeared!");
            return true;
        }

        private void ResolveEvent(AdventureEventDefinition adventureEvent)
        {
            if (adventureEvent == null) return;
            Message?.Invoke(adventureEvent.EventText);
            switch (adventureEvent.Effect)
            {
                case AdventureEffect.StatChange:
                    player.ChangeStat(adventureEvent.AffectedStat, adventureEvent.Value);
                    if (adventureEvent.AffectedStat != MeepStat.Experience)
                        player.AddExperience(5, specialAttacks);
                    break;
                case AdventureEffect.Encounter:
                    TryStartEncounter(activeAdventure.Biome, adventureEvent.Encounter, true);
                    break;
                case AdventureEffect.SkillCheck:
                    ResolveSkillCheck(adventureEvent.SkillCheck);
                    break;
            }
        }

        private void ResolveSkillCheck(SkillCheckDefinition check)
        {
            if (check == null) return;
            int stat = GetStat(check.Skill);
            bool passed = check.BaseDifficulty * player.Level <= stat + UnityEngine.Random.Range(0, player.Level + 1);
            if (passed)
            {
                player.ChangeStat(check.RewardStat, check.Reward);
                Message?.Invoke(check.RewardText);
            }
            else
            {
                player.ChangeStat(check.PunishmentStat, check.Punishment);
                Message?.Invoke(check.PunishmentText);
            }
        }

        private int GetStat(MeepStat stat)
        {
            switch (stat)
            {
                case MeepStat.Cleverness: return player.Cleverness;
                case MeepStat.Chonk: return player.Chonk;
                case MeepStat.Fitness: return player.Fitness;
                case MeepStat.Health: return player.Health;
                case MeepStat.Tiredness: return player.Tiredness;
                case MeepStat.Gold: return player.Gold;
                default: return player.Experience;
            }
        }

        private void EndEncounter(bool victory)
        {
            EnemyCharacter defeatedOrEscaped = activeEnemy;
            activeEnemy = null;
            if (defeatedOrEscaped != null) Destroy(defeatedOrEscaped.gameObject);
            EncounterEnded?.Invoke(defeatedOrEscaped, victory);
            if (player != null)
            {
                if (victory) player.AddTiredness(10);
                if (!activeEncounterFromEvent) player.AddTiredness(10);
            }
            activeEncounterFromEvent = false;
        }

        private void FinishAdventure()
        {
            if (activeAdventure == null) return;
            activeAdventure = null;
            if (player != null) player.Rest();
            AdventureEnded?.Invoke();
        }
    }
}