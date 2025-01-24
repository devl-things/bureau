using Bureau.Calendar.Models;
using Bureau.Core;
using Bureau.Core.Comparers;
using Bureau.Core.Factories;
using Bureau.Core.Models;
using Bureau.Core.Models.Data;
using Bureau.Factories;
using Bureau.Models;
using Bureau.Recipes.Models;

namespace Bureau.Recipes.Factories
{
    internal static class RecipeDtoFactoryExtension
    {
        internal static Result SetDetails(this RecipeDto dto, QueryAggregateModel aggregate, Edge edge)
        {
            dto.Id = edge.Id;
            if (!aggregate.TermEntries.TryGetValue(new TermEntry(edge.SourceNode.Id), out TermEntry? header))
            {
                return ResultErrorFactory.TermNotFound(edge.SourceNode.Id, $"{nameof(RecipeDto)}.{nameof(RecipeDto.Name)}");
            }
            dto.Name = header.Title;
            dto.CreatedAt = edge.CreatedAt;
            dto.UpdatedAt = edge.UpdatedAt;

            if (!aggregate.FlexRecords.TryGetValue(new FlexRecord(edge.Id), out FlexRecord? recipeFlex))
            {
                return ResultErrorFactory.InvalidRecord(edge.Id);
            }
            Result<FlexibleRecord<RecipeDetails>> detailsResult = FlexibleRecordFactory.CreateFlexibleRecord<RecipeDetails>(recipeFlex);
            if (detailsResult.IsError)
            {
                return detailsResult.Error;
            }
            RecipeDetails details = detailsResult.Value.Data;

            dto.PreparationTime = details.PreparationTime;
            dto.Servings = details.Servings;

            return true;
        }
    }

    internal class RecipeDtoFactory : IDtoFactory<RecipeDto>
    {
        public RecipeDtoFactory()
        {
        }

        public Result<RecipeDto> Create(InsertAggregateModel aggregate)
        {
            RecipeDto currentRecipe = RecipeDto.EmptyRecipe();
            HashSet<RecipeSubGroupDto> currentGroups = new HashSet<RecipeSubGroupDto>(new ReferenceComparer());
            foreach (Edge edge in aggregate.Edges)
            {
                Result tempResult = HandleEdge(aggregate, edge, currentRecipe, currentGroups);
                if (tempResult.IsError)
                {
                    return tempResult.Error;
                }
            }
            return currentRecipe;
        }

        public PaginatedResult<List<RecipeDto>> CreatePaged(QueryAggregateModel aggregate)
        {
            Dictionary<string, RecipeDto> recipes = new Dictionary<string, RecipeDto>();
            List<RecipeDto> result = new List<RecipeDto>();
            Dictionary<string, HashSet<RecipeSubGroupDto>> recipeGroups = new Dictionary<string, HashSet<RecipeSubGroupDto>>();
            foreach (Edge edge in aggregate.Edges)
            {
                RecipeDto currentRecipe = GetOrAddRecipe(ref recipes, edge.RootNode.Id, ref result);
                HashSet<RecipeSubGroupDto> currentGroups = GetOrAddRecipeGroups(ref recipeGroups, edge.RootNode.Id);
                Result tempResult = HandleEdge(aggregate, edge, currentRecipe, currentGroups);
                if (tempResult.IsError)
                {
                    return tempResult.Error;
                }
            }
            return new PaginatedResult<List<RecipeDto>>(result, aggregate.Pagination);
        }

        private static Result HandleEdge(QueryAggregateModel aggregate, Edge edge, RecipeDto currentRecipe, HashSet<RecipeSubGroupDto> currentGroups)
        {
            switch (edge.EdgeType)
            {
                case (int)EdgeTypeEnum.Recipe:
                    return currentRecipe.SetDetails(aggregate, edge);
                case (int)EdgeTypeEnum.Group:
                    // do we have a group already
                    if (!currentGroups.Contains(new RecipeSubGroupDto(edge.Id)))
                    {
                        Result<RecipeSubGroupDto> groupResult = CreateGroup(aggregate, edge.Id, edge.TargetNode.Id);
                        if (groupResult.IsError)
                        {
                            return groupResult.Error;
                        }
                        currentGroups.Add(groupResult.Value);
                        currentRecipe.SubGroups.Add(groupResult.Value);
                    }
                    return true;
                case (int)EdgeTypeEnum.Items:
                    if (edge.ParentNode == null)
                    {
                        return ResultErrorFactory.ParentNodeNotFound(edge.Id);
                    }
                    if (!currentGroups.TryGetValue(new RecipeSubGroupDto(edge.ParentNode!.Id), out RecipeSubGroupDto? group))
                    {
                        Result<RecipeSubGroupDto> groupResult = CreateGroup(aggregate, edge.ParentNode!.Id, edge.SourceNode.Id);
                        if (groupResult.IsError)
                        {
                            return groupResult.Error;
                        }
                        group = groupResult.Value;
                        currentGroups.Add(group);
                        currentRecipe.SubGroups.Add(group);
                    }
                    if (!aggregate.TermEntries.TryGetValue(new TermEntry(edge.TargetNode.Id), out TermEntry? ingredient))
                    {
                        return ResultErrorFactory.TermNotFound(edge.TargetNode.Id, nameof(ingredient));
                    }
                    QuantityDetails quantityDetails = GetQuantityDetails(aggregate, edge.Id);
                    group.Ingredients.Add(new RecipeIngredient(ingredient.Title) { Quantity = quantityDetails });
                    return true;
                default:
                    return ResultErrorFactory.UnknownEdgeType(edge.Id, edge.EdgeType, nameof(Recipes));
            }
        }

        private static HashSet<RecipeSubGroupDto> GetOrAddRecipeGroups(ref Dictionary<string, HashSet<RecipeSubGroupDto>> recipeGroups, string id)
        {
            if (!recipeGroups.TryGetValue(id, out HashSet<RecipeSubGroupDto>? groups))
            {
                groups = new HashSet<RecipeSubGroupDto>(new ReferenceComparer());
                recipeGroups.Add(id, groups);
            }
            return groups;
        }

        private static RecipeDto GetOrAddRecipe(ref Dictionary<string, RecipeDto> recipes, string id, ref List<RecipeDto> result)
        {
            if (!recipes.TryGetValue(id, out RecipeDto? recipe))
            {
                recipe = RecipeDto.EmptyRecipe();
                recipes.Add(id, recipe);
                result.Add(recipe);
            }
            return recipe;
        }

        private static QuantityDetails GetQuantityDetails(QueryAggregateModel aggregate, string edgeId)
        {
            if (aggregate.FlexRecords.TryGetValue(new FlexRecord(edgeId), out FlexRecord? quantityFlex))
            {
                Result<FlexibleRecord<QuantityDetails>> quantityResult = FlexibleRecordFactory.CreateFlexibleRecord<QuantityDetails>(quantityFlex);
                if (quantityResult.IsSuccess)
                {
                    return quantityResult.Value.Data;
                }
                //TODO log info if IsError
            }
            return default;
        }
        private static Result<RecipeSubGroupDto> CreateGroup(QueryAggregateModel aggregate, string groupEdgeId, string groupTermId)
        {
            if (!aggregate.TermEntries.TryGetValue(new TermEntry(groupTermId), out TermEntry? groupTerm))
            {
                return ResultErrorFactory.TermNotFound(groupTermId, $"{nameof(RecipeDto)}.{nameof(RecipeDto.SubGroups)}");
            }
            RecipeSubGroupDto group = new RecipeSubGroupDto(groupEdgeId)
            {
                Name = groupTerm.Title,
                Ingredients = new List<RecipeIngredient>(),
            };

            if (aggregate.FlexRecords.TryGetValue(new FlexRecord(groupEdgeId), out FlexRecord? instructionFlex))
            {
                Result<FlexibleRecord<NoteDetails>> instructionResult = FlexibleRecordFactory.CreateFlexibleRecord<NoteDetails>(instructionFlex);
                if (instructionResult.IsError)
                {
                    return instructionResult.Error;
                }
                group.Instructions = instructionResult.Value.Data.Note;
            }
            return group;
        }
    }
}
