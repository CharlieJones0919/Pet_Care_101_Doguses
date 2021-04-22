using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum StoreCatergory
{
   FOOD = 0, LEADS = 1, FURNITURE = 2, TREATS = 3, TOYS = 4, HYGIENE = 5
};

public enum ItemType { SUSTINANCE, BED, TOY, OTHER };

public class StoreController : MonoBehaviour
{
    [SerializeField] private DogController controller;
    
    [SerializeField] private string itemPrefabBaseDir = "Prefabs/Items";
    [SerializeField] private UnityEngine.Object[] itemPrefabs;

    private Dictionary<StoreCatergory, StoreSection> storeSections = new Dictionary<StoreCatergory, StoreSection>();
    private StoreCatergory currentSection;
    [SerializeField] private int currentSectionPage = 0;

    [SerializeField] private GameObject baseTab;
    [SerializeField] private RectTransform tabContentSpace;
    [SerializeField] private Scrollbar tabScrollbar; 

    [SerializeField] private GameObject itemInfoPanel;
    [SerializeField] private Button backPageButton;
    [SerializeField] private Button nextPageButton;

    [SerializeField] private GameObject itemSlotsParent;
    [SerializeField] private ItemSlot[] itemSlots;

    [SerializeField] private Text focusItemName;
    [SerializeField] private Image focusItemImage;
    [SerializeField] private Text focusItemPrice;
    [SerializeField] private Text focusItemDesc;
    [SerializeField] private Button purchaseButton;

    private void Start()
    {
        itemSlots = itemSlotsParent.GetComponentsInChildren<ItemSlot>();
        itemPrefabs = Resources.LoadAll(itemPrefabBaseDir, typeof(Item));

        int numStoreCatergories = StoreCatergory.GetNames(typeof(StoreCatergory)).Length;
        int numItemTypes = ItemType.GetNames(typeof(ItemType)).Length;

        float tabWidth = baseTab.transform.GetComponent<RectTransform>().sizeDelta.x;
        float requiredContentSpace = (tabWidth * (numStoreCatergories - 1)) - (3 * tabWidth);
        tabContentSpace.offsetMax = new Vector2(requiredContentSpace, tabContentSpace.offsetMax.y);

        for (int i = 0; i < numStoreCatergories; i++)
        {
            StoreCatergory tabCat = (StoreCatergory)i;

            GameObject currentTab;

            if (tabCat == (StoreCatergory)0) { currentTab = baseTab; }
            else
            {
                currentTab = Instantiate(baseTab, baseTab.transform.position, Quaternion.identity).gameObject;
                currentTab.transform.SetParent(baseTab.transform.parent);
                currentTab.transform.localScale = baseTab.transform.localScale;
                currentTab.transform.localPosition += new Vector3(i * tabWidth, 0, 0);
            }
            currentTab.transform.GetChild(0).GetComponent<Text>().text = tabCat.ToString();

            List<Item> catergoryItems = new List<Item>();
            foreach (Item item in itemPrefabs)
            {
                if (item.GetCatergory() == tabCat) { catergoryItems.Add(item); }

                if ((item.GetObjectType() != ItemType.OTHER) && item.IsTangible())
                {
                    item.InstantiatePool(5);
                    controller.AddToItemPools(item.GetObjectType(), item);
                }
            }

            StoreSection tabSection = currentTab.GetComponent<StoreSection>();
            tabSection.SetSectionValues(tabCat, catergoryItems);
            storeSections.Add(tabCat, tabSection);

            if (catergoryItems.Count == 0) { Debug.LogWarning("No definition or items found for store catergory: " + tabCat.ToString()); }
        }

        SectionSelected(storeSections[(StoreCatergory)0]);
        gameObject.SetActive(false);
    }

    public void SetFocusItem(ItemSlot focusSlot)
    {
        if (focusSlot.IsSet())
        {
            Item focusItem = focusSlot.GetItem();
            focusItemName.text = focusItem.GetName();
            focusItemImage.sprite = focusItem.GetSprite();
            focusItemPrice.text = string.Format("{0:F2}", focusItem.GetPrice());
            focusItemDesc.text = focusItem.GetDescription();
            return;
        }
    }

    private void UpdateDisplayedItems()
    {
        if (storeSections[currentSection].HasMultiplePages()) { nextPageButton.interactable = true; }
        else { nextPageButton.interactable = false; }

        List<Item> pageItems = storeSections[currentSection].GetPageItems(currentSectionPage);
        int numPageItems = pageItems.Count;

        for (int i = 0; i < itemSlots.Length; i++)
        {      
            itemSlots[i].SetToNoItem();

            if (i < numPageItems)
            {
                itemSlots[i].SetItem(true, pageItems[i].GetSprite(), pageItems[i]);
            }
        }

        if (itemSlots[0].IsSet()) { itemInfoPanel.SetActive(true); SetFocusItem(itemSlots[0]); }
        else { itemInfoPanel.SetActive(false); }
    }

    public void SectionSelected(StoreSection tabButton)
    {
        currentSection = tabButton.GetCatergory();
        backPageButton.interactable = false;
        currentSectionPage = 0;

        UpdateDisplayedItems();
    }

    public void ChangeSectionPage(bool turnRight)
    {
        switch (turnRight)
        {
            case (true):
                if (currentSectionPage < storeSections[currentSection].GetNumOfPages())
                {
                    currentSectionPage++;
                    backPageButton.interactable = true;
                    UpdateDisplayedItems();
                }
                if (currentSectionPage == storeSections[currentSection].GetNumOfPages()) { nextPageButton.interactable = false; }
                break;
            case (false):
                if (currentSectionPage > 0) {
                    currentSectionPage--;
                    nextPageButton.interactable = true;
                    UpdateDisplayedItems();
                }
                if (currentSectionPage == 0) { backPageButton.interactable = false; }
                break;
        }
    }
}