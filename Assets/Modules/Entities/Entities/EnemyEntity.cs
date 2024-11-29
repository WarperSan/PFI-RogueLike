using System.Collections;
using Entities.Interfaces;
using UnityEngine;

namespace Entities
{
    public class EnemyEntity : GridEntity, ITurnable, IMovable, IEventable
    {
        #region ITurnable

        /// <inheritdoc/>
        IEnumerator ITurnable.Think()
        {
            yield return Movement.LEFT;
        }

        #endregion

        #region IEventable

        /// <inheritdoc/>
        public void OnEntityLand(GridEntity entity)
        {
            // If not player, skip
            if (entity is not PlayerEntity)
                return;

            Debug.Log("hit");
        }

        #endregion
    }
}