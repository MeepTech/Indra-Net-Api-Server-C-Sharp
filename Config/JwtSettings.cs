namespace Indra.Api.Configuration {
  public class JwtSettings {

    public string SecurityKey 
      { get; init; }

    public string ValidIssuer 
      { get; init; }

    public string ValidAudience 
      { get; init; }

    public string ExpiryInMinutes 
      { get; init; }

    public string Secret 
      { get; init; }
  }
}
