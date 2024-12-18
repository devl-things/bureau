```csharp
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
```

```json
{
    "id": "sdf",
    "createdAt": "0001-01-01T00:00:00",
    "updatedAt": "0001-01-01T00:00:00",
    "name": "Fritaja",
    "ingredients": [
      "jaja"
    ],
    "instructions": "Zmuckaj",
    "preparationTime": "45 min",
    "servings": 1
  },
```