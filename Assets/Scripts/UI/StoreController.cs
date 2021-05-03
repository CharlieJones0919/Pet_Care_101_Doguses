using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ItemType
{
   FOOD = 0, WALKIES = 1, BED = 2, TOYS = 3, HYGIENE = 4
};

public class StoreController : MonoBehaviour
{
    [SerializeField] private DogController controller;
    [SerializeField] private AStarSearch worldFloor;
<<<<<<< HEAD
    [SerializeField] private List<Vector3> perminantItemPositions = new List<Vector3>();
    
=======
    [SerializeField] private List<Vector3> permanantItemPositions = new List<Vector3>();
    [SerializeField] private float traversableFloorOffset;

>>>>>>> parent of 041f908 (Broke Pathfinding.)
    [SerializeField] private string itemPrefabBaseDir = "Prefabs/Items";
    [SerializeField] private UnityEngine.Object[] itemPrefabs;

    private Dictionary<ItemType, StoreSection> storeSections = new Dictionary<ItemType, StoreSection>();
    private ItemType currentSection;
    [SerializeField] private int currentSectionPage = 0;
    private Item focusItem;

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

    [SerializeField] private GameObject nullObject;

    private void Start()
    {
        itemSlots = itemSlotsParent.GetComponentsInChildren<ItemSlot>();
        itemPrefabs = Resources.LoadAll(itemPrefabBaseDir, typeof(Item));

        int numStoreCatergories = ItemType.GetNames(typeof(ItemType)).Length;

        float tabWidth = baseTab.transform.GetComponent<RectTransform>().sizeDelta.x;
        float requiredContentSpace = (tabWidth * (numStoreCatergories - 1)) - (3 * tabWidth);
        tabContentSpace.offsetMax = new Vector2(requiredContentSpace, tabContentSpace.offsetMax.y);

        for (int i = 0; i < numStoreCatergories; i++)
        {
            ItemType tabCat = (ItemType)i;

            GameObject currentTab;

            if (tabCat == (ItemType)0) { currentTab = baseTab; }
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
                if (item.GetItemType() == tabCat) { catergoryItems.Add(item); }
            }

            StoreSection tabSection = currentTab.GetComponent<StoreSection>();
            tabSection.SetSectionValues(tabCat, catergoryItems);
            storeSections.Add(tabCat, tabSection);

            if (catergoryItems.Count == 0) { Debug.LogWarning("No definition or items found for store catergory: " + tabCat.ToString()); }
        }

        foreach (ItemType catergory in (ItemType[])ItemType.GetValues(typeof(ItemType)))
        {
            foreach (Item item in storeSections[catergory].GetItems())
            {
                item.SetNULLObjectRef(nullObject);
                item.SetInstanceParent(worldFloor.gameObject);
                item.InstantiatePool();
                controller.AddToItemPools(catergory, item);
            }
        }

<<<<<<< HEAD
        float floorStartX = (-worldFloor.transform.localScale.x / 2.0f) + 0.5f;
        float floorStartZ = (worldFloor.transform.localScale.z / 2.0f) - 0.5f;
        float floorEndX = (worldFloor.transform.localScale.x / 2.0f) - 0.5f;
        float floorEndZ = worldFloor.transform.position.z + 0.5f;
=======
        float floorStartX = (-worldFloor.transform.localScale.x / 2.0f) + traversableFloorOffset;
        float floorStartZ = (worldFloor.transform.localScale.z / 2.0f) - traversableFloorOffset;
        float floorEndX = (worldFloor.transform.localScale.x / 2.0f) - traversableFloorOffset;
        float floorEndZ = worldFloor.transform.position.z + traversableFloorOffset;
>>>>>>> parent of 041f908 (Broke Pathfinding.)
        int numPossiblePositions = 1;

        float posX = floorStartX;
        float posZ = floorStartZ;

        for (int i = 0; i < numPossiblePositions; i++)
        {
            numPossiblePositions++;

            if ((posX <= floorEndX))
            {
<<<<<<< HEAD
                perminantItemPositions.Add(new Vector3(posX, 0, posZ));
=======
                permanantItemPositions.Add(new Vector3(posX, 0, posZ));
>>>>>>> parent of 041f908 (Broke Pathfinding.)
                posX += 1.0f;
            }
            else if (posZ > floorEndZ)
            {
                posZ -= 1.0f;
                posX = floorStartX;
            }
            else { break; }
        }
     
        SectionSelected(storeSections[(ItemType)0]);
        gameObject.SetActive(false);
    }

    public void PurchaseAttempt()
    {
        if (!focusItem.IsSingleUse())
        {
<<<<<<< HEAD
            if (perminantItemPositions.Count > 0)
            {
                focusItem.ActivateAvailableInstanceTo(perminantItemPositions[0]);
                perminantItemPositions.Remove(perminantItemPositions[0]);
=======
            if (permanantItemPositions.Count > 0)
            {
                focusItem.ActivateAvailableInstanceTo(permanantItemPositions[0]);
                permanantItemPositions.Remove(permanantItemPositions[0]);
>>>>>>> parent of 041f908 (Broke Pathfinding.)
            }
            else { controller.tipPopUp.DisplayTipMessage("The shelter has no more room to place anymore items."); }
        }
    }

    public void SetFocusItem(ItemSlot focusSlot)
    {
        if (focusSlot.IsSet())
        {
            focusItem = focusSlot.GetItem();
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