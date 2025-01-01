using Bureau.Core.Factories;
using Bureau.Core.Models.Data;
using Bureau.Core.Models;
using Bureau.Core;
using Bureau.Models;
using Bureau.Recipes.Abstractions.Factories;
using Bureau.Recipes.Models;
using Bureau.Core.Comparers;

namespace Bureau.Recipes.Factories
{
    [Obsolete("This class is obsolete. Use RecipeDtoFactory instead.")]
    internal static class RecipeAggregateFactory
    {
        public static Result<RecipeAggregate> Create(RecipeDto recipeModel) 
        {
            int id = 0;
            TermEntry headerEntry = new TermEntry(BureauReferenceFactory.CreateTempId(id), recipeModel.Name)
            {
                CreatedAt = recipeModel.CreatedAt,
                UpdatedAt = recipeModel.UpdatedAt,
                Status = RecordStatus.Active,
            };
            string recipeId = recipeModel.Id;
            if (string.IsNullOrWhiteSpace(recipeModel.Id)) 
            {
                id++;
                recipeId = BureauReferenceFactory.CreateTempId(id);
            }
            Edge recipeEdge = new Edge(recipeId)
            {
                RootNode = BureauReferenceFactory.CreateReference(recipeId),
                SourceNode = BureauReferenceFactory.CreateReference(headerEntry.Id),
                TargetNode = BureauReferenceFactory.CreateReference(headerEntry.Id),
                CreatedAt = recipeModel.CreatedAt,
                UpdatedAt = recipeModel.UpdatedAt,
                Status = RecordStatus.Active,
                EdgeType = (int)EdgeTypeEnum.Recipe,
                Active = true,
            };

            RecipeAggregate recipeAggregate = new RecipeAggregate(recipeEdge);

            recipeAggregate.Details = new FlexibleRecord<RecipeDetails>(recipeEdge.Id)
            {
                Data = new RecipeDetails()
                {
                    PreparationTime = recipeModel.PreparationTime,
                    Servings = recipeModel.Servings,
                },
                CreatedAt = recipeModel.CreatedAt,
                UpdatedAt = recipeModel.UpdatedAt,
                Status = RecordStatus.Active,
            };

            recipeAggregate.Edges = new HashSet<Edge>([recipeEdge], new ReferenceComparer());
            recipeAggregate.TermEntries = new HashSet<TermEntry>(recipeModel.SubGroups.Count * 3 + 1, new ReferenceComparer())
            {
                headerEntry
            };

            recipeAggregate.Instructions = new HashSet<FlexibleRecord<NoteDetails>>(recipeModel.SubGroups.Count, new ReferenceComparer());
            //TODO ingredients details
            //recipeAggregate.IngredientsDetails = new HashSet<FlexibleRecord<QuantityDetails>>(recipeAggregate.Ingredients.Count, new ReferenceComparer());
            foreach (RecipeSubGroupDto group in recipeModel.SubGroups)
            {
                id++;
                TermEntry groupEntry = new TermEntry(BureauReferenceFactory.CreateTempId(id), group.Name)
                {
                    CreatedAt = recipeModel.CreatedAt,
                    UpdatedAt = recipeModel.UpdatedAt,
                    Status = RecordStatus.Active,
                };
                recipeAggregate.TermEntries.Add(groupEntry);
                id++;
                Edge groupEdge = new Edge(BureauReferenceFactory.CreateTempId(id))
                {
                    RootNode = BureauReferenceFactory.CreateReference(recipeEdge.Id),
                    SourceNode = BureauReferenceFactory.CreateReference(headerEntry.Id),
                    TargetNode = BureauReferenceFactory.CreateReference(groupEntry.Id),
                    ParentNode = BureauReferenceFactory.CreateReference(recipeEdge.Id),
                    CreatedAt = recipeModel.CreatedAt,
                    UpdatedAt = recipeModel.UpdatedAt,
                    Status = RecordStatus.Active,
                    EdgeType = (int)EdgeTypeEnum.Group,
                    Active = true,
                };
                if (!string.IsNullOrWhiteSpace(group.Instructions))
                {
                    recipeAggregate.Instructions.Add(new FlexibleRecord<NoteDetails>(groupEdge.Id)
                    {
                        CreatedAt = recipeModel.CreatedAt,
                        UpdatedAt = recipeModel.UpdatedAt,
                        Data = new NoteDetails()
                        {
                            Note = group.Instructions,
                        },
                        Status = RecordStatus.Active,
                    });
                }
                recipeAggregate.Edges.Add(groupEdge);
                foreach (string ingredient in group.Ingredients)
                {
                    id++;
                    TermEntry ingredientEntry = new TermEntry(BureauReferenceFactory.CreateTempId(id), ingredient)
                    {
                        CreatedAt = recipeModel.CreatedAt,
                        UpdatedAt = recipeModel.UpdatedAt,
                        Status = RecordStatus.Active,
                    };
                    id++;
                    Edge ingredientEdge = new Edge(BureauReferenceFactory.CreateTempId(id))
                    {
                        RootNode = BureauReferenceFactory.CreateReference(recipeEdge.Id),
                        SourceNode = BureauReferenceFactory.CreateReference(groupEntry.Id),
                        TargetNode = BureauReferenceFactory.CreateReference(ingredientEntry.Id),
                        ParentNode = BureauReferenceFactory.CreateReference(groupEdge.Id),
                        CreatedAt = recipeModel.CreatedAt,
                        UpdatedAt = recipeModel.UpdatedAt,
                        Status = RecordStatus.Active,
                        EdgeType = (int)EdgeTypeEnum.Items,
                        Active = true,
                    };
                    recipeAggregate.TermEntries.Add(ingredientEntry);
                    recipeAggregate.Edges.Add(ingredientEdge);
                    //TODO ingredients details
                    //recipeAggregate.IngredientsDetails.Add(new FlexibleRecord<QuantityDetails>()
                    //{
                    //    Id = ingredientEdge.Id,
                    //    CreatedAt = recipeModel.CreatedAt,
                    //    UpdatedAt = recipeModel.UpdatedAt,
                    //    Data = new QuantityDetails()
                    //    {
                    //        Quantity = 1,
                    //        Unit = "unit",
                    //    },
                    //    Status = RecordStatus.Active,
                    //});
                }
            }
            return recipeAggregate;
        }
        
        public static Result<RecipeAggregate> Create(AggregateModel dbAggregate)
        {
            RecipeAggregate result = new RecipeAggregate(dbAggregate.MainReference);
            result.Edges = dbAggregate.Edges;
            result.TermEntries = dbAggregate.TermEntries;

            foreach (Edge edge in dbAggregate.Edges)
            {
                Result tempResult = default;
                switch (edge.EdgeType)
                {
                    case (int)EdgeTypeEnum.Recipe:
                        tempResult = result.SetDetails(dbAggregate, edge);
                        break;
                    case (int)EdgeTypeEnum.Group:
                        tempResult = result.SetInstructions(dbAggregate, edge);
                        break;
                    case (int)EdgeTypeEnum.Items:
                        tempResult = true;
                        //TODO ingredient details
                        //result = result.SetIngredientDetails(dbAggregate, edge);
                        break;
                    default:
                        tempResult = RecipeResultErrorFactory.UnknownEdgeType(edge.Id, edge.EdgeType);
                        break;
                }
                if (tempResult.IsError)
                {
                    return tempResult.Error;
                }
            }
            return result;
        }

        //TODO ingredient details
        internal static Result SetIngredientDetails(this RecipeAggregate result, AggregateModel dbAggregate, IReference edge)
        {
            return new NotImplementedException();
        }

        internal static Result SetInstructions(this RecipeAggregate result, AggregateModel dbAggregate, IReference edge)
        {
            if (!dbAggregate.FlexRecords.TryGetValue(new FlexRecord(edge.Id), out FlexRecord? groupFlex))
            {
                return RecipeResultErrorFactory.InstructionsNotFound(edge.Id);
            }
            Result<FlexibleRecord<NoteDetails>> groupResult = FlexibleRecordFactory.CreateFlexibleRecord<NoteDetails>(groupFlex);
            if (groupResult.IsError)
            {
                return groupResult.Error;
            }
            result.Instructions.Add(groupResult.Value);
            return true;
        }

        internal static Result SetDetails(this RecipeAggregate result, AggregateModel dbAggregate, IReference edge) 
        {
            if (!dbAggregate.FlexRecords.TryGetValue(new FlexRecord(edge.Id), out FlexRecord? recipeFlex))
            {
                return RecipeResultErrorFactory.RecipeNotFound(edge.Id);
            }
            Result<FlexibleRecord<RecipeDetails>> detailsResult = FlexibleRecordFactory.CreateFlexibleRecord<RecipeDetails>(recipeFlex);
            if (detailsResult.IsError)
            {
                return detailsResult.Error;
            }
            result.Details = detailsResult.Value;
            return true;
        }
    }
}
