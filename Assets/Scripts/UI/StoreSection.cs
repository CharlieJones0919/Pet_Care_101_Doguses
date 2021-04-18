using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreSection : MonoBehaviour
{
    public StoreCatergory sectionCatergory;
    public List<Item> sectionItems;
    public int extraSectionPages = 0;

    public StoreSection(StoreCatergory catergory, List<Item> items)
    {
        sectionCatergory = catergory;
        sectionItems = items;
        extraSectionPages = Mathf.Abs(sectionItems.Count / 8);
    }
}