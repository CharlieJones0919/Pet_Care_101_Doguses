/** \file ASNode.cs
*  \brief An abstract class for node objects to inherit from which can be added to a heap class instance.
*/
using System.Collections;
using UnityEngine;

/** \class ASNode
*  \brief A node class which inherits from and implements the functions from the pure virtual abstract class "IHeapElement." (This allows the nodes can be added to and sorted in an instance of the heap class).
*/
public class ASNode : IHeapElement<ASNode>
{
    public Vector3 m_worldPos;         //!< The node's physical position in the game world.
    public bool m_traversable = false; //!< Whether or not the node can currently be navigated into. (Initialised to false until checked).
    public int m_gridX; //!< The node's position in the world map grid on the X-axis.
    public int m_gridY; //!< The node's position in the world map grid on the Y-axis.

    public float g;     //!< The distance to travel to the goal from the initial node.
    public float h;     //!< The heuristic estimate of "cost" to travel to the goal.

    public ASNode parentNode; //!< This node's parent node.
    int heapIndex;            //!< This node's priority index number in the heap.

    /** \fn ASNode
    *  \brief Constructor to create a node and initialise its values.
    *  \param nodePos Sets the node's physical position in the game world.
    *  \param traversable Sets whether or not the node can currently be navigated into.
    *  \param x Sets the node's position in the world map grid on the X-axis.
    *  \param y Sets the node's position in the world map grid on the y-axis.
    */
    public ASNode(Vector3 nodePos, bool traversable, int x, int y)
    {
        // Set the local values to the constructor parameter's values.
        m_worldPos = nodePos;
        m_traversable = traversable;
        m_gridX = x;
        m_gridY = y;
    }

    /** \fn HeapIndex
    *  \brief Implementation of the "IHeapElement" class' functions.
    */
    public int HeapIndex
    {
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }

    /** \fn f
    *  \brief Returns the total combined "cost" of travelling to this node. (Adds H and G costs).
    */
    public float f
    {
        get
        {
            return g + h;
        }
    }

    /** \fn CompareTo
    *  \brief Returns the 
    *  \param nodeCompare The node this one should be compared to.
    */
    public int CompareTo(ASNode nodeCompare)
    {
        int compare = f.CompareTo(nodeCompare.f); // Getting the difference between this node's travel cost and the input node's travel cost.

        // If there is no difference between the cost to travel to this node and the input node for comparison, made comparison the difference between just the 2 node's heuristic costs.
        if (compare == 0)
        {
            compare = h.CompareTo(nodeCompare.h);
        }

        // Return the difference in the travel costs.
        return -compare;
    }
}
