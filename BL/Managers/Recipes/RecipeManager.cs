using System;
using System.Collections.Generic;
using AutoMapper;
using BL.DTOs.Recipes;
using DAL.Recipes;
using DOM.Recipes;

namespace BL.Managers.Recipes;

public class RecipeManager : IRecipeManager
{
    private readonly IRecipeRepository _repository;
    private readonly IMapper _mapper;

    public RecipeManager(IRecipeRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public RecipeDto GetRecipeDtoById(string id)
    {
        Guid parsedGuid = Guid.Parse(id);
        var recipe = _repository.ReadRecipeById(parsedGuid);
        return _mapper.Map<RecipeDto>(recipe);
    }

    public RecipeDto GetRecipeDtoByName(string name)
    {
        var recipe = _repository.ReadRecipeByName(name);
        return _mapper.Map<RecipeDto>(recipe);
    }
    
    public ICollection<RecipeDto> GetRecipesCollectionByName(string name)
    {
        var recipes = _repository.ReadRecipesCollectionByName(name);
        return _mapper.Map<ICollection<RecipeDto>>(recipes);
    }
}