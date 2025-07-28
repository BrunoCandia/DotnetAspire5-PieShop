using Microsoft.AspNetCore.Identity;
using PieShop.BusinessLogic;
using PieShop.DataAccess;
using PieShop.DataAccess.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.AddSqlServerDbContext<PieShopContext>("PieShop");

// Identity will use Entity Framework and adds authentication by default
builder.Services.AddDefaultIdentity<IdentityUser>().AddRoles<IdentityRole>().AddEntityFrameworkStores<PieShopContext>();

builder.Services.AddAuthorization(policy =>
{
    policy.AddPolicy("IsAdministrator", policy => policy.RequireRole("Administrator"));
});

builder.AddServiceDefaults();

// Add services related to ASP.NET Core MVC to the container.
builder.Services.AddControllersWithViews();

// To use the pages related to Identity (Login, Register) we need to add the service related to razor pages because they were done with Razor
builder.Services.AddRazorPages();

builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("PieList", policy => policy.SetVaryByRouteValue("category").Expire(TimeSpan.FromSeconds(60)));
    options.AddPolicy("PieDetail", policy => policy.SetVaryByRouteValue("pieId").Expire(TimeSpan.FromSeconds(60)));
});

// Configure Redis for Distributed Caching
builder.AddRedisOutputCache("cache");

// Configure Redis for Distributed Caching
builder.AddRedisDistributedCache("cache");

// Cached responses are stored in Redis
// https://learn.microsoft.com/en-us/aspnet/core/performance/caching/output?view=aspnetcore-9.0
// https://redis.io/docs/latest/commands/command-getkeys/#:~:text=You%20can%20use%20COMMAND%20GETKEYS,how%20Redis%20parses%20the%20commands.
// https://www.atlassian.com/data/admin/how-to-get-all-keys-in-redis

builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IPieRepository, PieRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddScoped<IPieService, PieService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAccountService, AccountService>();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseSession();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseOutputCache();

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

////app.MapControllerRoute(
////    name: "default",
////    pattern: "{controller=Home}/{action=Index}/{id?}")
////    .WithStaticAssets();

// The order matters. The middleware to support MVC routing
////app.UseEndpoints(endpoints =>
////{
////    endpoints.MapControllerRoute(
////        name: "pieList",
////        pattern: "pie/{action=List}",
////        defaults: new { controller = "pie" });

////    endpoints.MapControllerRoute(
////        name: "pieDetail",
////        pattern: "pie/{action=Detail}/{pieId}",
////        defaults: new { controller = "pie" });

////    endpoints.MapControllerRoute(
////        name: "orderCheckout",
////        pattern: "order/{action=Checkout}",
////        defaults: new { controller = "order" });

////    endpoints.MapControllerRoute(
////        name: "orderCheckoutCompleted",
////        pattern: "order/{action=CheckoutCompleted}",
////        defaults: new { controller = "order" });

////    endpoints.MapControllerRoute(
////        name: "default",
////        pattern: "{controller=Home}/{action=Index}/{id?}");
////});

#region Pie Controller Routes

// List pies, optionally by category
app.MapControllerRoute(
    name: "pieList",
    pattern: "pie/List",
    defaults: new { controller = "Pie", action = "List" });

// Pie detail by pieId
app.MapControllerRoute(
    name: "pieDetail",
    pattern: "pie/Detail/{pieId:guid}",
    defaults: new { controller = "Pie", action = "Detail" });

// Paginated pies
app.MapControllerRoute(
    name: "piePaginated",
    pattern: "pie/Paginated",
    defaults: new { controller = "Pie", action = "Paginated" });

// Pie search
app.MapControllerRoute(
    name: "pieSearch",
    pattern: "pie/Search",
    defaults: new { controller = "Pie", action = "Search" });

// Full pie detail
app.MapControllerRoute(
    name: "pieFullDetail",
    pattern: "pie/FullDetail/{pieId:guid}",
    defaults: new { controller = "Pie", action = "FullDetail" });

// Add pie (GET and POST)
app.MapControllerRoute(
    name: "pieAdd",
    pattern: "pie/Add",
    defaults: new { controller = "Pie", action = "Add" });

// Edit pie (GET by pieId, POST by model)
app.MapControllerRoute(
    name: "pieEdit",
    pattern: "pie/Edit/{pieId:guid}",
    defaults: new { controller = "Pie", action = "Edit" });

app.MapControllerRoute(
    name: "pieEditPost",
    pattern: "pie/Edit",
    defaults: new { controller = "Pie", action = "Edit" });

// Delete pie (GET by pieId, POST by model)
app.MapControllerRoute(
    name: "pieDelete",
    pattern: "pie/Delete/{pieId:guid}",
    defaults: new { controller = "Pie", action = "Delete" });

app.MapControllerRoute(
    name: "piePostDelete",
    pattern: "pie/PostDelete",
    defaults: new { controller = "Pie", action = "PostDelete" });

#endregion

#region Category Controller Routes

app.MapControllerRoute(
    name: "categoryIndex",
    pattern: "category/Index",
    defaults: new { controller = "Category", action = "Index" });

// Add category (GET and POST)
app.MapControllerRoute(
    name: "categoryAdd",
    pattern: "category/Add",
    defaults: new { controller = "Category", action = "Add" });

// Detail category by categoryId
app.MapControllerRoute(
    name: "categoryDetail",
    pattern: "category/Detail/{categoryId:guid}",
    defaults: new { controller = "Category", action = "Detail" });

// Edit category (GET by categoryId, POST by model)
app.MapControllerRoute(
    name: "categoryEdit",
    pattern: "category/Edit/{categoryId:guid}",
    defaults: new { controller = "Category", action = "Edit" });

app.MapControllerRoute(
    name: "categoryEditPost",
    pattern: "category/Edit",
    defaults: new { controller = "Category", action = "Edit" });

// Delete category (GET by categoryId, POST by model)
app.MapControllerRoute(
    name: "categoryDelete",
    pattern: "category/Delete/{categoryId:guid}",
    defaults: new { controller = "Category", action = "Delete" });

app.MapControllerRoute(
    name: "categoryPostDelete",
    pattern: "category/PostDelete",
    defaults: new { controller = "Category", action = "PostDelete" });

#endregion

#region Order Controller Routes

// Order index, optionally with orderId and orderDetailId as query parameters
// query parameters sample: /order/Index?orderId=11111111-1111-1111-1111-111111111111&orderDetailId=22222222-2222-2222-2222-222222222222
app.MapControllerRoute(
    name: "orderIndex",
    pattern: "order/Index",
    defaults: new { controller = "Order", action = "Index" });

// Order checkout (GET and POST)
app.MapControllerRoute(
    name: "orderCheckout",
    pattern: "order/Checkout",
    defaults: new { controller = "order", action = "Checkout" });

// Order checkout completed (GET)
app.MapControllerRoute(
    name: "orderCheckoutCompleted",
    pattern: "order/CheckoutCompleted",
    defaults: new { controller = "order", action = "CheckoutCompleted" });

// Order detail by orderId
// route parameters sample: /order/Detail/11111111-1111-1111-1111-111111111111
app.MapControllerRoute(
    name: "orderDetail",
    pattern: "order/Detail/{orderId:guid}",
    defaults: new { controller = "Order", action = "Detail" });

#endregion

#region Role Controller Routes

// Role index
app.MapControllerRoute(
    name: "roleIndex",
    pattern: "role/Index",
    defaults: new { controller = "Role", action = "Index" });

// Create role (GET and POST)
app.MapControllerRoute(
    name: "roleCreate",
    pattern: "role/Create",
    defaults: new { controller = "Role", action = "Create" });

// Assign role (GET and POST)
app.MapControllerRoute(
    name: "roleAssign",
    pattern: "role/Assign",
    defaults: new { controller = "Role", action = "Assign" });

// Remove role (POST)
app.MapControllerRoute(
    name: "roleRemoveRole",
    pattern: "role/RemoveRole",
    defaults: new { controller = "Role", action = "RemoveRole" });

#endregion

#region User Controller Routes

app.MapControllerRoute(
    name: "userIndex",
    pattern: "user/Index",
    defaults: new { controller = "User", action = "Index" });

#endregion

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// To use the pages realted to Identity (Login, Register) we need to add the routing to support Razor routing
app.MapRazorPages();

await app.RunAsync();
