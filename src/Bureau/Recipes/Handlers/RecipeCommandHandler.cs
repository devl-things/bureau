using Bureau.Core;
using Bureau.Core.Factories;
using Bureau.Core.Models.Data;
using Bureau.Core.Models;
using Bureau.Core.Repositories;
using Bureau.Recipes.Models;
using Bureau.Core.Comparers;
using Bureau.Models;
using Bureau.Recipes.Factories;

namespace Bureau.Recipes.Handlers
{
    internal class RecipeCommandHandler : IRecipeCommandHandler
    {
        private readonly IRecordCommandRepository _repository;
        private readonly ITermRepository _termRepository;
        private readonly IInternalRecipeQueryHandler _queryHandler;

        private Dictionary<string, TermEntry> _termEntriesByLabel;
        private HashSet<string> _termEntryLabels;
        private Dictionary<string, string> _termEntryTitleByLabel;

        private int _tempId = 0;

        private RecipeDto _recipeDto = default!;
        private string _recipeId = default!;
        public RecipeCommandHandler(IRecordCommandRepository repository, 
            ITermRepository termRepository,
            IInternalRecipeQueryHandler queryHandler)
        {
            _repository = repository;
            _termRepository = termRepository;
            _queryHandler = queryHandler;

            _termEntriesByLabel = new Dictionary<string, TermEntry>();
            _termEntryLabels = new HashSet<string>();
            _termEntryTitleByLabel = new Dictionary<string, string>();
        }

        public async Task<Result<IReference>> UpdateRecipeAsync(RecipeDto recipeDto, CancellationToken cancellationToken)
        {
            return await ExecuteCommandRecipeAsync(recipeDto, cancellationToken, InternalUpdateRecipeAsync);
        }

        public async Task<Result<IReference>> InsertRecipeAsync(RecipeDto recipeDto, CancellationToken cancellationToken)
        {
            return await ExecuteCommandRecipeAsync(recipeDto, cancellationToken, InternalInsertRecipeAsync);
        }

        public async Task<Result> DeleteRecipeAsync(string id, CancellationToken cancellationToken)
        {
            if (BureauReferenceFactory.IsTempId(id))
            {
                return RecipeResultErrorFactory.RecipeIdBadFormat(id);
            }

            Result<InsertAggregateModel> existingRecipeResult = await _queryHandler.InternalGetRecipeAggregateAsync(BureauReferenceFactory.CreateReference(id), cancellationToken).ConfigureAwait(false);
            if (existingRecipeResult.IsError)
            {
                return existingRecipeResult.Error;
            }
            RemoveAggregateModel removeRecipe = new RemoveAggregateModel()
            {
                EdgesToDelete = new HashSet<Edge>(existingRecipeResult.Value.Edges.Count, new ReferenceComparer()),
                FlexRecordsToDelete = new HashSet<FlexRecord>(existingRecipeResult.Value.FlexRecords.Count, new ReferenceComparer())
            };
            foreach (Edge edge in existingRecipeResult.Value.Edges)
            {
                removeRecipe.EdgesToDelete.Add(edge);
                if (existingRecipeResult.Value.FlexRecords.TryGetValue(new FlexRecord(edge.Id), out FlexRecord? flexRecord))
                {
                    removeRecipe.FlexRecordsToDelete.Add(flexRecord);
                }
            }

            return await _repository.DeleteAggregateAsync(removeRecipe, cancellationToken);
        }

        private async Task<Result<IReference>> ExecuteCommandRecipeAsync(RecipeDto recipeDto, CancellationToken cancellationToken, Func<CancellationToken, Task<Result<IReference>>> operation)
        {
            _recipeDto = recipeDto;
            _recipeId = _recipeDto.Id;
            if (string.IsNullOrWhiteSpace(_recipeDto.Id))
            {
                _recipeId = GetNewTempId();
            }

            CollectTermEntries();

            Result tempResult = await FetchOrCreateTermEntriesAsync(cancellationToken);
            if (tempResult.IsError)
            {
                return tempResult.Error;
            }

            return await operation(cancellationToken);
        }

        private void CollectTermEntries()
        {
            //add header entry
            AddTermEntryTitle(_recipeDto.Name);
            foreach (RecipeSubGroupDto group in _recipeDto.SubGroups)
            {
                AddTermEntryTitle(group.Name);
                foreach (RecipeIngredient ingredient in group.Ingredients)
                {
                    AddTermEntryTitle(ingredient.Ingredient);
                }
            }
        }
        private async Task<Result> FetchOrCreateTermEntriesAsync(CancellationToken cancellationToken)
        {
            TermSearchRequest termSearchRequest = new TermSearchRequest()
            {
                Terms = _termEntryLabels,
                RequestType = TermRequestType.Label
            };

            Result<Dictionary<string, TermEntry>> termsResult = await _termRepository.FetchTermRecordsAsync(termSearchRequest, cancellationToken);

            if (termsResult.IsError)
            {
                return termsResult.Error;
            }
            foreach (string label in _termEntryLabels)
            {
                if (termsResult.Value.TryGetValue(label, out TermEntry? termEntry))
                {
                    termEntry.UpdatedAt = _recipeDto.UpdatedAt;
                }
                else
                {
                    termEntry = new TermEntry(GetNewTempId(), _termEntryTitleByLabel[label])
                    {
                        CreatedAt = _recipeDto.CreatedAt,
                        UpdatedAt = _recipeDto.UpdatedAt,
                        Status = RecordStatus.Active,
                    };
                }
                _termEntriesByLabel.Add(label, termEntry);
            }
            return true;
        }

        private string GetNewTempId()
        {
            _tempId++;
            return BureauReferenceFactory.CreateTempId(_tempId);
        }

        private bool TryGetTermEntry(string title, out TermEntry termEntry)
        {
            return _termEntriesByLabel.TryGetValue(TermEntry.GetLabel(title), out termEntry!);
        }

        private void AddTermEntryTitle(string title)
        {
            string label = TermEntry.GetLabel(title);
            if (_termEntryLabels.Add(label)) 
            {
                _termEntryTitleByLabel.Add(label, title);
            }
        }

        private async Task<Result<IReference>> InternalUpdateRecipeAsync(CancellationToken cancellationToken)
        {
            if (BureauReferenceFactory.IsTempId(_recipeId))
            {
                return RecipeResultErrorFactory.RecipeIdBadFormat(_recipeId);
            }

            Result<InsertAggregateModel> existingRecipeResult = await _queryHandler.InternalGetRecipeAggregateAsync(BureauReferenceFactory.CreateReference(_recipeId), cancellationToken).ConfigureAwait(false);
            if (existingRecipeResult.IsError)
            {
                return existingRecipeResult.Error;
            }
            Dictionary<string, ChangedEntry<Edge>> existingEdgesBySTTKey = new Dictionary<string, ChangedEntry<Edge>>(existingRecipeResult.Value.Edges.Count);

            Edge recipeEdge = default!;
            foreach (Edge edge in existingRecipeResult.Value.Edges)
            {
                ChangedEntry<Edge> edgeChanged = new ChangedEntry<Edge>(edge);
                if (existingRecipeResult.Value.MainReference.Equals(edge)) 
                {
                    edgeChanged.IsChanged = true;
                    recipeEdge = edge;
                }
                existingEdgesBySTTKey.Add(edge.SourceTypeTargetKey(), edgeChanged);
            }
            //recipe edge should remain the same always even if recipe name changes
            if (recipeEdge == default) 
            {
                return RecipeResultErrorFactory.RecipeNotFound(existingRecipeResult.Value.MainReference.Id);
            }

            if (!TryGetTermEntry(_recipeDto.Name, out TermEntry headerEntry))
            {
                return RecipeResultErrorFactory.UnknownTerm(_recipeDto.Name);
            }
            recipeEdge.SourceNode = BureauReferenceFactory.CreateReference(headerEntry.Id);
            recipeEdge.TargetNode = BureauReferenceFactory.CreateReference(headerEntry.Id);
            recipeEdge.UpdatedAt = _recipeDto.UpdatedAt;


            Result<FlexRecord> flexResult = FlexRecordFactory
                .CreateFlexRecord(new FlexibleRecord<RecipeDetails>(recipeEdge.Id)
                {
                    Data = new RecipeDetails()
                    {
                        PreparationTime = _recipeDto.PreparationTime,
                        Servings = _recipeDto.Servings,
                    },
                    CreatedAt = _recipeDto.CreatedAt,
                    UpdatedAt = _recipeDto.UpdatedAt,
                });
            if (flexResult.IsError)
            {
                return flexResult.Error;
            }
            FlexRecord details = flexResult.Value;

            Dictionary<string, ChangedEntry<FlexRecord>> existingFlexById = existingRecipeResult.Value.FlexRecords
                .ToDictionary(k => k.Id, v => new ChangedEntry<FlexRecord>(v));
            if (existingFlexById.TryGetValue(recipeEdge.Id, out ChangedEntry<FlexRecord>? existingDetails))
            {
                existingDetails!.Entry.UpdatedAt = _recipeDto.UpdatedAt;
                existingDetails!.Entry.DataType = flexResult.Value.DataType;
                existingDetails!.Entry.Data = flexResult.Value.Data;
                existingDetails.IsChanged = true;
                details = existingDetails.Entry;
            }


            UpdateAggregateModel updateRecipe = new UpdateAggregateModel()
            {
                MainReference = recipeEdge,
                TermEntries = new HashSet<TermEntry>(_termEntriesByLabel.Values, new ReferenceComparer()),
                Edges = new HashSet<Edge>([recipeEdge], new ReferenceComparer()),
                FlexRecords = new HashSet<FlexRecord>([details], new ReferenceComparer()),
                EdgesToDelete = new HashSet<Edge>(existingRecipeResult.Value.Edges.Count, new ReferenceComparer()),
                FlexRecordsToDelete = new HashSet<FlexRecord>(existingRecipeResult.Value.FlexRecords.Count, new ReferenceComparer())
            };

            foreach (RecipeSubGroupDto group in _recipeDto.SubGroups)
            {
                if (!TryGetTermEntry(group.Name, out TermEntry groupEntry))
                {
                    return RecipeResultErrorFactory.UnknownTerm(group.Name);
                }

                Edge groupEdge = new Edge(GetNewTempId())
                {
                    RootNode = BureauReferenceFactory.CreateReference(recipeEdge.Id),
                    SourceNode = BureauReferenceFactory.CreateReference(headerEntry.Id),
                    TargetNode = BureauReferenceFactory.CreateReference(groupEntry.Id),
                    ParentNode = BureauReferenceFactory.CreateReference(recipeEdge.Id),
                    CreatedAt = _recipeDto.CreatedAt,
                    UpdatedAt = _recipeDto.UpdatedAt,
                    EdgeType = (int)EdgeTypeEnum.Group,
                };
                if (existingEdgesBySTTKey.TryGetValue(groupEdge.SourceTypeTargetKey(), out ChangedEntry<Edge>? existingGroupEdge))
                {
                    existingGroupEdge!.Entry.UpdatedAt = recipeEdge.UpdatedAt;
                    existingGroupEdge.IsChanged = true;
                    groupEdge = existingGroupEdge.Entry;
                }
                updateRecipe.Edges.Add(groupEdge);

                bool newInstructionsExist = !string.IsNullOrWhiteSpace(group.Instructions);
                bool existingInstructionsExist = existingFlexById.TryGetValue(groupEdge.Id, out ChangedEntry<FlexRecord>? existingInstructions);

                if (newInstructionsExist)
                {
                    flexResult = FlexRecordFactory
                        .CreateFlexRecord(new FlexibleRecord<NoteDetails>(groupEdge.Id)
                        {
                            CreatedAt = _recipeDto.CreatedAt,
                            UpdatedAt = _recipeDto.UpdatedAt,
                            Data = new NoteDetails()
                            {
                                Note = group.Instructions!,
                            },
                        });
                    if (flexResult.IsError)
                    {
                        return flexResult.Error;
                    }
                }

                if (newInstructionsExist && existingInstructionsExist)
                {
                    existingInstructions!.Entry.UpdatedAt = groupEdge.UpdatedAt;
                    existingInstructions!.Entry.DataType = flexResult.Value.DataType;
                    existingInstructions!.Entry.Data = flexResult.Value.Data;
                    existingInstructions.IsChanged = true;
                    updateRecipe.FlexRecords.Add(existingInstructions.Entry);
                }
                else if (newInstructionsExist)
                {
                    updateRecipe.FlexRecords.Add(flexResult.Value);
                }
                else if (existingInstructionsExist)
                {
                    existingInstructions!.IsChanged = true;
                    updateRecipe.FlexRecordsToDelete.Add(existingInstructions!.Entry);
                }

                foreach (RecipeIngredient ingredient in group.Ingredients)
                {
                    if (!TryGetTermEntry(ingredient.Ingredient, out TermEntry ingredientEntry))
                    {
                        return RecipeResultErrorFactory.UnknownTerm(ingredient.Ingredient);
                    }
                    Edge ingredientEdge = new Edge(GetNewTempId())
                    {
                        RootNode = BureauReferenceFactory.CreateReference(recipeEdge.Id),
                        SourceNode = BureauReferenceFactory.CreateReference(groupEntry.Id),
                        TargetNode = BureauReferenceFactory.CreateReference(ingredientEntry.Id),
                        ParentNode = BureauReferenceFactory.CreateReference(groupEdge.Id),
                        CreatedAt = _recipeDto.CreatedAt,
                        UpdatedAt = _recipeDto.UpdatedAt,
                        EdgeType = (int)EdgeTypeEnum.Items,
                    };
                    if (existingEdgesBySTTKey.TryGetValue(ingredientEdge.SourceTypeTargetKey(), out ChangedEntry<Edge>? existingIngredientEdge))
                    {
                        existingIngredientEdge!.Entry.UpdatedAt = recipeEdge.UpdatedAt;
                        existingIngredientEdge.IsChanged = true;
                        ingredientEdge = existingIngredientEdge.Entry;
                    }
                    updateRecipe.Edges.Add(ingredientEdge);

                    bool existingQuantityExist = existingFlexById.TryGetValue(ingredientEdge.Id, out ChangedEntry<FlexRecord>? existingQuantity);

                    if (ingredient.HasQuantity())
                    {
                        flexResult = FlexRecordFactory
                            .CreateFlexRecord(new FlexibleRecord<QuantityDetails>(ingredientEdge.Id)
                            {
                                CreatedAt = _recipeDto.CreatedAt,
                                UpdatedAt = _recipeDto.UpdatedAt,
                                Data = ingredient.Quantity,
                            });
                        if (flexResult.IsError)
                        {
                            return flexResult.Error;
                        }
                    }

                    if (ingredient.HasQuantity() && existingQuantityExist)
                    {
                        existingQuantity!.Entry.UpdatedAt = ingredientEdge.UpdatedAt;
                        existingQuantity!.Entry.DataType = flexResult.Value.DataType;
                        existingQuantity!.Entry.Data = flexResult.Value.Data;
                        existingQuantity.IsChanged = true;
                        updateRecipe.FlexRecords.Add(existingQuantity.Entry);
                    }
                    else if (ingredient.HasQuantity())
                    {
                        updateRecipe.FlexRecords.Add(flexResult.Value);
                    }
                    else if (existingQuantityExist)
                    {
                        existingQuantity!.IsChanged = true;
                        updateRecipe.FlexRecordsToDelete.Add(existingQuantity!.Entry);
                    }
                }
            }

            foreach (ChangedEntry<Edge> item in existingEdgesBySTTKey.Values.Where(x => !x.IsChanged))
            {
                updateRecipe.EdgesToDelete.Add(item.Entry);
            }
            foreach (ChangedEntry<FlexRecord> item in existingFlexById.Values.Where(x => !x.IsChanged))
            {
                updateRecipe.FlexRecordsToDelete.Add(item.Entry);
            }

            return await _repository.UpdateAggregateAsync(updateRecipe, cancellationToken);
        }
        private async Task<Result<IReference>> InternalInsertRecipeAsync(CancellationToken cancellationToken)
        {
            if (!BureauReferenceFactory.IsTempId(_recipeId))
            {
                return RecipeResultErrorFactory.RecipeIdBadFormat(_recipeId);
            }

            if (!TryGetTermEntry(_recipeDto.Name, out TermEntry headerEntry))
            {
                return RecipeResultErrorFactory.UnknownTerm(_recipeDto.Name);
            }

            Edge recipeEdge = new Edge(_recipeId)
            {
                RootNode = BureauReferenceFactory.CreateReference(_recipeId),
                SourceNode = BureauReferenceFactory.CreateReference(headerEntry.Id),
                TargetNode = BureauReferenceFactory.CreateReference(headerEntry.Id),
                CreatedAt = _recipeDto.CreatedAt,
                UpdatedAt = _recipeDto.UpdatedAt,
                EdgeType = (int)EdgeTypeEnum.Recipe,
            };

            Result<FlexRecord> flexResult = FlexRecordFactory
                .CreateFlexRecord(new FlexibleRecord<RecipeDetails>(recipeEdge.Id)
                {
                    Data = new RecipeDetails()
                    {
                        PreparationTime = _recipeDto.PreparationTime,
                        Servings = _recipeDto.Servings,
                    },
                    CreatedAt = _recipeDto.CreatedAt,
                    UpdatedAt = _recipeDto.UpdatedAt,
                });
            if (flexResult.IsError)
            {
                return flexResult.Error;
            }

            InsertAggregateModel newRecipe = new InsertAggregateModel()
            {
                MainReference = recipeEdge,
                TermEntries = new HashSet<TermEntry>(_termEntriesByLabel.Values, new ReferenceComparer()),
                Edges = new HashSet<Edge>([recipeEdge], new ReferenceComparer()),
                FlexRecords = new HashSet<FlexRecord>([flexResult.Value], new ReferenceComparer()),
            };

            foreach (RecipeSubGroupDto group in _recipeDto.SubGroups)
            {
                if (!TryGetTermEntry(group.Name, out TermEntry groupEntry))
                {
                    return RecipeResultErrorFactory.UnknownTerm(group.Name);
                }

                Edge groupEdge = new Edge(GetNewTempId())
                {
                    RootNode = BureauReferenceFactory.CreateReference(recipeEdge.Id),
                    SourceNode = BureauReferenceFactory.CreateReference(headerEntry.Id),
                    TargetNode = BureauReferenceFactory.CreateReference(groupEntry.Id),
                    ParentNode = BureauReferenceFactory.CreateReference(recipeEdge.Id),
                    CreatedAt = _recipeDto.CreatedAt,
                    UpdatedAt = _recipeDto.UpdatedAt,
                    EdgeType = (int)EdgeTypeEnum.Group,
                };
                newRecipe.Edges.Add(groupEdge);

                if (!string.IsNullOrWhiteSpace(group.Instructions))
                {
                    flexResult = FlexRecordFactory
                        .CreateFlexRecord(new FlexibleRecord<NoteDetails>(groupEdge.Id)
                        {
                            CreatedAt = _recipeDto.CreatedAt,
                            UpdatedAt = _recipeDto.UpdatedAt,
                            Data = new NoteDetails()
                            {
                                Note = group.Instructions,
                            },
                        });
                    if (flexResult.IsError)
                    {
                        return flexResult.Error;
                    }
                    newRecipe.FlexRecords.Add(flexResult.Value);
                }

                foreach (RecipeIngredient ingredient in group.Ingredients)
                {
                    if (!TryGetTermEntry(ingredient.Ingredient, out TermEntry ingredientEntry))
                    {
                        return RecipeResultErrorFactory.UnknownTerm(ingredient.Ingredient);
                    }
                    Edge ingredientEdge = new Edge(GetNewTempId())
                    {
                        RootNode = BureauReferenceFactory.CreateReference(recipeEdge.Id),
                        SourceNode = BureauReferenceFactory.CreateReference(groupEntry.Id),
                        TargetNode = BureauReferenceFactory.CreateReference(ingredientEntry.Id),
                        ParentNode = BureauReferenceFactory.CreateReference(groupEdge.Id),
                        CreatedAt = _recipeDto.CreatedAt,
                        UpdatedAt = _recipeDto.UpdatedAt,
                        EdgeType = (int)EdgeTypeEnum.Items,
                    };
                    newRecipe.Edges.Add(ingredientEdge);
                    if (ingredient.HasQuantity()) {
                        flexResult = FlexRecordFactory
                            .CreateFlexRecord(new FlexibleRecord<QuantityDetails>(ingredientEdge.Id)
                            {
                                CreatedAt = _recipeDto.CreatedAt,
                                UpdatedAt = _recipeDto.UpdatedAt,
                                Data = ingredient.Quantity,
                                Status = RecordStatus.Active,
                            });
                        if (flexResult.IsError)
                        {
                            return flexResult.Error;
                        }
                        newRecipe.FlexRecords.Add(flexResult.Value);
                    }
                }
            }

            return await _repository.InsertAggregateAsync(newRecipe, cancellationToken);
        }
    }
}
