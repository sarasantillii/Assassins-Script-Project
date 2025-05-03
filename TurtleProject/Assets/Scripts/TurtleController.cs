using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurtleController : MonoBehaviour
{
    [SerializeField] private float acceleration;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float maxRotationSpeed;
    private Animator turtleanim;
    private GameObject oggettoscriptTrash;

    private Rigidbody rb;
    private float speed, verticalRotationSpeed, lateralRotationSpeed;
    private Vector3 eulerRotationSpeed;
    private float h, v, j;

    private GameObject posizioni_cageKey;
    private AudioManager audioManager;

    // Start is called before the first frame update
    void Awake()
    {
        this.rb = GetComponent<Rigidbody>();
        this.eulerRotationSpeed = new Vector3(0, this.maxRotationSpeed, 0);
        this.turtleanim = GetComponentInChildren<Animator>();

        this.posizioni_cageKey = GameObject.Find("Map/Posizioni_CageKey");
        posizioni_cageKey.SetActive(true);
        GameDirector.Instance.LoadGame();
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }


    private void FixedUpdate()      //In questa funzione vanno i calcoli delle velocità, che verranno passati ad Update
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
        j = Input.GetAxis("Jump");

        // -------------------------------------------------------------------- //
        //calcolo della velocità frontale
        if (speed >= 0 && speed <= maxSpeed)
            speed += acceleration * v * Time.deltaTime;
        if (v == 0 && speed >= 0) //se non è premuta W, la tartaruga rallenta
            speed -= acceleration * Time.deltaTime * 0.5f;
        if (speed < 0)
            speed = 0;
        if (speed > maxSpeed)
            speed = maxSpeed;

        // -------------------------------------------------------------------- //
        //animazione movimento tartaruga
        turtleanim.SetFloat("speed", speed);

        // -------------------------------------------------------------------- //
        //calcolo della rotazione laterale
        lateralRotationSpeed = 0;
        if (Mathf.Abs(lateralRotationSpeed) <= maxRotationSpeed && h != 0)
        {
            lateralRotationSpeed += acceleration * h * Time.deltaTime;
            //TODO: forse è meglio mettere un'accelerazione di rotazione separata
        }

        //Se non sono premuti tasti laterali, la rotazione rallenta fino a tornare dritti
        else if (h == 0 && lateralRotationSpeed != 0)
        {
            if (lateralRotationSpeed > 0)
                lateralRotationSpeed -= acceleration * Time.deltaTime * 0.2f;
            else
                lateralRotationSpeed += acceleration * Time.deltaTime * 0.2f * (maxSpeed / speed);
        }

        //Fattore di rotazione relativo alla velocità (più la tartaruga è lenta, più può ruotare veloce
        float lateralRotationFactor = 7f - (speed / (maxSpeed)) * 3f;
        // -------------------------------------------------------------------- //
        //calcolo della rotazione verticale
        verticalRotationSpeed = 0;

        //Limita l'input del giocatore una volta raggiunta l'altezza massima
        if (rb.position.y >= 40 && j>0)
        {
            j = 0;
        }
        if (Mathf.Abs(verticalRotationSpeed) <= maxRotationSpeed && j != 0)
        {
            verticalRotationSpeed += acceleration * Time.deltaTime * j; //TODO: forse è meglio mettere un'accelerazione di rotazione separata
        }


        //Se non sono premuti tasti verticali, la rotazione rallenta fino a tornare dritti
        else if (j == 0 && verticalRotationSpeed != 0)
        {
            if (verticalRotationSpeed > 0)
                verticalRotationSpeed -= acceleration * Time.deltaTime * 0.2f;
            else
                verticalRotationSpeed += acceleration * Time.deltaTime * 0.2f;
        }

        // -------------------------------------------------------------------- //
        //rotazione laterale
        this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y + lateralRotationSpeed * lateralRotationFactor, -lateralRotationSpeed * 30 * lateralRotationFactor);

        //rotazione verticale
        this.transform.eulerAngles = new Vector3(-verticalRotationSpeed * 100, this.transform.eulerAngles.y, this.transform.eulerAngles.z);

        //movimento frontale
        if (speed > 0)
            this.rb.MovePosition(this.rb.position + this.transform.forward * (speed * Time.deltaTime));
    }
    /*
    public void Update()        //In questo metodo vanno le funzioni dedicate allo spostamento
    {
        
    }*/


    // -------------------------------------------------------------------- //
    //Metodo che invia il nome del target attraversato alla funzione principale, nel minigioco di corsa ad ostacoli
    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Target")
        {
            other.GetComponentInParent<TargetHandler>().TargetCollision(other.name);
        }
        /*if (other.CompareTag("MazeExploring"))
        {
            //TODO: richiama metodo per far partire gioco libera granchi 
            Debug.Log("Entrata MazeExploring");
            //posizioni_cageKey.SetActive(true);
            //posizioni_cageKey.GetComponent<OpenCagesHandler>().restartMazeGame();
            //GameObject.Find("Posizioni_CageKey").GetComponent<OpenCagesHandler>().restartMazeGame();  //metodo che inizializza tutto e fa partire il gioco
        }*/
    }
    public void OnTriggerStay(Collider other)
    {
        if (other.name == "Anguilla_collider")
        {
            other.GetComponentInParent<TargetHandler>().raceStartPrompt();
        }
        else if (other.name == "PesceRosso_collider")
        {
            // other.GetComponentInChildren<SpawnPrefabsRandomly>().raceStartPrompt1();
            oggettoscriptTrash = GameObject.Find("ContenitoreStefano/oggettoscriptTrash");
            oggettoscriptTrash.GetComponent<SpawnPrefabsRandomly>().trashStartPrompt();
        }
        else if (other.name == "SpecialTarget")
        {
            other.GetComponentInParent<TargetHandler>().summonSpecialTarget();
        }
        else if (other.name == "pesceColorato")
        {
            posizioni_cageKey.GetComponent<OpenCagesHandler>().mazeStartPrompt();
        }
        else if (other.tag == "Chiave" || other.tag == "Gabbia")
        {
            posizioni_cageKey.GetComponent<OpenCagesHandler>().TriggerMethod(other);
        }

    }
    public void OnTriggerExit(Collider other)
    {
        if (other.name == "Anguilla_collider")
        {
            other.GetComponentInParent<TargetHandler>().AnguillaTriggerExit();
        }
        else if (other.name == "PesceRosso_collider")
        {
            oggettoscriptTrash = GameObject.Find("ContenitoreStefano/oggettoscriptTrash");
            oggettoscriptTrash.GetComponent<SpawnPrefabsRandomly>().PesceRossoTriggerExit();
            //other.GetComponentInParent<SpawnPrefabsRandomly>().PesceRossoTriggerExit();
        }
        else if (other.name == "pesceColorato")
        {
            posizioni_cageKey.GetComponent<OpenCagesHandler>().PesceTriggerExit();
        }
        else if (other.name == "SpecialTarget")
        {
            GameObject.Find("Canvas/SpecialTargetPrompt").gameObject.SetActive(false);
        }

    }
    // -------------------------------------------------------------------- //

    //Funzione per rallentare in caso di collisione, principalmente per prevenire movimenti fuori controllo e passaggi attraverso il terreno
    public void OnCollisionStay(Collision collision)
    {
        if (speed >= maxSpeed / 6)
            speed = maxSpeed / 6;


        if (collision.gameObject.CompareTag("spazzatura"))
        {
            float intensità = 0.5f;
            Vector3 force = collision.transform.position - transform.position;
            force.Normalize();
            Debug.Log("spinta");
            audioManager.PlayTrash();

            collision.gameObject.GetComponent<Rigidbody>().AddForce(force * intensità, ForceMode.Force);
        }
    }


    //Funzione per eliminare la velocità generata da collisioni
    public void OnCollisionExit(Collision collision)
    {
        StartCoroutine(stopForces()); //Questa coroutine aspetta un secondo dopo che si esce da una collisione, e poi resetta le velocità causate dalla spinta

    }
    IEnumerator stopForces()
    {
        yield return new WaitForSeconds(1);
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        //TODO: con Vector3.zero la velocità si taglia di colpo. Vedi se riesci a trovare un modo di rallentare in maniera graduale.
        //Intanto, per ora funziona.

    }

    // -------------------------------------------------------------------- //
}

//12.35 Adattato turtle controller per Sara
//13.41 Adattato turtle controller per Stefano
//17.47 Aggiunta animazione