using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ML;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PredictController : ControllerBase
    {
        private readonly PredictionEnginePool<ModelInput, ModelOutput> predictionEngine;
        public PredictController(PredictionEnginePool<ModelInput, ModelOutput> _predictionEngine) {

            predictionEngine = _predictionEngine;
        }

        [HttpPost]
        public IActionResult PredictVal([FromBody] ModelInput input)
        {
            ModelOutput result = predictionEngine.Predict(modelName: "SalaryModel", input);

            return Ok(new { YearsOfExperience = input.YearsExperience, PrecitedSalary = result.Salary });

        }
     
    }
}
