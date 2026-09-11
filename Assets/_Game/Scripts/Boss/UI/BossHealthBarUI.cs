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

        private bool configurationErrorLogged;

        private void Awake()
        {
            if (!ResolveAndValidateReferences()) return;
            Hide();
        }

        private void OnEnable()
        {
            if (!ResolveAndValidateReferences()) return;

            boss.Engaged += Show;
            boss.Disengaged += Hide;
            boss.ResetToDormant += Hide;
            boss.Died += Hide;
            bossHealth.Health.HealthChanged += UpdateFill;
        }

        private void OnDisable()
        {
            if (boss != null)
            {
                boss.Engaged -= Show;
                boss.Disengaged -= Hide;
                boss.ResetToDormant -= Hide;
                boss.Died -= Hide;
            }

            if (bossHealth != null && bossHealth.Health != null) bossHealth.Health.HealthChanged -= UpdateFill;
        }

        private void Show()
        {
            UpdateFill(0f, bossHealth.Health.CurrentHealth);
            barRoot.SetActive(true);
        }

        private void Hide()
        {
            if (barRoot != null) barRoot.SetActive(false);
        }

        private void UpdateFill(float _, float currentValue)
        {
            if (fill == null || bossHealth == null || bossHealth.Health == null) return;
            fill.fillAmount = bossHealth.Health.MaxHealth <= 0f
                ? 0f
                : Mathf.Clamp01(currentValue / bossHealth.Health.MaxHealth);
        }

        private void ResolveReferences()
        {
            if (boss == null && bossHealth != null) boss = bossHealth.GetComponent<BossController>();
            if (bossHealth == null && boss != null) bossHealth = boss.GetComponent<BossHealth>();
        }

        private bool HasValidConfiguration()
        {
            bool barRootKeepsHostActive = barRoot != null && barRoot != gameObject
                && !transform.IsChildOf(barRoot.transform);
            return boss != null && bossHealth != null && bossHealth.gameObject == boss.gameObject
                && bossHealth.Health != null && fill != null && barRootKeepsHostActive;
        }

        private bool ResolveAndValidateReferences()
        {
            ResolveReferences();
            if (HasValidConfiguration()) return true;

            if (!configurationErrorLogged)
            {
                configurationErrorLogged = true;
                Debug.LogError(
                    "BossHealthBarUI requires a BossController, its BossHealth with Health, an Image fill, and a separate visual barRoot that does not contain the UI host.",
                    this);
            }

            enabled = false;
            return false;
        }
    }
}
