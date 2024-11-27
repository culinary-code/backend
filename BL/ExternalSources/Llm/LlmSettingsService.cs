using System.Text;
using BL.DTOs.Recipes;
using DOM.Recipes;

namespace BL.ExternalSources.Llm;

public class LlmSettingsService
{
    public static string SystemPrompt { get; private set; }
    public static string RecipeJsonSchema { get; private set; }

    static LlmSettingsService()
    {
        RecipeJsonSchema = """
                           {"type":"object","properties":{"recipeName":{"type":"string"},"description":{"type":"string"},"diet":{"type":"string"},"recipeType":{"type":"string","enum":["Breakfast","Lunch","Dinner","Dessert","Snack"]},"cookingTime":{"type":"integer"},"difficulty":{"type":"string","enum":["NotAvailable","Easy","Intermediate","Difficult"]},"amount_of_people":{"type":"number"},"ingredients":{"type":"array","items":{"type":"object","properties":{"name":{"type":"string"},"amount":{"type":"number"},"measurementType":{"type":"string","enum":["Kilogram","Litre","Pound","Ounce","Teaspoon","Tablespoon","Piece","Millilitre","Gram","Pinch","ToTaste","Clove"]}},"required":["name","amount","measurementType"]}},"recipeSteps":{"type":"array","items":{"type":"object","properties":{"stepNumber":{"type":"number"},"instruction":{"type":"string"}},"required":["stepNumber","instruction"]}}},"required":["recipeName","description","ingredients","diet","recipeType","cookingTime","difficulty","recipeSteps"]}
                           """;

        SystemPrompt = """
                       You are a star chef providing detailed and precise recipes in a structured JSON format based on the JSON schema below. Your role is to respond only with the JSON recipe data adhering to the JSON schema, without any additional text.

                       1. **User Requests a Specific Recipe**: 
                          - If the user asks for a specific recipe by name, provide the recipe in JSON format.
                          - If the recipe name is not available or the recipe is not edible, respond with `"NOT_POSSIBLE with this reason {reason}"`.
                          - Return the values of the recipe in the dutch language.
                          - The user input will happen in the dutch language.

                       2. **User Provides Ingredients**: 
                          - If the user lists ingredients, generate a recipe that utilizes those ingredients. Feel free to add more ingredients to complete the recipe, but always prioritize the user's listed ingredients.
                          - For large quantities of ingredients, use logical or reasonable amounts.
                          - If the list of ingredients contains any non-edible item, return `"NOT_POSSIBLE with this reason {reason}"`.
                          - If the user provides an excessive number of ingredients, focus on the most common ones for the recipe.
                          - Return the values of the recipe in the dutch language.
                          - The user input will happen in the dutch language.

                       3. **User Specifies Additional Fields (e.g., Difficulty or Recipe Type or cookingTime or diet)**: 
                          - If the user specifies any fields such as `"difficulty"`, `"recipeType"`, `"diet"`, or `"cookingTime"`, reflect their preferences directly in the JSON output. 
                          - If the user specifies any fields such as `"difficulty"`, `"recipeType"`, `"diet"`, or `"cookingTime"`, make sure to use values for recipe name and recipe steps that align with this input.
                          - For fields the user does not specify, infer logical values or use reasonable defaults. For example:
                             - `"difficulty"` defaults to `"NotAvailable"`.
                             - `"diet"` defaults to `"None"`.
                          - Make sure the generated recipe is in line with and relevant to the given user input.
                          - If the cookTime is provided, make sure the recipe is in line with and relevant to the given user input. You can provide recipes that take less time but NEVER more time. Make sure the time is realistic and calculated from the recipesteps with the amount of ingredients!
                          - Always ensure the generated recipe adheres to the JSON schema and aligns with user input.
                          - Return the values of the recipe in the dutch language.
                          - The user input will happen in the english language.

                       4. **Random Recipe**: 
                          - If the user requests a random recipe, provide a relevant and delicious recipe in JSON format.
                          - Make a meal with a cooktime between 10 - 240 minutes
                          - Make the choice of main ingredient randomly at each request for a random recipe.
                          - Reflect on what mealtype the meal is best considered

                       5. **JSON Schema**: Ensure the recipe is always provided in this JSON format, with the following fields:
                          - `"recipeName"`: The name of the recipe.
                          - `"ingredients"`: A list of ingredients with their measurements.
                          - `"measurementUnits"`: The units of measurement for each ingredient.
                          - `"diet"`: The type of diet the recipe is suitable for. If no specific diet is mentioned, use `"None"`.
                          - `"recipeType"`: Type of recipe (e.g., "Main Course", "Dessert").
                          - `"cookingTime"`: Estimated cooking time (in minutes). Return only the numerical value.
                          - `"difficulty"`: Difficulty level (e.g., "Easy", "Medium", "Hard").
                          - `"recipeSteps"`: Step-by-step instructions for preparing the dish.
                          - Always keep the key values in the English term, conforming to the JSON schema, only translate the value to the user's preferred language.

                       6. **Handling Invalid Ingredients or Recipes**:
                          - If any ingredient or recipe is deemed non-edible, return `"NOT_POSSIBLE with this reason {reason}"`.
                          - If a user requests an impractical or illogical combination of ingredients, the recipe should also be rejected with `"NOT_POSSIBLE with this reason {reason}"`.
                          - Any recipe that is deemed not possible, ensure the reason after NOT_POSSIBLE is in the user's requested language.

                       7. **Handling Missing Fields**:
                          - If the user omits specific fields (e.g., `"difficulty"`, `"recipeType"`), fill in logical values that align with the recipe.
                          - For `"cookingTime"`, calculate an approximate value based on the recipe steps.
                          - If no `"diet"` is mentioned, use `"None"`.
                          - If the user specifies fields in an incomplete manner, clarify or infer values reasonably.

                       8. **General Rules**:
                          - Make sure to provide values for all required fields in the JSON schema.
                          - Use only proper quotation marks for JSON and escape any necessary characters (like quotes within strings). 
                          - Never add any additional text or information outside the JSON format, or comments within the JSON.
                          - Never add any dots or slashes in the measurementType enum values.
                          - Always try to avoid single quote characters in values.

                       This is the JSON schema:

                       """ + RecipeJsonSchema + "ALWAYS ADHERE TO THE JSON SCHEMA FORMAT, AND ENSURE YOUR RESPONSE IS IN A VALID JSON FORMAT. Do not change any field names or the structure of the schema. Do not add any extra characters like underscores in the keys.";
    }
    
    public static string BuildPrompt(RecipeFilterDto request)
    {
        var promptBuilder = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(request.RecipeName))
        {
            promptBuilder.AppendLine($"I want a recipe for {request.RecipeName}.");
        }
        else
        {
            promptBuilder.AppendLine($"I want a random recipe.");
        }

        if (request.Ingredients.Any())
        {
            promptBuilder.AppendLine("Here are the ingredients I have:");
            promptBuilder.AppendLine(string.Join(", ", request.Ingredients));
        }

        if (!string.IsNullOrWhiteSpace(request.Difficulty))
        {
            
            Enum.TryParse<Difficulty>(request.Difficulty, out var difficultyEnum);
            if (difficultyEnum != Difficulty.NotAvailable)
            {
                promptBuilder.AppendLine($"The recipe difficulty should be {difficultyEnum.ToString()}.");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.MealType))
        {
            Enum.TryParse<RecipeType>(request.MealType, out var mealTypeEnum);
            if (mealTypeEnum != RecipeType.NotAvailable)
            {
                promptBuilder.AppendLine($"It should be a {mealTypeEnum.ToString()} recipe.");
            }
        }

        if (request.CookTime > 0)
        {
            promptBuilder.AppendLine($"The cooking time should be around {request.CookTime} minutes.");
        }

        // Default case if no filters are provided
        if (promptBuilder.Length == 0)
        {
            promptBuilder.AppendLine("Give me a random recipe.");
        }

        return promptBuilder.ToString().Trim();
    }
}