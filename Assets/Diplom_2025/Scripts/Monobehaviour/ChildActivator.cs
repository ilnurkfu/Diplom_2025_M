using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Collider))]
public class ChildActivator : XRBaseInteractable
{
    private DialogueManager dialogueManager;

    protected override void Awake()
    {
        base.Awake();
        // Найдём DialogueManager на сцене
        dialogueManager = FindFirstObjectByType<DialogueManager>();
        if (dialogueManager == null)
            Debug.LogError("[ChildActivator] Не найден DialogueManager на сцене!");
    }

    // Этот метод вызывается, когда XR-луч «выбирает» (Select) этот объект
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        // При первом клике запускаем диалог
        if (dialogueManager != null)
            dialogueManager.StartConversation();
    }
}
