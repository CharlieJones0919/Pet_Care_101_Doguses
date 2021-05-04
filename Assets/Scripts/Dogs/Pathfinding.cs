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
    [SerializeField] private bool m_pathFound = false;

    [SerializeField] private GameObject m_currentTarget;
    [SerializeField] private bool m_reachedTarget = false;

    /** \fn Start
    *  \brief Instantiate variable values when application starts.
    */
    private void Start()
    {
        m_randomPoint = Instantiate(m_randomPoint, Vector3.zero, Quaternion.identity); //Instantiate a new empty game object in the scene for the random point.
        m_randomPoint.name = ("RandomPoint_" + name);
        m_randomPoint.transform.parent = m_randomPointStorage.transform;
        m_randomPoint.transform.position = Vector3.zero;
        m_currentSpeed = m_walkSpeed;
        m_currentTarget = m_randomPoint;
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
    private void FindPathTo(GameObject point)
    {
        //Where to store the generated path.
        List<ASNode> path = new List<ASNode>();
        m_pathFound = false;
        m_RB.velocity = Vector3.zero;

        //Find path if the parameter point is set to a GameObject.
        if (point != null)
        {
            ////Where to temporarily store the new A* search.
            AStarSearch tempAStar = m_aStarSearch;
            //Request a path between this object (the dog) and the input object parameter
            path = tempAStar.RequestPath(transform.position, point.transform.position);
        }

        //If path is not null and more than 0.
        if ((path != null) && (path.Count > 0) /*&& (Vector3.Distance(path[path.Count-1].m_worldPos, transform.position) < 1.0f)*/)
        {
            //Clear old m_foundPath.
            m_foundPath.Clear();

            //Populate m_foundPath with new path
            foreach (ASNode item in path)
            {
                m_foundPath.Add(item.m_worldPos);
            }
            m_pathFound =  true;
        }
    }

    public int GetFoundPathLength() {  return m_foundPath.Count;  }
    public bool IsSetToRandom() {return (m_currentTarget == m_randomPoint);  }
    public bool IsSetToObject(GameObject obj) {  return (m_currentTarget == obj); }

    public void SetTarget(GameObject targ) { m_currentTarget = targ; FindPathTo(m_currentTarget); }
    public void SetTargetToRandom() { m_currentTarget = m_randomPoint; FindPathTo(m_currentTarget); }

    /** \fn FollowPathTo
     *  \brief Moves this GameObject along a found A* path to a specified point.
     *  \param point The point to follow the path to.
     */
    public bool AttemptToReachTarget()
    {
        if (IsSetToRandom() && !m_randomNodeFound) { if (!GenerateRandomPointInWorld(m_randomPoint)) { return false; } }
        if (!m_pathFound) { FindPathTo(m_currentTarget); return false; }

        // if (transform.InverseTransformDirection(m_RB.velocity).z == 0.0f) { FindPathTo(m_currentTarget); }

        if (m_reachedTarget)
        {
            DogLookAt(m_currentTarget.transform.position, true);
            m_randomNodeFound = false;
            m_reachedTarget = false;
            m_pathFound = false;
            m_RB.velocity = Vector3.zero;
            return true;
        }
        else if (((m_foundPath.Count == 1) && !m_reachedTarget))  //If there are no more positions left in the path or no path was found...
        {
            m_pathFound = false;         //If the dog  isn't within range of the specified GameObject, find a new path to it.  
            return false;
        }
        else if (!m_collider.bounds.Contains(m_foundPath[0]))
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

    private void OnTriggerEnter(Collider collision)
    {
     //   if (collision.gameObject.layer == m_aStarSearch.obstacleLayerMask) {   m_foundPath.Clear();  };
        if (collision.gameObject == m_currentTarget) { m_reachedTarget = true; }
    }

    /** \fn GenerateRandomPointInWorld
     *  \brief Locates a random traversable node on the ground plane grid and sets the random point GameObject's position to the node's position.
     */
    public bool GenerateRandomPointInWorld(GameObject recievingObject)
    {
        AStarSearch tempAStar = m_aStarSearch; //A new temporary ground plane grid A* search. 
        ASNode randomNode = tempAStar.NodePositionInGrid(new Vector3(Random.Range(-m_aStarSearch.gridSize.x, m_aStarSearch.gridSize.x), 0.0f, Random.Range(-m_aStarSearch.gridSize.y, m_aStarSearch.gridSize.y))); //Locate a random node on the grid.

        if (tempAStar.IsTraversable(randomNode.m_worldPos))
        {
            if (recievingObject = m_randomPoint) { m_randomNodeFound = true; }//A random traversable node has been found.}
            recievingObject.transform.position = randomNode.m_worldPos; //Set the random point to the position of the random traversable node.
            return true;
        }

        m_randomNodeFound = false;
        return false;
    }

    /** \fn MoveDog
    *  \brief Move the dog object to its local direction of forwards - to the direction it's rotated towwards.
    */
    private void MoveDog()
    {
        m_RB.velocity = (transform.forward * (m_currentSpeed * Time.timeScale));
    }

    /** \fn DogLookAt
    *  \brief Rotates the dog object towards a specified position on the Y-axis.
    *  \param target The position to rotate towards.
    */
    private void DogLookAt(Vector3 target, bool instant)
    {
        //The looking direction/rotation of the target from the dog object.
        var targetPosition = Quaternion.LookRotation(target - transform.position);

        //Remove rotations on axes other than Y.
        targetPosition.x = 0.0f;
        targetPosition.z = 0.0f;

        //Rotate dog towards target position.
        if (!instant) transform.rotation = Quaternion.RotateTowards(transform.rotation, targetPosition, (m_currentSpeed * 2.0f) * Time.timeScale);
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
            Gizmos.color = Color.black;
            Gizmos.DrawCube(node, new Vector3(1.5f, 0.1f, 1.5f));
        }
#endif
    }
}
