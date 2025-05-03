
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpenCagesHandler : MonoBehaviour
{
    private GameObject canvas;
    private TextMeshProUGUI timer_text, crab_text, NPCName, dialogueText, victoryText;
    private Transform MazePrompt;
    private Image crab_icon, key_icon;
    private Button confirmMazeButton, cancelMazeButton;

    private float timeRemaining;
    private float seconds;

    private bool hasKey;
    private int openCages;
    private int totCages;
    private int finalFunctionCalled;

    private GameObject[] arr_cages;

    private AudioManager audioManager;

    void Awake()
    {
        this.totCages = this.GetComponent<SpawnCages>().totalCages;  //prendo il numero di casse
        this.canvas = GameObject.Find("Canvas");

        this.timer_text = canvas.transform.Find("MazeContainer/TimerText").gameObject.GetComponent<TextMeshProUGUI>();
        this.crab_icon = canvas.transform.Find("MazeContainer/CrabIcon").gameObject.GetComponent<Image>();
        this.key_icon = canvas.transform.Find("MazeContainer/KeyIcon").gameObject.GetComponent<Image>();
        this.crab_text = canvas.transform.Find("MazeContainer/FreedCrab").gameObject.GetComponent<TextMeshProUGUI>();

        //Elementi finestra di dialogo
        this.MazePrompt = canvas.transform.Find("MazeContainer/MazePrompt");
        MazePrompt.gameObject.SetActive(false);
        this.NPCName = canvas.transform.Find("DialoguePanel/TitlePanel/NPCName").gameObject.GetComponent<TextMeshProUGUI>();
        this.dialogueText = canvas.transform.Find("DialoguePanel/DialogueText").gameObject.GetComponent<TextMeshProUGUI>();
        canvas.transform.Find("DialoguePanel").gameObject.SetActive(false);

        //Bottoni 
        confirmMazeButton = canvas.transform.Find("DialoguePanel/ConfirmMazeButton").gameObject.GetComponent<Button>();
        cancelMazeButton = canvas.transform.Find("DialoguePanel/CancelMazeButton").gameObject.GetComponent<Button>();

        timer_text.enabled = false;
        crab_icon.enabled = false;
        key_icon.enabled = false;
        crab_text.enabled = false;

        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    //Funzione START MazeExploring
    public void restartMazeGame()
    {
        this.finalFunctionCalled = 0;
        this.timeRemaining = 180f;
        this.seconds = Mathf.Round(timeRemaining);

        this.hasKey = false;
        this.openCages = 0;

        timer_text.enabled = true;
        timer_text.SetText(seconds.ToString());

        crab_icon.enabled = true;
        key_icon.enabled = false;

        crab_text.enabled = true;
        crab_text.SetText(openCages.ToString() + "/" + totCages.ToString());

        this.GetComponent<SpawnCages>().restartGame();
    }

    //chiamato quando Shelly si avvicina al pesce
    public void mazeStartPrompt()
    {
        if (GameDirector.Instance.getGameState() != GameDirector.GameState.FreeRoaming)
            return;

        if (!canvas.transform.Find("DialoguePanel").gameObject.activeSelf)
        {
            MazePrompt.gameObject.SetActive(true);
        }

        if (Input.GetKey(KeyCode.E))
        {
            canvas.transform.Find("BarsPanel").gameObject.SetActive(false);
            MazePrompt.gameObject.SetActive(false);
            canvas.transform.Find("DialoguePanel").gameObject.SetActive(true);
            GameDirector.Instance.checkDialoguePanelButtons("MazeExploring");
            NPCName.SetText("Dory");
            dialogueText.SetText("Hey Shellie! Ci sono dei granchi che hanno bisogno di essere liberati! \n" +
                "Ti va di aiutarmi?" + " Nel labirinto troverai delle chiavi con cui poter aprire le gabbie \n" +
                "Attenta! Puoi prendere solo una chiave alla volta ed hai 3 minuti di tempo per liberarli tutti \n");
        }
    }

    public void PesceTriggerExit()
    {
        if (canvas.transform.Find("DialoguePanel").gameObject.activeSelf)
            canvas.transform.Find("DialoguePanel").gameObject.SetActive(false);

        if (MazePrompt.gameObject.activeSelf)
            MazePrompt.gameObject.SetActive(false);

        if (!canvas.transform.Find("BarsPanel").gameObject.activeSelf)
            canvas.transform.Find("BarsPanel").gameObject.SetActive(true);
    }

    //Listener bottoni di dialogo
    public void ConfirmMazeButton_onClick()
    {
        canvas.transform.Find("DialoguePanel").gameObject.SetActive(false);
        canvas.transform.Find("BarsPanel").gameObject.SetActive(true);

        GameDirector.Instance.setGameState(GameDirector.GameState.MazeExploring);

        MazePrompt.gameObject.SetActive(false);
        audioManager.PlaySFX(audioManager.selection);
        audioManager.ChangeMusic(audioManager.SaraGameSountrack, true, 0.3f);
        restartMazeGame();
    }
    public void CancelMazeButton_onClick()
    {
        PesceTriggerExit();
        MazePrompt.gameObject.SetActive(true);
        audioManager.PlaySFX(audioManager.selection);
    }

    private void FixedUpdate()
    {
        if (GameDirector.Instance.getGameState() != GameDirector.GameState.MazeExploring)
            return;

        if (this.timeRemaining > 0)
        {
            this.timeRemaining -= Time.deltaTime;
            this.seconds = Mathf.Round(timeRemaining);
            this.timer_text.SetText(seconds.ToString());
        }
        else
        {
            if (finalFunctionCalled == 0)
            {
                callFinalFunction();
            }
            finalFunctionCalled++;
        }
    }
    public void callFinalFunction()
    {
        checkEnd();
        GameDirector.Instance.setGameState(GameDirector.GameState.FreeRoaming);
        Debug.Log("FINE");

        this.timer_text.enabled = false;
        this.crab_text.enabled = false;
        this.crab_icon.enabled = false;
        this.key_icon.enabled = false;
        this.hasKey = false;

        //faccio scomparire le chiavi
        GameObject[] arr_keys = GameObject.FindGameObjectsWithTag("Chiave");
        for (int i = 0; i < arr_keys.Length; i++)
        {
            Destroy(arr_keys[i]);
        }

        //riempie array di gabbie
        this.arr_cages = GameObject.FindGameObjectsWithTag("Gabbia");
        Debug.Log("dim array: " + arr_cages.Length);

        if (arr_cages.Length != 0)
        {
            for (int i = 0; i < totCages; i++)
            {
                if (arr_cages[i] != null)
                {
                    arr_cages[i].GetComponent<CageScript>().GoUp();
                }

            }
            Debug.Log("arr_cages.Length: " + arr_cages.Length);
        }
        else
            GameDirector.Instance.setGameState(GameDirector.GameState.FreeRoaming);

    }

    private void checkEnd()
    {
        int gane = 8 * openCages;

        canvas.transform.Find("VictoryPanel").gameObject.SetActive(true);
        victoryText = canvas.transform.Find("VictoryPanel/RewardsPanel/RewardsText").GetComponent<TextMeshProUGUI>();
        audioManager.PlaySFX(audioManager.endMiniGame);
        audioManager.ChangeMusic(audioManager.SaraGameSountrack, false,0.3f);
        if (openCages == 0)
        {
            victoryText.SetText("Purtroppo non sei riuscita a liberare nesssun granchio... \n" +
                                "Non hai guadagnato nessuna perla...\n" +
                                "Il livello di biodiversità NON � aumentato...");
        }
        else if (openCages == totCages)
        {
            victoryText.SetText("Complimenti! Sei riuscita a salvare tutti i granchi! \n" +
                                "Perle guadagnate: " + gane + "\n" +
                                 "Livello di biodiversità aumentato del " + gane + "%");
        }
        else
        {
            victoryText.SetText("Perle guadagnate: " + gane + "\n" +
                            "Livello di biodiversità aumentato del " + gane + "%");
        }
        GameDirector.Instance.addPearls(gane);
        GameDirector.Instance.addBiodiversity(gane);
    }

    //metodo chiamato da TurtleController
    public void TriggerMethod(Collider other)
    {
        if (other.CompareTag("Chiave"))
        {
            if (Input.GetKey(KeyCode.E))
            {
                if (!hasKey)
                {
                    Destroy(other.gameObject);
                    this.hasKey = true;
                    this.key_icon.enabled = true;
                    Debug.Log("ho la chiave");
                    audioManager.PlaySFX(audioManager.KeyTaken);
                }
                else
                    return;
            }
        }

        if (other.CompareTag("Gabbia"))
        {
            if (Input.GetKey(KeyCode.E))
            {
                if (hasKey && other.GetComponent<CageScript>().isLocked)
                {
                    Debug.Log("apro gabbia");
                    other.GetComponent<CageScript>().OpenCage();
                    this.hasKey = false;
                    this.key_icon.enabled = false;
                    this.openCages++;
                    Debug.Log("GABBIE APERTE: " + this.openCages);
                    this.crab_text.SetText(openCages.ToString() + "/" + totCages.ToString());
                    audioManager.PlaySFX(audioManager.CageOpening);
                    if(openCages == totCages && finalFunctionCalled == 0)
                    {
                        callFinalFunction();
                        finalFunctionCalled++;
                    }
                }
                else
                    return;

            }
        }
    }

}