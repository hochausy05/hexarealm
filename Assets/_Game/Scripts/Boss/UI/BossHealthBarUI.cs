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
        private void Awake() { if (barRoot == null) barRoot = gameObject; if (boss == null) boss = FindFirstObjectByType<BossController>(); if (bossHealth == null && boss != null) bossHealth = boss.GetComponent<BossHealth>(); Hide(); }
        private void OnEnable()
        {
            if (boss != null) { boss.Engaged += Show; boss.ResetToDormant += Hide; boss.Died += Hide; }
            if (bossHealth != null && bossHealth.Health != null) bossHealth.Health.HealthChanged += UpdateFill;
        }
        private void OnDisable()
        {
            if (boss != null) { boss.Engaged -= Show; boss.ResetToDormant -= Hide; boss.Died -= Hide; }
            if (bossHealth != null && bossHealth.Health != null) bossHealth.Health.HealthChanged -= UpdateFill;
        }
        private void Show() { UpdateFill(0f, bossHealth.Health.CurrentHealth); barRoot.SetActive(true); }
        private void Hide() { if (barRoot != null) barRoot.SetActive(false); }
        private void UpdateFill(float oldValue, float currentValue) { if (fill != null && bossHealth != null && bossHealth.Health != null) fill.fillAmount = bossHealth.Health.MaxHealth <= 0f ? 0f : currentValue / bossHealth.Health.MaxHealth; }
    }
}
