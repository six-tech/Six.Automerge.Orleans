namespace Server.Silo;


[PublicAPI]
[ExcludeFromCodeCoverage]
[GenerateSerializer]
public record DocumentState
{
  /// <summary>
  /// Unique identifier for the user
  /// </summary>
  [Id(0)]
  public Guid Id { get; set; }
}