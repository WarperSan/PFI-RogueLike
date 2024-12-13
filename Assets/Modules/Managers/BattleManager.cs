using System.Collections;
using System.Collections.Generic;
using Battle;
using Battle.Options;
using BattleEntity;
using Enemies;
using Entities;
using TMPro;
using UnityEngine;
using Weapons;

namespace Managers
{
    public class BattleManager : MonoBehaviour
    {
        public EnemySO enemy1;
        public EnemySO enemy2;
        public EnemySO enemy3;
        private EnemyEntity enemyEntity;
        private BattleEnemyEntity[] battleEnemyEntities;
        private BattlePlayerEntity battlePlayerEntity;

        public IEnumerator StartBattle(EnemyEntity enemyEntity, PlayerEntity playerEntity)
        {
            this.enemyEntity = enemyEntity;
            this.battlePlayerEntity = new BattlePlayerEntity(playerEntity);
            yield return StartBattleTransition();
            yield return FindBattleUI();

            var weapon = playerEntity.GetWeapon();
            LoadBattleOptions();

            battleEnemyEntities = GenerateEnemies(enemyEntity);

            LoadEnemyOptions(weapon, battleEnemyEntities);

            yield return battleUI.DisableSpoiler();

            EnableBattleOption();
            AddInputs();
        }

        public void EndBattle(bool isVictory)
        {
            StartCoroutine(EndBattleCoroutine(isVictory));
        }

        private IEnumerator EndBattleCoroutine(bool isVictory)
        {
            RemoveInputs();
            DisableBattleOption();
            DisableEnemyOption();

            yield return battleUI.EnableSpoiler();

            GameManager.Instance.EndBattle(isVictory, enemyEntity);
        }

        #region Battle UI

        private BattleUI battleUI;

        private IEnumerator FindBattleUI()
        {
            do
            {
                battleUI = FindObjectOfType<BattleUI>();
                yield return null;
            } while (battleUI == null);
        }

        public IEnumerator DeadPlayerTextFadeIn(TextMeshProUGUI text)
        {
            const float FADE_TIME = 4f;

            text.text = $"You died \n at Level {GameManager.Instance.Level.Index + 1}";
            text.gameObject.SetActive(true);
            var textColor = text.color;
            textColor.a = 0f;
            text.color = textColor;

            yield return new WaitForSeconds(0.5f);

            for (int i = 1; i <= FADE_TIME; i++)
            {
                textColor.a = 1f / FADE_TIME * i;
                text.color = textColor;
                yield return new WaitForSeconds(0.2f);
            }

            yield return new WaitForSeconds(5);

            yield return GameManager.Instance.ReturnToTitle();
        }

        #endregion

        #region Battle Options

        private bool isSelectingBattleOption;

        private void LoadBattleOptions()
        {
            battleUI.Load(new BattleOptionData[] {
                new() {
                    Text = "Attack",
                    OnEnter = OnAttackPressed
                },
                new() {
                    Text = "Heal"
                },
                //new()
                //{
                //    Text = "Nuke",
                //    OnEnter = () => EndBattle(true)
                //},
                //new()
                //{
                //    Text = "Death",
                //    OnEnter = () => EndBattle(false)
                //}
            });
        }

        private void OnAttackPressed()
        {
            DisableBattleOption();
            EnableEnemyOption();
        }

        private void EnableBattleOption()
        {
            isSelectingBattleOption = true;
            battleUI.ShowSelection<BattleOptionData>();
        }

        private void DisableBattleOption()
        {
            isSelectingBattleOption = false;
            battleUI.HideSelection<BattleOptionData>();
        }

        #endregion

        #region Damage Player

        public void PlayerTakeDamage(int damage)
        {
            battlePlayerEntity.TakeDamage(damage);
            if (battlePlayerEntity.IsDead)
                EndBattle(false);
        }

        #endregion

        #region Enemy Options

        private bool isSelectingEnemyOption;

        private void LoadEnemyOptions(WeaponInstance weapon, params BattleEnemyEntity[] entities)
        {
            var options = new EnemyOptionData[entities.Length];

            for (int i = 0; i < options.Length; i++)
            {
                options[i] = new EnemyOptionData()
                {
                    Weapon = weapon,
                    Entity = entities[i],
                    OnEnter = OnEnemyConfirmed,
                    OnEscape = OnEnemyEscape
                };
            }

            battleUI.Load(options);
        }

        private BattleEnemyEntity[] GenerateEnemies(EnemyEntity enemy)
        {
            var random = GameManager.Instance.Level.Random;
            var allEnemies = GameManager.Instance.allEnemies;

            var enemies = new List<BattleEnemyEntity>();

            if (random.NextDouble() <= 0.9f)
                enemies.Add(new(allEnemies[random.Next(0, allEnemies.Length)]));

            enemies.Add(new(enemy.EnemyData));

            if (random.NextDouble() <= 0.9f)
                enemies.Add(new(allEnemies[random.Next(0, allEnemies.Length)]));

            return enemies.ToArray();
        }

        private void OnEnemyConfirmed()
        {
            RemoveInputs();
            DisableEnemyOption();

            var enemyOption = battleUI.GetSelection<EnemyOptionData, EnemyOption>();
            var enemy = enemyOption.GetOption();

            enemy.Entity.TakeAttack(enemy.Weapon);

            if (VerifyEnemiesState())
            {
                EnemyTurn();
                EnableBattleOption();
                AddInputs();
            }
            else
            {
                // All enemies dead, victory
                EndBattle(true);
            }
        }

        private void OnEnemyEscape()
        {
            isSelectingEnemyOption = false;
            battleUI.HideSelection<EnemyOptionData>();

            EnableBattleOption();
        }

        private void EnableEnemyOption()
        {
            isSelectingEnemyOption = true;
            battleUI.ShowSelection<EnemyOptionData>();
        }

        private void DisableEnemyOption()
        {
            isSelectingEnemyOption = false;
            battleUI.HideSelection<EnemyOptionData>();
        }

        private bool VerifyEnemiesState()
        {
            for (int i = 0; i < battleEnemyEntities.Length; i++)
            {
                // If at least one enemy alive
                if (!battleEnemyEntities[i].IsDead)
                    return true;
            }

            // If all enemies dead
            return false;
        }

        private void EnemyTurn()
        {
            int rdmDamage = Random.Range(5, 11);

            PlayerTakeDamage(rdmDamage);
        }

        #endregion

        #region Battle Transition

        [Header("Battle Transition")]
        [SerializeField]
        private Material transitionMaterial;

        [SerializeField]
        private Texture[] transitionTextures;

        private IEnumerator StartBattleTransition() => BattleTransition.ExecuteTransition(transitionMaterial, transitionTextures, 0.9f);

#if UNITY_EDITOR
        // For keeping consistency in editor
        private void OnApplicationQuit() => BattleTransition.ResetMaterial(transitionMaterial);
#endif

        #endregion

        #region Inputs

        private void Move(Vector2 dir)
        {
            if (isSelectingBattleOption)
            {
                battleUI.Move<BattleOptionData>(dir);
                return;
            }

            if (isSelectingEnemyOption)
            {
                battleUI.Move<EnemyOptionData>(dir);
                return;
            }
        }

        private void Enter()
        {
            if (isSelectingBattleOption)
            {
                battleUI.Enter<BattleOptionData>();
                return;
            }

            if (isSelectingEnemyOption)
            {
                battleUI.Enter<EnemyOptionData>();
                return;
            }
        }

        private void Escape()
        {
            if (isSelectingBattleOption)
            {
                battleUI.Escape<BattleOptionData>();
                return;
            }

            if (isSelectingEnemyOption)
            {
                battleUI.Escape<EnemyOptionData>();
                return;
            }
        }

        /// <summary>
        /// Adds the inputs for this object
        /// </summary>
        private void AddInputs()
        {
            InputManager.Instance.OnMoveUI.AddListener(Move);
            InputManager.Instance.OnEnterUI.AddListener(Enter);
            InputManager.Instance.OnEscapeUI.AddListener(Escape);
        }

        /// <summary>
        /// Removes the inputs for this object
        /// </summary>
        private void RemoveInputs()
        {
            InputManager.Instance.OnMoveUI.RemoveListener(Move);
            InputManager.Instance.OnEnterUI.RemoveListener(Enter);
            InputManager.Instance.OnEscapeUI.RemoveListener(Escape);
        }

        #endregion
    }
}