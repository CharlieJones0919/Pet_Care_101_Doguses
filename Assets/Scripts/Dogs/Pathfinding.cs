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
    public AStarSearch m_aStarSearch;                  //!< Reference to the A* script. (Retrieved from the ground plane).

    public GameObject m_randomPointStorage;
    [SerializeField] private GameObject m_randomPoint;                   //!< An empty game object which can be placed at a random position on the ground plane for random movement.
 
    [SerializeField] private float m_walkSpeed;      
    [SerializeField] private float m_runSpeed;        
    [SerializeField] private float m_currentSpeed;

    [SerializeField] private Rigidbody m_RB;
    [SerializeField] private Collider m_collider;

    private List<Vector3> m_foundPath = new List<Vector3>();   //!< Requested path to a destination as a list of Vector3 positions.
    [SerializeField] private bool m_randomNodeFound = false;             //!< Whether or not a random node has been generated.

    /** \fn Start
    *  \brief Instantiate variable values when application starts.
    */
    private void Start()
    {
        m_randomPoint = new GameObject(name + "_RandomPoint"); //Instantiate a new empty game object in the scene for the random point.
        m_randomPoint.transform.parent = m_randomPointStorage.transform;
        m_currentSpeed = m_walkSpeed;
    }

    public void SetRunning(bool isRunning)
    {
        if (isRunning) m_currentSpeed = m_runSpeed;
        else m_currentSpeed = m_walkSpeed;
    }

    /** \fn FindPathTo
    *  \brief Generates an A* path to a specified point in the world.
    *  \param point The point to find a path to.
    */
    public bool FindPathTo(GameObject point)
    {
        //Where to store the generated path.
        List<ASNode> path = new List<ASNode>();

        //Find path if the parameter point is set to a GameObject.
        if (point != null)
        {
            ////Where to temporarily store the new A* search.
            AStarSearch tempAStar = m_aStarSearch;
            //Request a path between this object (the dog) and the input object parameter
            path = tempAStar.RequestPath(transform.position, point.transform.position);
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
            return true;
        }
        return false;
    }

    public int GetFoundPathLength() {  return m_foundPath.Count;  }

    /** \fn FollowPathTo
     *  \brief Moves this GameObject along a found A* path to a specified point.
     *  \param point The point to follow the path to.
     */
    public bool AttemptToReach(GameObject point)
    {
        //Find path if the parameter point is set to an existing GameObject.
        if (point != null)
        {
            if (TargetReached(point))
            {
                DogLookAt(point.transform.position, true);
                m_randomNodeFound = false; //Random node needs generating again if applicable.
                m_RB.velocity = Vector3.zero;
                m_foundPath.Clear();
                return true;
            }
            else if (m_foundPath.Count == 0)  //If there are no more positions left in the path or no path was found...
            {
                FindPathTo(point); //If the dog  isn't within range of the specified GameObject, find a new path to it.    
                return false;
            }
<<<<<<< HEAD
            else if (!(m_collider.bounds.Contains(m_foundPath[0]))) 
=======
            else if (!m_collider.bounds.Contains(m_foundPath[0]) && m_aStarSearch.IsPositionTraversable(m_foundPath[0]))
>>>>>>> parent of 041f908 (Broke Pathfinding.)
            {
                DogLookAt(m_foundPath[0], false); //Look at the first position in the path list.
                MoveDog();   //Move forwards towards the position.
                return false;
            }
            else  //If within the "found" distance of the node, remove it from the list.
            {
                m_foundPath.Remove(m_foundPath[0]);
                return false;
            }
        }

        Debug.Log("Trying to find path to non-existant GameObject: " + point.name);
        return false;
    }

    private bool TargetReached(GameObject target)
    {
        if (target == m_randomPoint) { return m_collider.bounds.Contains(m_randomPoint.transform.position); }
        else { return m_collider.bounds.Contains(target.transform.position + target.transform.localScale); }
    }

    private void OnTriggerEnter(Collider collision)
    {
     //   if (collision.gameObject.layer == m_aStarSearch.obstacleLayerMask) {   m_foundPath.Clear();  };
    }

    /** \fn FollowPathToRandomPoint
     *  \brief Repositions the randomPoint GameObject to a new position on the map grid then finds/follows a path to that point.
     */
    public void FollowPathToRandomPoint()
    {
        if (m_randomNodeFound)  //If a random point has already been found, follow the path to it.
        {
            StopCoroutine(GenerateRandomPointInWorld());
            AttemptToReach(m_randomPoint);
        }
        else  //If a random node hasn't been found yet, generate a new random position and set the randomPoint to it.
        {
            StartCoroutine(GenerateRandomPointInWorld());
        }
    }

    /** \fn GenerateRandomPointInWorld
     *  \brief Locates a random traversable node on the ground plane grid and sets the random point GameObject's position to the node's position.
     */
    private IEnumerator GenerateRandomPointInWorld()
    {
        AStarSearch tempAStar = m_aStarSearch; //A new temporary ground plane grid A* search. 
        ASNode randomNode = tempAStar.NodePositionInGrid(new Vector3(Random.Range(-m_aStarSearch.gridSize.x, m_aStarSearch.gridSize.x), 0.0f, Random.Range(-m_aStarSearch.gridSize.y, m_aStarSearch.gridSize.y))); //Locate a random node on the grid.
    
        //If the located node isn't traversable find a new one.
<<<<<<< HEAD
        while (!randomNode.m_traversable)
        {
            randomNode = tempAStar.NodePositionInGrid(new Vector3(Random.Range(-m_aStarSearch.gridSize.x, m_aStarSearch.gridSize.x), 0.0f, Random.Range(-m_aStarSearch.gridSize.y, m_aStarSearch.gridSize.y)));
            yield return new WaitForEndOfFrame();
        }

        if (randomNode.m_traversable)
=======
        if (!randomNode.m_traversable)
        {
            //randomNode = tempAStar.NodePositionInGrid(new Vector3(Random.Range(-m_aStarSearch.gridSize.x, m_aStarSearch.gridSize.x), 0.0f, Random.Range(-m_aStarSearch.gridSize.y, m_aStarSearch.gridSize.y)));      
             yield return new WaitForEndOfFrame();
        }
        else
>>>>>>> parent of 041f908 (Broke Pathfinding.)
        {
            m_randomNodeFound = true; //A random traversable node has been found.
            m_randomPoint.transform.position = randomNode.m_worldPos; //Set the random point to the position of the random traversable node.
        }
    }

    public Vector3 GetRandomPointInWorld()
    {
        if (m_randomNodeFound)  //If a random point has already been found, return it.
        {
            StopCoroutine(GenerateRandomPointInWorld());
            return m_randomPoint.transform.position;
        }

        StartCoroutine(GenerateRandomPointInWorld());
        return new Vector3(0, 0, 0);
    }

    /** \fn MoveDog
    *  \brief Move the dog object to its local direction of forwards - to the direction it's rotated towwards.
    */
    public void MoveDog()
    {
        m_RB.velocity = (transform.forward * m_currentSpeed);
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
        if (!instant) transform.rotation = Quaternion.RotateTowards(transform.rotation, targetPosition, m_currentSpeed);
        else transform.rotation = targetPosition;
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
