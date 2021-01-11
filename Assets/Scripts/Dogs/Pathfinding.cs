/** \file Pathfinding.cs
*  \brief Implements instances and usage of A* classes for pathfinding.
*/
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

    [SerializeField] private float m_moveSpeed = 2.5f;         //!< Speed of movement.
    [SerializeField] private float m_rotationSpeed = 2.0f;     //!< Speed of rotation.
    [SerializeField] private float m_foundDistance = 1.0f;     //!< Distance the dog object must be to a node/point for it to be considered "found" and removed from the path list.
    [SerializeField] private float m_relativeFoundDistance;    //!< Found distance with compensation for the moving and target obejcts' sizes.

    private List<Vector3> m_foundPath = new List<Vector3>();    //!< Requested path to a destination as a list of Vector3 positions.
    private bool m_randomNodeFound = false;             //!< Whether or not a random node has been generated.
    private GameObject m_randomPoint;                   //!< An empty game object which can be placed at a random position on the ground plane for random movement.

    /** \class Start
    *  \brief Instantiate variable values when application starts.
    */
    private void Start()
    {
        m_aStarSearch = groundPlane.GetComponent<AStarSearch>(); //Get A* script from ground plane.
        m_randomPoint = new GameObject("RandomPoint"); //Instantiate a new empty game object in the scene for the random point.
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
        if ((path != null) && (path.Count >= 1))
        {
            //Clear old m_foundPath.
            m_foundPath.Clear();

            //Populate m_foundPath with new path
            foreach (ASNode item in path)
            {
                m_foundPath.Add(item.m_worldPos);
            }

            //Set relative required distance for "finding" the target.
            m_relativeFoundDistance = m_foundDistance + (transform.localScale.z + point.transform.localScale.z);
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
            if (Vector3.Distance(transform.position, point.transform.position) <= m_relativeFoundDistance)
            {
                DogLookAt(point.transform.position);
                m_randomNodeFound = false; //Random node needs generating again if applicable.
                m_foundPath.Clear();
                return true;
            }
            else if (m_foundPath.Count == 0)  //If there are no more positions left in the path or no path was found...
            {
                m_randomNodeFound = false; //Random node needs generating again if applicable.
                FindPathTo(point); //If the dog  isn't within range of the specified GameObject, find a new path to it.    
                Debug.Log(2);
                return false;
            }   //If a path has been found and hasn't been traversed yet...
            else if (Vector3.Distance(transform.position, m_foundPath[0]) > m_foundDistance) //If the first position in the path list is further than the specified "found" distance, continue moving towards that node.
            {
                DogLookAt(m_foundPath[0]); //Look at the first position in the path list.
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
        ASNode randomNode = tempAStar.NodePositionInGrid(new Vector3(Random.Range(-90, 90), 0, Random.Range(-90, 90))); //Locate a random node on the grid.

        //If the located node isn't traversable find a new one.
        while (!randomNode.m_traversable)
        {
            randomNode = tempAStar.NodePositionInGrid(new Vector3(Random.Range(-90, 90), 0, Random.Range(-90, 90)));
            yield return new WaitForEndOfFrame();
        }

        m_randomNodeFound = true; //A random traversable node has been found.
        m_randomPoint.transform.position = randomNode.m_worldPos; //Set the random point to the position of the random traversable node.
    }

    /** \fn MoveDog
    *  \brief Move the dog object to its local direction of forwards - to the direction it's rotated towwards.
    */
    private void MoveDog()
    {
       transform.position += transform.forward * m_moveSpeed * Time.deltaTime;
    }

    /** \fn DogLookAt
    *  \brief Rotates the dog object towards a specified position on the Y-axis.
    *  \param target The position to rotate towards.
    */
    private void DogLookAt(Vector3 target)
    {
        //The looking direction/rotation of the target from the dog object.
        var targetPosition = Quaternion.LookRotation(target - transform.position);

        //Remove rotations on axes other than Y.
        targetPosition.x = 0.0f;
        targetPosition.z = 0.0f;

        //Rotate dog towards target position.
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetPosition, m_rotationSpeed);
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
