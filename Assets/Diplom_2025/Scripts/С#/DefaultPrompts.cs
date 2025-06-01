using System.Collections.Generic;

public static class DefaultPrompts
{
    public static readonly SituationPrompt DefaultSituation = new SituationPrompt
    {
        description = "�� ����� �������� ������ ���� ���� ������ � ������ ����� ������.",
        loyalty = 50,
        additionalParams = new Dictionary<string, string>
        {
            { "subject", "����������" },
            { "gradeLevel", "����������" }
        }
    };

    public static readonly PersonaPrompt DefaultPersona = new PersonaPrompt
    {
        roleDescription = "�� ������, ������� �������� ������ ����� �����.",
        tone = "�����������"
    };
}
