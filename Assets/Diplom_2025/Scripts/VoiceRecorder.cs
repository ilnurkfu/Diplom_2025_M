using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

public class VoiceRecorder : MonoBehaviour
{
    [Header("UI Elements")]
    public Button recordButton;
    public TMP_InputField inputField;

    [Header("Recording Settings")]
    private const int SampleRate = 16000;
    private const int MaxRecordingTime = 10; // seconds

    [Header("Server Settings")]
    private const string VoskServerUrl = "http://localhost:5005/recognize";

    private AudioClip recordedClip;

    void Start()
    {
        recordButton.onClick.AddListener(OnRecordButtonPressed);
    }

    void OnRecordButtonPressed()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("Microphone not found!");
            return;
        }

        Debug.Log("🎙️ Начало записи...");
        recordedClip = Microphone.Start(null, false, MaxRecordingTime, SampleRate);
        StartCoroutine(StopRecordingAfterDelay(MaxRecordingTime));
    }

    IEnumerator StopRecordingAfterDelay(int seconds)
    {
        yield return new WaitForSeconds(seconds);

        // Подстраховка: дождаться следующего кадра
        yield return null;

        Microphone.End(null);
        Debug.Log("🛑 Запись завершена");

        byte[] wavData = WavUtility.FromAudioClip(recordedClip);
        Debug.Log($"📦 Размер WAV: {wavData.Length} байт");

        yield return StartCoroutine(SendToServer(wavData));
    }

    IEnumerator SendToServer(byte[] wavData)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", wavData, "audio.wav", "audio/wav");

        using (UnityWebRequest www = UnityWebRequest.Post(VoskServerUrl, form))
        {
            www.timeout = 20; // Увеличиваем таймаут до 20 секунд
            Debug.Log($"🌐 Отправка на сервер: {VoskServerUrl}");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"❌ Ошибка при отправке: {www.error}");
            }
            else
            {
                string rawResult = www.downloadHandler.text.Trim();
                Debug.Log($"✅ Ответ от сервера: {rawResult}");
                inputField.text = rawResult;
            }
        }
    }
}
