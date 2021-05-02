/** \file AStarSearch.cs
*  \brief An implementation of A* searching.
*/
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/** \class AStarSearch
*  \brief Contains the functions required to execute and A* search for a path to a goal node.
*/
public class AStarSearch : MonoBehaviour
{
    // Grid Map Data
    private ASNode[,] grid;       //!< A grid interpretation of the game world map/floor as X & Y co-ordinates.
    public Vector2 gridSize;      //!< Size of the grid on the X & Y axes.
    public Vector2 gridNodes;     //!< How many nodes high and wide the ground plane should be divided into to make the grid. (More divisions improves accuracy of movement, but increases path finding time).
    public float nodeSize;    //!< Translation of physical world scale to node size.
    public LayerMask obstacleLayerMask;   //!< Non-traversable object layer for the grid nodes. (The class checks if the node's position contains any GameObjects of this layer to determine if it's traversable).

    // Node Set Data
    private Heap<ASNode> openSet;        //!< A heap class instance for nodes that should be looked at for traversal next to be stored in.
    private HashSet<ASNode> closedSet;   //!< A list for the nodes which have already been considered for the path.
    private ASNode currentNode;          //!< The node the search is currently in and calculating from.

    // Path Data
    [SerializeField] private Vector3 rootNodePos;    //!< The starting node of the path.
    [SerializeField] private Vector3 goalNodePos;    //!< The intended goal/destination node to find a path to from the starting node.
    [SerializeField] private bool pathFound;         //!< Whether or not a path to the goal node has been found yet.
    [SerializeField] private bool searching = false; //!< If a search is currently occurring.
    private List<ASNode> m_path;                     //!< A path composed of nodes which can be used to reach the goal node.

    /** \fn Start
    *  \brief Initialises the grid data from the world when runtime starts then creates a grid map based on this data.
    */
    private void Start()
    {
        // Set grid size to the ground plane's world scale. (This script should be attached to the ground plane).
        gridSize = new Vector3(((transform.localScale.x * 10.0f) - 5.0f), ((transform.localScale.z * 10.0f) - 5.0f));
        // Calculate and set how many nodes high and wide the ground plane (and subsequent grid map) should be divided into.
        gridNodes = new Vector2(Mathf.RoundToInt(gridSize.x / nodeSize), Mathf.RoundToInt(gridSize.y / nodeSize));

        // Populate the grid with nodes based on its now initialised data.
        CreateGrid();
    }

    /** \fn CreateGrid
    *  \brief Creates a grid map based on the data set in the Start() function.
    */
    public void CreateGrid()
    {
        // Instantiate a new matrix to store nodes in using the specified size dimensions (number of divisions).
        grid = new ASNode[(int)gridNodes.x, (int)gridNodes.y];

        // Get the bottom left of the grid node position in world space. (Where to start the node population loop from).
        Vector3 gridBottomLeft = transform.position - Vector3.right * gridSize.x / 2 - Vector3.forward * gridSize.y / 2;

        // Populating every node in the X-axis of the grid map...
        for (int i = 0; i < gridNodes.x; i++)
        {
            // ...and populating every node in the Y-axis of the grid map.
            for (int j = 0; j < gridNodes.y; j++)
            {
                // Calculate the node's physical position in world space based on its position in the grid map.
                Vector3 nodePos = gridBottomLeft + Vector3.right * (i * nodeSize + (nodeSize / 2)) + Vector3.forward * (j * nodeSize + (nodeSize / 2));

                // Set whether or not the node is traverable: if it doesn't contain any objects in the obstacle layer.
                bool traversable = !(Physics.CheckSphere(nodePos, nodeSize, obstacleLayerMask));

                // Add the node to grid map with its newly found data AS a node.
                grid[i, j] = new ASNode(nodePos, traversable, i, j);
            }
        }
    }

    /** \fn RequestPath
    *  \brief Returns an A* found path between 2 input GameObjects.
    *  \param startObject The object to find a path FROM.
    *  \param goalObject The object to find a path TO.
    */
    public List<ASNode> RequestPath(Vector3 startPoint, Vector3 goalPoint)
    {
        // Set each object's node positions from their GameObject world positions. 
        rootNodePos = startPoint;
        goalNodePos = goalPoint;

        // Re-construct the grid. For use in the case of moving start points, goals, or obstacles.
        CreateGrid();
       
        AStarPathFind(); // Run path search.
        return m_path; // Return the path between objects found.
    }

    /** \fn NodePositionInGrid
    *  \brief Constrains a world space co-ordinate to a node position to find an object's position in the context of the grid map and returns which node it's closest to being in. 
    *  \param gridPosition The world space position which needs constraining to identify which node it's in.
    */
    public ASNode NodePositionInGrid(Vector3 gridPosition)
    {
        // Constrain the object's world position into the closest node's position.
        float pX = Mathf.Clamp01((gridPosition.x - ((gridSize.x / gridNodes.x) / 2) + (gridSize.x / 2)) / gridSize.x);
        float pY = Mathf.Clamp01((gridPosition.z - ((gridSize.y / gridNodes.y) / 2) + (gridSize.y / 2)) / gridSize.y);

        // Get the identified encapsulating node's X & Y axis positions.
        int nX = (int)Mathf.Clamp(Mathf.RoundToInt(gridNodes.x * pX), 0, gridNodes.x - 1);
        int nY = (int)Mathf.Clamp(Mathf.RoundToInt(gridNodes.y * pY), 0, gridNodes.y - 1);

        // Return the node the input position was found to be in. 
        return grid[nX, nY];
    }

    /** \fn AStarPathFind
    *  \brief Calculates then and sets a path between the set start/root node and set goal node.
    */
    public void AStarPathFind()
    {
        // Set start and goal nodes' positions in the grid.
        ASNode rootNode = NodePositionInGrid(rootNodePos);
        ASNode goalNode = NodePositionInGrid(goalNodePos);

        openSet = new Heap<ASNode>(grid.Length); // Instantiate a new list for open (currently unchecked/unconsidered) path nodes.
        closedSet = new HashSet<ASNode>();       // Instantiate a new list for closed (already checked/considered) path nodes.

        pathFound = false;      // Initialise path to not found yet.
        searching = true;       // Search has now started.
        openSet.Add(rootNode);  // Add the root/starting node to the open set.
        currentNode = new ASNode(Vector3.zero, false, -1, -1); // Set the current node to the start of the grid and as currently non-traversable.
        float newMoveCost;      // Initialised variable for storing the cost of a new movement in.

        // While a path to the goal node hasn't been found yet and there are still unconsidered nodes in the open set, continue searching for a path in the node grid map.
        while ((openSet.Count > 0) && searching)
        { 
            currentNode = openSet.RemoveTop(); // Make the current node the top node from the open set, and remove it from said set.
            closedSet.Add(currentNode);        // Add current node to the closed set as it is now being considered.

            // If the current node is the goal node, a path has been found and the loop can end.
            if (currentNode == goalNode)
            {
                RetracePath(rootNode, goalNode); // Retrace the path.
                pathFound = true;   // Path is now found.
                searching = false;  // No longer searching.
                break;
            }
            else // If a path to the goal node hasn't been found yet, continue searching.
            {
                // For each immediate neighbour of the current node...
                foreach (ASNode neighbour in GetNeighbours(currentNode))
                {
                    // If the neighbour isn't traverable and has already been checked, skip to the next neighbour in the loop.
                    if (!neighbour.m_traversable || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    // Calculate the heuristic distance cost to travel to this neighbour node from the current node.
                    newMoveCost = currentNode.g + GetDistance(currentNode, neighbour);

                    // If this neighbour node isn't in the open (unchecked) set or has a larger set distance cost set than what its calulated distance cost actually is...
                    if (!openSet.Contains(neighbour) || (newMoveCost < neighbour.g))
                    {
                        neighbour.g = newMoveCost;           // Set the neighbour node's distance cost to its newly calculated cost.
                        neighbour.h = GetDistance(neighbour, goalNode); // Set the neighbour node's new heuristic distance cost to the goal.
                        neighbour.parentNode = currentNode;  // Set the neighbour node's parent node to the current node so the path can be retraced later.

                        // If this neighbour hasn't been added to the open (unchecked) set yet, add it to the set.
                        if (!openSet.Contains(neighbour))
                        {
                            openSet.Add(neighbour);
                        }
                    }
                }
            }
        }
        // If all of the nodes in the open set have been checked without a path to the goal being found, the search still has to end.
        searching = false;
    }

    /** \fn RetracePath
    *  \brief Returns an already found path between 2 nodes from an already completed search.
    *  \param rNode The root node to trace the path back to.
    *  \param gNode The goal node to trace the path back from.
    */
    void RetracePath(ASNode rNode, ASNode gNode)
    {
        List<ASNode> path = new List<ASNode>(); // Resets the path to a new list to store the path of nodes in.
        ASNode currentNode = gNode; // Sets the current node to the previous goal node. 

        // While the current node isn't the original root node, add the then move into the node's parents until reached again.
        while (currentNode != rNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parentNode;
        }

        path.Reverse(); // Reverse the order of the path to get a path from the start node to the goal node.

        // If a path has been found, set the current local path to this retraced path.
        if (path != null)
        {
            m_path = path;
        }
    }

    /** \fn GetNeighbours
    *  \brief Returns the neighbouring nodes to an input node.
    *  \param node The node to find the neighbour nodes of.
    */
    public List<ASNode> GetNeighbours(ASNode node)
    {
        List<ASNode> neighbours = new List<ASNode>(); // A list to store all the found nodes in to return.

        // For the nodes 1 to the left and 1 to the right of this node...
        for (int x = -1; x <= 1; x++)
        {
            // And for the nodes 1 upwards and 1 downwards of this node...
            for (int y = -1; y <= 1; y++)
            {
                // If there isn't a node in this position, skip it and continue the check loop.
                if (x == 0 && y == 0)
                {
                    continue;
                }

                // Get the position of this neighbouring node on the grid map.
                int pX = node.m_gridX + x;
                int pY = node.m_gridY + y;

                // If this neighbour node's position is within the range of the gride node division scale, add it to the list of neighbours. 
                if ((pX >= 0) && (pX < gridNodes.x) && (pY >= 0) && (pY < gridNodes.y))
                {
                    neighbours.Add(grid[pX, pY]);
                }
            }
        }

        // Return the list of neighbouring nodes found to the input node.
        return neighbours;
    }

    // returns distance between nodeA and nodeB based on heuristic class

    /** \fn GetDistance 
    *  \brief Returns the heuristic (currently Euclidean) distance between 2 nodes.
    *  \param nodeA The first node to find the distance from.
    *  \param nodeB The second node to find the distance from.
    */
    public float GetDistance(ASNode nodeA, ASNode nodeB)
    {
        // Return the distance as calculated by the Heuristic class.
        return Heuristic.GetDistanceEuclidean(nodeA, nodeB); 
    }

    public bool IsTraversableFor(Vector3 point, Vector3 travellerSize)
    {
        return (!(Physics.CheckBox(point, travellerSize, Quaternion.identity, obstacleLayerMask)));
    }
}