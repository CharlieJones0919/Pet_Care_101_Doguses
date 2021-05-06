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
    [SerializeField] private Controller controller;     //!< Reference to the game's Controller script to retrieve data required by all dogs.
    [SerializeField] private Pathfinding navigation;   //!< Instance of the Pathfinding script for the dog to utalise for navigation around the map.
    public void SetController(Controller ctrl) { controller = ctrl; navigation.m_aStarSearch = controller.groundSearch; navigation.m_randomPointStorage = controller.randomPointStorage; }

    public string m_name;   //!< This dog's name. 
    public DogBreed m_breed;  //!< The breed of this dog.
    public int m_maxAge;
    public int m_age;       //!< Age of this dog - how long since it has was instantiated in game time. (Not yet implemented).
    public Dictionary<BodyPart, BodyComponent> m_body = new Dictionary<BodyPart, BodyComponent>();

    public Animator m_animationCTRL;
    public BoxCollider m_collider;
    [SerializeField] private Rigidbody m_RB;

    public Dictionary<DogCareValue, CareProperty> m_careValues = new Dictionary<DogCareValue, CareProperty>(); //!< A list of the dog's current care value properties so they can be easily iterated through.
    public Dictionary<DogPersonalityValue, PersonalityProperty> m_personalityValues = new Dictionary<DogPersonalityValue, PersonalityProperty>();   //!< A list of the dog's personality value properties so they can be easily iterated through. (Have not been implemented in full).

    private Item m_currentItemTarget;
    [SerializeField] private GameObject m_currentObjectTarget;  //!< An item on the map the dog is currently using or travelling towards.   

    private float m_heldItemPrevYPos;
    private Vector3 m_holdingOffset = new Vector3(0.0f, -0.45f, 1.0f);

    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////// BEHAVIOURAL TREE ////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Will be used to store definitions for the dog's different "facts" and rules based on them.
    public Dictionary<string, bool> m_facts = new Dictionary<string, bool>();
    public List<Rule> m_rules = new List<Rule>();

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
    ///// Hygiene Status /////
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

    public BTAction check_AllGood;
    public BTAction bonus_GoodCare;

    public BTAction found_Food;
    public BTAction found_Bed;
    public BTAction found_Toys;
    public BTAction found_N_Food;
    public BTAction found_N_Bed;
    public BTAction found_N_Toys;

    public BTAction move_Crawling;
    public BTAction move_Walking;
    public BTAction move_Running;

    //////////////////// Behaviour Tree Condition Sequences ////////////////////
    public List<BTSequence> GlobalSequences = new List<BTSequence>();
    public List<BTSequence> IdleEndSequences = new List<BTSequence>();
    public List<BTSequence> HungryEndSequences = new List<BTSequence>();
    public List<BTSequence> TiredEndSequences = new List<BTSequence>();
    public List<BTSequence> PlayfulEndSequences = new List<BTSequence>();

    /** \fn Awake
    *  \brief Callled once when the scene loads to instantiate variable values and functions before the application starts. Used to define and add states to the FSM.
    */
    private void Awake()
    {
        //Define the dog's FSM states then add them to the object's FSM. (Implementation is not finished yet).
        Dictionary<Type, State> newStates = new Dictionary<Type, State>();

        newStates.Add(typeof(Idle), new Idle(this));    // This is the default starting state. (Wandering until current care values need attending to).
        newStates.Add(typeof(Pause), new Pause(this));
        newStates.Add(typeof(Hungry), new Hungry(this));
        newStates.Add(typeof(Tired), new Tired(this));
        newStates.Add(typeof(Playful), new Playful(this));

        GetComponent<FiniteStateMachine>().SetStates(newStates); //Add defined states to FSM.
        GetExistingBodyParts();
    }

    void Start()
    {
        while (!navigation.GenerateRandomPointInWorld(gameObject)) ;
        float ground2FootDiff = navigation.m_aStarSearch.transform.position.y - m_body[BodyPart.Foot0].m_component.transform.position.y;
        transform.position += new Vector3(0, ground2FootDiff, 0);
        m_currentObjectTarget = controller.defaultNULL;

        ////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////// RBS FACTS ///////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////
        m_facts.Add("IDLE", true);
        m_facts.Add("HUNGRY", false);
        m_facts.Add("TIRED", false);
        m_facts.Add("PLAYFUL", false);

        m_facts.Add("IS_FOCUS", true);
        m_facts.Add("USING_ITEM", false);
        m_facts.Add("WAITING", false);
        m_facts.Add("NEEDS_2_FINISH_ANIM", false);
        m_facts.Add("HOLDING_ITEM", false);

        m_facts.Add("SWP_IDLE", false);
        m_facts.Add("SWP_PAUSE", false);
        m_facts.Add("SWP_HUNGRY", false);
        m_facts.Add("SWP_TIRED", false);
        m_facts.Add("SWP_PLAYFUL", false);

        ////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////// RBS RULES ///////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////
        m_rules.Add(new Rule("IDLE", "SWP_PAUSE", Rule.Predicate.And, typeof(Pause)));
        m_rules.Add(new Rule("IDLE", "SWP_HUNGRY", Rule.Predicate.And, typeof(Hungry)));
        m_rules.Add(new Rule("IDLE", "SWP_TIRED", Rule.Predicate.And, typeof(Tired)));
        m_rules.Add(new Rule("IDLE", "SWP_PLAYFUL", Rule.Predicate.And, typeof(Playful)));

        m_rules.Add(new Rule("HUNGRY", "SWP_PAUSE", Rule.Predicate.And, typeof(Pause)));
        m_rules.Add(new Rule("TIRED", "SWP_PAUSE", Rule.Predicate.And, typeof(Pause)));
        m_rules.Add(new Rule("PLAYFUL", "SWP_PAUSE", Rule.Predicate.And, typeof(Pause)));

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////// BEHAVIOUR TREE ACTIONS ///////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////
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

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////// BEHAVIOUR TREE SEQUENCES ///////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
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

        //leave_Idle_2_Tired = new BTSequence(new List<BTNode> { });
        //leave_Idle_2_Playful = new BTSequence(new List<BTNode> { });
        //
        //leave_Pause = new BTSequence(new List<BTNode> { });
        //leave_Hungry = new BTSequence(new List<BTNode> { });
        //leave_Tired = new BTSequence(new List<BTNode> { });
        //leave_Playful = new BTSequence(new List<BTNode> { });
    }


    /** \fn Update
     *  \brief Called every frame on a loop to check if the dog has been tapped on (is InFocus) and updates the dog's current care values with time progression.
     *  */
    private void Update()
    {
        UpdateCareValues();
        UpdatePersonalityValues();

#if UNITY_EDITOR //If in the editor, check for mouse input.
        if (Input.GetMouseButtonDown(0))
        {
            Ray raycast = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit raycastHit;

            if (Physics.Raycast(raycast, out raycastHit, Mathf.Infinity))
            {
                Debug.Log("Selected: " + raycastHit.collider.gameObject.name);
                if (raycastHit.collider == m_collider) { m_facts["IS_FOCUS"] = true; }
            }
        }
#elif UNITY_IOS || UNITY_ANDROID //If not in the editor check for touch input. 
         if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began)) //Gets first touch input.
        {
            Ray raycast = Camera.main.ScreenPointToRay(Input.GetTouch(0).position); //A raycast between the camera and touch position to get the world position of the touch.
            RaycastHit raycastHit;

            if (Physics.Raycast(raycast, out raycastHit, Mathf.Infinity)) //If the raycast hits anything...
            {
                if (raycastHit.collider == m_collider) m_facts["IS_FOCUS"] = true; //If the collider hit belongs to this object, this dog is in focus.
            } 
        }
#endif

        if (m_facts["IS_FOCUS"])
        {
            if (controller.UIOutput.GetFocusDog() != gameObject)
            {
                controller.UIOutput.SetFocusDog(this);
            }
        }

        m_animationCTRL.SetFloat("Speed", transform.InverseTransformDirection(m_RB.velocity).z);
    }

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

    //private void OnTriggerEnter(Collider collision)
    //{
    //    if (collision.gameObject.layer == 8) { StartCoroutine(Pause(5)); };
    //}

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

    public BTState IdleState() { if (m_facts["IDLE"]) { return BTState.SUCCESS; } return BTState.FAILURE; }
    public BTState HungryState() { if (m_facts["HUNGRY"]) { return BTState.SUCCESS; } return BTState.FAILURE; }
    public BTState TiredState() { if (m_facts["TIRED"]) { return BTState.SUCCESS; } return BTState.FAILURE; }
    public BTState PlayfulState() { if (m_facts["PLAYFUL"]) { return BTState.SUCCESS; } return BTState.FAILURE; }

    public BTState SWP_IdleState() { m_facts["SWP_IDLE"] = true; return BTState.SUCCESS; }
    public BTState SWP_PauseState() { m_facts["SWP_PAUSE"] = true; return BTState.SUCCESS; }
    public BTState SWP_HungryState() { m_facts["SWP_HUNGRY"] = true; return BTState.SUCCESS; }
    public BTState SWP_TiredState() { m_facts["SWP_TIRED"] = true; return BTState.SUCCESS; }
    public BTState SWP_PlayfulState() { m_facts["SWP_PLAYFUL"] = true; return BTState.SUCCESS; }

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

    ////////// Inverses of State Checks //////////
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

    public BTState GoodCareBonuses()
    {
        controller.GiveGoodCareBonus();
        m_personalityValues[DogPersonalityValue.Bond].UpdateValue(0.0005f);
        m_personalityValues[DogPersonalityValue.Affection].UpdateValue(0.00025f);
        m_personalityValues[DogPersonalityValue.Tolerance].UpdateValue(0.00005f);
        m_personalityValues[DogPersonalityValue.Obedience].UpdateValue(0.0005f * m_personalityValues[DogPersonalityValue.Intelligence].GetValue());
        return BTState.SUCCESS;
    }

    public BTState CanFindFood() { if (FindItemType(ItemType.FOOD)) { return BTState.SUCCESS; } return BTState.FAILURE; }
    public BTState CanFindBed() { if (FindItemType(ItemType.BED)) { return BTState.SUCCESS; } return BTState.FAILURE; }
    public BTState CanFindToys() { if (FindItemType(ItemType.TOYS)) { return BTState.SUCCESS; } return BTState.FAILURE; }
    public BTState CannotFindFood() { if (!FindItemType(ItemType.FOOD)) { return BTState.SUCCESS; } return BTState.FAILURE; }
    public BTState CannotFindBed() { if (!FindItemType(ItemType.BED)) { return BTState.SUCCESS; } return BTState.FAILURE; }
    public BTState CannotFindToys() { if (!FindItemType(ItemType.TOYS)) { return BTState.SUCCESS; } return BTState.FAILURE; }

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