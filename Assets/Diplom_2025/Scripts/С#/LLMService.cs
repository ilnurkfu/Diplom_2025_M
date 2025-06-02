// LLMService.cs
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class LLMService
{
    // �������� "���_�����_������" �� ������� ��������� ��� �� GET /v1/models
    private const string Url = "http://localhost:1234/v1/chat/completions";

    [Serializable]
    private class ChatPayload
    {
        public string model;
        public ChatMessage[] messages;
        public int max_tokens;
        public float temperature;
    }

    [Serializable]
    private class ChatMessage
    {
        public string role;
        public string content;
    }

    [Serializable]
    private class ChatCompletionsResponse
    {
        public ChatChoice[] choices;
    }

    [Serializable]
    private class ChatChoice
    {
        public ChatMessage message;
    }

    /// <summary>
    /// ���������� promptText � LLM Studio �� �������� /v1/chat/completions.
    /// </summary>
    public static IEnumerator SendPromptCoroutine(
        string modelName,
        string promptText,
        Action<string> onSuccess,
        Action<string> onError)
    {
        // �������� payload ����� ���������� ������, ����� JsonUtility ��������� ������������
        var payload = new ChatPayload
        {
            model = modelName,
            messages = new[]
            {
                new ChatMessage { role = "system", content = promptText }
            },
            max_tokens = 2048,
            temperature = 0.7f
        };

        string jsonData = JsonUtility.ToJson(payload);

        using (UnityWebRequest request = new UnityWebRequest(Url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(request.error);
                yield break;
            }

            string raw = request.downloadHandler.text;
            try
            {
                ChatCompletionsResponse parsed = JsonUtility.FromJson<ChatCompletionsResponse>(raw);
                if (parsed != null
                    && parsed.choices != null
                    && parsed.choices.Length > 0
                    && parsed.choices[0].message != null
                    && !string.IsNullOrEmpty(parsed.choices[0].message.content))
                {
                    onSuccess?.Invoke(parsed.choices[0].message.content);
                }
                else
                {
                    onError?.Invoke("LLMService: ������ ��� ������������ ����� �� /v1/chat/completions");
                }
            }
            catch (Exception e)
            {
                onError?.Invoke("LLMService: �� ������� ���������� JSON �� LLM Studio: " + e.Message);
            }
        }
    }
}
