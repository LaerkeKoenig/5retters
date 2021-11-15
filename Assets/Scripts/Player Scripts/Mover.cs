using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Mover : MonoBehaviour
{
    // Bevaegelse Variable

    [SerializeField]
    public float speed = 5;

    CharacterController controller;
    Vector3 direction = Vector3.zero;
    Vector2 inputVector = Vector2.zero;
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;
    
    [SerializeField]
    private int  playerIndex = 0;
    PlayerInputHandler playerInputHandler;
    public bool Interact = false;
    
    // Interger Variabler 
    public List<GameObject> iRaekkevide;    // Objekter indenfor raekkevidde af spilleren    
    public GameObject interaktionsobjekt;   // Det objekt man forsøger at interger med
    public bool holderObjekt;               // Bool til at tjekke om man holdet et objekt eller ej
    public Transform PickUpHolder;          // Placeringen af objektet på spilleren
    public GameObject objekthold;           // Objektet man holder

    // Kaste med ting
    Rigidbody m_Rigidbody;
    public float kraft;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        m_Rigidbody = GetComponent<Rigidbody>();
        playerInputHandler = GetComponent<PlayerInputHandler>();
    }

    public void SetInputVector(Vector2 direction)
    {
       
        inputVector = direction;
       
    }  

    public int GetPlayerIndex()
    {
        return playerIndex;
    }

    public void Update()
    {
        direction = new Vector3(inputVector.x, 0, inputVector.y) * speed;
        controller.SimpleMove(direction);

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);  
        }
        
        if (Interact == true)
        {
           Invoke("OnPickUp",0);
           Debug.Log(Interact);
        }
        else
        {
            Debug.Log(Interact);
        }
    }
    //Interger med objekt
    private void OnPickUp()
    {
        if(iRaekkevide.Count > 0)
        {
            StartCoroutine("FindTaettestObjekt");
        
            //Interger med Station
            if (interaktionsobjekt.tag.Contains("Station"))
            {
                if ((holderObjekt == true && !interaktionsobjekt.tag.Contains("Ingredient")) || (holderObjekt == false))
                {
                    Debug.Log(this.gameObject + " har aktivet" + interaktionsobjekt);
                    interaktionsobjekt.GetComponent<MasterStation>().spillerref = this;
                    interaktionsobjekt.GetComponent<MasterStation>().Activate();
                }
                else
                    Debug.Log("Kan ikke aktiver " + interaktionsobjekt);
            }
            //Interger med Pick Up
            else if (interaktionsobjekt.tag.Contains("PickUp"))
            {
                if (holderObjekt == true)
                    Smid();
                else
                    objekthold = interaktionsobjekt;
                    SamlOp();                     
            }
        }
        else if (objekthold != null)
            Smid();
        interaktionsobjekt = null;      
    }

    // Saml objekt op - attach object to player and removes gravity and collission
    public void SamlOp()
    {
       // Debug.Log(interaktionsobjekt + " samlet op");
        holderObjekt = true;
        objekthold.transform.position = PickUpHolder.position;
        objekthold.transform.parent = PickUpHolder;
        objekthold.GetComponent<MeshCollider>().enabled = false;        
        objekthold.GetComponent<Rigidbody>().useGravity = false;
        objekthold.GetComponent<Rigidbody>().isKinematic = true;
        iRaekkevide.Remove(objekthold);
    }

    // Smid objekt i hånden
    public void Smid()
    {
       // Debug.Log(objekthold + " smidt");
        holderObjekt = false;
        objekthold.transform.parent = null;        
        objekthold.GetComponent<MeshCollider>().enabled = true;
        objekthold.GetComponent<Rigidbody>().useGravity = true;
        objekthold.GetComponent<Rigidbody>().isKinematic = false;
        objekthold.GetComponent<Rigidbody>().AddForce(transform.up * kraft);
        objekthold.GetComponent<Rigidbody>().AddForce(transform.forward * kraft);
        iRaekkevide.Add(objekthold);
        objekthold = null;
      
    }
    
    // Boevling (har ingen funktion lige nu)
    void OnBoevle()
    {
        print("Boevler");
    }

    //Objekt kommer inden for raekkevidde
    private void OnTriggerEnter(Collider other)
    {        
        if(!iRaekkevide.Contains(other.gameObject) && (other.gameObject.tag.Contains("Station") || (other.gameObject.tag.Contains("PickUp"))))
            iRaekkevide.Add(other.gameObject);   
    }

    //Objekt forlader raekkevidde
    private void OnTriggerExit(Collider other)
    {
        if (iRaekkevide.Contains(other.gameObject))
            iRaekkevide.Remove(other.gameObject);   
    }

    //Find taetteste objekt
    IEnumerator FindTaettestObjekt()
    {
            // Find taetteste objekt fra listen af objekter i raekkevidde
            float kortestafstand = Mathf.Infinity;
            foreach (GameObject ting in iRaekkevide)
            {
                //Debug line
                Debug.DrawLine(ting.transform.position, this.transform.position, Color.red, 3);

                //Find afstand mellem objekt og spiller
                float afstand = Vector3.Distance(ting.transform.position, this.transform.position);
                           
                //Hvis afstanden er kortere end den korteste afstand saa saet den korteste afstand til den nye afstand
                if (afstand <= kortestafstand)
                {
                    kortestafstand = afstand;
                    interaktionsobjekt = ting;                    
                }
            }
            yield return interaktionsobjekt;
    }
}