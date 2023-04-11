var builder = WebApplication.CreateBuilder();

builder.Services.AddSystemWebAdapters()
    .WrapAspNetCoreSession()
    .AddSessionSerializer()
    .AddCustomSerialization()
    .AddJsonSessionSerializer(options =>
    {
        options.RegisterKey<int>("callCount");
    });

builder.Services.AddDistributedMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseSession();

app.UseSystemWebAdapters();

app.MapGroup("/session")
    .MapSessionExample();

app.Run();
