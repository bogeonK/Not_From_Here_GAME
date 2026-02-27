using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

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
    [SerializeField] private string exitExcuse = "(더이상 대화는 의미 없을 것 같다)";

    [SerializeField] private Image endingArtImage;

    [SerializeField] private string lobbySceneName = "Lobby";
    private bool endingReadyToExit;

    private int questionCount = 0;
    private int endPhase = 0;

    private bool isOpen;
    private bool isSending;
    private bool waitingForF;
    private bool pendingChoice;


    private int lastClosedFrame = -999;
    public int LastClosedFrame => lastClosedFrame;
    public bool IsOpen => isOpen;

    private NPCInteractable currentNpc;
    private bool finishedByMaxQuestions;

    //베드엔딩용 변수
    private bool isEndingSequence;
    private string endingLine1;
    private string endingLine2;
    private int endingStep;

    //인겜최초진입 대사용
    private bool isEnterSequence;
    private string[] enterLines;
    private int enterIndex;


    // 전투 모드
    private bool isChoiceMode = false;
    private string[] currentOptions;
    private int selectedIndex = 0;
    private System.Action<int> onChoiceSelected;
    private string choiceMessage;

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

        // 전투모드일 때는 다른 입력막고 여기서만 처리
        if (isChoiceMode)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                selectedIndex = (selectedIndex - 1 + currentOptions.Length) % currentOptions.Length;
                RefreshChoiceText();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                selectedIndex = (selectedIndex + 1) % currentOptions.Length;
                RefreshChoiceText();
            }

            // 결정키
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space))
            {
                int result = selectedIndex;
                var cb = onChoiceSelected;

                CloseChoice();   // 입력 모드 종료(로그는 유지)
                cb?.Invoke(result);
            }

            // 취소키(원하면 도망 처리로 쓰면 됨)
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.X))
            {
                int result = 2; // 기본: 2번(도망치기)
                var cb = onChoiceSelected;

                CloseChoice();
                cb?.Invoke(result);
            }

            return;
        }

        if (isEnterSequence)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                enterIndex++;

                if (enterIndex >= enterLines.Length)
                {
                    isEnterSequence = false;
                    Close();
                    return;
                }

                logText.text = $"{enterLines[enterIndex]}\n(F를 눌러 다음으로)";
            }
            return;
        }

        if (isEndingSequence)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (endingStep == 0)
                {
                    endingStep = 1;
                    logText.text = $"{endingLine2}\n(F를 눌러 종료)";
                }
                else
                {
                    isEndingSequence = false;
                    endingReadyToExit = true;

                    logText.text = $"(화면을 클릭하면 로비로 돌아갑니다)";
                }
            }
            return;
        }

        //엔딩 끝난 뒤 로비로
        if (endingReadyToExit)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                SceneManager.LoadScene(lobbySceneName);
            }
            return;
        }

        //대화 종료
        if (Input.GetKeyDown(KeyCode.Escape))
            Close();

        if (waitingForF && Input.GetKeyDown(KeyCode.F))
        {
            if (pendingChoice)
            {
                pendingChoice = false;

                //선택지 열기
                GameController.instance.GetManager<NpcChoiceManager>()
                    .OpenChoice(currentNpc, currentNpc.Profile);

                // 선택지 고르는 동안 대화 입력 막기
                ShowInput(false);
                waitingForF = false;
                return;
            }


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
        if (string.IsNullOrEmpty(npcName))
            logText.text = line;
        else
            logText.text = $"{npcName}: {line}\n(F를 눌러 종료)";

    }

    public void Close()
    {

        isEnterSequence = false;
        enterLines = null;
        enterIndex = 0;

        lastClosedFrame = Time.frameCount;
        SetNpcName("");

        CloseChoice();

        ShowInput(false);
        logText.text = "";

        root.SetActive(false);
        isOpen = false;
        isSending = false;
        waitingForF = false;

        currentNpc = null;
        finishedByMaxQuestions = false;
        endPhase = 0;

        endingReadyToExit = false;
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

        string systemPrompt = " ";
        if (currentNpc != null && currentNpc.Profile != null)
        {
            systemPrompt = currentNpc.Profile
                .GetSystemPrompt(currentNpc.RuntimeIdentity);
        }



        string userPrompt = $"[{questionCount}/{maxQuestions}] {userMsg}";


        yield return StartCoroutine(
        GameController.instance.OpenAI.SendMessage(systemPrompt, userPrompt, (reply) =>
        {
            if (questionCount >= maxQuestions)
                SetNpcText($"{reply}\n(F를 눌러 결정)");
            else
                SetNpcText($"{reply}\n(F를 눌러 다음 질문)");
        })

        );

        isSending = false;
        waitingForF = true;

        if (questionCount >= maxQuestions)
        {
            finishedByMaxQuestions = true;
            pendingChoice = true; //선택지 열기
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

    public void SetNpcName(string name)
    {
        if (npcNameText != null)
            npcNameText.text = name;
    }

    public void ShowExitExcuseAndClose(NPCInteractable npc)
    {
        currentNpc = npc;

        // NPC대화 완료 처리
        if (currentNpc != null)
            currentNpc.MarkDialogueFinished();

        // 핑계대사
        endPhase = 2;
        waitingForF = true;
        ShowInput(false);

        logText.text = $"{exitExcuse}\n(F를 눌러 종료)";
    }

    public void OpenBadEnding(Sprite art, string line1, string line2)
    {
        SetNpcName("");
        root.SetActive(true);
        isOpen = true;

        ShowInput(false);
        isSending = false;
        waitingForF = true;

        // 엔딩
        isEndingSequence = true;
        endingReadyToExit = false;
        endingLine1 = line1;
        endingLine2 = line2;
        endingStep = 0;

        if (endingArtImage != null)
        {
            endingArtImage.gameObject.SetActive(art != null);
            endingArtImage.sprite = art;
            endingArtImage.preserveAspect = true;
        }

        logText.text = $"{endingLine1}\n(F를 눌러 계속)";
    }

    public void OpenChoice(string message, string[] options, System.Action<int> onSelect)
    {
        // 패널 열기
        root.SetActive(true);
        isOpen = true;

        // 입력창/AI 기능끄기
        ShowInput(false);
        waitingForF = false;
        isSending = false;

        //전투모드 세팅
        isChoiceMode = true;
        choiceMessage = message;
        currentOptions = options;
        selectedIndex = 0;
        onChoiceSelected = onSelect;

        RefreshChoiceText();
    }

    public void CloseChoice()
    {
        isChoiceMode = false;
        choiceMessage = null;
        currentOptions = null;
        onChoiceSelected = null;
        selectedIndex = 0;
    }

    private void RefreshChoiceText()
    {
        if (currentOptions == null || currentOptions.Length == 0)
        {
            logText.text = choiceMessage ?? "";
            return;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.AppendLine(choiceMessage ?? "");
        sb.AppendLine();

        for (int i = 0; i < currentOptions.Length; i++)
        {
            if (i == selectedIndex) sb.AppendLine("▶ " + currentOptions[i]);
            else sb.AppendLine("  " + currentOptions[i]);
        }

        logText.text = sb.ToString();
    }

    public void SetBattleLog(string line)
    {
        root.SetActive(true);
        isOpen = true;

        CloseChoice();
        ShowInput(false);
        waitingForF = false;

        logText.text = line;
    }
    // 인겜 최초진입대사 열기
    public void OpenEnterLines(string[] lines)
    {
        if (lines == null || lines.Length == 0) return;

        enterLines = lines;
        enterIndex = 0;
        isEnterSequence = true;

        root.SetActive(true);
        isOpen = true;

        ShowInput(false);
        logText.text = $"{enterLines[enterIndex]}\n(F를 눌러 다음으로)";
    }

}
