using genial_dotnet_crm.Data;
using genial_dotnet_crm.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure MongoDB
var mongoDbSettings = builder.Configuration.GetSection("MongoDb").Get<MongoDbSettings>();
if (mongoDbSettings == null)
{
    throw new InvalidOperationException("MongoDB settings not found in configuration.");
}
builder.Services.AddSingleton(mongoDbSettings);
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICollectionService, CollectionService>();
builder.Services.AddScoped<IRecordService, RecordService>();
builder.Services.AddScoped<IFieldTypeService, FieldTypeService>();
builder.Services.AddScoped<IStageService, StageService>();

// Configure Sessions
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Collections}/{action=Index}/{id?}")
    .WithStaticAssets();

// Seed default field types
using (var scope = app.Services.CreateScope())
{
    var fieldTypeService = scope.ServiceProvider.GetRequiredService<IFieldTypeService>();
    await fieldTypeService.SeedDefaultFieldTypesAsync();
    
    var stageService = scope.ServiceProvider.GetRequiredService<IStageService>();
    await stageService.SeedDefaultStagesAsync();
}

app.Run();
