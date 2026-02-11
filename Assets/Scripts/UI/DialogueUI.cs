using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DialogueUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("NPC이름 UI")]
    [SerializeField] private TextMeshProUGUI npcNameText;

    [Header("채팅 UI")]
    [SerializeField] private TextMeshProUGUI logText;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button sendButton;

    [Header("대화종료")]
    [SerializeField] private int maxQuestions = 3;
    [SerializeField] private string exitExcuse = "아, 미안하지만 지금은 급한 일이 있어서… 다음에 이야기하자.";

    private int questionCount = 0;
    private int endPhase = 0;

    private bool isOpen;
    private bool isSending;
    private bool waitingForF;

    private int lastClosedFrame = -999;
    public int LastClosedFrame => lastClosedFrame;
    public bool IsOpen => isOpen;

    private NPCInteractable currentNpc;
    private bool finishedByMaxQuestions;

    private void Awake()
    {
        root.SetActive(false);
        isOpen = false;
        isSending = false;

        if (sendButton != null)
            sendButton.onClick.AddListener(Send);
    }

    private void Update()
    {
        if (!isOpen) return;

        //대화 종료
        if (Input.GetKeyDown(KeyCode.Escape))
            Close();

        if (waitingForF && Input.GetKeyDown(KeyCode.F))
        {
            if (endPhase == 1)
            {
                logText.text = $"{exitExcuse}\n(F를 눌러 종료)";
                endPhase = 2;
                return;
            }

            if (endPhase == 2)
            {
                if (finishedByMaxQuestions && currentNpc != null)
                    currentNpc.MarkDialogueFinished();

                Close();
                return;
            }

            waitingForF = false;
            logText.text = "";
            ShowInput(true);
        }

        if (!waitingForF && Input.GetKeyDown(KeyCode.Return))
            Send();
    }
    public void OpenChat(NPCInteractable npc)
    {
        currentNpc = npc;

        string npcName = (npc != null && npc.Profile != null && !string.IsNullOrEmpty(npc.Profile.displayName))
            ? npc.Profile.displayName
            : "NPC";

        SetNpcName(npcName);

        root.SetActive(true);
        isOpen = true;

        questionCount = 0;
        endPhase = 0;
        waitingForF = false;
        isSending = false;
        finishedByMaxQuestions = false;

        logText.text = "";
        ShowInput(true);
    }



    //대화 완료된 NPC
    public void OpenOneShot(string npcName, string line)
    {
        SetNpcName(npcName);
        root.SetActive(true);
        isOpen = true;

        questionCount = 0;
        isSending = false;
        finishedByMaxQuestions = false;

        endPhase = 2;
        waitingForF = true;

        ShowInput(false);
        logText.text = $"{npcName}: {line}\n(F를 눌러 종료)";
    }

    public void Close()
    {
        lastClosedFrame = Time.frameCount;
        SetNpcName("");

        ShowInput(false);                  
        logText.text = "";              

        root.SetActive(false);
        isOpen = false;
        isSending = false;
        waitingForF = false;

        currentNpc = null;
        finishedByMaxQuestions = false;
        endPhase = 0;
    }

    public void Send()
    {
        if (!isOpen || isSending || waitingForF) return;

        if (endPhase != 0) return;

        string userMsg = inputField.text.Trim();
        if (string.IsNullOrEmpty(userMsg)) return;

        questionCount++;


        inputField.text = "";

        ShowInput(false);

        StartCoroutine(SendToAI(userMsg));
    }

    private IEnumerator SendToAI(string userMsg)
    {
        isSending = true;
        inputField.interactable = false;
        sendButton.interactable = false;

        yield return StartCoroutine(
            GameController.instance.OpenAI.SendMessage(userMsg, (reply) =>
            {
                SetNpcText($"{reply}\n(F를 눌러 다음 질문)");
            })
        );

        isSending = false;

        waitingForF = true;

        if (questionCount >= maxQuestions)
        {
            finishedByMaxQuestions = true;
            endPhase = 1;
        }
    }

    private void AppendLine(string line)
    {
        logText.text += line + "\n";
    }

    private void SetNpcText(string text)
    {
        logText.text = text;
    }

    private void ShowInput(bool show)
    {
        inputField.gameObject.SetActive(show);
        sendButton.gameObject.SetActive(show);

        if (show)
        {
            inputField.interactable = true;
            sendButton.interactable = true;
            inputField.ActivateInputField();
        }
    }

    private void SetNpcName(string name)
    {
        if (npcNameText != null)
            npcNameText.text = name;
    }
}
