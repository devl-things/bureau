using Bureau;
using Bureau.Primitives.Errors;
using Watson.Nodes.Constants;

namespace Watson.Nodes.Services
{
    /// <summary>
    /// Default handler providing permissive validation and baseline defaults.
    /// </summary>
    public class DefaultNodeKindHandler : INodeKindHandler
    {
        protected static readonly IReadOnlyList<NodeAttributeKey> DefaultSearchKeys =
            new List<NodeAttributeKey>
            {
                new NodeAttributeKey(NodeAttributeKeys.Label)
            };

        protected static readonly IReadOnlyList<NodeAttributeKey> DefaultSummaryKeys =
            new List<NodeAttributeKey>
            {
                new NodeAttributeKey(NodeAttributeKeys.Label),
                new NodeAttributeKey(NodeAttributeKeys.Description)
            };

        public virtual NodeKind Kind
        {
            get { return NodeKind.None; }
        }

        public virtual Result ValidateCreate(CreateNodeCommand command)
        {
            //TODO validate scope and locale and attributes as needed
            return new Result();
        }

        public virtual Result ValidatePatch(Guid nodeId, PatchNodeAttributesCommand command)
        {
            //TODO validate locale and attributes as needed
            return new Result();
        }

        public virtual Task<Result> AfterCreateAsync(Node node, CreateNodeCommand command, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new Result());
        }

        public virtual IReadOnlyList<NodeAttributeKey> GetSearchAttributeKeys()
        {
            return DefaultSearchKeys;
        }

        public virtual IReadOnlyList<NodeAttributeKey> GetSummaryAttributeKeys()
        {
            return DefaultSummaryKeys;
        }

        protected static Result RequireCanonicalKey(CreateNodeCommand command, string nodeKindName)
        {
            if (string.IsNullOrWhiteSpace(command.CanonicalKey))
            {
                return ResultError.From(ProblemCodes.Validation.Required, $"{nameof(command.CanonicalKey)} is required", $"{nameof(command.CanonicalKey)} is required for " + nodeKindName + " nodes.");
            }

            return new Result();
        }
    }
}
