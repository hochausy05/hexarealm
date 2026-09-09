using System;
using HexaRealm.Player;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace HexaRealm.Progression
{
    /// <summary>Small runtime-built prototype UI. It presents progression state only.</summary>
    public sealed class SoulPillarPanel : MonoBehaviour
    {
        private static SoulPillarPanel instance;

        private PlayerUpgradeProgression progression;
        private Action closeRequested;
        private TMP_Text summaryText;
        private TMP_Text statusText;
        private readonly TMP_Text[] statTexts = new TMP_Text[5];
        private bool isSubscribed;

        public bool IsOpen => gameObject.activeSelf;

        public static SoulPillarPanel GetOrCreate()
        {
            if (instance != null) return instance;

            EnsureEventSystem();
            GameObject root = new GameObject("SoulPillarPanel", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(SoulPillarPanel));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            root.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            root.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1280f, 720f);
            instance = root.GetComponent<SoulPillarPanel>();
            instance.BuildVisuals();
            root.SetActive(false);
            return instance;
        }

        public void Open(PlayerUpgradeProgression targetProgression, Action onCloseRequested)
        {
            Unsubscribe();
            progression = targetProgression;
            closeRequested = onCloseRequested;
            gameObject.SetActive(true);
            Subscribe();
            statusText.text = string.Empty;
            Refresh();
        }

        public void Close()
        {
            Unsubscribe();
            progression = null;
            closeRequested = null;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            Unsubscribe();
            if (instance == this) instance = null;
        }

        private void BuildVisuals()
        {
            RectTransform panel = CreateRect("Panel", transform, new Color(0.06f, 0.08f, 0.13f, 0.96f));
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(520f, 500f);
            panel.anchoredPosition = Vector2.zero;

            summaryText = CreateText("Summary", panel, 22, TextAlignmentOptions.Center);
            SetRect(summaryText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -62f), new Vector2(-20f, -16f));
            statusText = CreateText("Status", panel, 18, TextAlignmentOptions.Center);
            statusText.color = new Color(1f, 0.75f, 0.3f);
            SetRect(statusText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -96f), new Vector2(-20f, -64f));

            PlayerStatType[] stats = { PlayerStatType.Vitality, PlayerStatType.Attack, PlayerStatType.Defense, PlayerStatType.Agility, PlayerStatType.Rage };
            for (int index = 0; index < stats.Length; index++)
            {
                int capturedIndex = index;
                statTexts[index] = CreateText(stats[index] + "Text", panel, 18, TextAlignmentOptions.Left);
                float top = -116f - index * 62f;
                SetRect(statTexts[index].rectTransform, new Vector2(0f, 1f), new Vector2(0.68f, 1f), new Vector2(24f, top - 45f), new Vector2(-4f, top));
                Button upgradeButton = CreateButton("Upgrade" + stats[index], panel, "Upgrade", () => Purchase(stats[capturedIndex]));
                SetRect(upgradeButton.GetComponent<RectTransform>(), new Vector2(0.7f, 1f), new Vector2(1f, 1f), new Vector2(0f, top - 46f), new Vector2(-24f, top));
            }

            Button closeButton = CreateButton("Close", panel, "Close", RequestClose);
            SetRect(closeButton.GetComponent<RectTransform>(), new Vector2(0.35f, 0f), new Vector2(0.65f, 0f), new Vector2(0f, 20f), new Vector2(0f, 62f));
        }

        private void Purchase(PlayerStatType stat)
        {
            UpgradePurchaseResult result = progression.TryPurchaseUpgrade(stat);
            statusText.text = result == UpgradePurchaseResult.Success ? string.Empty : FormatResult(result);
            Refresh();
        }

        private void Refresh()
        {
            if (progression == null) return;

            summaryText.text = $"Souls: {progression.SoulWallet.CurrentSouls}    Upgrades: {progression.TotalUpgradeCount}/{progression.CurrentUpgradeCap}    Next Cost: {progression.NextUpgradeCost}";
            PlayerStatType[] stats = { PlayerStatType.Vitality, PlayerStatType.Attack, PlayerStatType.Defense, PlayerStatType.Agility, PlayerStatType.Rage };
            for (int index = 0; index < stats.Length; index++)
            {
                PlayerStatType stat = stats[index];
                statTexts[index].text = $"{stat}  +{progression.GetUpgradeCount(stat)}  Final: {progression.PlayerStats.GetFinalStat(stat):0.##}";
            }
        }

        private void Subscribe()
        {
            if (isSubscribed || progression == null) return;
            progression.UpgradePurchased += HandleUpgradePurchased;
            progression.SoulWallet.SoulsChanged += HandleSoulsChanged;
            isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!isSubscribed || progression == null) return;
            progression.UpgradePurchased -= HandleUpgradePurchased;
            progression.SoulWallet.SoulsChanged -= HandleSoulsChanged;
            isSubscribed = false;
        }

        private void HandleUpgradePurchased(PlayerStatType stat) => Refresh();
        private void HandleSoulsChanged(int oldValue, int newValue) => Refresh();
        private void RequestClose() => closeRequested?.Invoke();

        private static string FormatResult(UpgradePurchaseResult result)
        {
            return result == UpgradePurchaseResult.CapReached ? "Cap Reached" :
                   result == UpgradePurchaseResult.InsufficientSouls ? "Not Enough Souls" :
                   "Upgrade unavailable";
        }

        private static RectTransform CreateRect(string name, Transform parent, Color color)
        {
            GameObject item = new GameObject(name, typeof(RectTransform), typeof(Image));
            item.transform.SetParent(parent, false);
            item.GetComponent<Image>().color = color;
            return item.GetComponent<RectTransform>();
        }

        private static TMP_Text CreateText(string name, Transform parent, int fontSize, TextAlignmentOptions alignment)
        {
            GameObject item = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            item.transform.SetParent(parent, false);
            TMP_Text text = item.GetComponent<TMP_Text>();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            return text;
        }

        private static Button CreateButton(string name, Transform parent, string label, UnityEngine.Events.UnityAction action)
        {
            GameObject item = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            item.transform.SetParent(parent, false);
            item.GetComponent<Image>().color = new Color(0.2f, 0.42f, 0.7f, 1f);
            item.GetComponent<Button>().onClick.AddListener(action);
            TMP_Text text = CreateText("Label", item.transform, 17, TextAlignmentOptions.Center);
            text.text = label;
            SetRect(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return item.GetComponent<Button>();
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }
    }
}
