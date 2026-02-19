using Microsoft.Extensions.DependencyInjection;
using Watson.Nodes.Services;

namespace Watson.Nodes.Configurations
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddWatsonNodesCore(this IServiceCollection services)
        {
            services.AddScoped<INodeService, NodeService>();

            services.AddScoped<INodeKindHandler, DefaultNodeKindHandler>();
            services.AddScoped<INodeKindHandler, ItemNodeKindHandler>();
            services.AddScoped<INodeKindHandler, TagNodeKindHandler>();
            services.AddScoped<INodeKindHandler, ProjectNodeKindHandler>();

            services.AddScoped<INodeEdgeService, NodeEdgeService>();
            return services;
        }
    }
}
