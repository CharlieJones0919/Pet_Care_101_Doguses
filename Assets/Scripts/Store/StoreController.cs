/** \file StoreSection.cs
*  \brief A script attached to the StoreMenuSection UI panel object.
*/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/** \enum ItemType
*  \brief An enumerated list of all the available types of Item in the store.
*/
public enum ItemType
{
   FOOD = 0, WALKIES = 1, BED = 2, TOYS = 3, HYGIENE = 4
};

/** \class StoreController
*  \brief Controls the Items displayed in the Store menu. Instantiates said Items and manages the store UI to output their data values and naviation through said UI for viewing the different types of Item. 
*  Allows for the activation and placement of ItemInstances in the world when purchased using in-game money.
*/
public class StoreController : MonoBehaviour
{
    [SerializeField] private Controller controller = null;             //!< Reference to the game controller.
    [SerializeField] private GameObject worldFloor = null;             //!< A reference to the game world's ground object. Required to place ItemInstances within the floor's bounds.
    [SerializeField] private float traversableFloorOffset = 0;         //!< An offset of how much space from the edge of the ground object should be made when populating the possible ItemInstance potencial positions.
                                                                
    [SerializeField] private string itemPrefabBaseDir = null;          //!< The asset directory path to the Item prefabs as a string for retrieving all the prefabs in the asset folder.

    private Dictionary<ItemType, StoreSection> storeSections = new Dictionary<ItemType, StoreSection>(); //!< A list of the different StoreSections (store tabs and associated Items of that type) in a dictionary with the key as their type for ease of access.
    private ItemType currentSection;                                   //!< The StoreSection currently open in the store menu.
    [SerializeField] private int currentSectionPage = 0;               //!< The current "page" (segment of Items from the StoreSection's Item list able to be displayed within the number of ItemSlots) the store is displaying.
    private Item focusItem;                                            //!< Which Item from an ItemSlot has been selected to output the details of in the UI.

    [SerializeField] private StoreSection baseTab = null;              //!< The base existing UI tab in the store menu UI to use as a base to instantiate more section tabs/StoreSections from. (Each tab has a StoreSection script).
    [SerializeField] private RectTransform tabContentSpace = null;     //!< The viewing rectangle of the tabs scrollbar where the store tabs are kept. A reference to this is required to scale the viewport with the number of tabs.

    [SerializeField] private GameObject itemInfoPanel = null;          //!< UI panel in which the data values about a selected ItemSlot toggle Item can be displayed to the player. Required to activate/deactivate depending on if there's anything to show for that section.
    [SerializeField] private Button backPageButton = null;             //!< Button to go to back a section page.
    [SerializeField] private Button nextPageButton = null;             //!< Button to go to back a section page.

    [SerializeField] private GameObject itemSlotsParent = null;        //!< Parent object of the ItemSlot toggles. Just used to retrieve the ItemSlots from.
    [SerializeField] private ItemSlot[] itemSlots = null;              //!< A list of all the ItemSlots in the StoreMenu page.

    ////////// Focus Item Data Display UI Elements //////////
    [SerializeField] private Text focusItemName = null;                //!< Text UI for outputting the name of the ItemSlot Item currently selected.
    [SerializeField] private Image focusItemImage = null;              //!< Image UI for outputting the sprite of the ItemSlot Item currently selected.
    [SerializeField] private Text focusItemPrice = null;               //!< Text UI for outputting the price of the ItemSlot Item currently selected.
    [SerializeField] private Text focusItemDesc = null;                //!< Text UI for outputting the description of the ItemSlot Item currently selected.

    [SerializeField] private Button purchaseButton = null;             //!< Reference to the purchase button for making interactable or not depending on if the player has enough money to buy the Item they're currently toggled on.                                                    

    /** \fn Start
    *  \brief Retrieves all the Item prefabs from the assets directory and instantiates tabs for each of the ItemType enums. Makes StoreSections for each ItemType/tab and populates their Item lists with the retrieved Item prefabs of the same ItemType.
    *  Also sets all the possible Item placement positions based on the ground's dimensions and saves them to Controller's list of available placement positions.
    */
    private void Start()
    {
        ////////////////////////////// Retrieve ItemSlot Toggles and Item Prefabs //////////////////////////////
        UnityEngine.Object[]  itemPrefabs = Resources.LoadAll(itemPrefabBaseDir, typeof(Item));
        itemSlots = itemSlotsParent.GetComponentsInChildren<ItemSlot>();
        int numStoreCatergories = ItemType.GetNames(typeof(ItemType)).Length; // Set number of catergories based on number of ItemType enums (+1 as indexing starts at 0).

        ////////////////////////////// Scale Tab Space to Number of Catergories to Tab Width //////////////////////////////
        float tabWidth = baseTab.transform.GetComponent<RectTransform>().sizeDelta.x;
        float requiredContentSpace = (tabWidth * (numStoreCatergories)) - (3 * tabWidth);
        tabContentSpace.offsetMax = new Vector2(requiredContentSpace, tabContentSpace.offsetMax.y);

        for (int i = 0; i < numStoreCatergories; i++)
        {
            ItemType tabCat = (ItemType)i;
            StoreSection currentTab;

            ////////////////////////////// Make UI Tab for Each Store Catergory //////////////////////////////
            if (tabCat == (ItemType)0) { currentTab = baseTab; } // First one is already made.
            else
            {
                currentTab = Instantiate(baseTab, baseTab.transform.position, Quaternion.identity);
                currentTab.transform.SetParent(baseTab.transform.parent);
                currentTab.transform.localScale = baseTab.transform.localScale;
                currentTab.transform.localPosition += new Vector3(i * tabWidth, 0, 0);
            }

            ////////////////////////////// Populate Tab StoreSection's List with Prefab Items of Same Type //////////////////////////////
            List<Item> catergoryItems = new List<Item>();
            foreach (Item item in itemPrefabs) { if ((item.GetItemType() == tabCat) && !item.doNotDeploy) { catergoryItems.Add(item); } }
            currentTab.SetSectionValues(tabCat, catergoryItems, itemSlots.Length);

            ////////////////////////////// Add New Section to Store Dictionary of Sections //////////////////////////////
            storeSections.Add(tabCat, currentTab);
            if (catergoryItems.Count == 0) { Debug.LogWarning("No definition or items found for store catergory: " + tabCat.ToString()); }
        }

        ////////////////////////////// Instantiate Values for New Store Items //////////////////////////////
        foreach (ItemType catergory in (ItemType[])ItemType.GetValues(typeof(ItemType)))
        {
            foreach (Item item in storeSections[catergory].GetItems())
            {
                item.SetNULLObjectRef(controller.defaultNULL);
                item.SetInstanceParent(worldFloor.gameObject);
                item.InstantiatePool();
                controller.AddToItemPools(catergory, item);
            }
        }

        ////////////////////////////// Calculate Available Floor Item Placement Positions //////////////////////////////
        float floorStartX = (-worldFloor.transform.localScale.x / 2.0f) + traversableFloorOffset;
        float floorStartZ = (worldFloor.transform.localScale.z / 2.0f) - traversableFloorOffset;
        float floorEndX = (worldFloor.transform.localScale.x / 2.0f) - traversableFloorOffset;
        float floorEndZ = worldFloor.transform.position.z + traversableFloorOffset;
        int numPossiblePositions = 1;

        float posX = floorStartX;
        float posZ = floorStartZ;

        for (int i = 0; i < numPossiblePositions; i++)
        {
            numPossiblePositions++;

            if ((posX <= floorEndX))
            {
                controller.bedItemPositions.Add(new Vector3(posX, worldFloor.transform.position.y, posZ));
                if (posZ > 3) { controller.foodItemPositions.Add(new Vector3(posX, worldFloor.transform.position.y, -posZ), false); }

                posX += 1.0f;
            }
            else if (posZ > floorEndZ)
            {
                posZ -= 1.0f;
                posX = floorStartX;
            }
            else { break; }
        }

        ////////////////////////////// Set Initial Store Settings //////////////////////////////
        SectionSelected(storeSections[(ItemType)0]);
        gameObject.SetActive(false);
        purchaseButton.interactable = true;
    }

    /** \fn PurchaseAttempt
    *  \brief Called by the store menu Purchase Button when clicked/tapped. If the player currently has enough money for the ItemSlot Item currently selected, and there are positions available to place Items in, an instance of that Item will be activated and positioned on the world floor.
    */
    public void PurchaseAttempt()
    {
        if (controller.HasEnoughMoney(focusItem.GetPrice())) 
        {
            switch (focusItem.GetItemType())
            {
                case (ItemType.BED):
                    if (controller.bedItemPositions.Count > 0)
                    {
                        focusItem.ActivateAvailableInstanceTo(controller.bedItemPositions[0]);
                        controller.bedItemPositions.Remove(controller.bedItemPositions[0]);
                        controller.MakePurchase(focusItem.GetPrice());
                        return;
                    }
                    controller.tipPopUp.DisplayTipMessage("The shelter has no more room for anymore beds.");
                    break;
                case (ItemType.FOOD):
                    foreach (KeyValuePair<Vector3, bool> position in controller.foodItemPositions)
                    {
                        if (!position.Value)
                        {
                            focusItem.ActivateAvailableInstanceTo(position.Key);
                            controller.foodItemPositions[position.Key] = true;
                            controller.MakePurchase(focusItem.GetPrice());
                            return;
                        }
                    }
                    controller.tipPopUp.DisplayTipMessage("The shelter has no more room to place anymore food items.");
                    break;
                case (ItemType.TOYS):
                    Vector3 randomToyPosition = worldFloor.transform.position;
                    randomToyPosition += new Vector3(Random.Range(0.0f, 1.0f), worldFloor.transform.position.y, Random.Range(0.0f, 1.0f));
                    focusItem.ActivateAvailableInstanceTo(randomToyPosition);
                    controller.MakePurchase(focusItem.GetPrice());
                    break;
                default:
                    Debug.Log("Hasn't been specified how to instantiate this item type.");
                    break;
            }
        }
        else
        {
            controller.tipPopUp.DisplayTipMessage("You don't have enough money for this item.");
            purchaseButton.interactable = false;
        }
    }

    /** \fn SetFocusItem
    *  \brief Called by ItemSlot toggles when clicked/tapped, with itself as the parameter. Sets the item display UI to the Item being referenced/displayed in that ItemSlot.  
    */
    public void SetFocusItem(ItemSlot focusSlot)
    {
        if (focusSlot.IsSet())
        {
            focusItem = focusSlot.GetItem();
            focusItemName.text = focusItem.GetName();
            focusItemImage.sprite = focusItem.GetSprite();
            focusItemPrice.text = string.Format("{0:F2}", focusItem.GetPrice());
            focusItemDesc.text = focusItem.GetDescription();

            if (!controller.HasEnoughMoney(focusItem.GetPrice())) { purchaseButton.interactable = false; }
            else { purchaseButton.interactable = true; }
            return;
        }
    }

    /** \fn SectionSelected
    *  \brief Called by StoreSection toggles when clicked/tapped, with itself as the parameter. Sets currentSection to that tab's ItemType catergory then updates the ItemSlots' Items to be the ItemList for that section by calling UpdateDisplayedItems().
    */
    public void SectionSelected(StoreSection tabButton)
    {
        currentSection = tabButton.GetCatergory();
        currentSectionPage = 0;
        backPageButton.interactable = false;
  
        UpdateDisplayedItems();
    }

    /** \fn ChangeSectionPage
    *  \brief Called by the page back/next buttons when clicked/tapped, with the bool as true for the next button and false for the back button. Adds or subtracts 1 from the currentSectionPage value then updates the ItemSlots' Items to the StoreSection Items of that page by calling UpdateDisplayedItems().
    */
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

    /** \fn UpdateDisplayedItems
    *  \brief Sets the ItemSlots' reference Items to the Item list of the current StoreSection and current page number of that list.
    */
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
                itemSlots[i].SetItem(pageItems[i].GetSprite(), pageItems[i]);
            }
        }

        if (itemSlots[0].IsSet()) { itemInfoPanel.SetActive(true); SetFocusItem(itemSlots[0]); }
        else { itemInfoPanel.SetActive(false); }
    }
}