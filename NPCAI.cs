using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.ParticleSystem;

public class NPCAI : MonoBehaviour
{

    //reference to the component navmesh agent
    private NavMeshAgent _agent;
    [SerializeField]
    private Vector3 _target;
    [SerializeField]
    private Vector3 _startingpos;
    private Vector3 _laskKnownPlayerLocation;
    private GameObject _player;
    public NPCLockerNumbers _npcLockerNumbers;

    public NPCRaycast npcRaycastScript;
    [Header("NPC Info")]

    public string firstName;

    public string lastName;
    [SerializeField]
    public string NameOfNPC;
    //States
    //NotAlert
    //Engaged
    //Searching
    private enum NPCBehaviours
    {
        NotAlert,
        Engaged,
        Searching
    }

    private enum NPCMovementState
    {
        Idle,
        Walking
    }
    private enum NPCToiletState
    {
        NotNeeded,
        Urinal,
        Toilet
    }

    [SerializeField]
    private NPCBehaviours currentState;
    [SerializeField]
    private NPCMovementState movementState;
    [SerializeField]
    private NPCToiletState doesNPCNeedToiletState;

    [SerializeField]
    private bool hasUsedToilet;
    [SerializeField]
    private bool PreWorkout;
    [SerializeField]
    private bool PostWorkout;
    [SerializeField]
    private bool HasShowered;
    [SerializeField]
    private bool HasWashedHands;
    [SerializeField]
    private bool Clean;

    [SerializeField]
    private float _engagedTime = 15.5f;
    [SerializeField]
    private float _searchTime = 15.5f;
    [SerializeField]
    private bool isSeaching = false;
    [SerializeField]
    private bool hasSearched = true;
    [SerializeField]
    private List<Transform> _waypoints;
    private int _currentWaypoint;
    [SerializeField]
    private GameObject _currentWaypointGameObject;
    private WaypointInformation _waypointInformation;
    [SerializeField]
    private float _currentWaypointInteractionTime;
    [SerializeField]
    private bool _stateChanged;
    [SerializeField]
    private bool _pauseAtEnd, _pauseAtEachWaypoint;

    public float _rotSpeed;
    float r;
    private bool isTurning;

    //WayPointVariables

    //NPC Traits
    [Header("NPC Traits")]
    //NPC will always wash hands and shower post workout.
    [SerializeField] public bool ValuesHygieneTrait;
    //NPC will sometimes not wish to shower or wash hands after using the toilet.
    [SerializeField] public bool GrossTrait;
    //NPC likes to strike up conversations with others.
    [SerializeField] public bool FriendlyTrait;
    //NPC avoids conversations with other NPC's.
    [SerializeField] public bool HostileTrait;
    //NPC won't be bothered by seeing player running around, assuming they are a bug and won't investigate.
    [SerializeField] public bool LikesBugsTrait;
    //NPC will want to kill player on sight, mistaking the player for a bug.
    [SerializeField] public bool HatesBugsTrait;
    //NPC's must be closer to be able to detect player.
    [SerializeField] public bool PoorEyesightTrait;

    [Header("Locker Room Config")]
    public GameObject[] _locker;

    public GameObject[] _benchSeats;

    public GameObject[] _urinal;

    public GameObject[] _toilet;

    public GameObject[] _sink;

    public GameObject[] _shower;

    public GameObject[] _saunaSeat;

    public GameObject myLocker;

    public GameObject myBench;

    public string[] firstNameArray = { "Jacob", "Michael", "Matthew", "Joshua", "Nicholas", "Christopher", "Andrew", "Joseph", "Shawn", "Daniel", "Tyler", "Brandon", "Ryan", "Austin", "William", "John", "David", "Zachary", "Anthony", "James", "Justin", "Alexander", "Jonathan", "Dylan", "Noah", "Christian", "Robert", "Samuel", "Kyle", "Benjamin", "Jordan", "Thomas", "Nathan", "Cameron", "Kevin", "Jose", "Hunter", "Ethan", "Aaron", "Eric", "Jason", "Caleb", "Logan", "Brian", "Adam", "Cody", "Juan", "Steven", "Connor", "Timothy", "Charles", "Isaiah", "Jack", "Gabriel", "Jared", "Luis", "Sean", "Evan", "Alex", "Elijah", "Richard", "Nathaniel", "Patrick", "Isaac", "Seth", "Trevor", "Luke", "Devin", "Mark", "Ian", "Mason", "Angel", "Bryan", "Cole", "Chase", "Dakota", "Garrett", "Adrian", "Antonio", "Jeremy", "Jesse", "Jackson", "Blake", "Dalton", "Stephen", "Tanner", "Alejandro", "Kenneth", "Lucas", "Spencer", "Bryce", "Paul", "Victor", "Brendan", "Jake", "Marcus", "Tristan", "Jeffrey", "Sebastian", "Gavin", "Julian", "Aidan", "Jeremiah", "Xavier", "Carson", "Colby", "Dominic", "Jaden", "Owen", "Hayden", "Diego", "Riley", "Jayden", "Caden", "Brayden", "Colin", "Max", "Liam", "Carter", "Landon", "Ashton", "Sam", "Parker", "Xander", "Will", "Wyatt", "Brady", "Charlie", "Cooper", "Ben", "Henry", "Drew", "Aiden", "Brody", "Miles", "Colton", "Nolan", "Preston", "Oliver", "Eli", "Grayson", "Josiah", "Levi", "Micah", "Hudson", "Ryder", "Bentley", "Asher", "Jace", "Bryson", "Leo", "Declan", "Easton", "Lincoln", "Harrison", "Jayce", "Camden", "Mateo" };
    public string[] lastNameArray = { "Smith", "Jones", "Williams", "Taylor", "Brown", "Davies", "Evans", "Wilson", "Johnson", "Roberts", "Robinson", "Thompson", " Wright", "Walker", "White", "Edwards", "Hughes", "Green", "Hall", "Lewis", "Harris", "Clarke", "Patel", "Jackson", "Wood", "Turner", "Martin", "Cooper", "Hill", "Morris", "Ward", "Moore", "Clark", "Baker", "Harrison", "King", "Morgan", "Lee", "Allen", "James", "Phillips", "Scott", "Watson", "Davis", "Parker", "Bennett", "Price", "Griffiths", "Young", "Khan", "Mitchell", "Cook", "Bailey", "Carter", "Richardson", "Shaw", "Kelly", "Collins", "Bell", "Hussain", "Richards", "Cox", "Miller", "Begum", "Murphy", "Marshall", "Simpson", "Anderson", "Ellis", "Adams", "Wilkinson", "Ahmed", "Foster", "Powell", "Chapman", "Singh", "Webb", "Rogers", "Mason", "Gray", "Hunt", "Palmer", "Holmes", "Mills", "Campbell", "Barnes", "Knight", "Butler", "Russell", "Barker", "Stevens", "Jenkins", "Dixon", "Fisher", "Harvey", "Pearson", "Graham", "Fletcher", "Howard", "Gibson", "Walsh", "Reynolds", "Saunders", "Ford", "Stewart", "Payne", "Fox", "Pearce", "Day", "Brooks", "Lawrence", "West", "Kaur", "Atkinson", "Gill", "Spencer", "Ball", "Dawson", "Burton", "Watts", "Rose", "Booth", "Perry", "Wells", "Armstrong", "O'Brien", "Francis", "Rees", "Grant", "Hart", "Hudson", "Hayes", "Newman", "Ryan", "Webster", "Barrett", "Gregory", "Hunter", "Marsh", "Lowe", "Carr", "Riley", "Page", "Shah", "Woods", "Dunn", "Stone", "Parsons", "Hawkins", "Harding", "Holland", "Porter", "Newton", "Reed", "Bird", "Reid" };

    private GameObject randomBench;
    private int LockerNumber;
    private int chanceNumber01;
    private int chanceNumber02;
    private int chanceNumber03;

    public GameObject doorEntryPoint;
    public GameObject doorExitPoint;
    public GameObject despawnPoint;

    // Start is called before the first frame update
    void Start()
    {
        npcStateSetup();
        TraitRangeGenerator();
        ToiletNeedGenerator();

        HasWashedHands = true;
        hasUsedToilet = false;
        isTurning = false;

        if (PreWorkout == true)
        {
            HasShowered = true;
        }

        firstName = firstNameArray[UnityEngine.Random.Range(0, firstNameArray.Length)];
        lastName = lastNameArray[UnityEngine.Random.Range(0, firstNameArray.Length)];


        Debug.Log("Generated Name: " + firstName + " " + lastName);

        NameOfNPC = firstName + " " + lastName;

        //This gives the NPC his own unique routine after the above setup
        npcRoutineSelector();

            _agent = GetComponent<NavMeshAgent>();
        if (_agent == null) 
        {
            Debug.LogError("Nav Mesh Agent is Null");
        }
        currentState = NPCBehaviours.NotAlert;

        _player = GameObject.FindGameObjectWithTag("Player");
        if (_player == null)
        {
            Debug.LogError("Player is Null");
        }

        _target = _startingpos;
        _stateChanged = true;
        movementState = NPCMovementState.Walking;
    }

// Update is called once per frame
    void Update()
    {
        //set destination to the target
        //_agent.SetDestination(_target.transform.position);
        switch (currentState)
        {
            case NPCBehaviours.NotAlert:
                if (_stateChanged == true) 
                {
                    StartCoroutine(MoveRoutine());
                    _stateChanged = false;
                }
                break;
            case NPCBehaviours.Engaged:
                _target = _player.transform.position;
                _laskKnownPlayerLocation = _player.transform.position;
                _agent.SetDestination(_target);
                //Debug.Log("NPC found the Player");
                break;
            case NPCBehaviours.Searching:
                _target = _laskKnownPlayerLocation;
                _agent.SetDestination(_target);
                //Debug.Log("NPC is looking for Player");
                break;
        }
        if (npcRaycastScript.canSeePlayer == true)
        {
            //Debug.Log("I can see the player");
            currentState = NPCBehaviours.Engaged;
            hasSearched = false;
        }
        else if(npcRaycastScript.canSeePlayer == false & isSeaching == false)
        {
            if (hasSearched == false)
            {
                StartCoroutine(EngagedCooldownRoutine());
            }
        }
        if (isTurning == true)
        {
            float targetAngle = _waypoints[_currentWaypoint].transform.rotation.eulerAngles.y;
            Quaternion currentRot = transform.rotation;
            Quaternion targetRotaion = _waypoints[_currentWaypoint].transform.rotation;
            if (Quaternion.Angle(transform.rotation, targetRotaion) <= 0.01f)
                {
                isTurning = false;
            }
            else
            {
                Debug.Log("Angle of " + _waypoints[_currentWaypoint] + " is " + targetAngle);
                float Angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref r, _rotSpeed);
                transform.rotation = Quaternion.Euler(0, Angle, 0);
            }
        }
    }

    private IEnumerator MoveRoutine()
    {
        //while currentState == non alert state
        while(currentState == NPCBehaviours.NotAlert && movementState == NPCMovementState.Walking)
        {
            yield return null;
            if (_waypoints.Count > 1 && _waypoints[_currentWaypoint] != null)
            {
                _agent.SetDestination(_waypoints[_currentWaypoint].position);
                float distance = Vector3.Distance(transform.position , _waypoints[_currentWaypoint].position);
                if(distance < 2.51f)
                {
                    if (_currentWaypoint == _waypoints.Count -1)
                    {
                        _currentWaypoint = 0;
                        if (_pauseAtEnd)
                        {
                            //_agent.updateRotation = Quaternion.Euler( 1.0f, 1.0f, 1.0f);
                            _waypointInformation = _waypoints[_currentWaypoint].GetComponent<WaypointInformation>();
                            yield return new WaitForSeconds(_waypointInformation._interactionTimeLength);
                        }
                    }
                    else
                    {
                        if (_pauseAtEachWaypoint)
                        {
                            if (movementState == NPCMovementState.Walking)
                            {
                                movementState = NPCMovementState.Idle;
                                if (movementState == NPCMovementState.Idle)
                                {
                                    _waypointInformation = _waypoints[_currentWaypoint].GetComponent<WaypointInformation>();
                                    isTurning = true;
                                    yield return new WaitForSeconds(_waypointInformation._interactionTimeLength);
                                }
                            }
                            //_currentWaypointGameObject = _waypoints[_currentWaypoint].gameObject;
                            //yield return new WaitForSeconds(1f);
                            //_waypointInformation = _waypoints[_currentWaypoint].GetComponent<WaypointInformation>();
                            //yield return new WaitForSeconds(_waypointInformation._interactionTimeLength);
                        }
                        movementState = NPCMovementState.Walking;
                        isTurning = false;
                        _currentWaypoint++;
                    }
                }
            }
        }
    }

    private void TraitRangeGenerator()
    {
        int TraitRange01 = (UnityEngine.Random.Range(1, 10));
        //Debug.Log(TraitRange01 + " is Trait Range 1's Number");
        int TraitRange02 = (UnityEngine.Random.Range(1, 10));
        //Debug.Log(TraitRange02 + " is Trait Range 2's Number");
        int TraitRange03 = (UnityEngine.Random.Range(1, 10));
        //Debug.Log(TraitRange03 + " is Trait Range 3's Number");
        int TraitRange04 = (UnityEngine.Random.Range(1, 10));
        //Debug.Log(TraitRange04 + " is Trait Range 4's Number");

        if (TraitRange01 <= 3)
        {
            ValuesHygieneTrait = true;
        }
        else if (TraitRange01 >= 8) 
        {
            GrossTrait = true;
        }

        if (TraitRange02 <= 3)
        {
            FriendlyTrait = true;
        }
        else if (TraitRange02 >= 8)
        {
            HostileTrait = true;
        }

        if (TraitRange03 <= 3)
        {
            LikesBugsTrait = true;
        }
        else if (TraitRange03 >= 8)
        {
            HatesBugsTrait = true;
        }

        if (TraitRange04 <= 3)
        {
            PoorEyesightTrait = true;
        }

    }

    private void npcStateSetup()
    {
        int hasNPCWorkedOutYetInt = (UnityEngine.Random.Range(1, 10));

        if (hasNPCWorkedOutYetInt <= 5)
        {
        PreWorkout = true;
        }
        else
        {
        PostWorkout = true;
        }
    }
    private void ToiletNeedGenerator()
    {
        if (hasUsedToilet == false)
        {
            int toiletNumGenerator = (UnityEngine.Random.Range(0, 9));

            if (toiletNumGenerator <= 2)
            {
                doesNPCNeedToiletState = NPCToiletState.Toilet;
            }
            else if (toiletNumGenerator >= 8)
            {
                doesNPCNeedToiletState = NPCToiletState.Urinal;
            }
            else
            {
                doesNPCNeedToiletState = NPCToiletState.NotNeeded;
            }
        }
    }
    private IEnumerator EngagedCooldownRoutine()
    {
        yield return new WaitForSeconds(_engagedTime);
        if (npcRaycastScript.canSeePlayer == false)
        {
            currentState = NPCBehaviours.Searching;
            isSeaching = true;
            StartCoroutine(SearchingCooldownRoutine());
        }
    }

    private IEnumerator SearchingCooldownRoutine()
    {
        yield return new WaitForSeconds(_searchTime);
        if (npcRaycastScript.canSeePlayer == false && currentState == NPCBehaviours.Searching)
        {
            currentState = NPCBehaviours.NotAlert;
            isSeaching = false;
            hasSearched = true;
            _stateChanged = true;
        }
    }

    public void npcRoutineSelector()
    {
        _waypoints.Add(doorEntryPoint.transform);

        _npcLockerNumbers.GetComponent<NPCLockerNumbers>();
        LockerNumber = UnityEngine.Random.Range(0, _locker.Length);
        if (_npcLockerNumbers.LockersInUse[LockerNumber] == true)
        {
            
        }
        GameObject randomLocker = _locker[LockerNumber];
        int displayLockerNumber = LockerNumber + 1;

        Debug.Log(NameOfNPC + " is now using locker " + displayLockerNumber);

        if (doesNPCNeedToiletState == NPCToiletState.Urinal)
        {
            int UrinalNumber = UnityEngine.Random.Range(0, _urinal.Length);
            GameObject randomUrinal = _urinal[UrinalNumber];
            int displayUrinalNumber = UrinalNumber + 1;

            Debug.Log(NameOfNPC + " will use urinal " + displayUrinalNumber);
            _waypoints.Add(randomUrinal.transform);
            SinkSelector();
        }
        else if (doesNPCNeedToiletState == NPCToiletState.Toilet)
        {
            int ToiletNumber = UnityEngine.Random.Range(0, _toilet.Length);
            GameObject randomToilet = _toilet[ToiletNumber];
            int displayToiletNumber = ToiletNumber + 1;

            Debug.Log(NameOfNPC + " will use toilet " + displayToiletNumber);
            _waypoints.Add(randomToilet.transform);
            SinkSelector();
        }
        else if (doesNPCNeedToiletState == NPCToiletState.NotNeeded)
        {
            UnityEngine.Debug.Log(NameOfNPC + " does not need to use the toilet or urinal");
        }
        myLocker = randomLocker;

        randomChanceSelect02();

        _waypoints.Add(randomLocker.transform);

        benchSeatSelector();



        if (PreWorkout == true)
        {
            PostWorkout = false;
            HasShowered = true;

            _waypoints.Add(randomLocker.transform);
        }
        else if (PostWorkout == true) 
        { 
            PreWorkout = false;
            HasShowered = false;

            postWorkoutOrderSelector();
        }

        _waypoints.Add(doorExitPoint.transform);
        _waypoints.Add(despawnPoint.transform);
    }

    private void SinkSelector()
    {
        if (ValuesHygieneTrait == true)
        {
            chanceNumber01 = UnityEngine.Random.Range(0, 4);
        }

        if (chanceNumber01 >= 4 & GrossTrait == true)
        {
            int SinkNumber = UnityEngine.Random.Range(0, _sink.Length);
            GameObject randomSink = _sink[SinkNumber];
            int displaySinkNumber = SinkNumber + 1;

            Debug.Log(NameOfNPC + " will use sink " + displaySinkNumber);

            _waypoints.Add(randomSink.transform);

        }
        else if (GrossTrait == false & ValuesHygieneTrait == false & chanceNumber01 >=2)
        {
            int SinkNumber = UnityEngine.Random.Range(0, _sink.Length);
            GameObject randomSink = _sink[SinkNumber];
            int displaySinkNumber = SinkNumber + 1;

            Debug.Log(NameOfNPC + " will use sink " + displaySinkNumber);

            _waypoints.Add(randomSink.transform);
        }
        else
        {
            int SinkNumber = UnityEngine.Random.Range(0, _sink.Length);
            GameObject randomSink = _sink[SinkNumber];
            int displaySinkNumber = SinkNumber + 1;

            Debug.Log(NameOfNPC + " will use sink " + displaySinkNumber);

            _waypoints.Add(randomSink.transform);
        }

    }
    public void benchSeatSelector()
    {

        if (myLocker == _locker[0])
        {
            _waypoints.Add(_benchSeats[0].transform);
        }
        else if (myLocker == _locker[1])
        {
            _waypoints.Add(_benchSeats[1].transform);
        }
        else if (myLocker == _locker[2])
        {
            _waypoints.Add(_benchSeats[2].transform);
        }
        else if (myLocker == _locker[3])
        {
            _waypoints.Add(_benchSeats[3].transform);

        }
        else
        {
            Debug.LogError("Unable To Assign _randomBench");
        }
    }
    public void randomChanceSelect02()
    {
        chanceNumber02 = UnityEngine.Random.Range(1, 3);
        if (chanceNumber02 >= 2)
        {
            benchSeatSelector();
        }
    }
    public void postWorkoutOrderSelector()
    {
        chanceNumber03 = UnityEngine.Random.Range(1, 5);
        if (chanceNumber03 == 1)
        {
            _waypoints.Add(myLocker.transform);
            SaunaIsUsed();
            ShowerIsUsed();
            _waypoints.Add(myLocker.transform);

        }
        else if (chanceNumber03 == 2)
        {
            _waypoints.Add(myLocker.transform);
            SaunaIsUsed();
            _waypoints.Add(myLocker.transform);
        }
        else if (chanceNumber03 == 3)
        {
            _waypoints.Add(myLocker.transform);
            ShowerIsUsed();
            SaunaIsUsed();
            _waypoints.Add(myLocker.transform);
        }
        else if (chanceNumber03 == 4)
        {
            _waypoints.Add(myLocker.transform);
            ShowerIsUsed();
            _waypoints.Add(myLocker.transform);
        }
        else if (chanceNumber03 == 5)
        {
            _waypoints.Add(myLocker.transform);
        }
    }
    private void SaunaIsUsed()
    {
        int SaunaSeatNumber = UnityEngine.Random.Range(0, _saunaSeat.Length);
        GameObject randomSaunaSeat = _saunaSeat[SaunaSeatNumber];
        int displaySaunaSeatNumber = SaunaSeatNumber + 1;

        _waypoints.Add(randomSaunaSeat.transform);
    }
    private void ShowerIsUsed()
    {
        int ShowerNumber = UnityEngine.Random.Range(0, _shower.Length);
        GameObject randomShower = _shower[ShowerNumber];
        int displayShowerNumber = ShowerNumber + 1;

        _waypoints.Add(randomShower.transform);
    }

    private void FaceTarget()
    {
        //_agent.updateRotation = 
    }
}
