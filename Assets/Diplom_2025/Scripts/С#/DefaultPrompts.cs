using System.Collections.Generic;

public static class DefaultPrompts
{
    public const string DefaultModelName = "vikhr-nemo-12b-instruct-r-21-09-24";

    public static readonly SituationPrompt DefaultSituation = new SituationPrompt
    {
        description = "���� ��������� � 8 ������. ���� �� �������� � ��������� ���� �����: \"��� ������ �� ����� ���� ���������, ��� ����������� �������. ����� �� �� �������� ���-�� ��������.\" ������ ������� �������� ������� ������� �� ����������. ���� ������ ��� ������� � ��������� �������� ������� ���, �����:\r\n1) �� ��������� ��������;\r\n2) ��������� ������������ ���;\r\n3) ����������� ������ ������� �������;\r\n4) ������������� �������� � �������������� �����;\r\n5) �������������� ������� ������� �����.",
        loyalty = 50,
        additionalParams = new Dictionary<string, string>
        {
            { "subject", "����������" },
            { "gradeLevel", "����������" }
        }
    };

    public static readonly PersonaPrompt DefaultPersona = new PersonaPrompt
    {
        roleDescription = "���� � ������ 8 ������. �� ������ �������� �� ����������, ������ � �����������, ������� ���������� �������. ������� ��������, �� � ���������� ��������� � ������������ ���������. ��������� ������� ����������� � �� �������� � ���������� ��������. ����� �������� ��� ������ �����, �� �� �����. � ����� ������������ ���� �������, ������ ������.",
        tone = "�����������"
    };
}
