/** \file Heap.cs
*  \brief A list to keep and sort generic type data in with a priority order.
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** \interface IHeapElement
*  \brief As the heap doesn't specify a data type and thus what can be done to the elements, this interface specifies functions the elements taken by the Heap class should have implementations of.
*/
public interface IHeapElement<T> : IComparable<T>
{
    int HeapIndex
    {
        get;
        set;
    }
}

/** \class Heap
*  \brief An emulation of a heap which takes generic "type" elements. However, these elements should have implementations of the functions specified by IHeapElement.
*/
public class Heap<T> where T : IHeapElement<T>
{
    T[] array;             //!< A list for the items/elements in the heap. Stores a generic "type" for polymorphism of storing any specified data type.
    int currentItemCount;  //!< Newly added element's position in the heap; sequentially incremented as items are added to keep count of how many items are on the heap so far.

    /** \fn Heap 
    *  \brief Constructor to create a heap of a specified size which the array list is set to.
    *  \param heapSize The initial size of this heap.
    */
    public Heap(int heapSize)
    {
        array = new T[heapSize];
    }

    /** \fn Add 
    *  \brief Allows elements to be added to the heap list.
    *  \param element The element to add to the list.
    */
    public void Add(T element)
    {
        element.HeapIndex = currentItemCount; // Uses the element's implementation of a "set" function to set its position in the heap.
        array[currentItemCount] = element;    // Add it to the heap list in the position just set.
        SortUp(element);                      // Put it at the top of the priority list.
        currentItemCount++;                   // Increase the currentItemCount by 1 for the next item.
    }

    /** \fn SortUp 
    *  \brief Allows an element to be sorted to the highest priority in the heap/list.
    *  \param element The element to move up the heap's priority index.
    */
    public void SortUp(T element)
    {
        int parentIndex = (element.HeapIndex - 1) / 2; // The position to find this element's parent in the heap.

        while (true)
        {
            T parentElement = array[parentIndex];      // This element's parent element.

            // If the parent element currently has a higher priority than the parameter element, swap these element's positions in the heap.
            if (element.CompareTo(parentElement) > 0)
            {
                Swap(element, parentElement);
            }
            else // End the while loop once the element has the highest priority. (It hasn't got a parent with higher priority).
            {
                break;
            }

            parentIndex = (element.HeapIndex - 1) / 2; // Get the heap position of the element's new parent now its priority has been increased.
        }
    }

    /** \fn SortDown
    *  \brief Allows an element's priority in the heap/list to be decreased.
    *  \param element The element to move down the heap's priority index.
    */
    public void SortDown(T element)
    {
        while (true)
        {
            // The parameter element's child element index numbers.
            int childIndexLeft = element.HeapIndex * 2 + 1;
            int childIndexRight = element.HeapIndex * 2 + 2;

            int swapIndex = 0; // Initialising the heap index number the element will be swapped with out of its 2 children.

            // If the left child element's index position is lower than this element's, set the swap index number to that one.
            if (childIndexLeft < currentItemCount)
            {
                swapIndex = childIndexLeft;

                // If the right child element's index position is lower than the left's and is higher in priority, set the swap index number to that one.
                if (childIndexRight < currentItemCount)
                {
                    if (array[childIndexLeft].CompareTo(array[childIndexRight]) < 0)
                    {
                        swapIndex = childIndexRight;
                    }
                }

                // Swap the element with the lower specified child element in the heap index if that child is lower in priority to it.
                if (element.CompareTo(array[swapIndex]) < 0)
                {
                    Swap(element, array[swapIndex]);
                }
                else // If there isn't a lower index position to move the element into, end the loop.
                {
                    return;
                }
            }
            else // If there isn't a lower index position to move the element into, end the loop.
            {
                return;
            }
        }
    }

    /** \fn Swap
    *  \brief Used to swap the priority positions of 2 elements in the heap.
    *  \param elementA The first element to swap positions.
    *  \param elementB The second element to swap positions.
    */
    public void Swap(T elementA, T elementB)
    {
        // Set the array positions of each element to equal the contents of the other element.
        array[elementA.HeapIndex] = elementB; 
        array[elementB.HeapIndex] = elementA;

        int elementAIndex = elementA.HeapIndex;  // A temporary value to store elementA's heap index as stored within the element. (Using the element's implemention of the "get" function).
        elementA.HeapIndex = elementB.HeapIndex; // Sets elementA's priority index number to that stored in elementB.
        elementB.HeapIndex = elementAIndex;      // Sets elementB's priority index number elementA was set as before being swapped.
    }

    /** \fn RemoveTop 
    *  \brief Removes the highest priority element from the heap list.
    */
    public T RemoveTop()
    {
        T topElement = array[0]; // A temporary variable to store the current top element in the heap.
        currentItemCount--;      // Decrease the current count by 1, as an item is about to be removed from the list.
        array[0] = array[currentItemCount]; // Make the top element of the heap the next element down from the top.
        array[0].HeapIndex = 0;  // Set the top element's index number within the element.
        SortDown(array[0]);      // Resort the heap downwards to move the elements into the "emptied" index position.
        return topElement;       // Returns the element which was removed from the top.
    }

    /** \fn Contains 
    *  \brief Returns a boolean to indicate whether or not a specific element is in the heap.
    *  \param element The element to check if is in the heap.
    */
    public bool Contains(T element)
    {
        return Equals(array[element.HeapIndex], element);
    }

    /** \fn Count 
    *  \brief Returns the current number of elements in the heap.
    */
    public int Count
    {
        get
        {
            return currentItemCount;
        }
    }

    /** \fn UpdateElement 
    *  \brief Used to update (increase) the priority of an element.
    *  \param element The element to update the priority of.
    */
    public void UpdateElement(T element)
    {
        SortUp(element);
    }
}
