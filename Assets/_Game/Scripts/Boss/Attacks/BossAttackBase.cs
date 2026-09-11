using System;
using System.Collections;
using HexaRealm.Combat;
using UnityEngine;

namespace HexaRealm.Boss.Attacks
{
    [RequireComponent(typeof(BossRuntime))]
    public abstract class BossAttackBase : MonoBehaviour
    {
        [SerializeField, Min(0f)] protected float telegraphDuration = .45f;
        [SerializeField, Min(0f)] protected float activeDuration = .15f;
        [SerializeField, Min(0f)] protected float recoveryDuration = .45f;
        [SerializeField, Min(0f)] private float cooldown = 1.2f;
        [SerializeField, Min(0f)] private float damageMultiplier = 1f;
        [SerializeField] private GameObject telegraph;

        protected BossRuntime Runtime { get; private set; }
        protected float RawDamage => Runtime != null && Runtime.HasValidData ? Runtime.Data.Attack * damageMultiplier : 0f;
        public bool IsRunning { get; private set; }
        internal bool HasValidRuntime
        {
            get
            {
                if (Runtime == null || Runtime.gameObject != gameObject) Runtime = GetComponent<BossRuntime>();
                return Runtime != null && Runtime.HasValidData;
            }
        }

        private float nextAllowedTime;
        private Coroutine routine;
        private Action<BossAttackBase> completed;

        protected virtual void Awake()
        {
            ResolveAndValidateRuntime();
            SetTelegraph(false);
        }

        public abstract bool CanStart(Transform target, float distance);

        public bool Begin(Transform target, Action<BossAttackBase> onCompleted)
        {
            if (!isActiveAndEnabled || IsRunning || Time.time < nextAllowedTime || target == null || !ResolveAndValidateRuntime()) return false;
            completed = onCompleted;
            IsRunning = true;
            routine = StartCoroutine(Run(target));
            return true;
        }

        public void Cancel()
        {
            if (!IsRunning) return;
            if (routine != null) StopCoroutine(routine);
            Finish();
        }

        protected bool IsReady => !IsRunning && Time.time >= nextAllowedTime;
        protected abstract IEnumerator ExecuteActive(Transform target);

        protected static IRawDamageReceiver FindDamageReceiver(Transform start)
        {
            for (Transform current = start; current != null; current = current.parent)
            {
                foreach (MonoBehaviour behaviour in current.GetComponents<MonoBehaviour>())
                {
                    if (behaviour is IRawDamageReceiver receiver) return receiver;
                }
            }

            return null;
        }

        private IEnumerator Run(Transform target)
        {
            SetTelegraph(true);
            yield return new WaitForSeconds(telegraphDuration);
            SetTelegraph(false);
            if (target != null) yield return ExecuteActive(target);
            yield return new WaitForSeconds(recoveryDuration);
            Finish();
        }

        internal bool ResolveAndValidateRuntime()
        {
            return HasValidRuntime;
        }

        protected virtual void OnCancelledOrFinished() { }

        private void Finish()
        {
            SetTelegraph(false);
            OnCancelledOrFinished();
            IsRunning = false;
            routine = null;
            nextAllowedTime = Time.time + cooldown;
            Action<BossAttackBase> callback = completed;
            completed = null;
            callback?.Invoke(this);
        }

        private void SetTelegraph(bool visible)
        {
            if (telegraph != null) telegraph.SetActive(visible);
        }

        protected virtual void OnDisable()
        {
            Cancel();
            SetTelegraph(false);
        }

        private void OnValidate()
        {
            telegraphDuration = Mathf.Max(0f, telegraphDuration);
            activeDuration = Mathf.Max(0f, activeDuration);
            recoveryDuration = Mathf.Max(0f, recoveryDuration);
            cooldown = Mathf.Max(0f, cooldown);
            damageMultiplier = Mathf.Max(0f, damageMultiplier);
        }
    }
}
