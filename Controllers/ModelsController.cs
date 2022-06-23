using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Meep.Tech.Data;
using Newtonsoft.Json.Linq;
using Indra.Data;
using Meep.Tech.Data.Utility;
using System.Linq;
using Meep.Tech.Collections.Generic;
using Meep.Tech.Web;
using Microsoft.AspNetCore.Http;
using Meep.Tech.Web.ViewFilters;
using Indra.Server.Dtos;

namespace Indra.Server.Controllers {

  [Route("models")]
  [ApiController]
  public class ModelsController : ControllerBase {

    public ModelsController() {}

    // GET: api/models/world
    [HttpGet("{modelType}")]
    public async Task<ActionResult<Dto>> GetAll(string modelType)
      => throw new NotImplementedException();

    // GET: api/models/world/5
    [HttpPost("{modelType}/{id}")]
    [ProducesResponseType(typeof(IDictionary<string, SuccessDto<ModelDataView>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(FailureDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(FailureDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Dto>> Get(string modelType, string id, [FromBody] ModelCommandInputDto commandExecutionData) {
      if (_checkIsLoggedIn(commandExecutionData.UserAuthenticationKey, out User currentUser)) {
          var getCommandType 
          = Archetypes.All.Get("Get_" + modelType.ToLower().Trim()) 
          as ICommandType;

        var model = getCommandType
          .Make()
          .ThenReturn(c => {
            c.Execute(
              id,
              commandExecutionData.ExecutingCharacterId,
              commandExecutionData.ExecutedFromPlaceId
            );

            return (c as ICommand.IReturn).Return as Indra.Data.IModel;
          });

        if (model == null) {
          return NotFound(new FailureDto { Message = $"Model of Type: {modelType}, with Id: {id}, Not Found!" });
        }

        View returnData;
        // characters get special treatment:
        if (model.GetType() == typeof(PlayerCharacter)) {
          ViewFilter<User> viewFilter = (model as PlayerCharacter).GetComponent<ViewFilter<User>>();
          if (viewFilter.IsVisibleFor(currentUser)) {
            returnData = viewFilter
              .GetViewFor(currentUser);
          } else return NotFound(new FailureDto { Message = $"Model of Type: {modelType}, with Id: {id}, Not Found!" });
        } else {
          returnData = new DefaultJsonView<Indra.Data.IModel>(model);  
        }

        return model != null 
          ? Ok(new SuccessDto<View> { Result = returnData }) 
          : NotFound(new FailureDto { Message = $"Model of Type: {modelType}, with Id: {id}, Not Found!" });
      } else return Unauthorized(new FailureDto { Message = "You cannot create models if you aren't logged in to the server!" });
    }

    public record CreateModelData(string ArchetypeKey = null, Dictionary<string, object> BuilderParameters = null);

    // POST: api/models/world
    [HttpPost("{modelType}")]
    [ProducesResponseType(typeof(SuccessDto<ModelDataView>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(FailureDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(FailureDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Dto>> Add(string modelType, [FromBody] CommandInputDto<CreateModelData> modelBuilderParameters) {
      if (_checkIsLoggedIn(modelBuilderParameters.UserAuthenticationKey, out User currentUser)) {
        Indra.Data.IModel @return;

        // characters get special treatment:
        if (modelType.ToLower() == nameof(PlayerCharacter).ToLower()) {
          @return = Models<PlayerCharacter>.Factory.Make(modelBuilderParameters.RequiredInput.BuilderParameters.Prepend(
            new(nameof(PlayerCharacter.Creator), currentUser))
          );
        }
        else {
          var createCommandType
            = Archetypes.All.Get("Create_" + modelType.ToLower().Trim())
            as ICommandType;

          @return = createCommandType
            .Make()
            .ThenReturn(c => {
              c.Execute(
                null,
                modelBuilderParameters.ExecutingCharacterId,
                modelBuilderParameters.ExecutedFromPlaceId,
                new Parameter(
                  createCommandType.Parameters[0],
                  Archetypes.All.Get(modelBuilderParameters.RequiredInput.ArchetypeKey),
                  true
                ).AsSingleItemEnumerable()
                .Concat(
                modelBuilderParameters.RequiredInput.BuilderParameters.Select(
                  p => new Parameter(new(p.Key, "Builder Parameter"), p.Value, true)
                )).ToList()
              );

              return (c as ICommand.IReturn).Return as Indra.Data.IModel;
            });
        }

        return @return != null
          ? Ok(new SuccessDto<View> {
            Message = "Model Created",
            Result = new DefaultJsonView<Indra.Data.IModel>(@return)
          }) : StatusCode(StatusCodes.Status500InternalServerError, new FailureDto { Message = $"Unable to create model of type: {modelType}" });
      } else if (modelType.ToLower() == nameof(Indra.Data.User).ToLower()) {
        return BadRequest(new FailureDto { Message = "You cannot create models of type User using the '/models/' controller. Use the '/login/' controller's '/new' action instead!" });
      } else return Unauthorized(new FailureDto { Message = "You cannot create models if you aren't logged in to the server!" });
    }

    // PUT: api/models/worlds/5
    [HttpPut("{modelType}/{id}")]
    [ProducesResponseType(typeof(SuccessDto<ModelDataView>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(FailureDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(FailureDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Replace(string modelType, string id, [FromBody] JObject newModelData)
      => throw new NotImplementedException();


    // PATCH: api/models/worlds/5
    [HttpPatch("{modelType}/{id}")]
    [ProducesResponseType(typeof(SuccessDto<ModelDataView>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(FailureDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(FailureDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Patch(string modelType, string id, [FromBody] JObject fieldsToUpdate)
      => throw new NotImplementedException();


    // DELETE: api/models/worlds/5
    [HttpDelete("{modelType}/{id}")]
    [ProducesResponseType(typeof(SuccessDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(FailureDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(FailureDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string modelType, string id)
      => throw new NotImplementedException();

    // POST: api/models/world/parse
    [HttpPost("{modelType}/parse")]
    [ProducesResponseType(typeof(SuccessDto<ModelDataView>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(FailureDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(FailureDto), StatusCodes.Status404NotFound)]
    public ActionResult<Dto> Parse(string modelType, [FromBody] JObject creationData)
      => throw new NotImplementedException();

    bool _checkIsLoggedIn(string userAuthenticationKey, out User currentUser) {
      throw new NotImplementedException();
    }
  }
}
