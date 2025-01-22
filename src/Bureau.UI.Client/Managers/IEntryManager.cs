using Bureau.UI.Client.Models;
namespace Bureau.UI.Client.Managers
{
    public interface IEntryManager
    {
        Task<EntryUI> GetEntryByIdAsync(string id, CancellationToken cancellationToken);
        Task<EntryUI> GetOrAddDraftEntryAsync(CancellationToken cancellationToken);
    }
}
