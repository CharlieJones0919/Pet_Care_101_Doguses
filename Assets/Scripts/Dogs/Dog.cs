/** \file Dog.cs */
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

    public Animator m_animationCTRL; //!< Animation controller. 
    public BoxCollider m_collider;   //!< Dog's collider component/bounds. Set dynamically in DogGeneration depending on the dog's mesh space varying by breed type.
    [SerializeField] private Rigidbody m_RB; //!< Dog's rigid body to derive it's speed from/to.

    public Dictionary<DogCareValue, CareProperty> m_careValues = new Dictionary<DogCareValue, CareProperty>(); //!< A list of the dog's current care value properties so they can be easily iterated through.
    public Dictionary<DogPersonalityValue, PersonalityProperty> m_personalityValues = new Dictionary<DogPersonalityValue, PersonalityProperty>();   //!< A list of the dog's personality value properties so they can be easily iterated through. (Have not been implemented in full to be conditional on the dog's average care values in the long term).

    private Item m_currentItemTarget; //!< An item the dog is currently focused on. Stored so it can be travelled to or used if reached and usable.
    [SerializeField] private GameObject m_currentObjectTarget;  //!< The specific object instance of the current target item.

    private float m_heldItemPrevYPos; //!< If an item has been picked up, it's previous Y position is stored so it can be placed back down.
    private Vector3 m_holdingOffset = new Vector3(0.0f, -0.45f, 1.0f); //!< Local offset from the dog's snout a held item should be positioned at.

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////// RULE BASED SYSTEM VARIABLES ////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Used to store definitions for the dog's different "facts" and rules based on them. (Just used to determine if a state should be exited into another based on the outcome of the state's behaviour tree sequences).
    public Dictionary<string, bool> m_facts = new Dictionary<string, bool>();
    public List<Rule> m_rules = new List<Rule>();

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////// BEHAVIOURAL TREE VARIABLES ////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Action nodes which are children to sequence nodes which must be successful in a sequence for the sequence node to succeed.
    //////////////////// Check Current FSM State Actions ////////////////////
    public BTAction check_Idle_State;
    public BTAction check_Hungry_State;
    public BTAction check_Tired_State;
    public BTAction check_Playful_State;
    //////////////////// Swap FSM State Actions ////////////////////
    public BTAction swp_Idle_State;
    public BTAction swp_Pause_State;
    public BTAction swp_Hungry_State;
    public BTAction swp_Tired_State;
    public BTAction swp_Playful_State;
    //////////////////// Status Check Actions ////////////////////
    ///// Hunger Status /////
    public BTAction check_Starving;
    public BTAction check_Hungry;
    public BTAction check_Fed;
    public BTAction check_Overfed;
    ///// Attention Status /////
    public BTAction check_Lonely;
    public BTAction check_Loved;
    public BTAction check_Overcrowded;
    ///// Rest Status /////
    public BTAction check_Exhausted;
    public BTAction check_Tired;
    public BTAction check_Rested;
    public BTAction check_Rejuvinated;
    ///// Hygiene Status /////              // Ran out of time to implement items to restore care value. 
    ///public BTAction check_Filthy;
    ///public BTAction check_Dirty;
    ///public BTAction check_Clean;
    ///// Health Status /////
    public BTAction check_Dying;
    public BTAction check_Sick;
    public BTAction check_Healthy;
    ///// Happiness Status /////
    public BTAction check_Distressed;
    public BTAction check_Upset;
    public BTAction check_Happy;

    public BTAction check_AllGood;
    public BTAction bonus_GoodCare;

    //////////////////// Status False Check Actions ////////////////////
    public BTAction check_N_Starving;
    public BTAction check_N_Hungry;
    public BTAction check_N_Fed;
    public BTAction check_N_Overfed;
    public BTAction check_N_Lonely;
    public BTAction check_N_Loved;
    public BTAction check_N_Overcrowded;
    public BTAction check_N_Exhausted;
    public BTAction check_N_Tired;
    public BTAction check_N_Rested;
    public BTAction check_N_Rejuvinated;
    public BTAction check_N_Dying;
    public BTAction check_N_Sick;
    public BTAction check_N_Healthy;
    public BTAction check_N_Distressed;
    public BTAction check_N_Upset;
    public BTAction check_N_Happy;

    //////////////////// Items Found Check Actions ////////////////////
    public BTAction found_Food;
    public BTAction found_Bed;
    public BTAction found_Toys;
    public BTAction found_N_Food;
    public BTAction found_N_Bed;
    public BTAction found_N_Toys;

    //////////////////// Set Move Speed Actions ////////////////////
    public BTAction move_Crawling;
    public BTAction move_Walking;
    public BTAction move_Running;

    //////////////////// Behaviour Tree Condition Sequence Lists for Leaving Each State ////////////////////
    public List<BTSequence> GlobalSequences = new List<BTSequence>(); // Checks general values which shouldn't result in state changes. 
    public List<BTSequence> IdleEndSequences = new List<BTSequence>();
    public List<BTSequence> HungryEndSequences = new List<BTSequence>();
    public List<BTSequence> TiredEndSequences = new List<BTSequence>();
    public List<BTSequence> PlayfulEndSequences = new List<BTSequence>();

    /** \fn Awake
    *  \brief Callled once when the scene loads to instantiate variable values and functions before the application starts. Used to define and add states to the FSM, rules abd facts.
    */
    private void Awake()
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////// BEHAVIOUR TREE STATES ///////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Defining the dog's FSM states then adding them to the dog's FSM. 
        Dictionary<Type, State> newStates = new Dictionary<Type, State>();
        newStates.Add(typeof(Idle), new Idle(this));    // This is the default starting state. (Wandering until current care values need attending to).
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

        check_AllGood = new BTAction(AllGood);
        bonus_GoodCare = new BTAction(GoodCareBonuses);

        found_Food = new BTAction(CanFindFood);
        found_Bed = new BTAction(CanFindBed);
        found_Toys = new BTAction(CanFindToys);
        found_N_Food = new BTAction(CannotFindFood);
        found_N_Bed = new BTAction(CannotFindBed);
        found_N_Toys = new BTAction(CannotFindToys);

        move_Crawling = new BTAction(MoveCrawling);
        move_Walking = new BTAction(MoveWalking);
        move_Running = new BTAction(MoveRunning);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////// BEHAVIOUR TREE SEQUENCES ///////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            BTSequence[] globalSeqs =
            {
                new BTSequence(new List<BTNode> { check_AllGood, bonus_GoodCare }),

                new BTSequence(new List<BTNode> { check_Lonely, move_Crawling }),
                new BTSequence(new List<BTNode> { check_Sick, move_Crawling }),
                new BTSequence(new List<BTNode> { check_Distressed, move_Crawling })
            };
            foreach (BTSequence sequence in globalSeqs) { GlobalSequences.Add(sequence); }
        }
        // The behaviour tree sequences (list of conditional node checks/actions) to determine if the dog should exit its current FSM state.
        {
            BTSequence[] idleEndSeqs =
            {
                new BTSequence(new List<BTNode> { check_Idle_State, check_Starving, found_N_Food, move_Crawling }),
                new BTSequence(new List<BTNode> { check_Idle_State, check_Exhausted, found_N_Bed, move_Crawling }),

                new BTSequence(new List<BTNode> { check_Idle_State, check_Hungry, check_Exhausted, found_N_Bed, found_Food, move_Crawling, swp_Hungry_State }),
                new BTSequence(new List<BTNode> { check_Idle_State, check_Hungry, check_N_Tired, found_Food, move_Walking, swp_Hungry_State }),
                new BTSequence(new List<BTNode> { check_Idle_State, check_Starving, check_N_Exhausted, found_Food, move_Running, swp_Hungry_State }),

                new BTSequence(new List<BTNode> { check_Idle_State, check_Tired, found_Bed, move_Walking, swp_Tired_State }),
                new BTSequence(new List<BTNode> { check_Idle_State, check_Exhausted, found_Bed, move_Crawling, swp_Tired_State }),

                new BTSequence(new List<BTNode> { check_Idle_State, check_N_Lonely, check_N_Tired, found_Food, move_Walking, swp_Hungry_State }),
                new BTSequence(new List<BTNode> { check_Idle_State, check_Rested, check_Fed, found_Toys, move_Walking, swp_Playful_State })
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
                new BTSequence(new List<BTNode> { check_Hungry_State, check_Fed, check_Lonely, found_Toys, swp_Pause_State })
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
    public BTState IdleState() { if (m_facts["IDLE"]) { return BTState.SUCCESS; } return BTState.FAILURE; }
    public BTState HungryState() { if (m_facts["HUNGRY"]) { return BTState.SUCCESS; } return BTState.FAILURE; }
    public BTState TiredState() { if (m_facts["TIRED"]) { return BTState.SUCCESS; } return BTState.FAILURE; }
    public BTState PlayfulState() { if (m_facts["PLAYFUL"]) { return BTState.SUCCESS; } return BTState.FAILURE; }

    public BTState SWP_IdleState() { m_facts["SWP_IDLE"] = true; return BTState.SUCCESS; }
    public BTState SWP_PauseState() { m_facts["SWP_PAUSE"] = true; return BTState.SUCCESS; }
    public BTState SWP_HungryState() { m_facts["SWP_HUNGRY"] = true; return BTState.SUCCESS; }
    public BTState SWP_TiredState() { m_facts["SWP_TIRED"] = true; return BTState.SUCCESS; }
    public BTState SWP_PlayfulState() { m_facts["SWP_PLAYFUL"] = true; return BTState.SUCCESS; }

    //////////////////// State Checks Based on Care Values ////////////////////
    public BTState Starving() { if (m_careValues[DogCareValue.Hunger].IsState("Starving")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    public BTState Hungry() { if (m_careValues[DogCareValue.Hunger].IsState("Hungry")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    public BTState Fed() { if (m_careValues[DogCareValue.Hunger].IsState("Fed")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    public BTState Overfed() { if (m_careValues[DogCareValue.Hunger].IsState("Overfed")) { return BTState.SUCCESS; } return BTState.FAILURE; }

    public BTState Lonely() { if (m_careValues[DogCareValue.Attention].IsState("Lonely")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    public BTState Loved() { if (m_careValues[DogCareValue.Attention].IsState("Loved")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    public BTState Overcrowded() { if (m_careValues[DogCareValue.Attention].IsState("Overcrowded")) { return BTState.SUCCESS; } return BTState.FAILURE; }

    public BTState Exhausted() { if (m_careValues[DogCareValue.Rest].IsState("Exhausted")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    public BTState Tired() { if (m_careValues[DogCareValue.Rest].IsState("Tired")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    public BTState Rested() { if (m_careValues[DogCareValue.Rest].IsState("Rested")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    public BTState Rejuvinated() { if (m_careValues[DogCareValue.Rest].IsState("Rejuvinated")) { return BTState.SUCCESS; } return BTState.FAILURE; }

    public BTState Filthy() { if (m_careValues[DogCareValue.Hygiene].IsState("Filthy")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    public BTState Dirty() { if (m_careValues[DogCareValue.Hygiene].IsState("Dirty")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    public BTState Clean() { if (m_careValues[DogCareValue.Hygiene].IsState("Clean")) { return BTState.SUCCESS; } return BTState.FAILURE; }

    public BTState Dying() { if (m_careValues[DogCareValue.Health].IsState("Dying")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    public BTState Sick() { if (m_careValues[DogCareValue.Health].IsState("Sick")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    public BTState Healthy() { if (m_careValues[DogCareValue.Health].IsState("Healthy")) { return BTState.SUCCESS; } return BTState.FAILURE; }

    public BTState Distressed() { if (m_careValues[DogCareValue.Happiness].IsState("Distressed")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    public BTState Upset() { if (m_careValues[DogCareValue.Happiness].IsState("Upset")) { return BTState.SUCCESS; } return BTState.FAILURE; }
    public BTState Happy() { if (m_careValues[DogCareValue.Happiness].IsState("Happy")) { return BTState.SUCCESS; } return BTState.FAILURE; }

    //////////////////// Inverse of State Checks ////////////////////
    public BTState NStarving() { if (m_careValues[DogCareValue.Hunger].IsState("Starving")) { return BTState.FAILURE; } return BTState.SUCCESS; }
    public BTState NHungry() { if (m_careValues[DogCareValue.Hunger].IsState("Hungry")) { return BTState.FAILURE; } return BTState.SUCCESS; }
    public BTState NFed() { if (m_careValues[DogCareValue.Hunger].IsState("Fed")) { return BTState.FAILURE; } return BTState.SUCCESS; }
    public BTState NOverfed() { if (m_careValues[DogCareValue.Hunger].IsState("Overfed")) { return BTState.FAILURE; } return BTState.SUCCESS; }
    public BTState NLonely() { if (m_careValues[DogCareValue.Attention].IsState("Lonely")) { return BTState.FAILURE; } return BTState.SUCCESS; }
    public BTState NLoved() { if (m_careValues[DogCareValue.Attention].IsState("Loved")) { return BTState.FAILURE; } return BTState.SUCCESS; }
    public BTState NOvercrowded() { if (m_careValues[DogCareValue.Attention].IsState("Overcrowded")) { return BTState.FAILURE; } return BTState.SUCCESS; }
    public BTState NExhausted() { if (m_careValues[DogCareValue.Rest].IsState("Exhausted")) { return BTState.FAILURE; } return BTState.SUCCESS; }
    public BTState NTired() { if (m_careValues[DogCareValue.Rest].IsState("Tired")) { return BTState.FAILURE; } return BTState.SUCCESS; }
    public BTState NRested() { if (m_careValues[DogCareValue.Rest].IsState("Rested")) { return BTState.FAILURE; } return BTState.SUCCESS; }
    public BTState NRejuvinated() { if (m_careValues[DogCareValue.Rest].IsState("Rejuvinated")) { return BTState.FAILURE; } return BTState.SUCCESS; }
    public BTState NFilthy() { if (m_careValues[DogCareValue.Hygiene].IsState("Filthy")) { return BTState.FAILURE; } return BTState.SUCCESS; }
    public BTState NDirty() { if (m_careValues[DogCareValue.Hygiene].IsState("Dirty")) { return BTState.FAILURE; } return BTState.SUCCESS; }
    public BTState NClean() { if (m_careValues[DogCareValue.Hygiene].IsState("Clean")) { return BTState.FAILURE; } return BTState.SUCCESS; }
    public BTState NDying() { if (m_careValues[DogCareValue.Health].IsState("Dying")) { return BTState.FAILURE; } return BTState.SUCCESS; }
    public BTState NSick() { if (m_careValues[DogCareValue.Health].IsState("Sick")) { return BTState.FAILURE; } return BTState.SUCCESS; }
    public BTState NHealthy() { if (m_careValues[DogCareValue.Health].IsState("Healthy")) { return BTState.FAILURE; } return BTState.SUCCESS; }
    public BTState NDistressed() { if (m_careValues[DogCareValue.Happiness].IsState("Distressed")) { return BTState.FAILURE; } return BTState.SUCCESS; }
    public BTState NUpset() { if (m_careValues[DogCareValue.Happiness].IsState("Upset")) { return BTState.FAILURE; } return BTState.SUCCESS; }
    public BTState NHappy() { if (m_careValues[DogCareValue.Happiness].IsState("Happy")) { return BTState.FAILURE; } return BTState.SUCCESS; }

    //////////////////// Good Care Value Check ////////////////////
    public BTState AllGood()
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

    //////////////////// Good Care Bonus Action ////////////////////
    public BTState GoodCareBonuses()
    {
        controller.GiveGoodCareBonus();
        m_personalityValues[DogPersonalityValue.Bond].UpdateValue(0.0005f);
        m_personalityValues[DogPersonalityValue.Affection].UpdateValue(0.00025f);
        m_personalityValues[DogPersonalityValue.Tolerance].UpdateValue(0.00005f);
        m_personalityValues[DogPersonalityValue.Obedience].UpdateValue(0.0005f * m_personalityValues[DogPersonalityValue.Intelligence].GetValue());
        return BTState.SUCCESS;
    }

    //////////////////// Item Location Check Actions
    public BTState CanFindFood() { if (FindItemType(ItemType.FOOD)) { return BTState.SUCCESS; } return BTState.FAILURE; }
    public BTState CanFindBed() { if (FindItemType(ItemType.BED)) { return BTState.SUCCESS; } return BTState.FAILURE; }
    public BTState CanFindToys() { if (FindItemType(ItemType.TOYS)) { return BTState.SUCCESS; } return BTState.FAILURE; }
    public BTState CannotFindFood() { if (!FindItemType(ItemType.FOOD)) { return BTState.SUCCESS; } return BTState.FAILURE; }
    public BTState CannotFindBed() { if (!FindItemType(ItemType.BED)) { return BTState.SUCCESS; } return BTState.FAILURE; }
    public BTState CannotFindToys() { if (!FindItemType(ItemType.TOYS)) { return BTState.SUCCESS; } return BTState.FAILURE; }

    //////////////////// Movement Speed Actions ////////////////////
    public BTState MoveCrawling() { navigation.SetSpeed(MoveSpeed.Crawling); return BTState.SUCCESS; }
    public BTState MoveWalking() { navigation.SetSpeed(MoveSpeed.Walking); return BTState.SUCCESS; }
    public BTState MoveRunning() { navigation.SetSpeed(MoveSpeed.Running); return BTState.SUCCESS; }

    public string GetPersonalityState(DogPersonalityValue forProperty)
    {
        return m_personalityValues[forProperty].GetState();
    }

    public bool HasTarget()
    {
        return (m_currentObjectTarget != controller.defaultNULL);
    }

    public bool TargetItemIsFor(DogCareValue value)
    {
        if (m_currentObjectTarget != controller.defaultNULL)
        {
            return m_currentItemTarget.FufillsCareValue(value);
        }
        return false;
    }

    public bool TargetItemIsFor(DogPersonalityValue value)
    {
        if (m_currentObjectTarget != controller.defaultNULL)
        {
            return m_currentItemTarget.FufillsPersonalityValue(value);
        }
        return false;
    }

    public void SetCurrentTargetItem(Item newItem, GameObject itemInstance)
    {
        m_currentItemTarget = newItem;
        m_currentObjectTarget = itemInstance;
    }

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

    public void UseItem()
    {
        if (m_currentObjectTarget != controller.defaultNULL)
        {
            if (!m_facts["USING_ITEM"])
            {
                if (m_currentItemTarget.UseItemInstance(gameObject, m_currentObjectTarget))
                {
                    m_facts["USING_ITEM"] = true;

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

    public void EndItemUse()
    {
        Debug.LogWarning("Ending Item Use");
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

        ClearCurrentTarget();
    }

    public void Wander()
    {
        if (navigation.IsSetToRandom())
        {
            navigation.AttemptToReachTarget();
        }
        else { navigation.SetTargetToRandom(); }
    }

    public IEnumerator Pause(float waitTime = 0.0f)
    {
        m_RB.velocity = Vector3.zero;
        m_facts["WAITING"] = true;
        yield return new WaitForSeconds(waitTime);
        m_facts["WAITING"] = false;
    }

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

    public void ClearCurrentTarget()
    {
        m_currentItemTarget = null;
        m_currentObjectTarget = controller.defaultNULL;
        navigation.SetTargetToRandom();
    }
}