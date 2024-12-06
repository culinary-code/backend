using System;
using System.ComponentModel.DataAnnotations;

namespace DOM.Recipes;

public class InstructionStep
{
    public Guid InstructionStepId { get; set; }
    public int StepNumber { get; set; }
    public string Instruction { get; set; } = "Default Instruction";
    
    // navigation properties
    public Recipe? Recipe { get; set; }
    
    // Foreign keys
    public Guid? RecipeId { get; set; }
}