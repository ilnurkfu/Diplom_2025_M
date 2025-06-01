using System.Collections.Generic;

public class ConversationState
{
    public SituationPrompt situation;
    public PersonaPrompt persona;
    public List<string> userHistory;
    public List<string> npcHistory;
    public bool isAwaitingUserResponse;
    public int lastScore;
}
