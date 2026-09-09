using UnityEngine;
using UnityEngine.Rendering;

namespace HexaRealm.Core
{
    [RequireComponent(typeof(SortingGroup))]
    public sealed class TopDownSorting : MonoBehaviour
    {
        [SerializeField] private Transform sortPoint;
        [SerializeField, Min(1)] private int precision = 100;
        [SerializeField] private int orderOffset;

        private SortingGroup sortingGroup;

        private void Awake()
        {
            sortingGroup = GetComponent<SortingGroup>();
            UpdateSortingOrder();
        }

        private void LateUpdate()
        {
            UpdateSortingOrder();
        }

        private void OnValidate()
        {
            precision = Mathf.Max(1, precision);
            sortingGroup = GetComponent<SortingGroup>();
            UpdateSortingOrder();
        }

        private void UpdateSortingOrder()
        {
            if (sortingGroup == null)
            {
                return;
            }

            Transform sortingTransform = sortPoint != null ? sortPoint : transform;
            sortingGroup.sortingOrder = Mathf.RoundToInt(-sortingTransform.position.y * precision) + orderOffset;
        }
    }
}
