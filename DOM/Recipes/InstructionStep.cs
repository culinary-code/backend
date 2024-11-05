using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DOM.Recipes;

public class InstructionStep
{
    [Key]
    public Guid InstructionStepId { get; set; }
    public int StepNumber { get; set; }
    public string Instruction { get; set; } = "Default Instruction";
    
    // navigation properties
    [JsonIgnore]
    public Recipe? Recipe { get; set; }
}