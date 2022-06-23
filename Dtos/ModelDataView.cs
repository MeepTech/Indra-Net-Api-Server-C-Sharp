using Indra.Data;
using Meep.Tech.Web;
using System;
using System.Collections.Generic;

namespace Indra.Server.Dtos {
  /// <summary>
  /// Placeholder to shape json data that all models have.
  /// </summary>
  /// <seealso cref="Indra.Data.IModel"/>
  public abstract record ModelDataView : View {
    private readonly Type _modelType;

    public string Id { get; }
    //public Place Location { get; }
    public IEnumerable<string> RequiredPermissions { get; }

    public override Type ModelType 
      => _modelType;

    ModelDataView(Indra.Data.IModel model)
      : base(model) { _modelType = model.GetType(); }
  }
}
