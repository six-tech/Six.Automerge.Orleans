using Orleans.Runtime;

namespace Server.Silo;

[PublicAPI]
public sealed class DocumentGrain : Grain, IDocumentGrain
{
  readonly IPersistentState<DocumentState> _userState;
  readonly ILogger<DocumentGrain> _logger;

  public DocumentGrain(
    [PersistentState("document-state", "document-grains")]
    IPersistentState<DocumentState> userState,
    ILogger<DocumentGrain> logger)
  {
    _userState = userState;
    _logger = logger;
  }
  
}