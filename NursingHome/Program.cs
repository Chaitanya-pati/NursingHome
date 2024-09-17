using NursingHome.Db.Implementation;
using NursingHome.Db.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddTransient<IUserService,UserService>(provider =>
{
    return new UserService(builder.Configuration.GetConnectionString("NursingHome"));
});
builder.Services.AddTransient<IConfig,Config>(provider =>
{
    return new Config(builder.Configuration.GetConnectionString("NursingHome"));
});
builder.Services.AddTransient<IOldAge,OldAge>(provider =>
{
    return new OldAge(builder.Configuration.GetConnectionString("NursingHome"));
});
builder.Services.AddTransient<INursingHome,HomeNursing>(provider =>
{
    return new HomeNursing(builder.Configuration.GetConnectionString("NursingHome"));
});
builder.Services.AddTransient<IHelpers,Helpers>(provider =>
{
    return new Helpers(builder.Configuration.GetConnectionString("NursingHome"));
});
builder.Services.AddTransient<ICashMemo,CashMemo>(provider =>
{
    return new CashMemo(builder.Configuration.GetConnectionString("NursingHome"));
});builder.Services.AddTransient<IAttedanceService,AttedanceService>(provider =>
{
    return new AttedanceService(builder.Configuration.GetConnectionString("NursingHome"));
});builder.Services.AddTransient<ISalarySlipService, SalarySlipService>(provider =>
{
    return new SalarySlipService(builder.Configuration.GetConnectionString("NursingHome"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())   
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
