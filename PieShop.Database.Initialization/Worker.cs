using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using OpenTelemetry.Trace;
using PieShop.DataAccess;
using PieShop.DataAccess.Data.Entitites.Category;
using PieShop.DataAccess.Data.Entitites.Pie;
using System.Diagnostics;

namespace PieShop.Database.Initialization;

public class Worker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ILogger<Worker> _logger;

    internal const string ActivityName = "DatabaseInitialization";
    private static readonly ActivitySource _activitySource = new(ActivityName);

    private static Dictionary<string, Category>? categories;

    public static Dictionary<string, Category> Categories
    {
        get
        {
            if (categories == null)
            {
                var categoryList = new Category[]
                {
                    new Category { Name = "Fruit pies" },
                    new Category { Name = "Cheese cakes" },
                    new Category { Name = "Seasonal pies" }
                };

                categories = new Dictionary<string, Category>();

                foreach (Category category in categoryList)
                {
                    categories.Add(category.Name, category);
                }
            }

            return categories;
        }
    }

    public Worker(
        IServiceProvider serviceProvider,
        IHostApplicationLifetime hostApplicationLifetime,
        ILogger<Worker> logger)
    {
        _serviceProvider = serviceProvider;
        _hostApplicationLifetime = hostApplicationLifetime;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker running at: {Time}", DateTimeOffset.UtcNow);

        using var activity = _activitySource.StartActivity("Initialize database", ActivityKind.Client);

        try
        {
            // For reference see: https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying?tabs=dotnet-core-cli
            await InitializeDatabaseAsync();
        }
        catch (Exception ex)
        {
            activity?.RecordException(ex);

            _logger.LogError(ex, "Error with message {ErrorMessage}", ex.Message);

            throw;
        }

        _logger.LogInformation("Worker stopping at: {Time}", DateTimeOffset.UtcNow);

        _hostApplicationLifetime.StopApplication();
    }

    private async Task InitializeDatabaseAsync()
    {
        using var scope = _serviceProvider.CreateScope();

        var services = scope.ServiceProvider;

        var pieShopContext = services.GetRequiredService<PieShopContext>();

        await EnsureDatabaseAsync(pieShopContext);

        await RunMigrationsAsync(pieShopContext);

        // TODO: Seed the DB in a separate process.
        await SeedDatabaseAsync(pieShopContext);
    }

    private static async Task EnsureDatabaseAsync(PieShopContext pieShopContext)
    {
        var dbCreator = pieShopContext.GetService<IRelationalDatabaseCreator>();

        var strategy = pieShopContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            if (!await dbCreator.ExistsAsync())
            {
                Console.WriteLine("Creating Database");

                await dbCreator.CreateAsync();

                Console.WriteLine("Database Created");
            }
        });
    }

    private static async Task RunMigrationsAsync(PieShopContext pieShopContext)
    {
        var strategy = pieShopContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            if ((await pieShopContext.Database.GetPendingMigrationsAsync()).Any())
            {
                Console.WriteLine("Applying Migrations");

                // https://github.com/dotnet/efcore/issues/35127
                ////await using var transaction = await pieShopContext.Database.BeginTransactionAsync();
                await pieShopContext.Database.MigrateAsync();
                ////await transaction.CommitAsync();

                Console.WriteLine("Migrations Applied");
            }
        });
    }

    private static async Task SeedDatabaseAsync(PieShopContext pieShopContext)
    {
        var strategy = pieShopContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            // https://github.com/dotnet/efcore/issues/35127
            ////await using var transaction = await pieShopContext.Database.BeginTransactionAsync();
            await SeedBaseAdministratorUserAsync(pieShopContext);
            await SeedBaseAsync(pieShopContext);
            await pieShopContext.SaveChangesAsync();
            ////await transaction.CommitAsync();
        });
    }

    private static async Task SeedBaseAsync(PieShopContext pieShopContext)
    {
        Console.WriteLine("Seeding Data...");

        if (!await pieShopContext.Category.AnyAsync())
        {
            await pieShopContext.Category.AddRangeAsync(Categories.Select(c => c.Value));

            Console.WriteLine("New Category data...");
        }

        if (!await pieShopContext.Pie.AnyAsync())
        {
            await pieShopContext.AddRangeAsync
            (
                new Pie { Name = "Caramel Popcorn Cheese Cake", Price = 22.95M, ShortDescription = "The ultimate cheese cake", LongDescription = "Icing carrot cake jelly-o cheesecake. Sweet roll marzipan marshmallow toffee brownie brownie candy tootsie roll. Chocolate cake gingerbread tootsie roll oat cake pie chocolate bar cookie dragée brownie. Lollipop cotton candy cake bear claw oat cake. Dragée candy canes dessert tart. Marzipan dragée gummies lollipop jujubes chocolate bar candy canes. Icing gingerbread chupa chups cotton candy cookie sweet icing bonbon gummies. Gummies lollipop brownie biscuit danish chocolate cake. Danish powder cookie macaroon chocolate donut tart. Carrot cake dragée croissant lemon drops liquorice lemon drops cookie lollipop toffee. Carrot cake carrot cake liquorice sugar plum topping bonbon pie muffin jujubes. Jelly pastry wafer tart caramels bear claw. Tiramisu tart pie cake danish lemon drops. Brownie cupcake dragée gummies.", Category = Categories["Cheese cakes"], ImageUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/cheesecakes/caramelpopcorncheesecake.jpg", IsInStock = true, IsPieOfTheWeek = true, ImageThumbnailUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/cheesecakes/caramelpopcorncheesecakesmall.jpg", AllergyInformation = "" },
                new Pie { Name = "Chocolate Cheese Cake", Price = 19.95M, ShortDescription = "The chocolate lover's dream", LongDescription = "Icing carrot cake jelly-o cheesecake. Sweet roll marzipan marshmallow toffee brownie brownie candy tootsie roll. Chocolate cake gingerbread tootsie roll oat cake pie chocolate bar cookie dragée brownie. Lollipop cotton candy cake bear claw oat cake. Dragée candy canes dessert tart. Marzipan dragée gummies lollipop jujubes chocolate bar candy canes. Icing gingerbread chupa chups cotton candy cookie sweet icing bonbon gummies. Gummies lollipop brownie biscuit danish chocolate cake. Danish powder cookie macaroon chocolate donut tart. Carrot cake dragée croissant lemon drops liquorice lemon drops cookie lollipop toffee. Carrot cake carrot cake liquorice sugar plum topping bonbon pie muffin jujubes. Jelly pastry wafer tart caramels bear claw. Tiramisu tart pie cake danish lemon drops. Brownie cupcake dragée gummies.", Category = Categories["Cheese cakes"], ImageUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/cheesecakes/chocolatecheesecake.jpg", IsInStock = true, IsPieOfTheWeek = true, ImageThumbnailUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/cheesecakes/chocolatecheesecakesmall.jpg", AllergyInformation = "" },
                new Pie { Name = "Pistache Cheese Cake", Price = 21.95M, ShortDescription = "We're going nuts over this one", LongDescription = "Icing carrot cake jelly-o cheesecake. Sweet roll marzipan marshmallow toffee brownie brownie candy tootsie roll. Chocolate cake gingerbread tootsie roll oat cake pie chocolate bar cookie dragée brownie. Lollipop cotton candy cake bear claw oat cake. Dragée candy canes dessert tart. Marzipan dragée gummies lollipop jujubes chocolate bar candy canes. Icing gingerbread chupa chups cotton candy cookie sweet icing bonbon gummies. Gummies lollipop brownie biscuit danish chocolate cake. Danish powder cookie macaroon chocolate donut tart. Carrot cake dragée croissant lemon drops liquorice lemon drops cookie lollipop toffee. Carrot cake carrot cake liquorice sugar plum topping bonbon pie muffin jujubes. Jelly pastry wafer tart caramels bear claw. Tiramisu tart pie cake danish lemon drops. Brownie cupcake dragée gummies.", Category = Categories["Cheese cakes"], ImageUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/cheesecakes/pistachecheesecake.jpg", IsInStock = true, IsPieOfTheWeek = true, ImageThumbnailUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/cheesecakes/pistachecheesecakesmall.jpg", AllergyInformation = "" },
                new Pie { Name = "Pecan Pie", Price = 21.95M, ShortDescription = "More pecan than you can handle!", LongDescription = "Icing carrot cake jelly-o cheesecake. Sweet roll marzipan marshmallow toffee brownie brownie candy tootsie roll. Chocolate cake gingerbread tootsie roll oat cake pie chocolate bar cookie dragée brownie. Lollipop cotton candy cake bear claw oat cake. Dragée candy canes dessert tart. Marzipan dragée gummies lollipop jujubes chocolate bar candy canes. Icing gingerbread chupa chups cotton candy cookie sweet icing bonbon gummies. Gummies lollipop brownie biscuit danish chocolate cake. Danish powder cookie macaroon chocolate donut tart. Carrot cake dragée croissant lemon drops liquorice lemon drops cookie lollipop toffee. Carrot cake carrot cake liquorice sugar plum topping bonbon pie muffin jujubes. Jelly pastry wafer tart caramels bear claw. Tiramisu tart pie cake danish lemon drops. Brownie cupcake dragée gummies.", Category = Categories["Fruit pies"], ImageUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/fruitpies/pecanpie.jpg", IsInStock = true, IsPieOfTheWeek = false, ImageThumbnailUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/fruitpies/pecanpiesmall.jpg", AllergyInformation = "" },
                new Pie { Name = "Birthday Pie", Price = 29.95M, ShortDescription = "A Happy Birthday with this pie!", LongDescription = "Icing carrot cake jelly-o cheesecake. Sweet roll marzipan marshmallow toffee brownie brownie candy tootsie roll. Chocolate cake gingerbread tootsie roll oat cake pie chocolate bar cookie dragée brownie. Lollipop cotton candy cake bear claw oat cake. Dragée candy canes dessert tart. Marzipan dragée gummies lollipop jujubes chocolate bar candy canes. Icing gingerbread chupa chups cotton candy cookie sweet icing bonbon gummies. Gummies lollipop brownie biscuit danish chocolate cake. Danish powder cookie macaroon chocolate donut tart. Carrot cake dragée croissant lemon drops liquorice lemon drops cookie lollipop toffee. Carrot cake carrot cake liquorice sugar plum topping bonbon pie muffin jujubes. Jelly pastry wafer tart caramels bear claw. Tiramisu tart pie cake danish lemon drops. Brownie cupcake dragée gummies.", Category = Categories["Seasonal pies"], ImageUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/seasonalpies/birthdaypie.jpg", IsInStock = true, IsPieOfTheWeek = false, ImageThumbnailUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/seasonalpies/birthdaypiesmall.jpg", AllergyInformation = "" },
                new Pie { Name = "Apple Pie", Price = 12.95M, ShortDescription = "Our famous apple pies!", LongDescription = "Icing carrot cake jelly-o cheesecake. Sweet roll marzipan marshmallow toffee brownie brownie candy tootsie roll. Chocolate cake gingerbread tootsie roll oat cake pie chocolate bar cookie dragée brownie. Lollipop cotton candy cake bear claw oat cake. Dragée candy canes dessert tart. Marzipan dragée gummies lollipop jujubes chocolate bar candy canes. Icing gingerbread chupa chups cotton candy cookie sweet icing bonbon gummies. Gummies lollipop brownie biscuit danish chocolate cake. Danish powder cookie macaroon chocolate donut tart. Carrot cake dragée croissant lemon drops liquorice lemon drops cookie lollipop toffee. Carrot cake carrot cake liquorice sugar plum topping bonbon pie muffin jujubes. Jelly pastry wafer tart caramels bear claw. Tiramisu tart pie cake danish lemon drops. Brownie cupcake dragée gummies.", Category = Categories["Fruit pies"], ImageUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/fruitpies/applepie.jpg", IsInStock = true, IsPieOfTheWeek = false, ImageThumbnailUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/fruitpies/applepiesmall.jpg", AllergyInformation = "" },
                new Pie { Name = "Blueberry Cheese Cake", Price = 18.95M, ShortDescription = "You'll love it!", LongDescription = "Icing carrot cake jelly-o cheesecake. Sweet roll marzipan marshmallow toffee brownie brownie candy tootsie roll. Chocolate cake gingerbread tootsie roll oat cake pie chocolate bar cookie dragée brownie. Lollipop cotton candy cake bear claw oat cake. Dragée candy canes dessert tart. Marzipan dragée gummies lollipop jujubes chocolate bar candy canes. Icing gingerbread chupa chups cotton candy cookie sweet icing bonbon gummies. Gummies lollipop brownie biscuit danish chocolate cake. Danish powder cookie macaroon chocolate donut tart. Carrot cake dragée croissant lemon drops liquorice lemon drops cookie lollipop toffee. Carrot cake carrot cake liquorice sugar plum topping bonbon pie muffin jujubes. Jelly pastry wafer tart caramels bear claw. Tiramisu tart pie cake danish lemon drops. Brownie cupcake dragée gummies.", Category = Categories["Cheese cakes"], ImageUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/cheesecakes/blueberrycheesecake.jpg", IsInStock = true, IsPieOfTheWeek = false, ImageThumbnailUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/cheesecakes/blueberrycheesecakesmall.jpg", AllergyInformation = "" },
                new Pie { Name = "Cheese Cake", Price = 18.95M, ShortDescription = "Plain cheese cake. Plain pleasure.", LongDescription = "Icing carrot cake jelly-o cheesecake. Sweet roll marzipan marshmallow toffee brownie brownie candy tootsie roll. Chocolate cake gingerbread tootsie roll oat cake pie chocolate bar cookie dragée brownie. Lollipop cotton candy cake bear claw oat cake. Dragée candy canes dessert tart. Marzipan dragée gummies lollipop jujubes chocolate bar candy canes. Icing gingerbread chupa chups cotton candy cookie sweet icing bonbon gummies. Gummies lollipop brownie biscuit danish chocolate cake. Danish powder cookie macaroon chocolate donut tart. Carrot cake dragée croissant lemon drops liquorice lemon drops cookie lollipop toffee. Carrot cake carrot cake liquorice sugar plum topping bonbon pie muffin jujubes. Jelly pastry wafer tart caramels bear claw. Tiramisu tart pie cake danish lemon drops. Brownie cupcake dragée gummies.", Category = Categories["Cheese cakes"], ImageUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/cheesecakes/cheesecake.jpg", IsInStock = true, IsPieOfTheWeek = false, ImageThumbnailUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/cheesecakes/cheesecakesmall.jpg", AllergyInformation = "" },
                new Pie { Name = "Cherry Pie", Price = 15.95M, ShortDescription = "A summer classic!", LongDescription = "Icing carrot cake jelly-o cheesecake. Sweet roll marzipan marshmallow toffee brownie brownie candy tootsie roll. Chocolate cake gingerbread tootsie roll oat cake pie chocolate bar cookie dragée brownie. Lollipop cotton candy cake bear claw oat cake. Dragée candy canes dessert tart. Marzipan dragée gummies lollipop jujubes chocolate bar candy canes. Icing gingerbread chupa chups cotton candy cookie sweet icing bonbon gummies. Gummies lollipop brownie biscuit danish chocolate cake. Danish powder cookie macaroon chocolate donut tart. Carrot cake dragée croissant lemon drops liquorice lemon drops cookie lollipop toffee. Carrot cake carrot cake liquorice sugar plum topping bonbon pie muffin jujubes. Jelly pastry wafer tart caramels bear claw. Tiramisu tart pie cake danish lemon drops. Brownie cupcake dragée gummies.", Category = Categories["Fruit pies"], ImageUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/fruitpies/cherrypie.jpg", IsInStock = true, IsPieOfTheWeek = false, ImageThumbnailUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/fruitpies/cherrypiesmall.jpg", AllergyInformation = "" },
                new Pie { Name = "Christmas Apple Pie", Price = 13.95M, ShortDescription = "Happy holidays with this pie!", LongDescription = "Icing carrot cake jelly-o cheesecake. Sweet roll marzipan marshmallow toffee brownie brownie candy tootsie roll. Chocolate cake gingerbread tootsie roll oat cake pie chocolate bar cookie dragée brownie. Lollipop cotton candy cake bear claw oat cake. Dragée candy canes dessert tart. Marzipan dragée gummies lollipop jujubes chocolate bar candy canes. Icing gingerbread chupa chups cotton candy cookie sweet icing bonbon gummies. Gummies lollipop brownie biscuit danish chocolate cake. Danish powder cookie macaroon chocolate donut tart. Carrot cake dragée croissant lemon drops liquorice lemon drops cookie lollipop toffee. Carrot cake carrot cake liquorice sugar plum topping bonbon pie muffin jujubes. Jelly pastry wafer tart caramels bear claw. Tiramisu tart pie cake danish lemon drops. Brownie cupcake dragée gummies.", Category = Categories["Seasonal pies"], ImageUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/seasonalpies/christmasapplepie.jpg", IsInStock = true, IsPieOfTheWeek = false, ImageThumbnailUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/seasonalpies/christmasapplepiesmall.jpg", AllergyInformation = "" },
                new Pie { Name = "Cranberry Pie", Price = 17.95M, ShortDescription = "A Christmas favorite", LongDescription = "Icing carrot cake jelly-o cheesecake. Sweet roll marzipan marshmallow toffee brownie brownie candy tootsie roll. Chocolate cake gingerbread tootsie roll oat cake pie chocolate bar cookie dragée brownie. Lollipop cotton candy cake bear claw oat cake. Dragée candy canes dessert tart. Marzipan dragée gummies lollipop jujubes chocolate bar candy canes. Icing gingerbread chupa chups cotton candy cookie sweet icing bonbon gummies. Gummies lollipop brownie biscuit danish chocolate cake. Danish powder cookie macaroon chocolate donut tart. Carrot cake dragée croissant lemon drops liquorice lemon drops cookie lollipop toffee. Carrot cake carrot cake liquorice sugar plum topping bonbon pie muffin jujubes. Jelly pastry wafer tart caramels bear claw. Tiramisu tart pie cake danish lemon drops. Brownie cupcake dragée gummies.", Category = Categories["Fruit pies"], ImageUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/fruitpies/cranberrypie.jpg", IsInStock = true, IsPieOfTheWeek = false, ImageThumbnailUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/fruitpies/cranberrypiesmall.jpg", AllergyInformation = "" },
                new Pie { Name = "Peach Pie", Price = 15.95M, ShortDescription = "Sweet as peach", LongDescription = "Icing carrot cake jelly-o cheesecake. Sweet roll marzipan marshmallow toffee brownie brownie candy tootsie roll. Chocolate cake gingerbread tootsie roll oat cake pie chocolate bar cookie dragée brownie. Lollipop cotton candy cake bear claw oat cake. Dragée candy canes dessert tart. Marzipan dragée gummies lollipop jujubes chocolate bar candy canes. Icing gingerbread chupa chups cotton candy cookie sweet icing bonbon gummies. Gummies lollipop brownie biscuit danish chocolate cake. Danish powder cookie macaroon chocolate donut tart. Carrot cake dragée croissant lemon drops liquorice lemon drops cookie lollipop toffee. Carrot cake carrot cake liquorice sugar plum topping bonbon pie muffin jujubes. Jelly pastry wafer tart caramels bear claw. Tiramisu tart pie cake danish lemon drops. Brownie cupcake dragée gummies.", Category = Categories["Fruit pies"], ImageUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/fruitpies/peachpie.jpg", IsInStock = false, IsPieOfTheWeek = false, ImageThumbnailUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/fruitpies/peachpiesmall.jpg", AllergyInformation = "" },
                new Pie { Name = "Pumpkin Pie", Price = 12.95M, ShortDescription = "Our Halloween favorite", LongDescription = "Icing carrot cake jelly-o cheesecake. Sweet roll marzipan marshmallow toffee brownie brownie candy tootsie roll. Chocolate cake gingerbread tootsie roll oat cake pie chocolate bar cookie dragée brownie. Lollipop cotton candy cake bear claw oat cake. Dragée candy canes dessert tart. Marzipan dragée gummies lollipop jujubes chocolate bar candy canes. Icing gingerbread chupa chups cotton candy cookie sweet icing bonbon gummies. Gummies lollipop brownie biscuit danish chocolate cake. Danish powder cookie macaroon chocolate donut tart. Carrot cake dragée croissant lemon drops liquorice lemon drops cookie lollipop toffee. Carrot cake carrot cake liquorice sugar plum topping bonbon pie muffin jujubes. Jelly pastry wafer tart caramels bear claw. Tiramisu tart pie cake danish lemon drops. Brownie cupcake dragée gummies.", Category = Categories["Seasonal pies"], ImageUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/seasonalpies/pumpkinpie.jpg", IsInStock = true, IsPieOfTheWeek = false, ImageThumbnailUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/seasonalpies/pumpkinpiesmall.jpg", AllergyInformation = "" },
                new Pie { Name = "Rhubarb Pie", Price = 15.95M, ShortDescription = "My God, so sweet!", LongDescription = "Icing carrot cake jelly-o cheesecake. Sweet roll marzipan marshmallow toffee brownie brownie candy tootsie roll. Chocolate cake gingerbread tootsie roll oat cake pie chocolate bar cookie dragée brownie. Lollipop cotton candy cake bear claw oat cake. Dragée candy canes dessert tart. Marzipan dragée gummies lollipop jujubes chocolate bar candy canes. Icing gingerbread chupa chups cotton candy cookie sweet icing bonbon gummies. Gummies lollipop brownie biscuit danish chocolate cake. Danish powder cookie macaroon chocolate donut tart. Carrot cake dragée croissant lemon drops liquorice lemon drops cookie lollipop toffee. Carrot cake carrot cake liquorice sugar plum topping bonbon pie muffin jujubes. Jelly pastry wafer tart caramels bear claw. Tiramisu tart pie cake danish lemon drops. Brownie cupcake dragée gummies.", Category = Categories["Fruit pies"], ImageUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/fruitpies/rhubarbpie.jpg", IsInStock = true, IsPieOfTheWeek = false, ImageThumbnailUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/fruitpies/rhubarbpiesmall.jpg", AllergyInformation = "" },
                new Pie { Name = "Strawberry Pie", Price = 15.95M, ShortDescription = "Our delicious strawberry pie!", LongDescription = "Icing carrot cake jelly-o cheesecake. Sweet roll marzipan marshmallow toffee brownie brownie candy tootsie roll. Chocolate cake gingerbread tootsie roll oat cake pie chocolate bar cookie dragée brownie. Lollipop cotton candy cake bear claw oat cake. Dragée candy canes dessert tart. Marzipan dragée gummies lollipop jujubes chocolate bar candy canes. Icing gingerbread chupa chups cotton candy cookie sweet icing bonbon gummies. Gummies lollipop brownie biscuit danish chocolate cake. Danish powder cookie macaroon chocolate donut tart. Carrot cake dragée croissant lemon drops liquorice lemon drops cookie lollipop toffee. Carrot cake carrot cake liquorice sugar plum topping bonbon pie muffin jujubes. Jelly pastry wafer tart caramels bear claw. Tiramisu tart pie cake danish lemon drops. Brownie cupcake dragée gummies.", Category = Categories["Fruit pies"], ImageUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/fruitpies/strawberrypie.jpg", IsInStock = true, IsPieOfTheWeek = false, ImageThumbnailUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/fruitpies/strawberrypiesmall.jpg", AllergyInformation = "" },
                new Pie { Name = "Strawberry Cheese Cake", Price = 18.95M, ShortDescription = "You'll love it!", LongDescription = "Icing carrot cake jelly-o cheesecake. Sweet roll marzipan marshmallow toffee brownie brownie candy tootsie roll. Chocolate cake gingerbread tootsie roll oat cake pie chocolate bar cookie dragée brownie. Lollipop cotton candy cake bear claw oat cake. Dragée candy canes dessert tart. Marzipan dragée gummies lollipop jujubes chocolate bar candy canes. Icing gingerbread chupa chups cotton candy cookie sweet icing bonbon gummies. Gummies lollipop brownie biscuit danish chocolate cake. Danish powder cookie macaroon chocolate donut tart. Carrot cake dragée croissant lemon drops liquorice lemon drops cookie lollipop toffee. Carrot cake carrot cake liquorice sugar plum topping bonbon pie muffin jujubes. Jelly pastry wafer tart caramels bear claw. Tiramisu tart pie cake danish lemon drops. Brownie cupcake dragée gummies.", Category = Categories["Cheese cakes"], ImageUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/cheesecakes/strawberrycheesecake.jpg", IsInStock = false, IsPieOfTheWeek = false, ImageThumbnailUrl = "https://stpieshop.blob.core.windows.net/files/pieshop/cheesecakes/strawberrycheesecakesmall.jpg", AllergyInformation = "" }
            );

            Console.WriteLine("New Pie data...");
        }

        Console.WriteLine("Data Seeded");
    }

    private static async Task SeedBaseAdministratorUserAsync(PieShopContext pieShopContext)
    {
        if (!await pieShopContext.Roles.AnyAsync(r => r.Name == "Administrator"))
        {
            await pieShopContext.Roles.AddAsync(new IdentityRole { Name = "Administrator", NormalizedName = "ADMINISTRATOR" });
            await pieShopContext.SaveChangesAsync();

            Console.WriteLine("Seeding Base Role...");
        }

        if (!await pieShopContext.Users.AnyAsync(u => u.UserName == "admin_user@gmail.com"))
        {
            var adminUser = new IdentityUser
            {
                UserName = "admin_user@gmail.com",
                NormalizedUserName = "ADMIN_USER@GMAIL.COM",
                Email = "admin_user@gmail.com",
                NormalizedEmail = "ADMIN_USER@GMAIL.COM",
                EmailConfirmed = false
            };

            var passwordHasher = new PasswordHasher<IdentityUser>();
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Admin@12345");

            await pieShopContext.Users.AddAsync(adminUser);
            await pieShopContext.SaveChangesAsync();

            Console.WriteLine("Seeding Base Admin User...");
        }

        var user = await pieShopContext.Users.FirstOrDefaultAsync(u => u.UserName == "admin_user@gmail.com");
        var role = await pieShopContext.Roles.FirstOrDefaultAsync(r => r.Name == "Administrator");

        if (user != null && role != null && !await pieShopContext.UserRoles.AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id))
        {
            await pieShopContext.UserRoles.AddAsync(new IdentityUserRole<string> { UserId = user.Id, RoleId = role.Id });
            await pieShopContext.SaveChangesAsync();

            Console.WriteLine("Seeding Base UserRole...");
        }
    }
}
