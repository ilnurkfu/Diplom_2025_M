using System.Collections.Generic;

public static class DefaultPrompts
{
    public static readonly SituationPrompt DefaultSituation = new SituationPrompt
    {
        description = "На уроке предмета ученик ведёт себя дерзко и мешает всему классу.",
        loyalty = 50,
        additionalParams = new Dictionary<string, string>
        {
            { "subject", "неизвестно" },
            { "gradeLevel", "неизвестно" }
        }
    };

    public static readonly PersonaPrompt DefaultPersona = new PersonaPrompt
    {
        roleDescription = "Ты ученик, который пытается понять смысл урока.",
        tone = "нейтральный"
    };
}
