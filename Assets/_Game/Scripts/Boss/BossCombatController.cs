using System.Collections.Generic;
using HexaRealm.Boss.Attacks;
using UnityEngine;

namespace HexaRealm.Boss
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BossController))]
    public sealed class BossCombatController : MonoBehaviour
    {
        [SerializeField] private BossController boss;
        [SerializeField] private List<BossAttackBase> attacks = new List<BossAttackBase>();

        private BossAttackBase activeAttack;

        public bool IsAttackRunning => activeAttack != null && activeAttack.IsRunning;
        public int AttackCount => attacks != null ? attacks.Count : 0;
        internal IReadOnlyList<BossAttackBase> ConfiguredAttacks => attacks;
        public bool HasResolvedReferences
        {
            get
            {
                if (boss == null || boss.gameObject != gameObject || attacks == null || attacks.Count == 0) return false;
                foreach (BossAttackBase attack in attacks)
                {
                    if (attack == null || attack.gameObject != gameObject || !attack.enabled || !attack.HasValidRuntime) return false;
                }

                return true;
            }
        }

        private void Awake()
        {
            if (!ResolveAndValidateReferences())
            {
                Debug.LogError("BossCombatController requires a BossController and at least one valid, enabled BossAttackBase on the same Boss root.", this);
                enabled = false;
            }
        }

        public void Tick(Transform target)
        {
            if (target == null || IsAttackRunning || !HasResolvedReferences) return;
            float distance = Vector2.Distance(transform.position, target.position);
            foreach (BossAttackBase attack in attacks)
            {
                if (attack == null || !attack.CanStart(target, distance)) continue;
                if (!attack.Begin(target, HandleAttackFinished)) continue;
                activeAttack = attack;
                boss.SetAttacking(true);
                return;
            }
        }

        public void CancelActiveAttack()
        {
            if (activeAttack != null) activeAttack.Cancel();
            activeAttack = null;
        }

        private void HandleAttackFinished(BossAttackBase attack)
        {
            if (activeAttack != attack) return;
            activeAttack = null;
            boss.SetAttacking(false);
        }

        private void OnDisable()
        {
            CancelActiveAttack();
        }

        private void ResolveReferences()
        {
            if (boss == null || boss.gameObject != gameObject) boss = GetComponent<BossController>();
            if (attacks == null) attacks = new List<BossAttackBase>();

            var uniqueAttacks = new HashSet<BossAttackBase>();
            for (int index = 0; index < attacks.Count;)
            {
                BossAttackBase attack = attacks[index];
                if (attack == null || attack.gameObject != gameObject || !attack.enabled
                    || !attack.ResolveAndValidateRuntime() || !uniqueAttacks.Add(attack))
                {
                    attacks.RemoveAt(index);
                    continue;
                }

                index++;
            }

            if (attacks.Count > 0) return;
            foreach (BossAttackBase attack in GetComponents<BossAttackBase>())
            {
                if (attack.enabled && attack.ResolveAndValidateRuntime()) attacks.Add(attack);
            }
        }

        internal bool ResolveAndValidateReferences()
        {
            ResolveReferences();
            return HasResolvedReferences;
        }
    }
}
