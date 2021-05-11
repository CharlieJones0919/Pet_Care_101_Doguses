/** \file Dog.cs */
/*! \mainpage Pets 101 - Doguses
 * \section intro_sec Project Aim Introduction
 * The aim of this project was to educate children (via an interactive experience) in dog behavior - and the impact provided care and environment can have on said behavior. 
 * Application of AI principles were used to aid this goal, by dynamically simulating responses to the dog's environment and available resources.  
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** \class Dog
*  \brief This class contains the implementations of functions (or behaviour actions) that can be employed by the dog's FSM states, and the individual dog's personality and care state values.
*/
public class Dog : MonoBehaviour
{
    [SerializeField] private Controller controller; //!< Reference to the game controller.     //!< Reference to the game's Controller script to retrieve data required by all dogs.
    [SerializeField] private Pathfinding navigation;    //!< Instance of the Pathfinding script for the dog to utalize for navigation around the map.
    public void SetController(Controller ctrl) { controller = ctrl; navigation.m_aStarSearch = controller.groundSearch; navigation.m_randomPointStorage = controller.randomPointStorage; }

    public string m_name;     //!< This dog's name. 
    public DogBreed m_breed;  //!< The breed of this dog.
    public int m_maxAge;      //!< Would have been a condition for killing the dog if death had of been implemented in time.
    public int m_age;         //!< Age of this dog - how long since it has was instantiated in game time. 
    public Dictionary<BodyPart, BodyComponent> m_body = new Dictionary<BodyPart, BodyComponent>(); //!< References to all the dog's body parts. (The definition for a BodyPart and BodyComponent are in the DogGeneration file).

    public Animator m_animationCTRL;         //!< Animation controller. 
    public BoxCollider m_collider;           //!< Dog's collider component/bounds. Set dynamically in DogGeneration depending on the dog's mesh space varying by breed type.
    [SerializeField] private Rigidbody m_RB; //!< Dog's rigid body to derive its speed from/to.

    public Dictionary<DogCareValue, CareProperty> m_careValues = new Dictionary<DogCareValue, CareProperty>();                                      //!< A list of the dog's current care value properties so they can be easily iterated through.
    public Dictionary<DogPersonalityValue, PersonalityProperty> m_personalityValues = new Dictionary<DogPersonalityValue, PersonalityProperty>();   //!< A list of the dog's personality value properties so they can be easily iterated through. (Have not been implemented in full to be conditional on the dog's average care values in the long term).

    private Item m_currentItemTarget;                           //!< An item the dog is currently focused on. Stored so it can be travelled to or used if reached and usable.
    [SerializeField] private GameObject m_currentObjectTarget;  //!< The specific object instance of the current target item.

    private float m_heldItemPrevYPos;                                  //!< If an item has been picked up, it's previous Y position is stored so it can be placed back down.
    private Vector3 m_holdingOffset = new Vector3(0.0f, -0.45f, 1.0f); //!< Local offset from the dog's snout a held item should be positioned at.

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////// RULE BASED SYSTEM VARIABLES ////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Used to store definitions for the dog's different "facts" and rules based on them. (Just used to determine if a state should be exited into another based on the outcome of the state's behaviour tree sequences).
    public Dictionary<string, bool> m_facts = new Dictionary<string, bool>(); //!< Pairs the names of "facts" to their current state of truth. (Essentially named booleans).
    public List<Rule> m_rules = new List<Rule>();                             //!< A list of the rules which return a state to swap to if evaluated as true.

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////// BEHAVIOURAL TREE VARIABLES ////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Action nodes which are children to sequence nodes which must be successful in a sequence for the sequence node to succeed.
    //////////////////// Check Current FSM State Actions ////////////////////
    private BTAction check_Idle_State;     //!< Whether the dog is currently in its Idle     state.
    private BTAction check_Hungry_State;   //!< Whether the dog is currently in its Hungry   state.
    private BTAction check_Tired_State;    //!< Whether the dog is currently in its Tired    state.
    private BTAction check_Playful_State;  //!< Whether the dog is currently in its Playful  state.
    //////////////////// Swap FSM State Actions ////////////////////
    private BTAction swp_Idle_State;       //!< Whether one of the dog's BT sequences have determined that the dog should swap into its Idle state.
    private BTAction swp_Pause_State;      //!< Whether one of the dog's BT sequences have determined that the dog should swap into its Hungry state.
    private BTAction swp_Hungry_State;     //!< Whether one of the dog's BT sequences have determined that the dog should swap into its Tired state.
    private BTAction swp_Tired_State;      //!< Whether one of the dog's BT sequences have determined that the dog should swap into its Playful state.
    private BTAction swp_Playful_State;
    //////////////////// Status Check Actions ////////////////////
    ///// Hunger Status /////
    private BTAction check_Starving;       //!< Whether the dog's Hunger CareValue is currently in the state of Starving. 
    private BTAction check_Hungry;         //!< Whether the dog's Hunger CareValue is currently in the state of Hungry.   
    private BTAction check_Fed;            //!< Whether the dog's Hunger CareValue is currently in the state of Fed.      
    private BTAction check_Overfed;        //!< Whether the dog's Hunger CareValue is currently in the state of Overfed.  
    ///// Attention Status /////         
    private BTAction check_Lonely;         //!< Whether the dog's Attention CareValue is currently in the state of Lonely.      
    private BTAction check_Loved;          //!< Whether the dog's Attention CareValue is currently in the state of Loved.       
    private BTAction check_Overcrowded;    //!< Whether the dog's Attention CareValue is currently in the state of Overcrowded. 
    ///// Rest Status /////              
    private BTAction check_Exhausted;      //!< Whether the dog's Rest CareValue is currently in the state of Exhausted.   
    private BTAction check_Tired;          //!< Whether the dog's Rest CareValue is currently in the state of Tired.       
    private BTAction check_Rested;         //!< Whether the dog's Rest CareValue is currently in the state of Rested.      
    private BTAction check_Rejuvinated;    //!< Whether the dog's Rest CareValue is currently in the state of Rejuvinated. 
    ///// Hygiene Status /////              
    ///private BTAction check_Filthy;
    ///private BTAction check_Dirty;
    ///private BTAction check_Clean;
    ///// Health Status /////
    private BTAction check_Dying;          //!< Whether the dog's Health CareValue is currently in the state of Dying.    
    private BTAction check_Sick;           //!< Whether the dog's Health CareValue is currently in the state of Sick.     
    private BTAction check_Healthy;        //!< Whether the dog's Health CareValue is currently in the state of Healthy.  
    ///// Happiness Status /////         
    private BTAction check_Distressed;     //!< Whether the dog's Happiness CareValue is currently in the state of Distressed.
    private BTAction check_Upset;          //!< Whether the dog's Happiness CareValue is currently in the state of Upset.     
    private BTAction check_Happy;          //!< Whether the dog's Happiness CareValue is currently in the state of Happy.     

    private BTAction check_AllGood;        //!< If all of the dog's CareValues are in "good" condition.
    private BTAction bonus_GoodCare;       //!< A function to pay the player and improve the dog's personality values if the dog is being cared for well.

    //////////////////// Status False Check Actions ////////////////////
    private BTAction check_N_Starving;     //!< Whether the dog's Hunger CareValue is NOT currently in the state of Starving. 
    private BTAction check_N_Hungry;       //!< Whether the dog's Hunger CareValue is NOT currently in the state of Hungry.   
    private BTAction check_N_Fed;          //!< Whether the dog's Hunger CareValue is NOT currently in the state of Fed.      
    private BTAction check_N_Overfed;      //!< Whether the dog's Hunger CareValue is NOT currently in the state of Overfed.  
    private BTAction check_N_Lonely;       //!< Whether the dog's Attention CareValue is NOT currently in the state of Lonely.     
    private BTAction check_N_Loved;        //!< Whether the dog's Attention CareValue is NOT currently in the state of Loved.      
    private BTAction check_N_Overcrowded;  //!< Whether the dog's Attention CareValue is NOT currently in the state of Overcrowded.
    private BTAction check_N_Exhausted;    //!< Whether the dog's Rest CareValue is NOT currently in the state of Exhausted.   
    private BTAction check_N_Tired;        //!< Whether the dog's Rest CareValue is NOT currently in the state of Tired.       
    private BTAction check_N_Rested;       //!< Whether the dog's Rest CareValue is NOT currently in the state of Rested.      
    private BTAction check_N_Rejuvinated;  //!< Whether the dog's Rest CareValue is NOT currently in the state of Rejuvinated. 
    private BTAction check_N_Dying;        //!< Whether the dog's Health CareValue is NOT currently in the state of Dying.    
    private BTAction check_N_Sick;         //!< Whether the dog's Health CareValue is NOT currently in the state of Sick.     
    private BTAction check_N_Healthy;      //!< Whether the dog's Health CareValue is NOT currently in the state of Healthy.  
    private BTAction check_N_Distressed;   //!< Whether the dog's Happiness CareValue is NOT currently in the state of Distressed.
    private BTAction check_N_Upset;        //!< Whether the dog's Happiness CareValue is NOT currently in the state of Upset.     
    private BTAction check_N_Happy;        //!< Whether the dog's Happiness CareValue is NOT currently in the state of Happy.     

    //////////////////// Items Found Check Actions ////////////////////
    private BTAction found_Food;           //!< If the dog has located a food item active on the map.
    private BTAction found_Bed;            //!< If the dog has located a bed item active on the map.
    private BTAction found_Toys;           //!< If the dog has located a toy item active on the map.
    private BTAction found_N_Food;         //!< If the dog cannot locate a food item active on the map.
    private BTAction found_N_Bed;          //!< If the dog cannot locate a bed item active on the map.
    private BTAction found_N_Toys;         //!< If the dog cannot locate a toy item active on the map.

    //////////////////// Set Move Speed Actions ////////////////////
    private BTAction move_Crawling;        //!< Sets the dog's Pathfinding script moving speed to its defined Crawling speed.
    private BTAction move_Walking;         //!< Sets the dog's Pathfinding script moving speed to its defined Walking speed.
    private BTAction move_Running;         //!< Sets the dog's Pathfinding script moving speed to its defined Running speed.

    //////////////////// Behaviour Tree Condition Sequence Lists for Leaving Each State ////////////////////
    public List<BTSequence> GlobalSequences = new List<BTSequence>();       //!< A sequence of BT node checks general values which shouldn't result in state changes. Includes a check for if the dog is in good health and the others are for setting the dog's walking speed. 
    public List<BTSequence> IdleEndSequences = new List<BTSequence>();      //!< A sequence of BT node checks which if true will end in a BT node to set one of the dog's state change facts to true so the next rule list check will result in the dog leaving the Idle state.
    public List<BTSequence> HungryEndSequences = new List<BTSequence>();    //!< A sequence of BT node checks which if true will end in a BT node to set one of the dog's state change facts to true so the next rule list check will result in the dog leaving the Hunger state.
    public List<BTSequence> TiredEndSequences = new List<BTSequence>();     //!< A sequence of BT node checks which if true will end in a BT node to set one of the dog's state change facts to true so the next rule list check will result in the dog leaving the Tired state.
    public List<BTSequence> PlayfulEndSequences = new List<BTSequence>();   //!< A sequence of BT node checks which if true will end in a BT node to set one of the dog's state change facts to true so the next rule list check will result in the dog leaving the Playful state.

    /** \fn Awake
    *  \brief Callled once when the scene loads to instantiate variable values and functions before the application starts. Used to define and add states to the FSM, rules and facts.
    */
    private void Awake()
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////// BEHAVIOUR TREE STATES ///////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Defining the dog's FSM states then adding them to the dog's FSM. 
        Dictionary<Type, State> newStates = new Dictionary<Type, State>();
        newStates.Add(typeof(Idle), new Idle(this));      // This is the default starting state. 
        newStates.Add(typeof(Pause), new Pause(this));

        newStates.Add(typeof(Hungry), new Hungry(this));
        newStates.Add(typeof(Tired), new Tired(this));
        newStates.Add(typeof(Playful), new Playful(this));
        GetComponent<FiniteStateMachine>().SetStates(newStates); //Add defined states to FSM.

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////// RBS FACTS ///////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        // The dog's "fact" values.
        m_facts.Add("IDLE", true);
        m_facts.Add("HUNGRY", false);
        m_facts.Add("TIRED", false);
        m_facts.Add("PLAYFUL", false);

        m_facts.Add("IS_FOCUS", true); // Is true by default as the dog is made the focus when it's first generated.
        m_facts.Add("USING_ITEM", false);
        m_facts.Add("WAITING", false);
        m_facts.Add("NEEDS_2_FINISH_ANIM", false);
        m_facts.Add("HOLDING_ITEM", false);

        m_facts.Add("SWP_IDLE", false);
        m_facts.Add("SWP_PAUSE", false);
        m_facts.Add("SWP_HUNGRY", false);
        m_facts.Add("SWP_TIRED", false);
        m_facts.Add("SWP_PLAYFUL", false);

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////// RBS RULES ///////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Defines which rules should result in the state being changed and to which one. 
        m_rules.Add(new Rule("IDLE", "SWP_PAUSE", Rule.Predicate.And, typeof(Pause)));
        m_rules.Add(new Rule("IDLE", "SWP_HUNGRY", Rule.Predicate.And, typeof(Hungry)));
        m_rules.Add(new Rule("IDLE", "SWP_TIRED", Rule.Predicate.And, typeof(Tired)));
        m_rules.Add(new Rule("IDLE", "SWP_PLAYFUL", Rule.Predicate.And, typeof(Playful)));

        m_rules.Add(new Rule("HUNGRY", "SWP_PAUSE", Rule.Predicate.And, typeof(Pause)));
        m_rules.Add(new Rule("TIRED", "SWP_PAUSE", Rule.Predicate.And, typeof(Pause)));
        m_rules.Add(new Rule("PLAYFUL", "SWP_PAUSE", Rule.Predicate.And, typeof(Pause)));

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////// BEHAVIOUR TREE ACTIONS ///////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Binding the behaviour tree actions to their BTState functions.

        //////////////////// Check Current FSM State Actions ////////////////////
        check_Idle_State = new BTAction(IdleState);
        check_Hungry_State = new BTAction(HungryState);
        check_Tired_State = new BTAction(TiredState);
        check_Playful_State = new BTAction(PlayfulState);

        //////////////////// Swap FSM State Actions ////////////////////
        swp_Idle_State = new BTAction(SWP_IdleState);
        swp_Pause_State = new BTAction(SWP_PauseState);
        swp_Hungry_State = new BTAction(SWP_HungryState);
        swp_Tired_State = new BTAction(SWP_TiredState);
        swp_Playful_State = new BTAction(SWP_PlayfulState);

        //////////////////// Check Current Care Value Actions ////////////////////
        ///// Hunger Status /////
        check_Starving = new BTAction(Starving);
        check_Hungry = new BTAction(Hungry);
        check_Fed = new BTAction(Fed);
        check_Overfed = new BTAction(Overfed);
        ///// Attention Status /////
        check_Lonely = new BTAction(Lonely);
        check_Loved = new BTAction(Loved);
        check_Overcrowded = new BTAction(Overcrowded);
        ///// Rest Status /////
        check_Exhausted = new BTAction(Exhausted);
        check_Tired = new BTAction(Tired);
        check_Rested = new BTAction(Rested);
        check_Rejuvinated = new BTAction(Rejuvinated);
        ///// Health Status /////
        check_Dying = new BTAction(Dying);
        check_Sick = new BTAction(Sick);
        check_Healthy = new BTAction(Healthy);
        ///// Happiness Status /////
        check_Distressed = new BTAction(Distressed);
        check_Upset = new BTAction(Upset);
        check_Happy = new BTAction(Happy);

        ///// Negative Statuses /////
        check_N_Starving = new BTAction(NStarving);
        check_N_Hungry = new BTAction(NHungry);
        check_N_Fed = new BTAction(NFed);
        check_N_Overfed = new BTAction(NOverfed);
        check_N_Lonely = new BTAction(NLonely);
        check_N_Loved = new BTAction(NLoved);
        check_N_Overcrowded = new BTAction(NOvercrowded);
        check_N_Exhausted = new BTAction(NExhausted);
        check_N_Tired = new BTAction(NTired);
        check_N_Rested = new BTAction(NRested);
        check_N_Rejuvinated = new BTAction(NRejuvinated);
        check_N_Dying = new BTAction(NDying);
        check_N_Sick = new BTAction(NSick);
        check_N_Healthy = new BTAction(NHealthy);
        check_N_Distressed = new BTAction(NDistressed);
        check_N_Upset = new BTAction(NUpset);
        check_N_Happy = new BTAction(NHappy);

        ///// Good Care Statuses /////
        check_AllGood = new BTAction(AllGood);
        bonus_GoodCare = new BTAction(GoodCareBonuses);

        //////////////////// Item Location Actions ////////////////////
        found_Food = new BTAction(CanFindFood);
        found_Bed = new BTAction(CanFindBed);
        found_Toys = new BTAction(CanFindToys);
        found_N_Food = new BTAction(CannotFindFood);
        found_N_Bed = new BTAction(CannotFindBed);
        found_N_Toys = new BTAction(CannotFindToys);

        //////////////////// Set Move Speed Actions ////////////////////
        move_Crawling = new BTAction(MoveCrawling);
        move_Walking = new BTAction(MoveWalking);
        move_Running = new BTAction(MoveRunning);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////// BEHAVIOUR TREE SEQUENCES ///////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            BTSequence[] globalSeqs =
            {
                new BTSequence(new List<BTNode> { check_AllGood, move_Running, bonus_GoodCare }),

                new BTSequence(new List<BTNode> { check_Lonely, move_Crawling }),
                new BTSequence(new List<BTNode> { check_Sick, move_Crawling }),
                new BTSequence(new List<BTNode> { check_Distressed, move_Crawling }),
                new BTSequence(new List<BTNode> { check_Starving, move_Crawling }),
                new BTSequence(new List<BTNode> { check_Exhausted, move_Crawling }),

                new BTSequence(new List<BTNode> { check_N_Lonely, check_N_Sick, check_N_Distressed, check_N_Starving, check_N_Exhausted, move_Walking })
            };
            foreach (BTSequence sequence in globalSeqs) { GlobalSequences.Add(sequence); }
        }
        // The behaviour tree sequences (list of conditional node checks/actions) to determine if the dog should exit its current FSM state.
        {
            BTSequence[] idleEndSeqs =
            {
                new BTSequence(new List<BTNode> { check_Idle_State, check_Hungry, check_N_Tired, found_Food, swp_Hungry_State }),
                new BTSequence(new List<BTNode> { check_Idle_State, check_Hungry, check_Tired, found_N_Bed, found_Food, swp_Hungry_State }),
                new BTSequence(new List<BTNode> { check_Idle_State, check_Starving, check_Exhausted, found_N_Bed, found_Food, swp_Hungry_State }),
                new BTSequence(new List<BTNode> { check_Idle_State, check_Starving, check_N_Exhausted, found_Food, swp_Hungry_State }),

                new BTSequence(new List<BTNode> { check_Idle_State, check_Tired, found_Bed, swp_Tired_State }),

                new BTSequence(new List<BTNode> { check_Idle_State, check_N_Overfed, check_N_Lonely, check_N_Tired, found_Food, swp_Hungry_State }),
                new BTSequence(new List<BTNode> { check_Idle_State, check_Rested, check_Fed, found_Toys, swp_Playful_State })
            };
            foreach (BTSequence sequence in idleEndSeqs) { IdleEndSequences.Add(sequence); }
        }
        {
            BTSequence[] hungryEndSeqs =
            {
                new BTSequence(new List<BTNode> { check_Hungry_State, found_N_Food, swp_Pause_State }),
                new BTSequence(new List<BTNode> { check_Hungry_State, check_Overfed, swp_Pause_State }),
                new BTSequence(new List<BTNode> { check_Hungry_State, check_Tired, check_Fed, found_Bed, swp_Pause_State }),
                new BTSequence(new List<BTNode> { check_Hungry_State, check_Exhausted, check_N_Starving, found_Bed, swp_Pause_State }),
                new BTSequence(new List<BTNode> { check_Hungry_State, check_Fed, check_Rested, check_Lonely, found_Toys, swp_Pause_State })
            };
            foreach (BTSequence sequence in hungryEndSeqs) { HungryEndSequences.Add(sequence); }
        }
        {
            BTSequence[] tiredEndSeqs =
            {
                new BTSequence(new List<BTNode> { check_Tired_State, found_N_Bed, swp_Pause_State }),
                new BTSequence(new List<BTNode> { check_Tired_State, check_Rejuvinated, swp_Pause_State }),
                new BTSequence(new List<BTNode> { check_Tired_State, check_Hungry, check_Rested, found_Food, swp_Pause_State }),
                new BTSequence(new List<BTNode> { check_Tired_State, check_Starving, check_N_Exhausted, found_Food, swp_Pause_State }),
                new BTSequence(new List<BTNode> { check_Tired_State, check_Rested, check_Lonely, found_Toys, swp_Pause_State })
            };
            foreach (BTSequence sequence in tiredEndSeqs) { TiredEndSequences.Add(sequence); }
        }
        {
            BTSequence[] playfulEndSeqs =
            {
                new BTSequence(new List<BTNode> { check_Playful_State, found_N_Toys, swp_Pause_State }),
                new BTSequence(new List<BTNode> { check_Playful_State, check_Tired, swp_Pause_State }),
                new BTSequence(new List<BTNode> { check_Playful_State, check_Hungry, swp_Pause_State }),

            };
            foreach (BTSequence sequence in playfulEndSeqs) { PlayfulEndSequences.Add(sequence); }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////// RETRIEVE THE DOG PREFAB'S DEFAULT BODY PARTS ///////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        GetExistingBodyParts();
    }
    /** \fn Start
    *   \brief Called when the dog object is first made active. Finds itself a random point in the world to spawn to, sets its Y position so its feet are on the ground (as its height will vary on breed), then sets its current target as the default null object.
    */
    private void Start()
    {
        while (!navigation.GenerateRandomPointInWorld(gameObject)) ;
        float ground2FootDiff = navigation.m_aStarSearch.transform.position.y - m_body[BodyPart.Foot0].m_component.transform.position.y;
        transform.position += new Vector3(0, ground2FootDiff, 0);
        m_currentObjectTarget = controller.defaultNULL;
    }


    /** \fn Update
    *  \brief Called every frame on a loop to check if the dog has been tapped on (is InFocus) and updates the dog's current care/personality values with time progression/item use.
    */
    private void Update()
    {
        UpdateCareValues();
        UpdatePersonalityValues();

        // Set the animator's Speed value to the dog's current forwards velocity to determine if the walking, running or stationary animations should be called.
        m_animationCTRL.SetFloat("Speed", transform.InverseTransformDirection(m_RB.velocity).z);
    }

    /** \fn GetExistingBodyParts
    *  \brief Retrieves all the Dog prefab's default body part component's game objects and adds them to the dog's m_body dictionary. The others are set in DogGeneration as they are assigned dynamically based on the dog's randomised breed.
    */
    private void GetExistingBodyParts()
    {
        m_collider = gameObject.GetComponent<BoxCollider>();

        var allChildObjects = transform.GetComponentsInChildren<Transform>();

        foreach (Transform child in allChildObjects)
        {
            if (Enum.IsDefined(typeof(BodyPart), child.name))
            {
                BodyPart type = (BodyPart)Enum.Parse(typeof(BodyPart), child.name);
                m_body.Add(type, new BodyComponent(type, child.gameObject, child.transform.parent.gameObject));
            }
        }

        m_body[BodyPart.Chest].SetData(DogDataField.Body_Length);
        m_body[BodyPart.Rear].SetData(DogDataField.Body_Length);
        foreach (BodyPart part in (BodyPart[])Enum.GetValues(typeof(BodyPart)))
        {
            if (part.ToString().Contains("Leg")) { m_body[part].SetData(DogDataField.Leg_Length); }
        }
    }

    /** \fn UpdateCareValues
    *  \brief If the dog isn't using an item the care values are decreased by a default value multiplied by the current time scale. If the dog is using an item the "fufillement amount" of that item is added/subtracted from the affected care values instead.
    */
    private void UpdateCareValues()
    {
        foreach (KeyValuePair<DogCareValue, CareProperty> prop in m_careValues)
        {
            if (m_facts["USING_ITEM"] && TargetItemIsFor(prop.Key))
            {
                prop.Value.UpdateValue(m_currentItemTarget.GetCareFufillmentAmount(prop.Key));
            }
            else { prop.Value.UpdateValue(prop.Value.GetUsualDecrement()); }
        }
    }

    /** \fn UpdatePersonalityValues
    *  \brief The same as UpdateCareValues(), however the personality values are changed on a increment indefintely by only changed when the dog is using an item.
    */
    private void UpdatePersonalityValues()
    {
        if (m_facts["USING_ITEM"])
        {
            foreach (KeyValuePair<DogPersonalityValue, PersonalityProperty> prop in m_personalityValues)
            {
                if (TargetItemIsFor(prop.Key))
                {
                    prop.Value.UpdateValue(m_currentItemTarget.GetPersonalityFufillmentAmount(prop.Key));
                }
            }
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////// BEHAVIOUR TREE ACTION FUNCTION BINDINGS ///////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////// Actions to Check and Change the Current FSM State ////////////////////
    private BTState IdleState() { if (m_facts["IDLE"]) { return BTState.SUCCESS; } return BTState.FAILURE; }       //!< Function implementation of its BTAction.
    private BTState HungryState() { if (m_facts["HUNGRY"]) { return BTState.SUCCESS; } return BTState.FAILURE; }   //!< Function implementation of its BTAction.
    private BTState TiredState() { if (m_facts["TIRED"]) { return BTState.SUCCESS; } return BTState.FAILURE; }     //!< Function implementation of its BTAction.
    private BTState PlayfulState() { if (m_facts["PLAYFUL"]) { return BTState.SUCCESS; } return BTState.FAILURE; } //!< Function implementation of its BTAction.

    private BTState SWP_IdleState() { m_facts["SWP_IDLE"] = true; return BTState.SUCCESS; }                        //!< Function implementation of its BTAction.
    private BTState SWP_PauseState() { m_facts["SWP_PAUSE"] = true; return BTState.SUCCESS; }                      //!< Function implementation of its BTAction.
    private BTState SWP_HungryState() { m_facts["SWP_HUNGRY"] = true; return BTState.SUCCESS; }                    //!< Function implementation of its BTAction.
    private BTState SWP_TiredState() { m_facts["SWP_TIRED"] = true; return BTState.SUCCESS; }                      //!< Function implementation of its BTAction.
    private BTState SWP_PlayfulState() { m_facts["SWP_PLAYFUL"] = true; return BTState.SUCCESS; }                  //!< Function implementation of its BTAction.

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////// CARE VALUE BTACTION FUNCTION BINDINGS ///////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /** \fn Starving 
     * \brief Function implementation of its BTAction - checking that the Hunger CareValue is currently in the Starving state. */
    private BTState Starving() { if (m_careValues[DogCareValue.Hunger].IsState("Starving")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn Hungry 
     * \brief Function implementation of its BTAction - checking that the Hunger CareValue is currently in the Hungry state. */
    private BTState Hungry() { if (m_careValues[DogCareValue.Hunger].IsState("Hungry")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn Fed 
     * \brief Function implementation of its BTAction - checking that the Hunger CareValue is currently in the Fed state. */
    private BTState Fed() { if (m_careValues[DogCareValue.Hunger].IsState("Fed")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn Overfed 
     * \brief Function implementation of its BTAction - checking that the Hunger CareValue is currently in the Overfed state. */
    private BTState Overfed() { if (m_careValues[DogCareValue.Hunger].IsState("Overfed")) { return BTState.SUCCESS; } return BTState.FAILURE; }

    /** \fn Lonely 
     * \brief Function implementation of its BTAction - checking that the Attention CareValue is currently in the Lonely state. */
    private BTState Lonely() { if (m_careValues[DogCareValue.Attention].IsState("Lonely")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn Loved 
     * \brief Function implementation of its BTAction - checking that the Attention CareValue is currently in the Loved state. */
    private BTState Loved() { if (m_careValues[DogCareValue.Attention].IsState("Loved")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn Overcrowded 
     * \brief Function implementation of its BTAction - checking that the Attention CareValue is currently in the Overcrowded state. */
    private BTState Overcrowded() { if (m_careValues[DogCareValue.Attention].IsState("Overcrowded")) { return BTState.SUCCESS; } return BTState.FAILURE; }

    /** \fn Exhausted 
     * \brief Function implementation of its BTAction - checking that the Tired CareValue is currently in the Exhausted state. */
    private BTState Exhausted() { if (m_careValues[DogCareValue.Rest].IsState("Exhausted")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn Tired 
     * \brief Function implementation of its BTAction - checking that the Tired CareValue is currently in the Tired state. */
    private BTState Tired() { if (m_careValues[DogCareValue.Rest].IsState("Tired")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn Rested 
     * \brief Function implementation of its BTAction - checking that the Tired CareValue is currently in the Rested state. */
    private BTState Rested() { if (m_careValues[DogCareValue.Rest].IsState("Rested")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn Rejuvinated 
     * \brief Function implementation of its BTAction - checking that the Tired CareValue is currently in the Rejuvinated state. */
    private BTState Rejuvinated() { if (m_careValues[DogCareValue.Rest].IsState("Rejuvinated")) { return BTState.SUCCESS; } return BTState.FAILURE; }

    /** \fn Filthy 
     * \brief Function implementation of its BTAction - checking that the Hygiene CareValue is currently in the Filthy state. */
    private BTState Filthy() { if (m_careValues[DogCareValue.Hygiene].IsState("Filthy")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn Dirty 
      * \brief Function implementation of its BTAction - checking that the Hygiene CareValue is currently in the Dirty state. */
    private BTState Dirty() { if (m_careValues[DogCareValue.Hygiene].IsState("Dirty")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn Clean 
     * \brief Function implementation of its BTAction - checking that the Hygiene CareValue is currently in the Clean state. */
    private BTState Clean() { if (m_careValues[DogCareValue.Hygiene].IsState("Clean")) { return BTState.SUCCESS; } return BTState.FAILURE; }

    /** \fn Dying 
     * \brief Function implementation of its BTAction - checking that the Health CareValue is currently in the Dying state. */
    private BTState Dying() { if (m_careValues[DogCareValue.Health].IsState("Dying")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn Sick 
     * \brief Function implementation of its BTAction - checking that the Health CareValue is currently in the Sick state. */
    private BTState Sick() { if (m_careValues[DogCareValue.Health].IsState("Sick")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn Healthy 
     * \brief Function implementation of its BTAction - checking that the Health CareValue is currently in the Healthy state. */
    private BTState Healthy() { if (m_careValues[DogCareValue.Health].IsState("Healthy")) { return BTState.SUCCESS; } return BTState.FAILURE; }

    /** \fn Distressed 
      * \brief Function implementation of its BTAction - checking that the Happiness CareValue is currently in the Distressed state. */
    private BTState Distressed() { if (m_careValues[DogCareValue.Happiness].IsState("Distressed")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn Upset 
     * \brief Function implementation of its BTAction - checking that the Happiness CareValue is currently in the Upset state. */
    private BTState Upset() { if (m_careValues[DogCareValue.Happiness].IsState("Upset")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn Happy 
     * \brief Function implementation of its BTAction - checking that the Happiness CareValue is currently in the Happy state. */
    private BTState Happy() { if (m_careValues[DogCareValue.Happiness].IsState("Happy")) { return BTState.SUCCESS; } return BTState.FAILURE; }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////// INVERSE CARE VALUE BTACTION FUNCTION BINDINGS ///////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /** \fn NStarving 
     * \brief Function implementation of its BTAction - checking that the Hunger CareValue is NOT currently in the Starving state. */
    private BTState NStarving() { if (!m_careValues[DogCareValue.Hunger].IsState("Starving")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn NHungry 
     * \brief Function implementation of its BTAction - checking that the Hunger CareValue is NOT currently in the Hungry state. */
    private BTState NHungry() { if (!m_careValues[DogCareValue.Hunger].IsState("Hungry")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn NFed 
     * \brief Function implementation of its BTAction - checking that the Hunger CareValue is NOT currently in the Fed state. */
    private BTState NFed() { if (!m_careValues[DogCareValue.Hunger].IsState("Fed")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn NOverfed 
     * \brief Function implementation of its BTAction - checking that the Hunger CareValue is NOT currently in the Overfed state. */
    private BTState NOverfed() { if (!m_careValues[DogCareValue.Hunger].IsState("Overfed")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn NLonely 
     * \brief Function implementation of its BTAction - checking that the Attention CareValue is NOT currently in the Lonely state. */
    private BTState NLonely() { if (!m_careValues[DogCareValue.Attention].IsState("Lonely")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn NLoved 
     * \brief Function implementation of its BTAction - checking that the Attention CareValue is NOT currently in the Loved state. */
    private BTState NLoved() { if (!m_careValues[DogCareValue.Attention].IsState("Loved")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn NOvercrowded 
     * \brief Function implementation of its BTAction - checking that the Attention CareValue is NOT currently in the Overcrowded state. */
    private BTState NOvercrowded() { if (!m_careValues[DogCareValue.Attention].IsState("Overcrowded")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn NExhausted 
     * \brief Function implementation of its BTAction - checking that the Tired CareValue is NOT currently in the Exhausted state. */
    private BTState NExhausted() { if (!m_careValues[DogCareValue.Rest].IsState("Exhausted")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn NTired 
     * \brief Function implementation of its BTAction - checking that the Tired CareValue is NOT currently in the Tired state. */
    private BTState NTired() { if (!m_careValues[DogCareValue.Rest].IsState("Tired")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn NRested 
     * \brief Function implementation of its BTAction - checking that the Tired CareValue is NOT currently in the Rested state. */
    private BTState NRested() { if (!m_careValues[DogCareValue.Rest].IsState("Rested")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn NRejuvinated 
     * \brief Function implementation of its BTAction - checking that the Tired CareValue is NOT currently in the Rejuvinated state. */
    private BTState NRejuvinated() { if (!m_careValues[DogCareValue.Rest].IsState("Rejuvinated")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn NFilthy 
     * \brief Function implementation of its BTAction - checking that the Hygiene CareValue is NOT currently in the Filthy state. */
    private BTState NFilthy() { if (!m_careValues[DogCareValue.Hygiene].IsState("Filthy")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn NDirty 
      * \brief Function implementation of its BTAction - checking that the Hygiene CareValue is NOT currently in the Dirty state. */
    private BTState NDirty() { if (!m_careValues[DogCareValue.Hygiene].IsState("Dirty")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn NClean 
     * \brief Function implementation of its BTAction - checking that the Hygiene CareValue is NOT currently in the Clean state. */
    private BTState NClean() { if (!m_careValues[DogCareValue.Hygiene].IsState("Clean")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn NDying 
     * \brief Function implementation of its BTAction - checking that the Health CareValue is NOT currently in the Dying state. */
    private BTState NDying() { if (!m_careValues[DogCareValue.Health].IsState("Dying")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn NSick 
     * \brief Function implementation of its BTAction - checking that the Health CareValue is NOT currently in the Sick state. */
    private BTState NSick() { if (!m_careValues[DogCareValue.Health].IsState("Sick")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn NHealthy 
     * \brief Function implementation of its BTAction - checking that the Health CareValue is NOT currently in the Healthy state. */
    private BTState NHealthy() { if (!m_careValues[DogCareValue.Health].IsState("Healthy")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn NDistressed 
      * \brief Function implementation of its BTAction - checking that the Happiness CareValue is NOT currently in the Distressed state. */
    private BTState NDistressed() { if (!m_careValues[DogCareValue.Happiness].IsState("Distressed")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn NUpset 
     * \brief Function implementation of its BTAction - checking that the Happiness CareValue is NOT currently in the Upset state. */
    private BTState NUpset() { if (!m_careValues[DogCareValue.Happiness].IsState("Upset")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn NHappy 
     * \brief Function implementation of its BTAction - checking that the Happiness CareValue is NOT currently in the Happy state. */
    private BTState NHappy() { if (!m_careValues[DogCareValue.Happiness].IsState("Happy")) { return BTState.SUCCESS; } return BTState.FAILURE; }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////// GOOD CARE BTACTION FUNCTION BINDINGS ///////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /** \fn AllGood 
    * \brief Function implementation of its BTAction - checking that all the DogCareValues are in their "good" states. */
    private BTState AllGood()
    {
        if (m_careValues[DogCareValue.Hunger].IsState("Fed") &&
            m_careValues[DogCareValue.Attention].IsState("Loved") &&
            m_careValues[DogCareValue.Rest].IsState("Rested") &&
            m_careValues[DogCareValue.Health].IsState("Healthy") &&
            m_careValues[DogCareValue.Happiness].IsState("Happy")
            )
        { return BTState.SUCCESS; }
        return BTState.FAILURE;
    }

    /** \fn GoodCareBonuses 
    * \brief Function implementation of its BTAction - updating some of the personality values slightly while the dog is being cared for well, and adding a pay bonus to the player's money value. */
    private BTState GoodCareBonuses()
    {
        controller.GiveGoodCareBonus();
        m_personalityValues[DogPersonalityValue.Bond].UpdateValue(0.0005f);
        m_personalityValues[DogPersonalityValue.Affection].UpdateValue(0.00025f);
        m_personalityValues[DogPersonalityValue.Tolerance].UpdateValue(0.00005f);
        m_personalityValues[DogPersonalityValue.Obedience].UpdateValue(0.0005f * m_personalityValues[DogPersonalityValue.Intelligence].GetValue());
        return BTState.SUCCESS;
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////// CHECK ITEM AVAILABILITY BTACTION FUNCTION BINDINGS ///////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /** \fn CanFindFood
         * \brief Function implementation of its BTAction - checks the Control script for currently active Food Items in its Item list from the StoreController. */
    private BTState CanFindFood() { if (FindItemType(ItemType.FOOD)) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn CanFindBed                                                                                                                                           
         * \brief Function implementation of its BTAction - checks the Control script for currently active Bed Items in its Item list from the StoreController.  */
    private BTState CanFindBed() { if (FindItemType(ItemType.BED)) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn CanFindToys                                                                                                                                          
         * \brief Function implementation of its BTAction - checks the Control script for currently active Toy Items in its Item list from the StoreController.  */
    private BTState CanFindToys() { if (FindItemType(ItemType.TOYS)) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn CannotFindFood
         * \brief Function implementation of its BTAction - checks the Control script for if there are NOT any active Food Items in its Item list from the StoreController. */
    private BTState CannotFindFood() { if (!FindItemType(ItemType.FOOD)) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn CannotFindBed                                                                                                                     
         * \brief Function implementation of its BTAction - checks the Control script for if there are NOT any active Bed Items in its Item list from the StoreController.  */
    private BTState CannotFindBed() { if (!FindItemType(ItemType.BED)) { return BTState.SUCCESS; } return BTState.FAILURE; }
    /** \fn CannotFindToys                                                                                                               
         * \brief Function implementation of its BTAction - checks the Control script for if there are NOT any active Toy Items in its Item list from the StoreController.  */
    private BTState CannotFindToys() { if (!FindItemType(ItemType.TOYS)) { return BTState.SUCCESS; } return BTState.FAILURE; }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////// MOVEMENT SPEED BTACTION FUNCTION BINDINGS ///////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /** \fn MoveCrawling                                                                                                               
    * \brief Function implementation of its BTAction - sets the dog's Pathfinding script movement speed to Crawling.  */
    public BTState MoveCrawling() { navigation.SetSpeed(MoveSpeed.Crawling); return BTState.SUCCESS; }
    /** \fn MoveWalking                                                                                                               
    * \brief Function implementation of its BTAction - sets the dog's Pathfinding script movement speed to Walking.  */
    public BTState MoveWalking() { navigation.SetSpeed(MoveSpeed.Walking); return BTState.SUCCESS; }
    /** \fn MoveRunning                                                                                                               
    * \brief Function implementation of its BTAction - sets the dog's Pathfinding script movement speed to Running.  */
    public BTState MoveRunning() { navigation.SetSpeed(MoveSpeed.Running); return BTState.SUCCESS; }

    /** \fn GetPersonalityState                                                                                                               
    * \brief Retrieves the given Personality Value's current state. (Not currently used or in the form of multiple BTState functions like the CareValues as the personality values don't yet affect the BT sequences).
    */
    public string GetPersonalityState(DogPersonalityValue forProperty)
    {
        return m_personalityValues[forProperty].GetState();
    }

    /** \fn TargetItemIsFor                                                                                                               
    * \brief Returns whether or not the dog's current target Item modifies the given Care Value when used.
    */
    private bool TargetItemIsFor(DogCareValue value)
    {
        if (m_currentObjectTarget != controller.defaultNULL)
        {
            return m_currentItemTarget.FufillsCareValue(value);
        }
        return false;
    }

    /** \fn TargetItemIsFor                                                                                                               
    * \brief Returns whether or not the dog's current target Item modifies the given Personality Value when used.
    */
    private bool TargetItemIsFor(DogPersonalityValue value)
    {
        if (m_currentObjectTarget != controller.defaultNULL)
        {
            return m_currentItemTarget.FufillsPersonalityValue(value);
        }
        return false;
    }

    /** \fn SetCurrentTargetItem                                                                                                               
    * \brief This is called by an Item itself when the dog has successfully attempted to retrieve a usable instance from it. The Item sets an available instance as the dog's target object and itself as the dog's target Item.
    */
    public void SetCurrentTargetItem(Item newItem, GameObject itemInstance)
    {
        m_currentItemTarget = newItem;
        m_currentObjectTarget = itemInstance;
    }

    /** \fn FindItemType                                                                                                               
    * \brief Checks the store controller for an Item of the given ItemType, which if it does will then call that Item's function to find an available ItemInstance from its object pool. Only if both searches are successful will this function return as true.
    */
    public bool FindItemType(ItemType type)
    {
        if (controller.GetClosestActiveItemFor(type, this))
        {
            return true;
        }
        else if (controller.GetActiveItemFor(type, this))
        {
            return true;
        }
        return false;
    }

    /** \fn ReachedTarget                                                                                                               
    * \brief Sets the Pathfinding script's target to this dog's current ItemInstance object target if it isn't set to it already, then runs Pathfinding's AttemptToReachTarget() function to move the dog towards said target. 
    * This function will return as true only when that function returns true, which will occur when the dog collides with its target's collider.
    */
    public bool ReachedTarget()
    {
        if (m_currentObjectTarget != controller.defaultNULL)
        {
            if (navigation.IsSetToObject(m_currentObjectTarget))
            {
                if (navigation.AttemptToReachTarget())
                {
                    if (m_currentItemTarget.UseItemInstance(gameObject, m_currentObjectTarget))
                    {
                        return true;
                    }
                }
            }
            else { navigation.SetTarget(m_currentObjectTarget); }
        }
        return false;
    }

    /** \fn UseItem                                                                                                               
    * \brief Will usually be called after ReachedTarget() has returned as true. If the dog isn't already "using" the item it has reached, the fact "USING_ITEM" will be set to true and the dog will be moved to the Item's using position if applicable.
    * The target Item's user will be set to this dog to stop other dogs from using the Item at the same time.
    * If the Item is single-use, the dog's Care and Personality values will be updated here, as the Item use will end almost instantly so the Update function which calls these value updates will likey miss that an Item has been used otherwise.
    */
    public void UseItem()
    {
        if (m_currentObjectTarget != controller.defaultNULL)
        {
            if (!m_facts["USING_ITEM"])
            {
                if (m_currentItemTarget.UseItemInstance(gameObject, m_currentObjectTarget))
                {
                    m_facts["USING_ITEM"] = true;

                    Debug.Log(name + ": ITEM USE START");

                    if (m_currentItemTarget.NeedsUseOffset())
                    {
                        Vector3 usePosition = m_currentObjectTarget.transform.position;
                        Vector2 usePosOffset = m_currentItemTarget.GetUsePosOffset();
                        usePosition.x += usePosOffset.x;
                        usePosition.y = transform.position.y;
                        usePosition.z += usePosOffset.y;
                        transform.position = usePosition;
                    }

                    if (m_currentItemTarget.IsSingleUse())
                    {
                        UpdateCareValues();
                        UpdatePersonalityValues();
                        EndItemUse();
                    }
                }
                else { Debug.Log("Failed use."); }
            }
        }
        else { Debug.LogWarning("No item to use."); }
    }

    /** \fn PickUpTarget                                                                                                               
    * \brief Makes the current target object a child of the dog's snout object in the hierarchy, moves its local position to the snout's holding position (front of mouth), then sets the fact "HOLDING_ITEM" to true. 
    * This parenting will make the object move with the dog.
    */
    public void PickUpTarget()
    {
        if (m_currentObjectTarget != controller.defaultNULL)
        {
            if (!m_facts["HOLDING_ITEM"])
            {
                m_heldItemPrevYPos = m_currentObjectTarget.transform.position.y;
                m_currentObjectTarget.transform.SetParent(m_body[BodyPart.Snout].m_component.transform);
                m_currentObjectTarget.transform.localPosition = Vector3.zero + m_holdingOffset;
                m_facts["HOLDING_ITEM"] = true;
            }
        }
    }


    /** \fn EndItemUse                                                                                                               
    * \brief Calls the EndItemUse() function in Controller so if the Item was temporary its position can be made placable again. That function will then set the ItemInstance's user back to null and deactivate the instance if it was single use.
    * If the dog was holding the Item it was using, the instance object is unparented from the snout and its Y-position reset back to what it was before being picked up. (This Y-position is set in PickUpTarget() before the instance is "picked up").
    * The fact "USING_ITEM" is set back to false and the dog's target Item and ItemInstance object are reset back to their default values.
    */
    public void EndItemUse()
    {
        controller.EndItemUse(m_currentItemTarget, m_currentObjectTarget);
        m_facts["USING_ITEM"] = false;

        if (m_facts["HOLDING_ITEM"])
        {
            m_currentObjectTarget.transform.SetParent(m_currentItemTarget.GetInstanceParent(m_currentObjectTarget).transform);

            Vector3 groundPos = m_currentObjectTarget.transform.position;
            groundPos.y = m_heldItemPrevYPos;
            m_currentObjectTarget.transform.position = groundPos;

            m_facts["HOLDING_ITEM"] = false;
        }
        Debug.Log(name + ": ITEM USE END");
        ClearCurrentTarget();
    }

    /** \fn Wander                                                                                                               
    * \brief Sets the Pathfinding script's target to the dog's random point object (if it isn't set to that already), then generates/follows a path to that point. In Pathfinding, the random point's position will be randomised within the floor's bounds once the target is reached.
    */
    public void Wander()
    {
        if (navigation.IsSetToRandom())
        {
            navigation.AttemptToReachTarget();
        }
        else { navigation.SetTargetToRandom(); }
    }

    /** \fn Pause                                                                                                               
    * \brief Stops the dog and sets the fact "WAITING" to true so the Pause state can't be exited until the given parameter wait time in seconds has passed. After the wait period the fact will be set back to false so the dog can exit the state back into Idle.
    */
    public IEnumerator Pause(float waitTime = 0.0f)
    {
        m_RB.velocity = Vector3.zero;
        m_facts["WAITING"] = true;
        yield return new WaitForSeconds(waitTime);
        m_facts["WAITING"] = false;
    }

    /** \fn ClearCurrentTarget                                                                                                               
    * \brief Sets the dog's target Item back to null and the ItemInstance object back to the defaultNULL empty object, then sets Pathfinding's target to the random point so it doesn't have a lost reference when next called on.
    */
    private void ClearCurrentTarget()
    {
        m_currentItemTarget = null;
        m_currentObjectTarget = controller.defaultNULL;
        navigation.SetTargetToRandom();
    }
}