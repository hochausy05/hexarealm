using HexaRealm.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace HexaRealm.Boss.UI
{
    public sealed class BossHealthBarUI : MonoBehaviour
    {
        [SerializeField] private BossController boss;
        [SerializeField] private BossHealth bossHealth;
        [SerializeField] private Image fill;
        [SerializeField] private GameObject barRoot;

        private BossController subscribedBoss;
        private Health subscribedHealth;
        private bool configurationErrorLogged;

        private void Awake()
        {
            if (!ValidateConfiguration()) return;
            Hide();
        }

        private void OnEnable()
        {
            if (!ValidateConfiguration()) return;

            subscribedBoss = boss;
            subscribedHealth = bossHealth.Health;
            subscribedBoss.Engaged += Show;
            subscribedBoss.Disengaged += Hide;
            subscribedBoss.ResetToDormant += Hide;
            subscribedBoss.Died += Hide;
            subscribedHealth.HealthChanged += UpdateFill;

            SynchronizeWithBossState();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void SynchronizeWithBossState()
        {
            if (boss == null) return;

            if (boss.CurrentState == BossController.BossState.Engaged
                || boss.CurrentState == BossController.BossState.Attacking)
            {
                Show();
                return;
            }

            Hide();
        }

        private void Show()
        {
            if (!ValidateLiveVisualReferences()) return;

            UpdateFill(0f, bossHealth.Health.CurrentHealth);
            barRoot.SetActive(true);
        }

        private void Hide()
        {
            if (!ValidateLiveVisualReferences()) return;
            barRoot.SetActive(false);
        }

        private void UpdateFill(float _, float currentValue)
        {
            if (!ValidateLiveVisualReferences()) return;

            Health health = bossHealth.Health;
            fill.fillAmount = health.MaxHealth <= 0f
                ? 0f
                : Mathf.Clamp01(currentValue / health.MaxHealth);
        }

        private bool ValidateConfiguration()
        {
            string error = GetConfigurationError();
            if (error == null) return true;

            LogConfigurationError(error);
            enabled = false;
            return false;
        }

        private bool ValidateLiveVisualReferences()
        {
            string error = GetConfigurationError();
            if (error == null) return true;

            LogConfigurationError(error);
            enabled = false;
            return false;
        }

        private string GetConfigurationError()
        {
            if (ReferenceEquals(boss, null)) return "BossHealthBarUI: Boss is missing.";
            if (boss == null) return "BossHealthBarUI: Boss has been destroyed.";
            if (ReferenceEquals(bossHealth, null)) return "BossHealthBarUI: Boss Health is missing.";
            if (bossHealth == null) return "BossHealthBarUI: Boss Health has been destroyed.";
            if (bossHealth.gameObject != boss.gameObject)
                return "BossHealthBarUI: Boss and Boss Health must reference components on the same Boss GameObject.";

            Health health = bossHealth.Health;
            if (ReferenceEquals(health, null)) return "BossHealthBarUI: Boss Health's Health reference is missing.";
            if (health == null) return "BossHealthBarUI: Boss Health's Health reference has been destroyed.";
            if (ReferenceEquals(fill, null)) return "BossHealthBarUI: Fill Image is missing.";
            if (fill == null) return "BossHealthBarUI: Fill Image has been destroyed.";
            if (ReferenceEquals(barRoot, null)) return "BossHealthBarUI: BarRoot is missing.";
            if (barRoot == null) return "BossHealthBarUI: BarRoot has been destroyed.";
            if (barRoot == gameObject)
                return "BossHealthBarUI: BarRoot cannot be the same GameObject that contains BossHealthBarUI.";
            if (transform.IsChildOf(barRoot.transform))
                return "BossHealthBarUI: BarRoot cannot be a parent of the GameObject that contains BossHealthBarUI.";
            if (!fill.transform.IsChildOf(barRoot.transform))
                return "BossHealthBarUI: Fill Image must be a child of BarRoot so the whole HP visual hides together.";

            return null;
        }

        private void LogConfigurationError(string message)
        {
            if (configurationErrorLogged) return;
            configurationErrorLogged = true;
            Debug.LogError(message, this);
        }

        private void Unsubscribe()
        {
            if (subscribedBoss != null)
            {
                subscribedBoss.Engaged -= Show;
                subscribedBoss.Disengaged -= Hide;
                subscribedBoss.ResetToDormant -= Hide;
                subscribedBoss.Died -= Hide;
            }

            if (subscribedHealth != null) subscribedHealth.HealthChanged -= UpdateFill;
            subscribedBoss = null;
            subscribedHealth = null;
        }
    }
}
