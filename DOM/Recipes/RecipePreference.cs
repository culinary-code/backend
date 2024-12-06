using DOM.Accounts;

namespace DOM.Recipes;

public class RecipePreference
{
    public Guid RecipePreferenceId { get; set; }
    public Recipe Recipe { get; set; }
    public Preference Preference { get; set; }
    
    //foreign keys
    public Guid PreferenceId { get; set; }
    public Guid RecipeId { get; set; }

}