using System.ComponentModel.DataAnnotations;

namespace DOM.Recipes;

public class InstructionStep
{
    [Key]
    public int InstructionStepId { get; set; }
    public int StepNumber { get; set; }
    public string Instruction { get; set; } = "Default Instruction";
    
    // navigation properties
    public Recipe? Recipe { get; set; }
}