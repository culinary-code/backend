using System;

namespace BL.DTOs.Recipes;

public class InstructionStepDto
{
    public Guid InstructionStepId { get; set; }

    public int StepNumber { get; set; }

    public string Instruction { get; set; } = "Default Instruction";

    // Navigation properties omitted as they are not needed for the DTO
}