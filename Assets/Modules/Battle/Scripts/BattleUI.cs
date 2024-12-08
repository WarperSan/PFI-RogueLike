using System.Collections;
using Battle.Options;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// Class that serves as an intermediate between the UI and the manager
    /// </summary>
    public class BattleUI : MonoBehaviour
    {
        #region UI Options

        [Header("UI Options")]
        [SerializeField]
        private BattleOptions battleOptions;

        [SerializeField]
        private EnemyOptions enemyOptions;

        private UIOptions GetOptions<T>() where T : UIOptionData
        {
            var type = typeof(T);

            if (type == typeof(EnemyOptionData))
                return enemyOptions;

            if (type == typeof(BattleOptionData))
                return battleOptions;

            return null;
        }

        public void Load<T>(params T[] options) where T : UIOptionData => GetOptions<T>()?.LoadOptions(options);
        public void Move<T>(Vector2 dir) where T : UIOptionData => GetOptions<T>()?.Move(dir);
        public void Enter<T>() where T : UIOptionData => GetOptions<T>()?.Enter();
        public void Escape<T>() where T : UIOptionData => GetOptions<T>()?.Escape();
        public void ShowSelection<T>() where T : UIOptionData => GetOptions<T>()?.ShowSelection();
        public void HideSelection<T>() where T : UIOptionData => GetOptions<T>()?.HideSelection();
        public U GetSelection<T, U>() where T : UIOptionData where U : UIOption<T> => GetOptions<T>()?.GetSelection<T, U>();

        #endregion

        #region Spoiler

        [Header("Spoiler")]
        [SerializeField]
        private GameObject spoiler;

        public IEnumerator DisableSpoiler()
        {
            spoiler.SetActive(false);
            yield return null;
        }

        #endregion
    }
}