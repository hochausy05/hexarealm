using System.Collections.Generic;
using HexaRealm.Boss.Attacks;
using UnityEngine;

namespace HexaRealm.Boss
{
    [RequireComponent(typeof(BossController))]
    public sealed class BossCombatController : MonoBehaviour
    {
        [SerializeField] private BossController boss;
        [SerializeField] private List<BossAttackBase> attacks = new List<BossAttackBase>();
        private BossAttackBase activeAttack;
        public bool IsAttackRunning => activeAttack != null && activeAttack.IsRunning;

        private void Awake() { if (boss == null) boss = GetComponent<BossController>(); if (attacks.Count == 0) attacks.AddRange(GetComponents<BossAttackBase>()); }
        public void Tick(Transform target)
        {
            if (target == null || IsAttackRunning) return;
            float distance = Vector2.Distance(transform.position, target.position);
            foreach (BossAttackBase attack in attacks)
            {
                if (attack != null && attack.CanStart(target, distance)) { activeAttack = attack; attack.Begin(target, HandleAttackFinished); boss.SetAttacking(true); return; }
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
    }
}
