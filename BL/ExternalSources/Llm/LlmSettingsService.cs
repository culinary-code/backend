namespace BL.ExternalSources.Llm;

public class LlmSettingsService
{
    public static string SystemPrompt { get; private set; }
    public static string RecipeJsonSchema { get; private set; }
    public static string ExampleJson { get; private set; }

    static LlmSettingsService()
    {
        RecipeJsonSchema = """
                           {
                               "$schema": "http://json-schema.org/draft-07/schema#",
                               "type": "object",
                               "properties": {
                                   "recipeName": { "type": "string" },
                                   "description": { "type": "string" },
                                   "diet": { "type": "string" },
                               "recipeType": {
                                   "type": "string",
                                   "enum": ["Breakfast", "Lunch", "Dinner", "Dessert", "Snack"]
                               },
                               "cookingTime": { "type": "integer" },
                               "difficulty": {
                                   "type": "string",
                                   "enum": ["NotAvailable", "Easy", "Intermediate", "Difficult"]
                               },
                                "amount_of_people": { "type": "number" },
                                   "ingredients": {
                                       "type": "array",
                                       "items": {
                                           "type": "object",
                                           "properties": {
                                               "name": { "type": "string" },
                                               "amount": { "type": "number" },
                                               "measurementType": {
                                                   "type": "string",
                                                   "enum": ["Kilogram", "Litre", "Pound", "Ounce", "Teaspoon", "Tablespoon", "Piece", "Millilitre", "Gram", "Pinch", "ToTaste"]
                                               }
                                           },
                                           "required": ["name", "amount", "measurementType"]
                                       }
                                   },
                                   "recipeSteps": {
                                       "type": "array",
                                       "items": {
                                           "type": "object",
                                           "properties": {
                                               "stepNumber": { "type": "number" },
                                               "instruction": { "type": "string" }
                                           },
                                           "required": ["stepNumber", "instruction"]
                                       }
                                   }
                               },
                               "required": [
                                   "recipeName",
                                   "description",
                                   "ingredients",
                                   "diet",
                                   "recipeType",
                                   "cookingTime",
                                   "difficulty",
                                   "recipeSteps"
                               ]
                           }
                           """;

        SystemPrompt = """
                           You are a star chef providing detailed and precise recipes in a structured JSON format based on the JSON schema below. Your role is to respond only with the JSON recipe data adhering to the JSON schema, without any additional text.

                       1. **User Requests a Specific Recipe**: 
                          - If the user asks for a specific recipe by name, provide the recipe in JSON format.
                          - If the recipe name is not available or the recipe is not edible, respond with `"NOT_POSSIBLE with this reason {reason}"`.
                          - Return the values of the recipe in the language in which the user requested it.
                          
                       2. **User Provides Ingredients**: 
                          - If the user lists ingredients, generate a recipe that utilizes those ingredients. Feel free to add more ingredients to complete the recipe, but always prioritize the user's listed ingredients.
                          - For large quantities of ingredients, use logical or reasonable amounts.
                          - If the list of ingredients contains any non-edible item, return `"NOT_POSSIBLE with this reason {reason}"`.
                          - If the user provides an excessive number of ingredients, focus on the most common ones for the recipe.

                       3. **Random Recipe**: 
                          - If the user requests a random recipe, provide a relevant and delicious recipe in JSON format.

                       4. **JSON Schema**: Ensure the recipe is always provided in this JSON format, with the following fields:
                          - `"recipeName"`: The name of the recipe.
                          - `"ingredients"`: A list of ingredients with their measurements.
                          - `"measurementUnits"`: The units of measurement for each ingredient.
                          - `"diet"`: The type of diet the recipe is suitable for. If no specific diet is mentioned, use `"None"`.
                          - `"recipeType"`: Type of recipe (e.g., "Main Course", "Dessert").
                          - `"cookingTime"`: Estimated cooking time (in minutes). Return only the numerical value.
                          - `"difficulty"`: Difficulty level (e.g., "Easy", "Medium", "Hard").
                          - `"recipeSteps"`: Step-by-step instructions for preparing the dish.

                       5. **Handling Invalid Ingredients or Recipes**:
                          - If any ingredient or recipe is deemed non-edible, return `"NOT_POSSIBLE with this reason {reason}"`.
                          - If a user requests an impractical or illogical combination of ingredients, the recipe should also be rejected with `"NOT_POSSIBLE with this reason {reason}"`.

                       6. **General Rules**:
                          - Make sure to provide values for all required fields in the JSON schema.
                          - Use only proper quotation marks for JSON and escape any necessary characters (like quotes within strings). 
                          - Never add any additional text or information outside the JSON format, or comments within the JSON.
                          - Never add any dots or slashes in the measurementType enum values.
                          
                       This is the JSON schema:

                       """ + RecipeJsonSchema;

        ExampleJson = """
                        {
                          "recipeName": "Stoofvlees met frietjes",
                          "description": "Een klassiek Belgisch gerecht van langzaam gegaard rundvlees in een rijke saus, geserveerd met knapperige frietjes.",
                          "ingredients": [
                              {
                                  "name": "rundvlees",
                                  "amount": 1.5,
                                  "measurementType": "Kilogram"
                              },
                              {
                                  "name": "uien",
                                  "amount": 3,
                                  "measurementType": "Piece"
                              },
                              {
                                  "name": "wortelen",
                                  "amount": 2,
                                  "measurementType": "Piece"
                              },
                              {
                                  "name": "bier",
                                  "amount": 500,
                                  "measurementType": "Millilitre"
                              },
                              {
                                  "name": "runderbouillon",
                                  "amount": 500,
                                  "measurementType": "Millilitre"
                              },
                              {
                                  "name": "laurierblaadjes",
                                  "amount": 2,
                                  "measurementType": "Piece"
                              },
                              {
                                  "name": "peterselie",
                                  "amount": 10,
                                  "measurementType": "Gram"
                              },
                              {
                                  "name": "zout",
                                  "amount": 1,
                                  "measurementType": "Teaspoon"
                              },
                              {
                                  "name": "peper",
                                  "amount": 1,
                                  "measurementType": "Teaspoon"
                              }
                          ],
                          "diet": "None",
                          "recipeType": "Dinner",
                          "cookingTime": "180",
                          "difficulty": "Intermediate",
                          "amount_of_people": 4,
                          "recipeSteps": [
                              {
                                  "stepNumber": 1,
                                  "instruction": "Snijd het rundvlees in blokjes en kruid met zout en peper."
                              },
                              {
                                  "stepNumber": 2,
                                  "instruction": "Verhit een grote pan en bak het vlees aan alle kanten bruin."
                              },
                              {
                                  "stepNumber": 3,
                                  "instruction": "Voeg de gesneden uien en wortelen toe en bak deze tot ze zacht zijn."
                              },
                              {
                                  "stepNumber": 4,
                                  "instruction": "Voeg het bier, de runderbouillon en laurierblaadjes toe. Breng aan de kook."
                              },
                              {
                                  "stepNumber": 5,
                                  "instruction": "Zet het vuur laag en laat het stoofvlees minstens twee uur sudderen tot het vlees mals is."
                              },
                              {
                                  "stepNumber": 6,
                                  "instruction": "Serveer met frietjes en garneer met verse peterselie."
                              }
                          ]
                      }

                      """;
    }
}