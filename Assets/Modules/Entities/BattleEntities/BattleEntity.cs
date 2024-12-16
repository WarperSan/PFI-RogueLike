using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Weapons;

namespace BattleEntity
{
    [Flags]
    public enum Type
    {
        NONE = 0,
        NORMAL = 1 << 1,
        UNDEAD = 1 << 2,
        GHOST = 1 << 3,
        GIANT = 1 << 4,
        ANIMAL = 1 << 5,
    }

    public abstract class BattleEntity
    {
        public int Attack { get; protected set; }

        #region Health

        public int Health { get; protected set; }
        public bool IsDead { get; private set; }

        public void TakeAttack(WeaponInstance weapon)
        {
            int damage = Mathf.RoundToInt(weapon.GetDamage() * CalculateEffectiveness(weapon.GetAttackType()) / 100f);
            TakeDamage(damage);
        }

        public void TakeDamage(int damage)
        {
            Health -= damage;

            if (Health <= 0)
            {
                IsDead = true;
                OnDeath(damage);
                return;
            }

            OnHit(damage);
        }

        /// <summary>
        /// Called when this entity gets hit
        /// </summary>
        protected virtual void OnHit(int damage) { }

        /// <summary>
        /// Called when this entity dies
        /// </summary>
        protected virtual void OnDeath(int damage) { }

        #endregion

        #region Type

        public Type Type { get; protected set; }

        // x0   = IMMUNE
        // x0.5 = NOT EFFECTIVE
        // x1   = NORMAL
        // x1.5 = VERY EFFECTIVE
        // x3   = SUPER EFFECTIVE

        private static float[,] TYPE_CHART = {
            // ATK \ DEF   NONE NORMAL UNDEAD GHOST GIANT ANIMAL
            /* NONE */   {  1f,    1f,     1f,   1f,   1f,   1f },
            /* NORMAL */ {  1f,    1f,   0.5f, 0.5f,   1f, 1.5f }, // VERY WEAK
            /* UNDEAD */ {  1f,    3f,   0.5f,   1f, 0.5f,   3f }, // GLASS CANNON
            /* GHOST */  {  1f,  1.5f,   0.5f,   3f, 0.5f, 1.5f }, // SAFER GLASS CANNON
            /* GIANT */  {  1f,  1.5f,   1.5f, 0.5f,   1f, 1.5f }, // BALANCED
            /* ANIMAL */ {  1f,  1.5f,   0.5f,   1f, 0.5f,   3f }, // WEAK
        };

        public float CalculateEffectiveness(Type attackType)
        {
            // Find index of attack type
            int[] attackIndexes = GetTypeIndexes(attackType);

            // Find index of defence type
            int[] defenceIndexes = GetTypeIndexes(Type);

            // Get multiplier
            float percent = 100f;

            foreach (var attackIndex in attackIndexes)
            {
                foreach (var defenceIndex in defenceIndexes)
                    percent *= TYPE_CHART[attackIndex, defenceIndex];
            }


            return percent;
        }

        private static int[] GetTypeIndexes(Type type)
        {
            var types = Enum.GetValues(typeof(Type));
            var indexes = new List<int>();

            for (int i = 0; i < types.Length; i++)
            {
                Type item = (Type)types.GetValue(i);

                if ((type & item) != 0)
                    indexes.Add(i);
            }

            return indexes.ToArray();
        }

        public static Type[] GetTypes(Type type)
        {
            var types = Enum.GetValues(typeof(Type));
            var indexes = new List<Type>();

            for (int i = 0; i < types.Length; i++)
            {
                Type item = (Type)types.GetValue(i);

                if ((type & item) != 0)
                    indexes.Add(item);
            }

            return indexes.ToArray();
        }

        public static string GetIcons(Type type)
        {
            StringBuilder builder = new();

            var types = Enum.GetValues(typeof(Type));
            foreach (Type item in types)
            {
                if ((type & item) != 0)
                {
                    builder.Append(
                        string.Format(
                            "<sprite name=\"icon_type_{0}\" color={1}>",
                            item.ToString().ToLower(),
                            "#" + ColorUtility.ToHtmlStringRGB(GetTypeColor(item))
                        )
                    );
                }
            }


            return builder.ToString();
        }

        public static Color GetTypeColor(Type type)
        {
            var types = GetTypes(type);

            var color = types[0] switch
            {
                Type.NONE => "#D3D3D3",
                Type.NORMAL => "#B9E5EB",
                Type.UNDEAD => "#6BD36B",
                Type.GHOST => "#AA74FF",
                Type.GIANT => "#FF5E47",
                Type.ANIMAL => "#FF8B00",
                _ => "white"
            };

            return ColorUtility.TryParseHtmlString(color, out Color clr) ? clr : Color.white;
        }

        #endregion
    }
}