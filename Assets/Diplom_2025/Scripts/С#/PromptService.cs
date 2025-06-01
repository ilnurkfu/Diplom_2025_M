using System.Collections.Generic;
using System.Text;

public static class PromptService
{
    public static string BuildPrompt(
       DialoguePrompt dialoguePrompt,
       ConversationState state,
       PromptType promptType,
       string playerLine = null)
    {
        var sb = new StringBuilder();

        // 1) GlobalPrompt
        sb.AppendLine(dialoguePrompt.globalPrompt);
        sb.AppendLine();

        // 2) Situation
        sb.AppendLine("Ситуация: " + dialoguePrompt.situation.description);
        sb.AppendLine($"Loyalty: {dialoguePrompt.situation.loyalty}");
        if (dialoguePrompt.situation.additionalParams != null &&
            dialoguePrompt.situation.additionalParams.Count > 0)
        {
            sb.AppendLine("Дополнительные параметры:");
            foreach (KeyValuePair<string, string> kvp in dialoguePrompt.situation.additionalParams)
            {
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
            }
        }
        sb.AppendLine();

        // 3) Persona
        sb.AppendLine("Роль: " + dialoguePrompt.persona.roleDescription);
        sb.AppendLine("Тон: " + dialoguePrompt.persona.tone);
        sb.AppendLine();

        // 4) История (если есть)
        if (state.npcHistory != null && state.npcHistory.Count > 0)
        {
            sb.AppendLine("--- История диалога ---");
            for (int i = 0; i < state.npcHistory.Count; i++)
            {
                sb.AppendLine($"NPC: {state.npcHistory[i]}");
                if (i < state.userHistory.Count)
                    sb.AppendLine($"Player: {state.userHistory[i]}");
            }
            sb.AppendLine();
        }

        // 5) Инструкция по типу запроса
        switch (promptType)
        {
            case PromptType.InitialNPC:
                sb.AppendLine("Задача: это первое сообщение NPC (ученика). Вырази недовольство ученика.");
                sb.AppendLine("Формат вывода (только JSON): { \"npcLine\": \"...\" }");
                break;

            case PromptType.GenerateOptions:
                sb.AppendLine("Задача: предложи ровно 4 варианта ответа учителя (или игрока).");
                sb.AppendLine("Формат вывода (только JSON): { \"options\": [\"ответ1\",\"ответ2\",\"ответ3\",\"ответ4\"] }");
                break;

            case PromptType.NPCReaction:
                sb.AppendLine("Player: " + playerLine);
                sb.AppendLine();
                sb.AppendLine("Задача: отыграй реакцию ученика (NPC). Дай одну фразу и поставь оценку.");
                sb.AppendLine("Формат вывода (только JSON): { \"npcLine\": \"...\", \"score\": X }");
                sb.AppendLine("Где score ∈ { -10, 0, 10 }");
                break;
        }

        return sb.ToString();
    }
}

/// <summary>
/// Различаем, для какой именно цели вызываем сборку промпта.
/// </summary>
public enum PromptType
{
    InitialNPC,
    GenerateOptions,
    NPCReaction
}