/** \file ItemSlot.cs
*  \brief A script attached to each Item Slot UI object in the store menu.
*/
using UnityEngine;
using UnityEngine.UI;

/** \class ItemSlot
*  \brief Stores a reference to an Item class along with references to the slot's UI elements. The Item reference this slot script is set to is set by StoreController to display the Item images/data of each StoreSection's page[s].
*/
public class ItemSlot : MonoBehaviour
{
    [SerializeField] private Toggle itemToggle;  //!< Toggle/checkbox UI component of the slot.
    [SerializeField] private Image displayImage; //!< Image UI component of the slot - this will be set to the sprite of the Item it's currently set to.
    private Item displayItem;                    //!< Reference to the Item this slot is displaying/representing.

    /** \fn SetItem
    *  \brief Called by StoreController and set when populating a StoreSection's page Items to set which Item this slot should be displaying in the page.
    */
    public void SetItem(Sprite itemSprite, Item item)
    {
        if (item != null)
        {
            itemToggle.interactable = true;
            displayImage.sprite = itemSprite;
            displayItem = item;
            return;
        }
        Debug.LogError("ItemSlot attempting to be set to a null Item.");
    }

    /** \fn SetToNoItem
    *  \brief Called by StoreController and set when populating a StoreSection's page Items to set this ItemSlot as empty (not displaying an Item) and non-interactable.
    */
    public void SetToNoItem()
    {
        itemToggle.interactable = false;
        displayImage.sprite = null;
    }

    /** \fn IsSet
    *  \brief Returns whether this slot has a reference to an Item or not as the slot will only be interactive if it has. */
    public bool IsSet() { return itemToggle.IsInteractable(); }
    /** \fn GetItem
    *  \brief Returns the Item this slot is displaying. */
    public Item GetItem() { if (IsSet()) { return displayItem; } else { return null; } }
}