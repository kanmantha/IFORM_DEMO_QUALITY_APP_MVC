using IFormQualityApp.Data;
using IFormQualityApp.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IFormQualityApp.Services;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        await db.Database.MigrateAsync();

        await SeedRolesAsync(roleManager);
        await SeedUsersAsync(userManager);
        await SeedProjectsAsync(db);
        await SeedProductsAsync(db);
        await SeedEmailTemplatesAsync(db);
        await SeedQueriesAsync(db, userManager);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in new[] { "Manager", "SiteEngineer", "Admin" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<AppUser> userManager)
    {
        var users = new[]
        {
            new { Email = "admin@iform.in", Name = "System Administrator", Pass = "Iform@2026", Role = "Admin", Code = "ADM001", Dept = "IT" },
            new { Email = "venkatesh@iform.in", Name = "Venkatesh K", Pass = "Iform@2026", Role = "Manager", Code = "MGR001", Dept = "Design" },
            new { Email = "sowmya@iform.in", Name = "Sowmya K", Pass = "Iform@2026", Role = "Manager", Code = "MGR002", Dept = "Contracts" },
            new { Email = "swapnika@iform.in", Name = "Swapnika N", Pass = "Iform@2026", Role = "Manager", Code = "MGR003", Dept = "Design" },
            new { Email = "sai@iform.in", Name = "T. Venkata Sai", Pass = "Iform@2026", Role = "SiteEngineer", Code = "ENG001", Dept = "Site" },
            new { Email = "basha@iform.in", Name = "Basha", Pass = "Iform@2026", Role = "SiteEngineer", Code = "ENG002", Dept = "Site" },
            new { Email = "ramesh@iform.in", Name = "Ramesh", Pass = "Iform@2026", Role = "SiteEngineer", Code = "ENG003", Dept = "Site" },
            new { Email = "suresh@iform.in", Name = "Suresh", Pass = "Iform@2026", Role = "SiteEngineer", Code = "ENG004", Dept = "Site" }
        };

        foreach (var u in users)
        {
            if (await userManager.FindByEmailAsync(u.Email) != null)
            {
                continue;
            }

            var user = new AppUser
            {
                UserName = u.Email,
                Email = u.Email,
                FullName = u.Name,
                EmployeeCode = u.Code,
                Department = u.Dept,
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, u.Pass);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, u.Role);
            }
        }
    }

    private static async Task SeedProjectsAsync(ApplicationDbContext db)
    {
        if (await db.Projects.AnyAsync())
        {
            return;
        }

        var projects = new[]
        {
            new Project { Name = "Hallmark", Client = "Hallmark Developers", Location = "Hyderabad" },
            new Project { Name = "SRR Khammam", Client = "SRR Constructions", Location = "Khammam" },
            new Project { Name = "Golkonda Tattvam", Client = "Golkonda Builders", Location = "Hyderabad" },
            new Project { Name = "Reliance E-1", Client = "Reliance Group", Location = "Mumbai" },
            new Project { Name = "North Star", Client = "North Star Infra", Location = "Hyderabad" },
            new Project { Name = "OLYMPUS-2 HILITE-A", Client = "Hilite Group", Location = "Kochi" },
            new Project { Name = "PROFOUND VANAM TOWER-1", Client = "Profound Infra", Location = "Vijayawada" },
            new Project { Name = "TECHNO PAINTS ONE NINE", Client = "Techno Paints", Location = "Hyderabad" },
            new Project { Name = "SIDDHARTHA ACADEMIC", Client = "Siddhartha Academy", Location = "Vijayawada" }
        };

        db.Projects.AddRange(projects);
        await db.SaveChangesAsync();
    }

    private static async Task SeedEmailTemplatesAsync(ApplicationDbContext db)
    {
        if (await db.EmailTemplates.AnyAsync())
        {
            return;
        }

        db.EmailTemplates.AddRange(new[]
        {
            new EmailTemplate
            {
                Name = "Missing Item",
                IssueType = IssueType.Missing,
                Subject = "Site Query - Missing Item | IPO {IPO} | {Project}",
                Body = "Dear Team,{br}{br}This is to notify that the following item has been identified as MISSING on site.{br}{br}IPO Number: {IPO}{br}Project: {Project}{br}Issue Type: {IssueType}{br}Quantity: {QtyNos} nos / {QtySqm} sqm{br}Raised By: {Sender}{br}Date Raised: {RaisedAt}{br}Status: {Status}{br}{br}Kindly arrange for the dispatch of the missing item at the earliest.{br}{br}Thanks & Regards,{br}{Sender}{br}I-FORM Aluminium & Design LLP"
            },
            new EmailTemplate
            {
                Name = "Production Mistake",
                IssueType = IssueType.ProductionMistake,
                Subject = "Site Query - Production Mistake | IPO {IPO} | {Project}",
                Body = "Dear Team,{br}{br}This is to notify that the following item has been identified with a PRODUCTION MISTAKE on site.{br}{br}IPO Number: {IPO}{br}Project: {Project}{br}Issue Type: {IssueType}{br}Quantity: {QtyNos} nos / {QtySqm} sqm{br}Raised By: {Sender}{br}Date Raised: {RaisedAt}{br}Status: {Status}{br}{br}Please verify the production records and arrange for rectification or replacement.{br}{br}Thanks & Regards,{br}{Sender}{br}I-FORM Aluminium & Design LLP"
            },
            new EmailTemplate
            {
                Name = "Design Mistake",
                IssueType = IssueType.DesignMistake,
                Subject = "Site Query - Design Mistake | IPO {IPO} | {Project}",
                Body = "Dear Team,{br}{br}This is to notify that the following item has been identified with a DESIGN MISTAKE on site.{br}{br}IPO Number: {IPO}{br}Project: {Project}{br}Issue Type: {IssueType}{br}Quantity: {QtyNos} nos / {QtySqm} sqm{br}Raised By: {Sender}{br}Date Raised: {RaisedAt}{br}Status: {Status}{br}{br}Kindly review the drawings and advise the correct design/revision at the earliest.{br}{br}Thanks & Regards,{br}{Sender}{br}I-FORM Aluminium & Design LLP"
            },
            new EmailTemplate
            {
                Name = "Dispatch Missing",
                IssueType = IssueType.DispatchMissing,
                Subject = "Site Query - Dispatch Missing | IPO {IPO} | {Project}",
                Body = "Dear Team,{br}{br}This is to notify that the following item is MISSING FROM DISPATCH on site.{br}{br}IPO Number: {IPO}{br}Project: {Project}{br}Issue Type: {IssueType}{br}Quantity: {QtyNos} nos / {QtySqm} sqm{br}Raised By: {Sender}{br}Date Raised: {RaisedAt}{br}Status: {Status}{br}{br}Kindly verify the dispatch documents and arrange for the missing material.{br}{br}Thanks & Regards,{br}{Sender}{br}I-FORM Aluminium & Design LLP"
            }
        });

        await db.SaveChangesAsync();
    }

    private static async Task SeedQueriesAsync(ApplicationDbContext db, UserManager<AppUser> userManager)
    {
        if (await db.SiteQueries.AnyAsync())
        {
            return;
        }

        var engineers = new[] { "sai@iform.in", "basha@iform.in", "ramesh@iform.in", "suresh@iform.in" };
        var managers = new[] { "venkatesh@iform.in", "sowmya@iform.in" };
        var engUsers = new List<AppUser>();
        foreach (var email in engineers)
        {
            var u = await userManager.FindByEmailAsync(email);
            if (u != null) engUsers.Add(u);
        }
        var mgrUsers = new List<AppUser>();
        foreach (var email in managers)
        {
            var u = await userManager.FindByEmailAsync(email);
            if (u != null) mgrUsers.Add(u);
        }

        var projectNames = await db.Projects.ToDictionaryAsync(p => p.Name, p => p);
        var queries = new List<SiteQuery>();

        SiteQuery MakeQuery(string project, string ipo, IssueType type, decimal nos, decimal sqm,
            int daysOpen, QueryStatus status, AppUser raiser, AppUser? resolver, string desc, DateTime raisedAt)
        {
            var resolvedAt = status == QueryStatus.Resolved
                ? raisedAt.AddDays(daysOpen)
                : (DateTime?)null;

            return new SiteQuery
            {
                QueryNumber = $"Q-{raisedAt:yyyyMMdd}-{ipo}",
                IPO = ipo,
                Project = projectNames[project],
                IssueType = type,
                Status = status,
                QtyNos = nos,
                QtySqm = sqm,
                Description = desc,
                RaisedBy = raiser,
                RaisedAt = raisedAt,
                ResolvedBy = resolver,
                ResolvedAt = resolvedAt,
                UpdatedAt = resolvedAt ?? raisedAt,
                SlabTargetDate = raisedAt.AddDays(20),
                SlabCompletedDate = status == QueryStatus.Resolved ? raisedAt.AddDays(daysOpen) : (DateTime?)null,
                SlabDelayDays = status == QueryStatus.Resolved ? daysOpen : (int?)null
            };
        }

        var now = DateTime.UtcNow.Date;

        // Sample rows from the tracker document
        queries.Add(MakeQuery("Hallmark", "556", IssueType.Missing, 12, 3.06m, 45, QueryStatus.Pending, engUsers[0], null, "Missing items at site - pending dispatch", now.AddDays(-45)));
        queries.Add(MakeQuery("SRR Khammam", "571", IssueType.Missing, 8, 2.14m, 46, QueryStatus.Pending, engUsers[1], null, "Items missing as per dispatch schedule", now.AddDays(-46)));
        queries.Add(MakeQuery("Golkonda Tattvam", "565", IssueType.Missing, 12, 3.06m, 46, QueryStatus.Pending, engUsers[2], null, "Missing - qty 12 nos / 3.06 sqm raised 12/06/2026", now.AddDays(-46)));
        queries.Add(MakeQuery("Reliance E-1", "561", IssueType.Missing, 15, 4.2m, 61, QueryStatus.InProgress, engUsers[3], null, "Longest open delay - 61 days", now.AddDays(-61)));
        queries.Add(MakeQuery("North Star", "535", IssueType.Missing, 6, 1.8m, 32, QueryStatus.Pending, engUsers[0], null, "Missing items", now.AddDays(-32)));
        queries.Add(MakeQuery("OLYMPUS-2 HILITE-A", "589", IssueType.DispatchMissing, 15, 5.0m, 27, QueryStatus.Pending, engUsers[1], null, "Dispatch missing", now.AddDays(-27)));
        queries.Add(MakeQuery("OLYMPUS-2 HILITE-A", "590", IssueType.DesignMistake, 4, 1.2m, 27, QueryStatus.InProgress, engUsers[2], null, "Design mistake reported", now.AddDays(-27)));
        queries.Add(MakeQuery("OLYMPUS-2 HILITE-A", "591", IssueType.ProductionMistake, 3, 0.9m, 27, QueryStatus.Pending, engUsers[3], null, "Production mistake", now.AddDays(-27)));
        queries.Add(MakeQuery("PROFOUND VANAM TOWER-1", "598", IssueType.ProductionMistake, 5, 1.5m, 25, QueryStatus.Pending, engUsers[0], null, "Production mistakes", now.AddDays(-25)));
        queries.Add(MakeQuery("PROFOUND VANAM TOWER-1", "599", IssueType.DispatchMissing, 7, 2.1m, 25, QueryStatus.Pending, engUsers[1], null, "Dispatch missings", now.AddDays(-25)));
        queries.Add(MakeQuery("TECHNO PAINTS ONE NINE", "605", IssueType.DispatchMissing, 9, 2.7m, 19, QueryStatus.Pending, engUsers[2], null, "Dispatch missing", now.AddDays(-19)));
        queries.Add(MakeQuery("SIDDHARTHA ACADEMIC", "601", IssueType.DispatchMissing, 1, 23.0m, 18, QueryStatus.Pending, engUsers[3], null, "Dispatch missings - qty 1 nos / 23 sqm", now.AddDays(-18)));
        queries.Add(MakeQuery("Hallmark", "552", IssueType.ProductionMistake, 10, 3.0m, 60, QueryStatus.Resolved, engUsers[0], mgrUsers[0], "Resolved after rework", now.AddDays(-60)));
        queries.Add(MakeQuery("Golkonda Tattvam", "550", IssueType.DesignMistake, 5, 1.4m, 40, QueryStatus.Resolved, engUsers[2], mgrUsers[1], "Design revision issued and resolved", now.AddDays(-40)));
        queries.Add(MakeQuery("Reliance E-1", "548", IssueType.Missing, 20, 6.0m, 35, QueryStatus.Resolved, engUsers[3], mgrUsers[0], "Material dispatched and received", now.AddDays(-35)));

        db.SiteQueries.AddRange(queries);
        await db.SaveChangesAsync();

        // Add comments and audit logs for a couple of queries
        var q1 = queries[3]; // Reliance E-1, InProgress
        db.QueryComments.Add(new QueryComment
        {
            Query = q1,
            User = engUsers[3],
            Text = "Followed up with dispatch - material partially available.",
            CreatedAt = now.AddDays(-20)
        });
        db.QueryComments.Add(new QueryComment
        {
            Query = q1,
            User = mgrUsers[0],
            Text = "Production team working on it. Expected dispatch next week.",
            CreatedAt = now.AddDays(-15)
        });

        db.AuditLogs.Add(new AuditLog
        {
            Query = q1,
            User = engUsers[3],
            Action = "Raised",
            Details = "Query raised with status Pending",
            Timestamp = now.AddDays(-61)
        });
        db.AuditLogs.Add(new AuditLog
        {
            Query = q1,
            User = mgrUsers[0],
            Action = "StatusChanged",
            Details = "Status changed to In Progress",
            Timestamp = now.AddDays(-30)
        });

        foreach (var rq in queries.Where(x => x.Status == QueryStatus.Resolved))
        {
            db.AuditLogs.Add(new AuditLog
            {
                Query = rq,
                User = rq.ResolvedBy!,
                Action = "Resolved",
                Details = "Query marked as Resolved",
                Timestamp = rq.ResolvedAt!.Value
            });
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedProductsAsync(ApplicationDbContext db)
    {
        if (await db.Products.AnyAsync())
        {
            return;
        }

        db.Products.AddRange(new[]
        {
            new Product { Code = "DAAA", Name = "Snap Tie", Category = "Ties", Description = "Snap tie", Specification = "Wall thickness (mm)", Material = "Steel" },
            new Product { Code = "DABA", Name = "2 Hole - Reusable Tie", Category = "Ties", Description = "2-hole reusable tie", Specification = "Wall thickness (mm)", Material = "Steel" },
            new Product { Code = "DACA", Name = "3 Hole - Reusable Tie (W37)", Category = "Ties", Description = "3-hole reusable tie W37", Specification = "Wall thickness (mm)", Material = "Steel" },
            new Product { Code = "DAHA", Name = "3 Hole - Reusable Tie (W33)", Category = "Ties", Description = "3-hole reusable tie W33", Specification = "Wall thickness (mm)", Material = "Steel" },
            new Product { Code = "DTGD", Name = "Re-Cone Tie", Category = "Ties", Description = "Re-cone tie 1/2", Specification = "1/2 - Wall thickness (mm)", Material = "Steel + PVC" },
            new Product { Code = "DADA", Name = "T-Tie", Category = "Ties", Description = "T-tie", Specification = "Wall thickness (mm)", Material = "Steel" },
            new Product { Code = "DAFA", Name = "Double Pour Tie", Category = "Ties", Description = "Double pour tie", Specification = "Wall thk - Wall space distance", Material = "Steel" },
            new Product { Code = "DAGA", Name = "Al-Rod Tie", Category = "Ties", Description = "Aluminum rod tie", Specification = "Wall thickness (mm)", Material = "Steel" },
            new Product { Code = "DAGB", Name = "Tie Rod (1/2)", Category = "Ties", Description = "Tie rod 1/2", Specification = "Length", Material = "Steel" },
            new Product { Code = "DAGC", Name = "Tie Rod (5/8)", Category = "Ties", Description = "Tie rod 5/8", Specification = "Length", Material = "Steel" },
            new Product { Code = "DAIB", Name = "Sepa Bolt (1/2)", Category = "Bolts", Description = "Separation bolt 1/2", Specification = "Wall thickness (mm)", Material = "Steel" },
            new Product { Code = "DAIC", Name = "Sepa Bolt (5/8)", Category = "Bolts", Description = "Separation bolt 5/8", Specification = "Wall thickness (mm)", Material = "Steel" },
            new Product { Code = "DRVA0001", Name = "Support (V1)", Category = "Prop Support", Description = "Prop support V1", Specification = "Min - Max Length", Material = "Steel" },
            new Product { Code = "DRVA0002", Name = "Support (V2)", Category = "Prop Support", Description = "Prop support V2", Specification = "Min - Max Length", Material = "Steel" },
            new Product { Code = "DRWA0001", Name = "Support (V3)", Category = "Prop Support", Description = "Prop support V3", Specification = "Min - Max Length", Material = "Steel" },
            new Product { Code = "DRWA0002", Name = "Support (V4)", Category = "Prop Support", Description = "Prop support V4", Specification = "Min - Max Length", Material = "Steel" },
            new Product { Code = "DRTA0005", Name = "Pipe Head Adaptor", Category = "Prop Support", Description = "Pipe head adaptor", Specification = "Pipe Dia.", Material = "Steel" },
            new Product { Code = "DBAA0000", Name = "D-Cone 1/2 - 40MM", Category = "Cones", Description = "D-cone 1/2 40mm", Specification = "1/2 40MM", Material = "Steel + PVC" },
            new Product { Code = "DBAB0000", Name = "D-Cone 5/8 - 60MM", Category = "Cones", Description = "D-cone 5/8 60mm", Specification = "5/8 60MM", Material = "Steel + PVC" },
            new Product { Code = "DCAA0001", Name = "Pin (KK-Type)", Category = "Pins", Description = "Pin KK type", Specification = "KK", Material = "Steel" },
            new Product { Code = "DCAA0015", Name = "Pin (ALFA-Type)", Category = "Pins", Description = "Pin ALFA type", Specification = "ASIA", Material = "Steel" },
            new Product { Code = "DCAB0059", Name = "Pin (AO-Type)", Category = "Pins", Description = "Pin AO type", Specification = "A-ONE", Material = "Steel" },
            new Product { Code = "DCAC0059", Name = "Pin (ALFU-Type)", Category = "Pins", Description = "Pin ALFU type", Specification = "USA", Material = "Steel" },
            new Product { Code = "DCBA0064", Name = "Long Pin 64L", Category = "Pins", Description = "Long pin 64L", Specification = "ALF - Form Clip", Material = "Steel" },
            new Product { Code = "DCBB0100", Name = "Long Pin 100L", Category = "Pins", Description = "Long pin 100L", Specification = "HD - 100L", Material = "Steel" },
            new Product { Code = "DCBB0150", Name = "Long Pin 150L", Category = "Pins", Description = "Long pin 150L", Specification = "SM - 150L", Material = "Steel" },
            new Product { Code = "DCBB0152", Name = "Long Pin 152L", Category = "Pins", Description = "Long pin 152L", Specification = "KK - 152L", Material = "Steel" },
            new Product { Code = "DCBC0157", Name = "Long Pin 157L", Category = "Pins", Description = "Long pin 157L", Specification = "ALF - Pin", Material = "Steel" },
            new Product { Code = "DCCA0001", Name = "Wedge (ALFA-Type)", Category = "Wedges", Description = "Wedge ALFA type", Specification = "ASIA", Material = "Steel" },
            new Product { Code = "DCCB0001", Name = "Wedge (AO-Type)", Category = "Wedges", Description = "Wedge AO type", Specification = "A-ONE", Material = "Steel" },
            new Product { Code = "DCCC0001", Name = "Straight Wedge (ALFU-Type)", Category = "Wedges", Description = "Straight wedge ALFU", Specification = "USA", Material = "Steel" },
            new Product { Code = "DCCD0001", Name = "5 Degree Curved Wedge (ALFU-Type)", Category = "Wedges", Description = "5 degree curved wedge ALFU", Specification = "USA", Material = "Steel" },
            new Product { Code = "DCCE0001", Name = "Curved Wedge (ALFU-Type)", Category = "Wedges", Description = "Curved wedge ALFU", Specification = "USA", Material = "Steel" },
            new Product { Code = "DDAA0001", Name = "Adjustable Waler Bracket (ALFA-Type)", Category = "Waler", Description = "Adjustable waler bracket ALFA", Specification = "50x50", Material = "Steel" },
            new Product { Code = "DDAA0003", Name = "Adjustable Waler Bracket (ALFU-Type)", Category = "Waler", Description = "Adjustable waler bracket ALFU", Specification = "2x4", Material = "Steel" },
            new Product { Code = "DDBA0001", Name = "Std. Waler (ALFU-Type)", Category = "Waler", Description = "Standard waler ALFU", Specification = "2x4", Material = "Steel" },
            new Product { Code = "DRMA", Name = "Waler Board 50x50x3.2t", Category = "Waler", Description = "Waler board", Specification = "Length (M)", Material = "Steel" },
            new Product { Code = "DRMA001", Name = "Waler Band Set", Category = "Waler", Description = "Waler band set", Specification = "Width x Length", Material = "Steel" },
            new Product { Code = "DDCA0099", Name = "KL Bracket U-Type - 99.2MM", Category = "Brackets", Description = "KL bracket U type 99.2", Specification = "U-99.2MM", Material = "Steel" },
            new Product { Code = "DDCB0099", Name = "KL Bracket Z-Type - 99.2MM", Category = "Brackets", Description = "KL bracket Z type 99.2", Specification = "Z-99.2MM", Material = "Steel" },
            new Product { Code = "DDCE0092", Name = "KL Bracket U-Type - 92.5MM", Category = "Brackets", Description = "KL bracket U type 92.5", Specification = "U-92.5MM", Material = "Steel" },
            new Product { Code = "DDCF0092", Name = "KL Bracket Z-Type - 92.5MM", Category = "Brackets", Description = "KL bracket Z type 92.5", Specification = "Z-92.5MM", Material = "Steel" },
            new Product { Code = "DEAA0600", Name = "Std. Wall Bracket (DYVIDAG-Type)", Category = "Brackets", Description = "Standard wall bracket", Specification = "1150X1000X600", Material = "Steel" },
            new Product { Code = "DEAA0740", Name = "Wall Bracket (TIE-Type)", Category = "Brackets", Description = "Wall bracket tie type", Specification = "1070X950X740", Material = "Steel" },
            new Product { Code = "DEBA1000", Name = "Slab Bracket", Category = "Brackets", Description = "Slab bracket", Specification = "1150X1000", Material = "Steel" },
            new Product { Code = "DECA0245", Name = "Special Wall Bracket", Category = "Brackets", Description = "Special wall bracket", Specification = "1150X1000X245", Material = "Steel" },
            new Product { Code = "DFAA", Name = "Bracket Bolt", Category = "Bolts", Description = "Bracket bolt", Specification = "17 x Length", Material = "Steel" },
            new Product { Code = "DFAB1600", Name = "Kicker Anchor Nut", Category = "Anchors", Description = "Kicker anchor nut", Specification = "M16 x 2.0", Material = "Steel" },
            new Product { Code = "DFAB1601", Name = "Kicker Anchor Washer", Category = "Anchors", Description = "Kicker anchor washer", Specification = "M16", Material = "Steel" },
            new Product { Code = "DFAB1610", Name = "Anchor Sleeve 100MM", Category = "Anchors", Description = "Anchor sleeve", Specification = "100MM", Material = "PVC" },
            new Product { Code = "DFAB1675", Name = "Kicker Anchor Bolt", Category = "Anchors", Description = "Kicker anchor bolt", Specification = "M16x75L", Material = "Steel" },
            new Product { Code = "DFAC1610", Name = "Dywidag Kicker Anchor Bolt", Category = "Anchors", Description = "Dywidag kicker anchor bolt", Specification = "100mm", Material = "Steel" },
            new Product { Code = "DFAC1611", Name = "Dywidag Kicker Anchor AL-Nut", Category = "Anchors", Description = "Dywidag kicker anchor aluminum nut", Specification = "Aluminum", Material = "Aluminum" },
            new Product { Code = "DFAC1635", Name = "Panel Join - Bolt", Category = "Bolts", Description = "Panel join bolt", Specification = "M16x35", Material = "Steel" },
            new Product { Code = "DFAC1636", Name = "Panel Join - Nut", Category = "Bolts", Description = "Panel join nut", Specification = "M16", Material = "Steel" },
            new Product { Code = "DFAE", Name = "Dywidag Bolt", Category = "Bolts", Description = "Dywidag bolt", Specification = "17 x Length", Material = "Steel" },
            new Product { Code = "DFAF0150", Name = "Waler Fixing Bolt (Hex Bolt-Type)", Category = "Bolts", Description = "Waler fixing bolt hex", Specification = "M16*35 - Length", Material = "Steel" },
            new Product { Code = "DFAG0200", Name = "Waler Fixing Bolt (Pin-Type) - 5/8", Category = "Bolts", Description = "Waler fixing bolt pin 5/8", Specification = "Length", Material = "Steel" },
            new Product { Code = "DFAH2012", Name = "Waler Fixing Bolt (Pin-Type) - 1/2", Category = "Bolts", Description = "Waler fixing bolt pin 1/2", Specification = "Length", Material = "Steel" },
            new Product { Code = "DHAA0001", Name = "Wing Nut 1/2", Category = "Nuts", Description = "Wing nut 1/2", Specification = "1/2", Material = "Cast-iron" },
            new Product { Code = "DHBA0001", Name = "Wing Nut 5/8", Category = "Nuts", Description = "Wing nut 5/8", Specification = "5/8", Material = "Cast-iron" },
            new Product { Code = "DIAA0001", Name = "Form Clip-LH (ALFA-Type)", Category = "Form Clips", Description = "Form clip LH ALFA", Specification = "LH(Asia)", Material = "Steel" },
            new Product { Code = "DIAB0001", Name = "Form Clip-RH (ALFA-Type)", Category = "Form Clips", Description = "Form clip RH ALFA", Specification = "RH(Asia)", Material = "Steel" },
            new Product { Code = "DIBA0001", Name = "Form Clip-LH (ALFU-Type)", Category = "Form Clips", Description = "Form clip LH ALFU", Specification = "LH(USA)", Material = "Steel" },
            new Product { Code = "DIBB0001", Name = "Form Clip-RH (ALFU-Type)", Category = "Form Clips", Description = "Form clip RH ALFU", Specification = "RH(USA)", Material = "Steel" },
            new Product { Code = "DJAC0001", Name = "Pin Lock PVC Cylinder", Category = "Pin Locks", Description = "Pin lock PVC cylinder", Specification = "PVC", Material = "PVC" },
            new Product { Code = "DJBA0001", Name = "Pin Lock LH-16.5 (WALL)", Category = "Pin Locks", Description = "Pin lock LH wall", Specification = "LH(Asia)", Material = "Steel + PVC" },
            new Product { Code = "DJBB0001", Name = "Pin Lock RH-16.5 (WALL)", Category = "Pin Locks", Description = "Pin lock RH wall", Specification = "RH(Asia)", Material = "Steel + PVC" },
            new Product { Code = "DKAA", Name = "PVC Tie Sleeve", Category = "PVC", Description = "PVC tie sleeve", Specification = "Wall thickness (mm)", Material = "PVC" },
            new Product { Code = "DLAA0000", Name = "PVC Pipe 22, 2M", Category = "PVC", Description = "PVC pipe", Specification = "22 / 2M", Material = "PVC" },
            new Product { Code = "DLAA0002", Name = "PVC Pipe 1/2, 2M", Category = "PVC", Description = "PVC pipe 1/2", Specification = "1/2 - 2M", Material = "PVC" },
            new Product { Code = "DLAA0003", Name = "PVC Pipe 5/8, 2M", Category = "PVC", Description = "PVC pipe 5/8", Specification = "5/8 - 2M", Material = "PVC" },
            new Product { Code = "DQAA04000900", Name = "Door Brace 400~900", Category = "Braces", Description = "Door brace 400-900", Specification = "400~900", Material = "Steel" },
            new Product { Code = "DQAA05000700", Name = "Door Brace 500~700", Category = "Braces", Description = "Door brace 500-700", Specification = "600", Material = "Steel" },
            new Product { Code = "DQAA06000800", Name = "Door Brace 600~800", Category = "Braces", Description = "Door brace 600-800", Specification = "600~800", Material = "Steel" },
            new Product { Code = "DQAA07000900", Name = "Door Brace 700~900", Category = "Braces", Description = "Door brace 700-900", Specification = "700~900", Material = "Steel" },
            new Product { Code = "DQAA07001100", Name = "Door Brace 700~1100", Category = "Braces", Description = "Door brace 700-1100", Specification = "700~1100", Material = "Steel" },
            new Product { Code = "DQAA07500950", Name = "Door Brace 750~950", Category = "Braces", Description = "Door brace 750-950", Specification = "750~950", Material = "Steel" },
            new Product { Code = "DQAA09001100", Name = "Door Brace 900~1100", Category = "Braces", Description = "Door brace 900-1100", Specification = "900~1100", Material = "Steel" },
            new Product { Code = "DQAA09001600", Name = "Door Brace 900~1600", Category = "Braces", Description = "Door brace 900-1600", Specification = "900~1600", Material = "Steel" },
            new Product { Code = "DQAA09501100", Name = "Door Brace 950~1100", Category = "Braces", Description = "Door brace 950-1100", Specification = "950~1100", Material = "Steel" },
            new Product { Code = "DQAA10501200", Name = "Door Brace 1050~1200", Category = "Braces", Description = "Door brace 1050-1200", Specification = "1050~1200", Material = "Steel" },
            new Product { Code = "DQAA11001300", Name = "Door Brace 1100~1300", Category = "Braces", Description = "Door brace 1100-1300", Specification = "1100~1300", Material = "Steel" },
            new Product { Code = "DQAA11501300", Name = "Door Brace 1150~1300", Category = "Braces", Description = "Door brace 1150-1300", Specification = "1150~1300", Material = "Steel" },
            new Product { Code = "DQAA12001400", Name = "Door Brace 1200~1400", Category = "Braces", Description = "Door brace 1200-1400", Specification = "1200~1400", Material = "Steel" },
            new Product { Code = "DQAA14001600", Name = "Door Brace 1400~1600", Category = "Braces", Description = "Door brace 1400-1600", Specification = "1400~1600", Material = "Steel" },
            new Product { Code = "DQAA16001800", Name = "Door Brace 1600~1800", Category = "Braces", Description = "Door brace 1600-1800", Specification = "1600~1800", Material = "Steel" },
            new Product { Code = "DQAA18002000", Name = "Door Brace 1800~2000", Category = "Braces", Description = "Door brace 1800-2000", Specification = "1800~2000", Material = "Steel" },
            new Product { Code = "DEDA0001", Name = "Low Control Brace", Category = "Braces", Description = "Low control brace", Specification = "600L", Material = "Steel" },
            new Product { Code = "DQAE2000", Name = "Plumbing Wall Brace 2000", Category = "Braces", Description = "Plumbing wall brace 2000", Specification = "2000[2400H]", Material = "Steel" },
            new Product { Code = "DQAE2200", Name = "Plumbing Wall Brace 2200", Category = "Braces", Description = "Plumbing wall brace 2200", Specification = "2200[3000H]", Material = "Steel" },
            new Product { Code = "DQAE2700", Name = "Plumbing Wall Brace 2700", Category = "Braces", Description = "Plumbing wall brace 2700", Specification = "2700[3500H]", Material = "Steel" },
            new Product { Code = "DQAE2800", Name = "Plumbing Wall Brace 2800", Category = "Braces", Description = "Plumbing wall brace 2800", Specification = "2800[3500H]", Material = "Steel" },
            new Product { Code = "DQAG3000", Name = "Plumbing Wall Brace 3000", Category = "Braces", Description = "Plumbing wall brace 3000", Specification = "3000", Material = "Steel" },
            new Product { Code = "DZAA", Name = "Push-Pull Bracing Set", Category = "Braces", Description = "Push-pull bracing set", Specification = "Long 1800L & Short 800L", Material = "Steel" },
            new Product { Code = "DQAB0001", Name = "Cap Braces (ALFU-Type)", Category = "Braces", Description = "Cap braces ALFU", Specification = "STD(USA)", Material = "Steel" },
            new Product { Code = "DQAB0700", Name = "Cap Braces (Special)", Category = "Braces", Description = "Cap braces special", Specification = "Special(700)", Material = "Steel" },
            new Product { Code = "DQAF0600", Name = "Cap Braces (ALFA-Type)", Category = "Braces", Description = "Cap braces ALFA", Specification = "STD(Asia)", Material = "Steel" },
            new Product { Code = "DPAA0001", Name = "Tie Keeper (Omniwedge)", Category = "Tools", Description = "Tie keeper", Specification = "Omniwedge", Material = "Steel" },
            new Product { Code = "SPONGESLEEVE400", Name = "Sponge Tie Sleeve 400MM", Category = "Tools", Description = "Sponge tie sleeve", Specification = "400MM", Material = "Sponge" },
            new Product { Code = "DRAA1710", Name = "Bracket Flange Nut", Category = "Tools", Description = "Bracket flange nut", Specification = "17-100", Material = "Cast-iron" },
            new Product { Code = "DRBA0001", Name = "Tie Puller", Category = "Tools", Description = "Tie puller", Specification = "Standard", Material = "Steel" },
            new Product { Code = "DRAA0001", Name = "Pin Lock Stripping Tool", Category = "Tools", Description = "Pin lock stripping tool", Specification = "Standard", Material = "Cast-iron" },
            new Product { Code = "SCRAPER001", Name = "Scraper", Category = "Tools", Description = "Scraper", Specification = "Standard", Material = "Steel + PVC" },
            new Product { Code = "DRCA0002", Name = "Panel Puller", Category = "Tools", Description = "Panel puller", Specification = "Y style", Material = "Steel" },
            new Product { Code = "DRDA0001", Name = "Hole Aligner", Category = "Tools", Description = "Hole aligner", Specification = "Standard", Material = "Steel" },
            new Product { Code = "DRFA0001", Name = "Tie Breaker Bar", Category = "Tools", Description = "Tie breaker bar", Specification = "Standard", Material = "Steel" },
            new Product { Code = "DRGA0001", Name = "Sleeve Eject Bar", Category = "Tools", Description = "Sleeve eject bar", Specification = "Standard", Material = "Steel" },
            new Product { Code = "DRNA0002", Name = "Work Bench (1000H)", Category = "Tools", Description = "Work bench 1000H", Specification = "1200x500x1000(H)", Material = "Steel" },
            new Product { Code = "DRNA0004", Name = "Work Bench (750H)", Category = "Tools", Description = "Work bench 750H", Specification = "1200X500X750(H)", Material = "Steel" },
            new Product { Code = "DROB0001", Name = "Wire Turnbuckle", Category = "Tools", Description = "Wire turnbuckle", Specification = "5/8*6M", Material = "Steel" },
            new Product { Code = "PLYWOODADPT", Name = "Plywood Adaptor", Category = "Tools", Description = "Plywood adaptor", Specification = "Standard", Material = "Aluminum" },
            new Product { Code = "DTGA0001", Name = "PVC Cone", Category = "Cones", Description = "PVC cone", Specification = "Standard", Material = "PVC" },
            new Product { Code = "DUAA0001", Name = "Square Washer", Category = "Nuts", Description = "Square washer", Specification = "Standard", Material = "Steel" },
            new Product { Code = "DZAA0004", Name = "Double Waler Nut Clamp", Category = "Waler", Description = "Double waler nut clamp", Specification = "Standard", Material = "Steel" },
            new Product { Code = "DZAA0005", Name = "Double Waler Clamp Washer", Category = "Waler", Description = "Double waler clamp washer", Specification = "130X50", Material = "Steel" },
            new Product { Code = "DZAA0006", Name = "Plastic Cap 16", Category = "PVC", Description = "Plastic cap 16", Specification = "16", Material = "PVC" },
            new Product { Code = "DZAA0008", Name = "Plastic Cap 18", Category = "PVC", Description = "Plastic cap 18", Specification = "18", Material = "PVC" }
        });

        await db.SaveChangesAsync();
    }
}
