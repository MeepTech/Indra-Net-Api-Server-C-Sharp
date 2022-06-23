namespace Indra.Server.Dtos {

  /// <summary>
  /// Input for a command being executed on a model
  /// </summary>
  public class ModelCommandInputDto {
    public string UserAuthenticationKey;
    public string ExecutingCharacterId;
    public string ExecutedFromPlaceId;
  }

  public class CommandInputDto<TRequiredInput> : ModelCommandInputDto {
    public TRequiredInput RequiredInput;
  }
}
