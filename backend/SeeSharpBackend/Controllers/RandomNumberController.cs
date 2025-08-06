using Microsoft.AspNetCore.Mvc;

namespace SeeSharpBackend.Controllers;

[ApiController]
[Route("api/Random")]
public class RandomNumberController : ControllerBase
{
    private readonly ILogger<RandomNumberController> _logger;
    private readonly Random _random;

    public RandomNumberController(ILogger<RandomNumberController> logger)
    {
        _logger = logger;
        _random = new Random();
    }

    /// <summary>
    /// 生成随机数
    /// </summary>
    /// <param name="min">最小值（默认1）</param>
    /// <param name="max">最大值（默认100）</param>
    /// <returns>随机数结果</returns>
    [HttpGet("single")]
    public ActionResult<RandomNumberResponse> GenerateRandomNumber([FromQuery] int min = 1, [FromQuery] int max = 100)
    {
        try
        {
            if (min >= max)
            {
                return BadRequest(new { message = "最小值必须小于最大值" });
            }

            var randomNumber = _random.Next(min, max + 1);
            var response = new RandomNumberResponse
            {
                Value = randomNumber,
                Min = min,
                Max = max,
                GeneratedAt = DateTime.Now,
                Message = $"生成随机数：{randomNumber}"
            };

            _logger.LogInformation("Generated random number: {RandomNumber} (range: {Min}-{Max})", 
                randomNumber, min, max);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating random number");
            return StatusCode(500, new { message = "生成随机数时发生错误" });
        }
    }

    /// <summary>
    /// 批量生成随机数
    /// </summary>
    /// <param name="count">数量（默认5）</param>
    /// <param name="min">最小值（默认1）</param>
    /// <param name="max">最大值（默认100）</param>
    /// <returns>随机数数组</returns>
    [HttpGet("batch")]
    public ActionResult<BatchRandomNumberResponse> GenerateBatchRandomNumbers(
        [FromQuery] int count = 5,
        [FromQuery] int min = 1, 
        [FromQuery] int max = 100)
    {
        try
        {
            if (min >= max)
            {
                return BadRequest(new { message = "最小值必须小于最大值" });
            }

            if (count <= 0 || count > 1000)
            {
                return BadRequest(new { message = "数量必须在1-1000之间" });
            }

            var randomNumbers = new List<int>();
            for (int i = 0; i < count; i++)
            {
                randomNumbers.Add(_random.Next(min, max + 1));
            }

            var response = new BatchRandomNumberResponse
            {
                Values = randomNumbers,
                Count = count,
                Min = min,
                Max = max,
                GeneratedAt = DateTime.Now,
                Average = randomNumbers.Average(),
                Message = $"批量生成{count}个随机数"
            };

            _logger.LogInformation("Generated {Count} random numbers (range: {Min}-{Max})", 
                count, min, max);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating batch random numbers");
            return StatusCode(500, new { message = "批量生成随机数时发生错误" });
        }
    }
}

public class RandomNumberResponse
{
    public int Value { get; set; }
    public int Min { get; set; }
    public int Max { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class BatchRandomNumberResponse
{
    public List<int> Values { get; set; } = new();
    public int Count { get; set; }
    public int Min { get; set; }
    public int Max { get; set; }
    public DateTime GeneratedAt { get; set; }
    public double Average { get; set; }
    public string Message { get; set; } = string.Empty;
}
