using UnityEngine;

namespace PiterGame
{
    /// <summary>
    /// Anything that reacts to Piter's sword.
    /// </summary>
    public interface IAttackable
    {
        void ReceiveAttack(int damage, Vector2 origin);
    }
}
