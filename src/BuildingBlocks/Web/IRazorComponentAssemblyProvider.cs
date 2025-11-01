using System.Reflection;

namespace WebSearchIndexing.BuildingBlocks.Web;

public interface IRazorComponentAssemblyProvider
{
    Assembly Assembly { get; }
}
