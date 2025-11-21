using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;


public class TabSystem : MonoBehaviour
{
    private List<Button> createdTabButtons = new List<Button>();
    private int currentSelectedIndex = -1;

    public System.Action<int> OnTabChanged;

    [Header("Visual Settings")]
    [SerializeField] private float selectedAlpha = 1f;
    [SerializeField] private float nonSelectedAlpha = 0.5f;



    public void InitializeTabs(string[] tabNames, Button buttonPrefab, Transform tabsContainer)
    {
        ClearTabs();

        for (int i = 0; i < tabNames.Length; i++)
        {
            int index = i;
            Button newButton = CreateTabButton(buttonPrefab, tabNames[i], tabsContainer);
            newButton.onClick.AddListener(() => OnTabClicked(index));
        }

        if (createdTabButtons.Count > 0)
        {
            OnTabClicked(0);
        }

    }


    private Button CreateTabButton(Button prefab, string labelText, Transform tabsContainer)
    {
        Button newButton = Instantiate(prefab, tabsContainer);
        var tmpText = newButton.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText != null)
            tmpText.text = labelText;
        else
        {
            var uiText = newButton.GetComponentInChildren<Text>();
            if (uiText != null)
                uiText.text = labelText;
        }

        createdTabButtons.Add(newButton);
        return newButton;
    }

    private void OnTabClicked(int index)
    {
        currentSelectedIndex = index;

        for (int i = 0; i < createdTabButtons.Count; i++)
        {
            var button = createdTabButtons[i];
            if (button == null)
                continue;
            bool isSelected = (i == index);

            var tmpText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null)
            {
                Color c = tmpText.color;
                c.a = isSelected ? selectedAlpha : nonSelectedAlpha;
                tmpText.color = c;
            }

            var uiText = button.GetComponentInChildren<Text>();
            if (uiText != null)
            {
                Color c = uiText.color;
                c.a = isSelected ? selectedAlpha : nonSelectedAlpha;
                uiText.color = c;
            }
        }

        OnTabChanged?.Invoke(index);
    }

    private void ClearTabs()
    {
        foreach (var button in createdTabButtons)
        {
            if (button != null)
                Destroy(button.gameObject);
        }
        createdTabButtons.Clear();
        currentSelectedIndex = -1;
    }

    public int GetCurrentSelectedIndex() => currentSelectedIndex;

    public void SelectTab(int index)
    {
        if (index >= 0 && index < createdTabButtons.Count)
            OnTabClicked(index);
    }

    private void OnDestroy() => ClearTabs();
}