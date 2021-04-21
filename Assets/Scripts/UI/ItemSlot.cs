using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    [SerializeField] private Toggle itemToggle;
    [SerializeField] private Image displayImage;
    private Item displayItem;

    public void SetItem(bool interactive, Sprite itemSprite, Item item)
    {
        if (item != null)
        {
            itemToggle.interactable = interactive;
            displayImage.sprite = itemSprite;
            displayItem = item;
            return;
        }
        Debug.LogError("ItemSlot attempting to be set to a null Item.");
    }

    public void SetToNoItem()
    {
        itemToggle.interactable = false;
        displayImage.sprite = null;
    }

    public bool IsSet() { return itemToggle.IsInteractable(); }
    public Item GetItem() { if (IsSet()) { return displayItem; } else { return null; } }
}
