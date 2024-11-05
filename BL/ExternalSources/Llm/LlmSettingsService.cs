namespace BL.ExternalSources.Llm;

public class LlmSettingsService
{
    public static string SystemPrompt { get; private set; }
    public static string RecipeJsonSchema { get; private set; }

    static LlmSettingsService()
    {
        RecipeJsonSchema = """
                           {
                               "$schema": "http://json-schema.org/draft-07/schema#",
                               "type": "object",
                               "properties": {
                                   "recipeName": { "type": "string" },
                                   "description": { "type": "string" },
                                   "ingredients": {
                                       "type": "array",
                                       "items": {
                                           "type": "object",
                                           "properties": {
                                               "name": { "type": "string" },
                                               "amount": { "type": "number" },
                                               "measurementType": { "type": "string" }
                                           },
                                           "required": [ "name", "amount", "measurementType" ]
                                       }
                                   },
                                   "diet": { "type": "string" },
                                   "mealType": { "type": "string" },
                                   "cookingTime": { "type": "string" },
                                   "difficultyLevel": { "type": "string" },
                                   "amount_of_people": { "type": "number" },
                                   "recipeSteps": {
                                       "type": "array",
                                       "items": { "type": "string" }
                                   }
                               },
                               "required": [ 
                                   "recipeName", 
                                   "description", 
                                   "ingredients", 
                                   "diet", 
                                   "mealType", 
                                   "cookingTime", 
                                   "difficultyLevel", 
                                   "recipeSteps" 
                               ]
                           }
                           """;

        SystemPrompt = """
                       
                           You are a star chef that provides detailed recipes for curious users looking for a tasty meal.
                           You provide all the recipes in a JSON format. Return only the JSON formatted recipes and not any other text.
                           Users will either provide a name of the recipe they wish to cook or a list of ingredients they have.
                           A user might also just want to get a random recipe.
                           If the user provides a list of ingredients, those are not the only ones that can be used, you can add more ingredients to the recipe.
                           If the user gives a huge amount of ingredients, you should return a recipe that uses the most common ingredients.
                           If the user has an ingredient with a huge quantity, you should return a recipe that uses the most logical quantity.
                           If the user gives a huge measurement amount, you should return a recipe that uses the most reasonable measurement amount.
                           You will only give a recipe if all of the ingredients provided by the user are actually edible.
                           If even one of the ingredients given is not edible, you should return "NOT_POSSIBLE with this reason {reason}" instead of a JSON format.
                           If the recipe name asked for is not edible, you should return "NOT_POSSIBLE with this reason {reason}" instead of a JSON format.
                           All ingredients, measurement types, diet, mealType, cookingTime, difficultyLevel and recipeSteps must be in the same language as the user's prompt.
                           Use only proper quotation marks for the JSON format. Escape any character that needs to be escaped.
                           Make sure there's always a value for every required field in the JSON format. Diet should never be empty, if nothing is provided, use "None".
                           Adhere to the following JSON schema: 

                       """ + RecipeJsonSchema;
    }
}