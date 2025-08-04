using Microsoft.AspNetCore.Mvc;

namespace SeeSharpBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SimpleRandomController : ControllerBase
    {
        private readonly ILogger<SimpleRandomController> _logger;
        private readonly Random _random;

        public SimpleRandomController(ILogger<SimpleRandomController> logger)
        {
            _logger = logger;
            _random = new Random();
        }

        [HttpGet("single")]
        public IActionResult GenerateRandomNumber([FromQuery] int min = 1, [FromQuery] int max = 100)
        {
            try
            {
                var randomValue = _random.Next(min, max + 1);
                
                var response = new
                {
                    success = true,
                    value = randomValue,
                    min = min,
                    max = max,
                    message = $"生成随机数: {randomValue} (范围: {min}-{max})",
                    generatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                _logger.LogInformation("Generated random number: {Value} (range: {Min}-{Max})", randomValue, min, max);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成随机数失败");
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        [HttpGet("batch")]
        public IActionResult GenerateBatchRandomNumbers([FromQuery] int count = 10, [FromQuery] int min = 1, [FromQuery] int max = 100)
        {
            try
            {
                if (count <= 0 || count > 1000)
                {
                    return BadRequest(new { success = false, error = "数量必须在1-1000之间" });
                }

                var randomValues = new List<int>();
                for (int i = 0; i < count; i++)
                {
                    randomValues.Add(_random.Next(min, max + 1));
                }

                var response = new
                {
                    success = true,
                    values = randomValues,
                    count = count,
                    min = min,
                    max = max,
                    message = $"生成{count}个随机数 (范围: {min}-{max})",
                    generatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    statistics = new
                    {
                        average = randomValues.Average(),
                        minimum = randomValues.Min(),
                        maximum = randomValues.Max()
                    }
                };

                _logger.LogInformation("Generated batch of {Count} random numbers (range: {Min}-{Max})", count, min, max);
                
                return Ok(response);
            }
            catch (Exception ex)  
            {
                _logger.LogError(ex, "生成批量随机数失败");
                return BadRequest(new { success = false, error = ex.Message });
            }
        }
    }
}
