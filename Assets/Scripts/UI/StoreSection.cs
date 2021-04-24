using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreSection : MonoBehaviour
{
    [SerializeField] private ItemType sectionCatergory;
    [SerializeField] private List<Item> sectionItems;
    [SerializeField] private int extraSectionPages = 0;
    [SerializeField] private int numItems = 0;

    public void SetSectionValues(ItemType catergory, List<Item> items)
    {
        sectionCatergory = catergory;
        sectionItems = items;
        extraSectionPages = (int)Mathf.Abs(sectionItems.Count / 8);
        numItems = sectionItems.Count;
    }

    public ItemType GetCatergory() { return sectionCatergory; }

    public bool HasMultiplePages()
    {
        if (extraSectionPages > 0) { return true; }
        else { return false; }
    }

    public int GetNumOfPages() { return extraSectionPages; }

    public List<Item> GetItems() { return sectionItems; }
    public List<Item> GetPageItems(int pageNum)
    {
        List<Item> pageItems = new List<Item>();
        for (int i = (8 * pageNum); i < numItems; i++)
        {
            pageItems.Add(sectionItems[i]);
        }
        return pageItems;
    }
}