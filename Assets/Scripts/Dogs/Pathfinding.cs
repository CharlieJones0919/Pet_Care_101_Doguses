/** \file Pathfinding.cs */
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/** \class Pathfinding
*  \brief Contains functions which utilise the A* pathfinding classes/functionalities.
*/
public class Pathfinding : MonoBehaviour
{
    private AStarSearch m_aStarSearch;                  //!< Reference to the A* script. (Retrieved from the ground plane).
    [SerializeField] private GameObject groundPlane;    //!< A reference to the ground plane object to retrieve the A* script from.
    private Vector2 randomPosRange;                     //!< The range random positions on the grid can be generated within based on the groundPlane's size.

    [SerializeField] private float m_moveSpeed = 2.75f;        //!< Speed of movement.
    [SerializeField] private float m_rotationSpeed = 2.5f;     //!< Speed of rotation.
    [SerializeField] private float m_foundDistance = 1.0f;     //!< Distance the dog object must be to a node for it to be considered "found" and removed from the path list.

    private List<Vector3> m_foundPath = new List<Vector3>();   //!< Requested path to a destination as a list of Vector3 positions.
    private bool m_randomNodeFound = false;             //!< Whether or not a random node has been generated.
    private GameObject m_randomPoint;                   //!< An empty game object which can be placed at a random position on the ground plane for random movement.

    /** \fn Start
    *  \brief Instantiate variable values when application starts.
    */
    private void Start()
    {
        m_aStarSearch = groundPlane.GetComponent<AStarSearch>(); //Get A* script from ground plane.
        m_randomPoint = new GameObject("RandomPoint"); //Instantiate a new empty game object in the scene for the random point.

        //Set random position range.
        Vector3 groundWorldScale = (groundPlane.transform.localScale / 2.0f) * 10.0f; //Possible positions are from the centre add the ground's half extents, so half the scale.
        float randX = groundWorldScale.x - 5.0f; //Subtract 5 from the edge to leave room to turn.
        float randZ = groundWorldScale.z - 5.0f;
        randomPosRange = new Vector2(randX, randZ);
    }

    public void SetMoveSpeed(float newSpeed) 
    {
        m_moveSpeed = newSpeed;
    }

    public void SetRotationSpeed(float newSpeed)
    {
        m_rotationSpeed = newSpeed;
    }

    /** \fn FindPathTo
    *  \brief Generates an A* path to a specified point in the world.
    *  \param point The point to find a path to.
    */
    public void FindPathTo(GameObject point)
    {
        //Where to store the generated path.
        List<ASNode> path = new List<ASNode>();

        //Find path if the parameter point is set to a GameObject.
        if (point != null)
        { 
            //Where to temporarily store the new A* search.
            AStarSearch tempAStar = m_aStarSearch;

            //Request a path between this object (the dog) and the input object parameter
            path = tempAStar.RequestPath(this.gameObject, point);
        }

        //If path is not null and more than 0.
        if ((path != null) && (path.Count > 0))
        {
            //Clear old m_foundPath.
            m_foundPath.Clear();

            //Populate m_foundPath with new path
            foreach (ASNode item in path)
            {
                m_foundPath.Add(item.m_worldPos);
            }
        }
    }

    public int GetFoundPathLength()
    {
        return m_foundPath.Count;
    }

    /** \fn FollowPathTo
     *  \brief Moves this GameObject along a found A* path to a specified point.
     *  \param point The point to follow the path to.
     */
    public bool FollowPathTo(GameObject point)
    {
        //Find path if the parameter point is set to an existing GameObject.
        if (point != null)
        {
            if (Vector3.Distance(transform.position, point.transform.position) <= m_foundDistance + point.transform.localScale.z)
            {
                DogLookAt(point.transform.position, true);
                m_randomNodeFound = false; //Random node needs generating again if applicable.
                m_foundPath.Clear();
                return true;
            }
            else if (m_foundPath.Count == 0)  //If there are no more positions left in the path or no path was found...
            {
                m_randomNodeFound = false; //Random node needs generating again if applicable.
                FindPathTo(point); //If the dog  isn't within range of the specified GameObject, find a new path to it.    
                return false;
            }   //If a path has been found and hasn't been traversed yet...
            else if (Vector3.Distance(transform.position, m_foundPath[0]) > m_foundDistance) //If the first position in the path list is further than the specified "found" distance, continue moving towards that node.
            {
                DogLookAt(m_foundPath[0], false); //Look at the first position in the path list.
                MoveDog();   //Move forwards towards the position.
                return false;
            }
            else //If within the "found" distance of the node, remove it from the list.
            {
                m_foundPath.Remove(m_foundPath[0]);
                return false;
            }
        }

        Debug.Log("Trying to find path to non-existant GameObject.");
        return false;
    }

    /** \fn FollowPathToRandomPoint
     *  \brief Repositions the randomPoint GameObject to a new position on the map grid then finds/follows a path to that point.
     */
    public void FollowPathToRandomPoint()
    {
        if (m_randomNodeFound)  //If a random point has already been found, follow the path to it.
        {
            FollowPathTo(m_randomPoint);
        }
        else  //If a random node hasn't been found yet, generate a new random position and set the randomPoint to it.
        {
            StartCoroutine(GenerateRandomPointInWorld());
        }
    }

    /** \fn GenerateRandomPointInWorld
     *  \brief Locates a random traversable node on the ground plane grid and sets the random point GameObject's position to the node's position.
     */
    IEnumerator GenerateRandomPointInWorld()
    {
        AStarSearch tempAStar = m_aStarSearch; //A new temporary ground plane grid A* search. 
        ASNode randomNode = tempAStar.NodePositionInGrid(new Vector3(Random.Range(-randomPosRange.x, randomPosRange.x), 0, Random.Range(-randomPosRange.y, randomPosRange.y))); //Locate a random node on the grid.

        //If the located node isn't traversable find a new one.
        while (!randomNode.m_traversable)
        {
            randomNode = tempAStar.NodePositionInGrid(new Vector3(Random.Range(-groundPlane.transform.localScale.x, groundPlane.transform.localScale.x), 0, Random.Range(-groundPlane.transform.localScale.z, groundPlane.transform.localScale.z)));
            yield return new WaitForEndOfFrame();
        }

        m_randomNodeFound = true; //A random traversable node has been found.
        m_randomPoint.transform.position = randomNode.m_worldPos; //Set the random point to the position of the random traversable node.
    }

    /** \fn MoveDog
    *  \brief Move the dog object to its local direction of forwards - to the direction it's rotated towwards.
    */
    public void MoveDog()
    {
       transform.position += transform.forward * m_moveSpeed * Time.deltaTime;
    }

    /** \fn DogLookAt
    *  \brief Rotates the dog object towards a specified position on the Y-axis.
    *  \param target The position to rotate towards.
    */
    public void DogLookAt(Vector3 target, bool instant)
    {
        //The looking direction/rotation of the target from the dog object.
        var targetPosition = Quaternion.LookRotation(target - transform.position);

        //Remove rotations on axes other than Y.
        targetPosition.x = 0.0f;
        targetPosition.z = 0.0f;

        //Rotate dog towards target position.
        if (!instant)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetPosition, m_rotationSpeed);
        else
            transform.rotation = targetPosition;
    }

    /** \fn DogLookAt
    *  \brief Allows found A* path nodes to be drawn in the editor while the dog object is selected.
    */
    private void OnDrawGizmosSelected()
    {
    #if UNITY_EDITOR
        foreach (Vector3 node in m_foundPath)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawCube(node, new Vector3(3.0f * 0.25f, 0.1f, 3.0f * 0.25f));
        }
    #endif
    }

}
