using Bureau.Core.Models.Data;
using Bureau.Core;
using Bureau.Recipes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bureau.Core.Factories;
using Bureau.Core.Models;
using Bureau.Models;
using Bureau.Recipes.Abstractions.Factories;
using Bureau.Core.Comparers;

namespace Bureau.Recipes.Factories
{
    [Obsolete("This class is obsolete. Use RecipeDtoFactory instead.")]
    internal static class AggregateModelFactory
    {
        public static Result<AggregateModel> Create(RecipeAggregate aggregate)
        {
            AggregateModel result = new AggregateModel
            {
                MainReference = aggregate.RecipeReference,
                TermEntries = aggregate.TermEntries,
                Edges = aggregate.Edges
            };
            Result tempResult = SetFlexRecords(aggregate, result);
            if(tempResult.IsError)
            {
                return tempResult.Error;
            }
            return result;
        }

        internal static Result<AggregateModel> Create(RecipeAggregate aggregate, Dictionary<string, TermEntry> existingTerms)
        {
            AggregateModel result = new AggregateModel()
            {
                MainReference = aggregate.RecipeReference,
                TermEntries = new HashSet<TermEntry>(aggregate.TermEntries.Count, new ReferenceComparer()),
                Edges = new HashSet<Edge>(aggregate.Edges.Count, new ReferenceComparer())
            };

            Dictionary<IReference, IReference> termReferenceMap = new Dictionary<IReference, IReference>(aggregate.TermEntries.Count, new ReferenceComparer());
            foreach (TermEntry newTerm in aggregate.TermEntries)
            {
                if (existingTerms.ContainsKey(newTerm.Label))
                {
                    if (result.TermEntries.Add(existingTerms[newTerm.Label]))
                    {
                        existingTerms[newTerm.Label].UpdatedAt = newTerm.UpdatedAt;
                    };
                    termReferenceMap.Add(newTerm, existingTerms[newTerm.Label]);
                }
                else 
                {
                    //TODO [first] this terms are not checked if there are doubles
                    result.TermEntries.Add(newTerm);
                    termReferenceMap.Add(newTerm, newTerm);
                }
            }

            foreach (Edge edge in aggregate.Edges)
            {
                if (!termReferenceMap.TryGetValue(edge.SourceNode, out IReference? uSourceNode))
                {
                    return RecipeResultErrorFactory.UnknownEdgeReference(edge.Id, edge.SourceNode.Id);
                }
                if (!termReferenceMap.TryGetValue(edge.TargetNode, out IReference? uTargetNode))
                {
                    return RecipeResultErrorFactory.UnknownEdgeReference(edge.Id, edge.TargetNode.Id);
                }
                edge.SourceNode = uSourceNode;
                edge.TargetNode = uTargetNode;
                result.Edges.Add(edge);
            }
            Result tempResult = SetFlexRecords(aggregate, result);
            if (tempResult.IsError)
            {
                return tempResult.Error;
            }
            return result;
        }

        private static Result SetFlexRecords(RecipeAggregate aggregate, AggregateModel result) 
        {
            Result<FlexRecord> detailsResult = FlexRecordFactory.CreateFlexRecord(aggregate.Details);
            if (detailsResult.IsError)
            {
                return detailsResult.Error;
            }
            result.FlexRecords = new HashSet<FlexRecord>(aggregate.Instructions.Count + 1, new ReferenceComparer())
            {
                detailsResult.Value
            };

            foreach (FlexibleRecord<NoteDetails> instructions in aggregate.Instructions)
            {
                Result<FlexRecord> insResult = FlexRecordFactory.CreateFlexRecord(instructions);
                if (insResult.IsError)
                {
                    return insResult.Error;
                }
                result.FlexRecords.Add(insResult.Value);
            }
            //TODO ingredient details
            //foreach (FlexibleRecord<QuantityDetails> ingredient in aggregate.IngredientsDetails)
            //{
            //    Result<FlexRecord> ingResult = FlexRecordFactory.CreateFlexRecord(ingredient);
            //    if (ingResult.IsError)
            //    {
            //        return ingResult.Error;
            //    }
            //    insertRequest.FlexRecords.Add(ingResult.Value);
            //}
            return true;
        }

        /// <summary>
        /// existingRecipe.TermEntries not needed
        /// </summary>
        /// <param name="existingRecipe"></param>
        /// <param name="newRecipe"></param>
        /// <returns></returns>
        internal static Result<ExtendedAggregateModel> UpdateExisting(AggregateModel existingRecipe, AggregateModel newRecipe)
        {
            ExtendedAggregateModel result = new ExtendedAggregateModel()
            {
                MainReference = newRecipe.MainReference,
                TermEntries = newRecipe.TermEntries,
                Edges = new HashSet<Edge>(newRecipe.Edges.Count, new ReferenceComparer()),
                FlexRecords = new HashSet<FlexRecord>(newRecipe.FlexRecords.Count + existingRecipe.FlexRecords.Count, new ReferenceComparer()),
                EdgesToDelete = new HashSet<Edge>(existingRecipe.Edges.Count, new ReferenceComparer()),
                FlexRecordsToDelete = new HashSet<FlexRecord>(existingRecipe.FlexRecords.Count, new ReferenceComparer())
            };
            Dictionary<string, ChangedEntry<Edge>> existingEdgesBySTTKey = existingRecipe.Edges
                .ToDictionary(k => k.SourceTypeTargetKey(), v => new ChangedEntry<Edge>(v));
            Dictionary<string, ChangedEntry<FlexRecord>> existingFlexById = existingRecipe.FlexRecords
                .ToDictionary(k => k.Id, v => new ChangedEntry<FlexRecord>(v));

            Dictionary<IReference, IReference> edgeMap = new Dictionary<IReference, IReference>(newRecipe.Edges.Count);
            foreach (Edge edge in newRecipe.Edges)
            {
                bool edgeExists = existingEdgesBySTTKey.TryGetValue(edge.SourceTypeTargetKey(), out ChangedEntry<Edge>? existingEdge);
                if (edgeExists)
                {
                    existingEdge!.Entry.UpdatedAt = edge.UpdatedAt;
                    existingEdge.IsChanged = true;
                    result.Edges.Add(existingEdge.Entry);
                    bool newFlexExist = newRecipe.FlexRecords.TryGetValue(new FlexRecord(edge.Id), out FlexRecord? newFlex);
                    bool existingFlexExist = existingFlexById.TryGetValue(existingEdge.Entry.Id, out ChangedEntry<FlexRecord>? existingFlex);
                    if(newFlexExist && existingFlexExist)
                    {
                        existingEdge!.Entry.UpdatedAt = edge.UpdatedAt;
                        existingFlex!.Entry.DataType = newFlex!.DataType;
                        existingFlex.Entry.Data = newFlex.Data;
                        existingFlex.IsChanged = true;
                        result.FlexRecords.Add(existingFlex.Entry);
                    }
                    else if (newFlexExist)
                    {
                        result.FlexRecords.Add(newFlex!.Clone(existingEdge.Entry.Id));
                    }
                    else if (existingFlexExist)
                    {
                        existingFlex!.IsChanged = true;
                        result.FlexRecordsToDelete.Add(existingFlex!.Entry);
                    }

                    edgeMap.Add(edge, existingEdge.Entry);
                }
                else
                {
                    result.Edges.Add(edge);
                    if (newRecipe.FlexRecords.TryGetValue(new FlexRecord(edge.Id), out FlexRecord? newFlex))
                    {
                        result.FlexRecords.Add(newFlex!);
                    }
                    edgeMap.Add(edge, edge);
                }
            }

            foreach (Edge updatedEdge in result.Edges)
            {
                if (updatedEdge.ParentNode != null && edgeMap.TryGetValue(updatedEdge.ParentNode, out IReference newParentNode)) 
                {
                    updatedEdge.ParentNode = newParentNode;
                }
            }

            if (edgeMap.TryGetValue(result.MainReference, out IReference? newMainReference)) 
            {
                result.MainReference = newMainReference;
            }

            foreach (ChangedEntry<Edge> item in existingEdgesBySTTKey.Values.Where(x => !x.IsChanged))
            {
                result.EdgesToDelete.Add(item.Entry);
            }
            foreach (ChangedEntry<FlexRecord> item in existingFlexById.Values.Where(x => !x.IsChanged))
            {
                result.FlexRecordsToDelete.Add(item.Entry);
            }

            return result;
        }

        internal class ChangedEntry<T> 
        {
            public T Entry { get; set; }
            public bool IsChanged { get; set; } = false;

            public ChangedEntry(T entry)
            {
                Entry = entry;
            }
        }
    }
}
