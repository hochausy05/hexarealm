using System;
using System.Collections;
using HexaRealm.Boss;
using UnityEngine;

namespace HexaRealm.Boss.Attacks
{
    [RequireComponent(typeof(BossRuntime))]
    public abstract class BossAttackBase : MonoBehaviour
    {
        [SerializeField, Min(0f)] protected float telegraphDuration = .45f;
        [SerializeField, Min(0f)] protected float activeDuration = .15f;
        [SerializeField, Min(0f)] protected float recoveryDuration = .45f;
        [SerializeField, Min(0f)] protected float cooldown = 1.2f;
        [SerializeField, Min(0f)] protected float damageMultiplier = 1f;
        [SerializeField] private GameObject telegraph;

        protected BossRuntime Runtime { get; private set; }
        public bool IsRunning { get; private set; }
        protected float RawDamage => Runtime != null && Runtime.Data != null ? Runtime.Data.Attack * damageMultiplier : 0f;
        private float nextAllowedTime;
        private Coroutine routine;
        private Action<BossAttackBase> completed;

        protected virtual void Awake() { Runtime = GetComponent<BossRuntime>(); SetTelegraph(false); }
        public abstract bool CanStart(Transform target, float distance);
        public void Begin(Transform target, Action<BossAttackBase> onCompleted)
        {
            if (IsRunning || Time.time < nextAllowedTime || target == null) return;
            completed = onCompleted; IsRunning = true; routine = StartCoroutine(Run(target));
        }
        public void Cancel()
        {
            if (!IsRunning) return;
            if (routine != null) StopCoroutine(routine);
            Finish();
        }
        protected abstract IEnumerator ExecuteActive(Transform target);
        private IEnumerator Run(Transform target)
        {
            SetTelegraph(true); yield return new WaitForSeconds(telegraphDuration); SetTelegraph(false);
            yield return ExecuteActive(target);
            yield return new WaitForSeconds(recoveryDuration); Finish();
        }
        protected void SetTelegraph(bool visible) { if (telegraph != null) telegraph.SetActive(visible); }
        protected virtual void OnCancelledOrFinished() { }
        private void Finish()
        {
            SetTelegraph(false); OnCancelledOrFinished(); IsRunning = false; routine = null; nextAllowedTime = Time.time + cooldown;
            Action<BossAttackBase> callback = completed; completed = null; callback?.Invoke(this);
        }
        protected virtual void OnDisable() { Cancel(); SetTelegraph(false); }
        private void OnValidate() { telegraphDuration = Mathf.Max(0f, telegraphDuration); activeDuration = Mathf.Max(0f, activeDuration); recoveryDuration = Mathf.Max(0f, recoveryDuration); cooldown = Mathf.Max(0f, cooldown); damageMultiplier = Mathf.Max(0f, damageMultiplier); }
    }
}
