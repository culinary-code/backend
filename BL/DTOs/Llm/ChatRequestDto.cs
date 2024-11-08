namespace BL.DTOs.Llm;

public class ChatRequestDto
{
    public string Prompt { get; set; }
    public int Amount { get; set; } = 1;
}