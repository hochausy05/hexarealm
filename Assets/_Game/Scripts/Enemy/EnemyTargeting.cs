using HexaRealm.Combat;
using UnityEngine;

namespace HexaRealm.Enemy
{
    public static class EnemyTargeting
    {
        public static bool TryFindDamageReceiver(
            Transform start,
            out Transform receiverTransform,
            out IRawDamageReceiver receiver,
            out Health health)
        {
            Transform current = start;

            while (current != null)
            {
                MonoBehaviour[] behaviours = current.GetComponents<MonoBehaviour>();

                foreach (MonoBehaviour behaviour in behaviours)
                {
                    if (behaviour is IRawDamageReceiver rawDamageReceiver)
                    {
                        receiverTransform = current;
                        receiver = rawDamageReceiver;
                        health = current.GetComponent<Health>();
                        return true;
                    }
                }

                current = current.parent;
            }

            receiverTransform = null;
            receiver = null;
            health = null;
            return false;
        }
    }
}
