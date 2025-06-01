// DialogueManager.cs
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    private const string CombinedFileName = "combined.json";
    private readonly string globalPrompt =
        "Ты экзаменатор, который помогает оценивать молодого педагога по его действиям с трудным школьником 8 класса.";

    private DialoguePrompt dialoguePrompt;
    private ConversationState conversationState;
    private DialogueUI dialogueUI;

    // Кэш для вариантов подсказки текущего хода NPC
    private string[] cachedOptions = null;

    private void Awake()
    {
        dialogueUI = FindFirstObjectByType<DialogueUI>();
        if (dialogueUI == null)
            Debug.LogError("[DialogueManager] Не найден DialogueUI на сцене!");
    }

    private void Start()
    {
        InitializeDialogue();
    }

    public void InitializeDialogue()
    {
        DialoguePrompt promptsFromFile = LoadOrCreateCombinedPrompts();

        dialoguePrompt = new DialoguePrompt
        {
            globalPrompt = globalPrompt,
            situation = promptsFromFile.situation,
            persona = promptsFromFile.persona
        };

        conversationState = new ConversationState
        {
            situation = dialoguePrompt.situation,
            persona = dialoguePrompt.persona,
            userHistory = new List<string>(),
            npcHistory = new List<string>(),
            isAwaitingUserResponse = false,
            lastScore = 0
        };

        // Показываем начальную лояльность
        dialogueUI.SetLoyalty(conversationState.situation.loyalty);

        RequestInitialNPCLine();
    }

    private DialoguePrompt LoadOrCreateCombinedPrompts()
    {
        string fullPath = Path.Combine(Application.persistentDataPath, CombinedFileName);

        if (!File.Exists(fullPath))
        {
            var emptyContainer = new CombinedContainer
            {
                situation = new SituationPrompt
                {
                    description = "",
                    loyalty = 0,
                    additionalParams = new Dictionary<string, string>()
                },
                persona = new PersonaPrompt
                {
                    roleDescription = "",
                    tone = ""
                }
            };

            string emptyJson = JsonUtility.ToJson(emptyContainer, true);

            try
            {
                File.WriteAllText(fullPath, emptyJson);
                Debug.Log($"[DialogueManager] Файл combined.json не найден — создан пустой шаблон по адресу: {fullPath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DialogueManager] Не удалось создать файл combined.json: {e.Message}");
                return new DialoguePrompt
                {
                    situation = DefaultPrompts.DefaultSituation,
                    persona = DefaultPrompts.DefaultPersona
                };
            }
        }

        string json = File.ReadAllText(fullPath);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new DialoguePrompt
            {
                situation = DefaultPrompts.DefaultSituation,
                persona = DefaultPrompts.DefaultPersona
            };
        }

        CombinedContainer loaded = null;
        try
        {
            loaded = JsonUtility.FromJson<CombinedContainer>(json);
        }
        catch
        {
            Debug.LogWarning("[DialogueManager] Некорректный JSON в combined.json, используем дефолты");
            return new DialoguePrompt
            {
                situation = DefaultPrompts.DefaultSituation,
                persona = DefaultPrompts.DefaultPersona
            };
        }

        SituationPrompt sit = loaded.situation;
        PersonaPrompt pers = loaded.persona;

        if (sit == null || string.IsNullOrWhiteSpace(sit.description))
            sit = DefaultPrompts.DefaultSituation;
        if (pers == null || string.IsNullOrWhiteSpace(pers.roleDescription))
            pers = DefaultPrompts.DefaultPersona;

        return new DialoguePrompt
        {
            situation = sit,
            persona = pers
        };
    }

    private void RequestInitialNPCLine()
    {
        // Новый ход NPC — сбрасываем кэш
        cachedOptions = null;

        string fullPrompt = PromptService.BuildPrompt(
            dialoguePrompt,
            conversationState,
            PromptType.InitialNPC
        );

        StartCoroutine(LLMService.SendPromptCoroutine(
            fullPrompt,
            rawJson => OnInitialNPCResponse(rawJson),
            error => Debug.LogError("[DialogueManager] Ошибка InitialNPC: " + error)
        ));
    }

    private void OnInitialNPCResponse(string rawJson)
    {
        string cleanJson = ExtractJson(rawJson);

        NPCInitialResponse response = JsonUtility.FromJson<NPCInitialResponse>(cleanJson);

        if (response != null && !string.IsNullOrEmpty(response.npcLine))
        {
            conversationState.npcHistory.Add(response.npcLine);
            dialogueUI.ShowNPCText(response.npcLine);
            conversationState.isAwaitingUserResponse = true;
            dialogueUI.OnNPCReplied(response.npcLine);
        }
        else
        {
            Debug.LogError("[DialogueManager] Некорректный ответ InitialNPC: " + cleanJson);
        }
    }

    public void RequestUserOptions()
    {
        // Если уже есть сгенерированные варианты для текущего хода NPC, просто показываем их
        if (cachedOptions != null)
        {
            dialogueUI.OnOptionsReady(cachedOptions);
            return;
        }

        string fullPrompt = PromptService.BuildPrompt(
            dialoguePrompt,
            conversationState,
            PromptType.GenerateOptions
        );

        StartCoroutine(LLMService.SendPromptCoroutine(
            fullPrompt,
            rawJson => OnUserOptionsResponse(rawJson),
            error => Debug.LogError("[DialogueManager] Ошибка GenerateOptions: " + error)
        ));
    }

    private void OnUserOptionsResponse(string rawJson)
    {
        string cleanJson = ExtractJson(rawJson);
        UserOptionsResponse response = JsonUtility.FromJson<UserOptionsResponse>(cleanJson);

        if (response != null && response.options != null && response.options.Length == 4)
        {
            // Кэшируем варианты
            cachedOptions = response.options;
            dialogueUI.OnOptionsReady(response.options);
        }
        else
        {
            Debug.LogError("[DialogueManager] Некорректный ответ GenerateOptions: " + cleanJson);
        }
    }

    public void RequestNPCReaction(string playerLine)
    {
        // Пользователь выбрал вариант или ввёл ответ вручную — сбрасываем кэш вариантов
        cachedOptions = null;

        conversationState.userHistory.Add(playerLine);

        string fullPrompt = PromptService.BuildPrompt(
            dialoguePrompt,
            conversationState,
            PromptType.NPCReaction,
            playerLine
        );

        StartCoroutine(LLMService.SendPromptCoroutine(
            fullPrompt,
            rawJson => OnNPCReactionResponse(rawJson),
            error => Debug.LogError("[DialogueManager] Ошибка NPCReaction: " + error)
        ));
    }

    private void OnNPCReactionResponse(string rawJson)
    {
        string cleanJson = ExtractJson(rawJson);
        NPCReactionResponse response = JsonUtility.FromJson<NPCReactionResponse>(cleanJson);

        if (response != null && !string.IsNullOrEmpty(response.npcLine))
        {
            conversationState.npcHistory.Add(response.npcLine);

            int newLoyalty = conversationState.situation.loyalty + response.score;
            newLoyalty = Mathf.Clamp(newLoyalty, 0, 100);
            conversationState.situation.loyalty = newLoyalty;
            conversationState.lastScore = response.score;

            dialogueUI.SetLoyalty(newLoyalty);

            Debug.Log("[DialogueManager] NPC: " + response.npcLine);
            Debug.Log("[DialogueManager] Новая loyalty: " + newLoyalty);

            dialogueUI.OnNPCReplied(response.npcLine);
        }
        else
        {
            Debug.LogError("[DialogueManager] Некорректный ответ NPCReaction: " + cleanJson);
        }
    }

    private string ExtractJson(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        int start = input.IndexOf("```json");
        if (start >= 0)
        {
            start = input.IndexOf('\n', start) + 1;
            if (start == 0) start = 0;
        }
        else
        {
            start = input.IndexOf("```");
            if (start >= 0)
                start += 3;
            else
                start = 0;
        }

        int end = input.LastIndexOf("```");
        if (end < 0)
            end = input.Length;

        int length = Mathf.Max(0, end - start);
        string candidate = input.Substring(start, length).Trim();

        return candidate;
    }
}
