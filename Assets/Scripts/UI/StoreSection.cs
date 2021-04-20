using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreSection : MonoBehaviour
{
    [SerializeField] private StoreCatergory sectionCatergory;
    [SerializeField] private List<Item> sectionItems;
    [SerializeField] private int extraSectionPages = 0;
    [SerializeField] private int numItems = 0;

    public void SetSectionValues(StoreCatergory catergory, List<Item> items)
    {
        sectionCatergory = catergory;
        sectionItems = items;
        extraSectionPages = Mathf.Abs(sectionItems.Count / 8);
        numItems = sectionItems.Count;
    }

    public StoreCatergory GetCatergory() { return sectionCatergory; }

    public bool HasMultiplePages()
    {
        if (extraSectionPages > 0) { return true; }
        else { return false; }
    }

    public int GetNumOfPages() { return extraSectionPages; }
    public List<Item> GetItems() { return sectionItems; }
    public int GetNumItems() { return numItems; }
}