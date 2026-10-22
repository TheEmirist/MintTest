using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmployeeListView : MonoBehaviour
{
    const int PoolSize = 14;
    const string ResourceName = "test_task_mock_data";

    [SerializeField] EmployeeCard cardPrefab;
    [SerializeField] ScrollRect scrollRect;

    readonly EmployeeCardPool pool = new EmployeeCardPool();
    readonly List<ActiveCard> active = new List<ActiveCard>();

    EmployeeRecord[] records = Array.Empty<EmployeeRecord>();
    RectTransform content;
    float cardHeight = 140f;
    float spacing = 16f;
    float paddingTop = 50f;
    float paddingBottom;
    float paddingLeft = 40f;
    float paddingRight = 40f;
    float stride = 156f;
    int shownFirst = int.MinValue;
    int shownLast = int.MinValue;

    void Awake()
    {
        if (scrollRect == null)
            scrollRect = GetComponentInChildren<ScrollRect>(true);

        if (scrollRect == null || scrollRect.content == null)
        {
            Debug.LogError("EmployeeListView: нет ScrollRect или Content.", this);
            return;
        }

        content = scrollRect.content;
        ReadLayout();
        records = LoadRecords();
        BuildPool();
        ResizeContent();
        Refresh();
    }

    void OnEnable()
    {
        if (scrollRect != null)
            scrollRect.onValueChanged.AddListener(OnScroll);
    }

    void OnDisable()
    {
        if (scrollRect != null)
            scrollRect.onValueChanged.RemoveListener(OnScroll);
    }

    void OnScroll(Vector2 _)
    {
        Refresh();
    }

    void ReadLayout()
    {
        VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
        if (layout != null)
        {
            paddingLeft = layout.padding.left;
            paddingRight = layout.padding.right;
            paddingTop = layout.padding.top;
            paddingBottom = layout.padding.bottom;
            spacing = layout.spacing;
            layout.enabled = false;
        }

        ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
        if (fitter != null)
            fitter.enabled = false;
    }

    EmployeeRecord[] LoadRecords()
    {
        TextAsset asset = Resources.Load<TextAsset>(ResourceName);
        if (asset == null)
        {
            Debug.LogError("EmployeeListView: нет Resources/test_task_mock_data.", this);
            return Array.Empty<EmployeeRecord>();
        }

        EmployeeCatalog catalog = JsonUtility.FromJson<EmployeeCatalog>(asset.text);
        if (catalog == null || catalog.data == null)
        {
            Debug.LogError("EmployeeListView: не разобрал test_task_mock_data.", this);
            return Array.Empty<EmployeeRecord>();
        }

        return catalog.data;
    }

    void BuildPool()
    {
        List<EmployeeCard> cards = CollectExisting();
        int target = Mathf.Min(PoolSize, records.Length);

        for (int i = cards.Count - 1; i >= target; i--)
        {
            Destroy(cards[i].gameObject);
            cards.RemoveAt(i);
        }

        if (cards.Count < target && cardPrefab == null)
        {
            Debug.LogError("EmployeeListView: назначь префаб EmployeeCard.", this);
            target = cards.Count;
        }

        while (cards.Count < target)
            cards.Add(Instantiate(cardPrefab, content));

        if (cards.Count > 0)
            cardHeight = ReadHeight(cards[0]);

        stride = cardHeight + spacing;
        if (stride < 1f)
            stride = 1f;

        for (int i = 0; i < cards.Count; i++)
            pool.Add(cards[i]);
    }

    List<EmployeeCard> CollectExisting()
    {
        List<EmployeeCard> cards = new List<EmployeeCard>();
        for (int i = 0; i < content.childCount; i++)
        {
            EmployeeCard card = content.GetChild(i).GetComponent<EmployeeCard>();
            if (card != null)
                cards.Add(card);
        }

        return cards;
    }

    static float ReadHeight(EmployeeCard card)
    {
        RectTransform rect = (RectTransform)card.transform;
        if (rect.rect.height > 1f)
            return rect.rect.height;

        return rect.sizeDelta.y > 1f ? rect.sizeDelta.y : 140f;
    }

    void ResizeContent()
    {
        int count = records.Length;
        float height = paddingTop + paddingBottom;
        if (count > 0)
            height += count * cardHeight + (count - 1) * spacing;

        Vector2 size = content.sizeDelta;
        size.y = height;
        content.sizeDelta = size;
    }

    void Refresh()
    {
        if (content == null)
            return;

        int count = records.Length;
        int window = Mathf.Min(PoolSize, count);
        int first = 0;
        if (window > 0 && count > window)
        {
            int raw = Mathf.FloorToInt((content.anchoredPosition.y - paddingTop) / stride);
            first = Mathf.Clamp(raw, 0, count - window);
        }

        int last = window == 0 ? -1 : first + window - 1;
        if (first == shownFirst && last == shownLast)
            return;

        for (int i = active.Count - 1; i >= 0; i--)
        {
            int index = active[i].Index;
            if (index < first || index > last)
            {
                pool.Release(active[i].Card);
                active.RemoveAt(i);
            }
        }

        for (int index = first; index <= last; index++)
        {
            if (Contains(index))
                continue;

            EmployeeCard card = pool.Get();
            if (card == null)
            {
                Debug.LogError("EmployeeListView: пул карточек пуст.", this);
                break;
            }

            card.Bind(records[index]);
            Place(card, index);
            active.Add(new ActiveCard(card, index));
        }

        shownFirst = first;
        shownLast = last;
    }

    bool Contains(int index)
    {
        for (int i = 0; i < active.Count; i++)
        {
            if (active[i].Index == index)
                return true;
        }

        return false;
    }

    void Place(EmployeeCard card, int index)
    {
        RectTransform rect = (RectTransform)card.transform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(-(paddingLeft + paddingRight), cardHeight);
        rect.anchoredPosition = new Vector2(
            (paddingLeft - paddingRight) * 0.5f,
            -(paddingTop + index * stride));
    }

    readonly struct ActiveCard
    {
        public readonly EmployeeCard Card;
        public readonly int Index;

        public ActiveCard(EmployeeCard card, int index)
        {
            Card = card;
            Index = index;
        }
    }
}

public class EmployeeCardPool
{
    readonly Stack<EmployeeCard> free = new Stack<EmployeeCard>();

    public void Add(EmployeeCard card)
    {
        card.gameObject.SetActive(false);
        free.Push(card);
    }

    public EmployeeCard Get()
    {
        if (free.Count == 0)
            return null;

        EmployeeCard card = free.Pop();
        card.gameObject.SetActive(true);
        return card;
    }

    public void Release(EmployeeCard card)
    {
        card.gameObject.SetActive(false);
        free.Push(card);
    }
}

[Serializable]
public class EmployeeCatalog
{
    public EmployeeRecord[] data;
}

[Serializable]
public class EmployeeRecord
{
    public int id;
    public string first_name;
    public string last_name;
    public string email;
    public string gender;
    public string ip_address;
}
