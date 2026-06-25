using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;

namespace WeatherAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    // Semantic Kernel'ı tutacak değişken
    private readonly Kernel _kernel;

    // Constructor
    public WeatherController(Kernel kernel) => _kernel = kernel;

    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] AskRequest request)
    {
        var settings = new PromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        // prompt
        var prompt = $"""
        Sen yardımcı bir hava durumu asistanısın. Türkçe cevap ver.
    Cevabını sadece düz Türkçe metin olarak yaz.
    Kesinlikle JSON, kod bloğu veya kaçış karakteri (\n, \t gibi) kullanma.
    Önce şu anki hava durumunu söyle, sonra saatlik tahminleri listele.
    Kullanıcı sorusu: {request.Question}
    """;

        // Kernel'a promptu gönder, cevabı bekle
        var result = await _kernel.InvokePromptAsync(prompt, new KernelArguments(settings));

        // Cevabı JSON olarak döndür
        return Ok(new { answer = result.GetValue<string>() });
    }
}

// Kullanıcıdan gelecek JSON'ın modeli
public record AskRequest(string Question);