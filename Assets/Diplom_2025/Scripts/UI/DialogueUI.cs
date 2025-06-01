// DialogueUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("Основной UI")]
    [SerializeField] private TextMeshProUGUI npcText;
    [SerializeField] private Button needHintButton;
    [SerializeField] private TMP_InputField playerInputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private TextMeshProUGUI loyaltyText;

    [Header("Основная группа UI (кроме optionsPanel)")]
    [SerializeField] private GameObject mainUIGroup;

    [Header("Панель с вариантами (4 кнопки и кнопка возврата)")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private Button[] optionButtons = new Button[4];
    [SerializeField] private Button returnButton;

    private DialogueManager dialogueManager;

    private void Awake()
    {
        dialogueManager = FindFirstObjectByType<DialogueManager>();

        optionsPanel.SetActive(false);

        needHintButton.onClick.AddListener(OnNeedHintClicked);
        sendButton.onClick.AddListener(OnSendButtonClicked);

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i;
            optionButtons[i].onClick.AddListener(() => OnOptionButtonClicked(index));
        }

        returnButton.onClick.AddListener(OnReturnButtonClicked);
    }

    public void ShowNPCText(string text)
    {
        npcText.text = text;
    }

    public void SetLoyalty(int loyalty)
    {
        loyaltyText.text = $"Loyalty: {loyalty}";
    }

    private void OnNeedHintClicked()
    {
        // Если панель вариантов уже открыта — просто показываем её
        if (optionsPanel.activeSelf)
        {
            mainUIGroup.SetActive(false);
            optionsPanel.SetActive(true);
            return;
        }

        // Иначе скрываем основной UI и запрашиваем варианты
        mainUIGroup.SetActive(false);
        optionsPanel.SetActive(false); // на всякий случай
        dialogueManager.RequestUserOptions();
    }

    private void OnSendButtonClicked()
    {
        string playerLine = playerInputField.text.Trim();
        if (!string.IsNullOrEmpty(playerLine))
        {
            playerInputField.text = "";
            playerInputField.interactable = false;
            sendButton.interactable = false;
            needHintButton.interactable = false;
            optionsPanel.SetActive(false);

            dialogueManager.RequestNPCReaction(playerLine);
        }
    }

    public void ShowOptions(string[] options)
    {
        if (options == null || options.Length != 4) return;

        // Просто показываем панель с вариантами, без блокировки input
        optionsPanel.SetActive(true);
        for (int i = 0; i < 4; i++)
        {
            TextMeshProUGUI btnText = optionButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = options[i];
            optionButtons[i].interactable = true;
        }
    }

    private void OnOptionButtonClicked(int index)
    {
        string chosen = optionButtons[index].GetComponentInChildren<TextMeshProUGUI>().text;
        optionsPanel.SetActive(false);
        mainUIGroup.SetActive(true);

        playerInputField.interactable = false;
        sendButton.interactable = false;
        needHintButton.interactable = false;

        dialogueManager.RequestNPCReaction(chosen);
    }

    private void OnReturnButtonClicked()
    {
        // Закрываем панель вариантов, возвращаем основной UI
        optionsPanel.SetActive(false);
        mainUIGroup.SetActive(true);
    }

    public void OnNPCReplied(string npcLine)
    {
        ShowNPCText(npcLine);
        mainUIGroup.SetActive(true);
        playerInputField.interactable = true;
        sendButton.interactable = true;
        needHintButton.interactable = true;
    }

    public void OnOptionsReady(string[] options)
    {
        // Убираем блокировку input здесь — пользователь может вводить даже когда видны варианты
        ShowOptions(options);
    }
}
