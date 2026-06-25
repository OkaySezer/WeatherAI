using Microsoft.SemanticKernel;
using WeatherAI.Plugins;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.Services.AddControllers();

// Swagger — API'yi test etmek için
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var httpFactory = sp.GetRequiredService<IHttpClientFactory>();

    // Kernel'ý oluþturuyoruz ollama baðlýyoruz
#pragma warning disable SKEXP0070
    var kernel = Kernel.CreateBuilder()
        .AddOllamaChatCompletion(
            modelId: "llama3.2",                    //  model
            endpoint: new Uri("http://localhost:11434")) // ollama'nýn adresi
        .Build();
#pragma warning restore SKEXP0070

    // WeatherPlugin'i kernel'a ekle — LLM artýk bu fonksiyonlarý görebilir
    kernel.Plugins.AddFromObject(new WeatherPlugin(httpFactory, config));

    return kernel;
});


var app = builder.Build();

// Swagger sadece geliþtirme ortamýnda açýlsýn
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();  
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
