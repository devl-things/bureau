using Bureau.Core;
using Bureau.Core.Models;
using Bureau.Core.Repositories;
using Bureau.Models;
using Bureau.Recipes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Bureau.Recipes.Managers.RecipeManager;

namespace Bureau.Recipes.Services
{
    internal class RecipeService : IRecipeService
    {
        private ITermEntryRepository _termEntryRepository;

        public RecipeService(ITermEntryRepository termEntryRepository)
        {
            _termEntryRepository = termEntryRepository;
        }

        public async Task<Result<RecipeAggregate>> GetRecipeAggregateAsync(string id, CancellationToken cancellationToken)
        {
            await _termEntryRepository.GetAsync(new Reference(id), cancellationToken).ConfigureAwait(false);

            RecipeAggregate aggregate = new RecipeAggregate()
            {
                Header = new TermEntry()
                {
                    Id = id,
                    Title = "Fritaja"
                },
                Details = new FlexibleRecord<RecipeDetails>()
                {
                    Type = typeof(RecipeDetails).AssemblyQualifiedName!,
                    Data = new RecipeDetails()
                    {
                        Instructions = "Zmuckaj",
                        PreparationTime = "45 min",
                        Servings = 1,
                    }
                },
                Ingredients = new HashSet<TermEntry>() { new TermEntry()
            {
                Title = "jaja"
            } },
                IngredientsDetails = new HashSet<FlexibleRecord<QuantityDetails>>()
                {
                    new FlexibleRecord<QuantityDetails>()
                    {
                        Type = typeof(QuantityDetails).AssemblyQualifiedName!,
                        Data = new QuantityDetails()
                        {
                            Unit = "lopate"
                        }
                    }
                }
            };
            aggregate.Edges = new HashSet<Edge>()
            {
                new Edge()
                {
                    Active = true,
                    EdgeType = (int)EdgeTypeEnum.Details,
                    SourceNode = aggregate.Header,
                    TargetNode = aggregate.Details
                },
                new Edge()
                {
                    Active = true,
                    EdgeType = (int)EdgeTypeEnum.Items,
                    SourceNode = aggregate.Header,
                    TargetNode = aggregate.Ingredients.First()
                },
            };
            return aggregate;
        }
    }
}
