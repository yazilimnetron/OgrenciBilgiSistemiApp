using StudentTrackingSystem.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// --- EKLE: CORS Politikasýný Tanýmla ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});
// --------------------------------------

builder.Services.AddControllers();
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<ClassService>();
builder.Services.AddScoped<StudentService>();
builder.Services.AddScoped<UnitService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- EKLE: CORS Politikasýný Kullan ---
app.UseCors("AllowAll");
// --------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// DÝKKAT: Test aþamasýnda HTTPS yönlendirmesini geçici olarak yorum satýrý yapabilirsin
// app.UseHttpsRedirection(); 

app.UseAuthorization();
app.MapControllers();
app.Run();