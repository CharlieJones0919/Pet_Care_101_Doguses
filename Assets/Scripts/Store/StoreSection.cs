/** \file StoreSection.cs
*  \brief A script attached to the store menu UI tabs for each section (e.g. FOOD, BEDS, TOYS, etc...).
*/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/** \class StoreSection
*  \brief A class for storing all the Items and number of pages a store "section" has. A section is just all the ItemTypes with their own tab in the store; the section contains all the items of that type.
*/
public class StoreSection : MonoBehaviour
{
    [SerializeField] private ItemType sectionCatergory; //!< ItemType of this StoreSection and all the Items within.
    [SerializeField] private Text sectionLabel;         //!< The text displayed on this tab toggle button. Gets set to the ItemType of this StoreSection.
    [SerializeField] private List<Item> sectionItems;   //!< The items of the same ItemType in this section.
    private Dictionary<int, List<Item>> pageItems = new Dictionary<int, List<Item>>();

    [SerializeField] private int extraSectionPages = 0; //!< How many "pages" are in this section. This is simply the number of sectionItems divided by how many ItemSlots are in the store UI to display said Items.
    [SerializeField] private int numItems = 0;          //!< How many Items are in this section. Seeing as this will be static at run-time, this variable saves calling sectionItems.Count so often.

    /** \fn SetSectionValues
    *  \brief Instantiation class to set this tab to an ItemType and populate its list of corresponding Items (of that type). Extrapolates the number of pages and Items from that given Item list.
    */
    public void SetSectionValues(ItemType catergory, List<Item> items, int numItemSlots)
    {
        sectionLabel.text = catergory.ToString();
        sectionCatergory = catergory;
        sectionItems = items;
        extraSectionPages = (int)Mathf.Abs(sectionItems.Count / numItemSlots);
        numItems = sectionItems.Count;

        for (int page = 0; page <= extraSectionPages; page++)
        {
            List<Item> itemList = new List<Item>();
            for (int pagePoint = (numItemSlots * page); pagePoint < numItems; pagePoint++)
            {
                itemList.Add(sectionItems[pagePoint]);
            }

           pageItems.Add(page, itemList);
        }
    }

    /** \fn GetCatergory
    *  \brief Returns this tab's ItemType enum. */
    public ItemType GetCatergory() { return sectionCatergory; }

    /** \fn HasMultiplePages
    *  \brief Returns if this section has more than 1 page to determine if the store's next page button needs to be interactive or not for this section. */
    public bool HasMultiplePages()
    {
        if (extraSectionPages > 0) { return true; }
        else { return false; }
    }

    /** \fn GetNumOfPages
    *  \brief Returns how many pages this section has. */
    public int GetNumOfPages() { return extraSectionPages; }

    /** \fn GetItems
    *  \brief Returns the list of all the Items in this ItemType section. */
    public List<Item> GetItems() { return sectionItems; }

    /** \fn GetPageItems
    *  \brief Returns a list of the Items that will fit on one "page" (set of ItemSlots) for a specific page. */
    public List<Item> GetPageItems(int pageNum)
    {
        return pageItems[pageNum];
    }
}