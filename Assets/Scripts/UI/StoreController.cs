using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum StoreCatergory
{
    FOOD = 0, LEADS = 1, FURNITURE = 2, TREATS = 3, TOYS = 4, HYGIENE = 5
};

public class StoreController : MonoBehaviour
{
    [SerializeField] private string itemPrefabBaseDir = "Prefabs/Items";
    [SerializeField] private Object[] itemPrefabs;

    private Dictionary<StoreCatergory, StoreSection> storeSections = new Dictionary<StoreCatergory, StoreSection>();
    private StoreCatergory currentSection;
    [SerializeField] private int currentSectionPage = 0;

    [SerializeField] private GameObject baseTab;
    [SerializeField] private RectTransform tabContentSpace;
    [SerializeField] private Scrollbar tabScrollbar; 
   // private Dictionary<GameObject, Text> sectionTabs = new Dictionary<GameObject, Text>();

    [SerializeField] private GameObject itemInfoPanel;
    [SerializeField] private Button backPageButton;
    [SerializeField] private Button nextPageButton;

    [SerializeField] private GameObject itemSlotsParent;
    private Item[] itemSlots;

    [SerializeField] private Text focusItemName;
    [SerializeField] private Image focusItemImage;
    [SerializeField] private Text focusItemPrice;
    [SerializeField] private Text focusItemDesc;
    [SerializeField] private Button purchaseButton;

    // Start is called before the first frame update
    void Start()
    {
        itemSlots = itemSlotsParent.GetComponentsInChildren<Item>();
        itemPrefabs = Resources.LoadAll(itemPrefabBaseDir, typeof(Item));

        int numStoreCatergories = StoreCatergory.GetNames(typeof(StoreCatergory)).Length;

        float tabWidth = baseTab.transform.GetComponent<RectTransform>().sizeDelta.x;
  
       // tabScrollbar.numberOfSteps = numStoreCatergories;
        //tabScrollbar.size = 0.16f;
        //  tabScrollbar.value = 0;



        for (int i = 0; i < numStoreCatergories; i++)
        {
            string tabName = ((StoreCatergory)i).ToString();

            if (i == 0) { baseTab.transform.GetChild(0).GetComponent<Text>().text = tabName; }
            else
            {
                GameObject newTab = Instantiate(baseTab, baseTab.transform.position, Quaternion.identity).gameObject;
                newTab.transform.parent = baseTab.transform.parent;
                newTab.transform.localScale = baseTab.transform.localScale;
                newTab.transform.localPosition += new Vector3(i * tabWidth, 0, 0);
                newTab.transform.GetChild(0).GetComponent<Text>().text = tabName;
            }

        }

        float requiredContentSpace = (tabWidth * numStoreCatergories) - (3 * tabWidth);
        tabContentSpace.offsetMax = new Vector2(requiredContentSpace, tabContentSpace.offsetMax.y);



        ////foreach (StoreCatergory catergory in (StoreCatergory[])StoreCatergory.GetValues(typeof(StoreCatergory)))
        ////{
        ////    List<Item> catergoryItems = new List<Item>();
        ////    foreach (Item item in itemPrefabs) { if (item.GetCatergory() == catergory) { catergoryItems.Add(item); } }

        ////    if (catergoryItems.Count > 0) { storeSections.Add(catergory, new StoreSection(catergory, catergoryItems)); }
        ////    else { Debug.LogWarning("No definition or items found for store catergory: " + catergory.ToString()); }
        ////}
        //currentSection = (StoreCatergory)0;
        //baseTab.text = ((StoreCatergory)0).ToString();



        //backPageButton.interactable = false;
        //UpdateDisplayedItems();
        //if (storeSections[currentSection].extraSectionPages > 0) { nextPageButton.interactable = true; }
        //else { nextPageButton.interactable = false; }

        //if (itemSlots[0].GetName() != null) { SetFocusItem(itemSlots[0]); }
        //else { itemInfoPanel.SetActive(false); }

        //   Instantiate<sectionPanel>();



        // gameObject.SetActive(false);
    }

    private void SetFocusItem(Item focus)
    {
        focusItemName.text = focus.GetName();
        focusItemImage.sprite = focus.GetSprite();
        focusItemPrice.text = focus.GetPrice().ToString();
        focusItemDesc.text = focus.GetDescription();
    }

    private void UpdateDisplayedItems()
    {
        for (int i = 0; i < itemSlots.Length; i++)
        {
            int pageItem = (currentSectionPage * 8) + i;

            if (i < storeSections[currentSection].sectionItems.Count)
            {
                itemSlots[i] = storeSections[currentSection].sectionItems[pageItem];
            }
        }
    }

    public void SectionSelected()
    {

    }

    public void ChangeSectionPage(bool turnRight)
    {
        switch (turnRight)
        {
            case (true):
                if (currentSectionPage < storeSections[currentSection].extraSectionPages) { currentSectionPage++; }
                if (currentSectionPage == storeSections[currentSection].extraSectionPages) { nextPageButton.interactable = false; }
                break;
            case (false):
                if (currentSectionPage > 0) { currentSectionPage--; }
                if (currentSectionPage == 0) { backPageButton.interactable = false; }
                break;
        }
    }

    public void ItemSelected(Item item)
    {

    }
}

public enum ItemType { BOWL, BED };

public class ItemPool
{
    public ItemType type;
    private GameObject prefabRef;
    private GameObject destinationRef;
    private List<Vector3> allowedPositions;
    public List<Item> itemList;

    //    public ItemPool(ItemType iType, GameObject prefab, GameObject parentObj, List<Vector3> spawnPositions)
    //    {
    //        type = iType;
    //        prefabRef = prefab;
    //        destinationRef = new GameObject(type.ToString() + " OBJECTS");
    //        destinationRef.transform.parent = parentObj.transform;
    //        allowedPositions = spawnPositions;
    //        itemList = new List<Item>();
    //    }

    //    public void InstantiateNewToList()
    //    {
    //        DogCareValue propertySubject = DogCareValue.NONE;
    //        Vector3 foundFreePos = Vector3.zero;
    //        bool singleUse = false;
    //        bool centrePref = false;

    //        foreach (Vector3 position in allowedPositions)
    //        {
    //            foundFreePos = position;

    //            switch (type)
    //            {
    //                case (ItemType.BOWL):
    //                    propertySubject = DogCareValue.Hunger;
    //                    singleUse = true;
    //                    centrePref = false;
    //                    break;
    //                case (ItemType.BED):
    //                    propertySubject = DogCareValue.Rest;
    //                    centrePref = true;
    //                    break;
    //            }

    //            itemList.Add(new Item(prefabRef, destinationRef, foundFreePos, "GENERIC " + type.ToString(), 1.00f, "Desc.", 0.05f, 0.015f, propertySubject, singleUse, centrePref));
    //            break;
    //        }

    //        if ((propertySubject != DogCareValue.NONE))
    //        {
    //            allowedPositions.Remove(foundFreePos);
    //            return;
    //        }
    //        Debug.Log("Maximum Number of " + type.ToString() + "'s Already Placed");
    //    }
};