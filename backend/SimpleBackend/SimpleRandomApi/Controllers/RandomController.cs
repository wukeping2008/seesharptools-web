using Microsoft.AspNetCore.Mvc;

namespace SimpleRandomApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RandomController : ControllerBase
    {
        private readonly ILogger<RandomController> _logger;
        private readonly Random _random;

        public RandomController(ILogger<RandomController> logger)
        {
            _logger = logger;
            _random = new Random();
        }

        [HttpGet("single")]
        public IActionResult GetSingle([FromQuery] int min = 1, [FromQuery] int max = 100)
        {
            try
            {
                var value = _random.Next(min, max + 1);
                return Ok(new
                {
                    success = true,
                    value = value,
                    min = min,
                    max = max,
                    message = $"生成随机数: {value} (范围: {min}-{max})",
                    generatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        [HttpGet("batch")]
        public IActionResult GetBatch([FromQuery] int count = 10, [FromQuery] int min = 1, [FromQuery] int max = 100)
        {
            try
            {
                if (count <= 0 || count > 1000)
                {
                    return BadRequest(new { success = false, error = "数量必须在1-1000之间" });
                }

                var values = new List<int>();
                for (int i = 0; i < count; i++)
                {
                    values.Add(_random.Next(min, max + 1));
                }

                return Ok(new
                {
                    success = true,
                    values = values,
                    count = count,
                    min = min,
                    max = max,
                    message = $"生成{count}个随机数 (范围: {min}-{max})",
                    generatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    statistics = new
                    {
                        average = values.Average(),
                        minimum = values.Min(),
                        maximum = values.Max()
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }
    }
}
