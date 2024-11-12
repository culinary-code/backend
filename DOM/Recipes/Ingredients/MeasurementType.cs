namespace DOM.Recipes.Ingredients;

// If you change the values in this enum, you must also update the values in the system prompt in LlmSettingsService.cs
public enum MeasurementType : byte
{
    Kilogram = 0,
    Litre = 1,
    Pound = 2,
    Ounce = 3,
    Teaspoon = 4,
    Tablespoon = 5,
    Piece = 6,
    Millilitre = 7,
    Gram = 8,
    Pinch = 9,
    ToTaste = 10,
    Clove = 11
}