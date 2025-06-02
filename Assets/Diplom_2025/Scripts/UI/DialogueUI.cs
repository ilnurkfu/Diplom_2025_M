// DialogueUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("�������� UI")]
    [SerializeField] private TextMeshProUGUI npcText;
    [SerializeField] private Button needHintButton;
    [SerializeField] private TMP_InputField playerInputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private TextMeshProUGUI loyaltyText;

    [Header("�������� ������ UI (����� optionsPanel)")]
    [SerializeField] private GameObject mainUIGroup;

    [Header("������ � ���������� (4 ������ � ������ ��������)")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private Button[] optionButtons = new Button[4];
    [SerializeField] private Button returnButton;

    private DialogueManager dialogueManager;

    private void Awake()
    {
        dialogueManager = FindFirstObjectByType<DialogueManager>();

        // �������� ������ ��������� �������
        optionsPanel.SetActive(false);

        // ��������� �������
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

    /// <summary>
    /// ��������� ���������������� ���� �����, ������ ����������� � ����������.
    /// </summary>
    public void SetUserInputInteractable(bool value)
    {
        playerInputField.interactable = value;
        sendButton.interactable = value;
        needHintButton.interactable = value;
    }

    /// <summary>
    /// ���������� ��������� ��������� (�����/������) ������ � ��������� ���� UI.
    /// </summary>
    public void ShowEndState(string message, Color color)
    {
        // �������� �� ���������
        mainUIGroup.SetActive(false);
        optionsPanel.SetActive(false);

        // ���������� ��������� �������� ������ NPC-������
        npcText.text = message;
        npcText.color = color;
    }

    private void OnNeedHintClicked()
    {
        // ���� ������ ��������� ��� ������� � ������ ���������� �
        if (optionsPanel.activeSelf)
        {
            mainUIGroup.SetActive(false);
            optionsPanel.SetActive(true);
            return;
        }

        // ����� �������� �������� UI � ����������� �������� � DialogueManager
        mainUIGroup.SetActive(false);
        optionsPanel.SetActive(false); // �� ������ ������
        dialogueManager.RequestUserOptions();
    }

    private void OnSendButtonClicked()
    {
        string playerLine = playerInputField.text.Trim();
        if (!string.IsNullOrEmpty(playerLine))
        {
            playerInputField.text = "";
            SetUserInputInteractable(false);
            optionsPanel.SetActive(false);

            dialogueManager.RequestNPCReaction(playerLine);
        }
    }

    public void ShowOptions(string[] options)
    {
        if (options == null || options.Length != 4) return;

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

        SetUserInputInteractable(false);
        dialogueManager.RequestNPCReaction(chosen);
    }

    private void OnReturnButtonClicked()
    {
        // ��������� ������ ���������, ���������� �������� UI
        optionsPanel.SetActive(false);
        mainUIGroup.SetActive(true);
    }

    public void OnNPCReplied(string npcLine)
    {
        ShowNPCText(npcLine);
        mainUIGroup.SetActive(true);
        SetUserInputInteractable(true);
    }

    public void OnOptionsReady(string[] options)
    {
        // ��� ��������� ��������� ���� ������� ���������; ������ ��������
        ShowOptions(options);
    }
}
