using Microsoft.EntityFrameworkCore;
using Relyf.Api.Models;

namespace Relyf.Api;

public class RelyfDbContext(DbContextOptions<RelyfDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasDefaultSchema("app");

        // --- User ---
        b.Entity<User>(e =>
        {
            e.ToTable("User");
            e.HasKey(x => x.UserId);
            e.Property(x => x.Email).HasMaxLength(256).IsRequired();
            e.Property(x => x.DisplayName).HasMaxLength(120).IsRequired();
            e.Property(x => x.CountryCode).HasMaxLength(2);
        });

        // --- Item ---
        b.Entity<Item>(e =>
        {
            e.ToTable("Item");
            e.HasKey(x => x.ItemId);
            e.Property(x => x.Title).HasMaxLength(140).IsRequired();
            e.Property(x => x.Description).HasMaxLength(1000);
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId);
        });

        // --- CoherePrompt ---
        b.Entity<CoherePrompt>(e =>
        {
            e.ToTable("CoherePrompt");
            e.HasKey(x => x.CoherePromptId);
            e.Property(x => x.Model).HasMaxLength(80);
            e.Property(x => x.Temperature).HasColumnType("decimal(4,2)");
            e.Property(x => x.TopP).HasColumnType("decimal(4,2)");
            e.Property(x => x.PromptText).IsRequired();

            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId);
            e.HasOne<Item>().WithMany().HasForeignKey(x => x.ItemId);
        });

        // --- AiIdea ---
        b.Entity<AiIdea>(e =>
        {
            e.ToTable("AiIdea");
            e.HasKey(x => x.IdeaId);
            e.Property(x => x.Title).HasMaxLength(160).IsRequired();
            e.Property(x => x.IdeaText).IsRequired();
            e.Property(x => x.Difficulty).HasMaxLength(20);
            e.Property(x => x.EstCostUSD).HasColumnType("decimal(10,2)");

            e.HasOne<CoherePrompt>().WithMany().HasForeignKey(x => x.CoherePromptId);
            e.HasOne<Item>().WithMany().HasForeignKey(x => x.ItemId);
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId);
        });

        // --- ApiRequestLog ---
        b.Entity<ApiRequestLog>(e =>
        {
            e.ToTable("ApiRequestLog");
            e.HasKey(x => x.ApiRequestLogId);
            e.Property(x => x.Provider).HasMaxLength(40).IsRequired();
            e.Property(x => x.Endpoint).HasMaxLength(200).IsRequired();
            e.Property(x => x.Model).HasMaxLength(80);

            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId);
        });

        // --- Project ---
        b.Entity<Project>(e =>
        {
            e.ToTable("Project");
            e.HasKey(x => x.ProjectId);
            e.Property(x => x.Title).HasMaxLength(160).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("draft");
            e.HasOne<AiIdea>().WithMany().HasForeignKey(x => x.IdeaId);
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId);
        });

        // --- ProjectStep ---
        b.Entity<ProjectStep>(e =>
        {
            e.ToTable("ProjectStep");
            e.HasKey(x => x.ProjectStepId);
            e.Property(x => x.Instruction).HasMaxLength(1500).IsRequired();
            e.HasOne<Project>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.ProjectId, x.StepNumber }).IsUnique();
        });

        // --- ProjectMaterial ---
        b.Entity<ProjectMaterial>(e =>
        {
            e.ToTable("ProjectMaterial");
            e.HasKey(x => new { x.ProjectId, x.MaterialId });
            e.Property(x => x.QuantityText).HasMaxLength(80);
            e.HasOne<Project>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            // Material table exists in DB; you can add a light POCO later if you plan to query it via EF.
        });

        // --- SavedIdea ---
        b.Entity<SavedIdea>(e =>
        {
            e.ToTable("SavedIdea");
            e.HasKey(x => new { x.UserId, x.IdeaId });
            e.Property(x => x.SavedAtUtc).HasColumnType("datetime2(3)");
        });

        // --- Reaction ---
        b.Entity<Reaction>(e =>
        {
            e.ToTable("Reaction");
            e.HasKey(x => x.ReactionId);
            e.Property(x => x.TargetType).HasMaxLength(20).IsRequired(); // CK in DB: Idea/Project
            e.Property(x => x.Kind).HasMaxLength(20).IsRequired();       // CK in DB: like/upvote/helpful
            e.Property(x => x.CreatedAtUtc).HasColumnType("datetime2(3)");
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId);
            e.HasIndex(x => new { x.UserId, x.TargetType, x.TargetId, x.Kind }).IsUnique(); // match DB UQ
        });

        // --- Image ---
        b.Entity<Image>(e =>
        {
            e.ToTable("Image");
            e.HasKey(x => x.ImageId);
            e.Property(x => x.OwnerType).HasMaxLength(20).IsRequired();  // DB CHECK enforces valid values
            e.Property(x => x.Source).HasMaxLength(20).IsRequired();
            e.Property(x => x.Url).HasMaxLength(500).IsRequired();
            e.Property(x => x.AltText).HasMaxLength(160);
        });

        // --- Tag ---
        b.Entity<Tag>(e =>
        {
            e.ToTable("Tag");
            e.HasKey(x => x.TagId);
            e.Property(x => x.Name).HasMaxLength(80).IsRequired();
        });

        // --- IdeaTag ---
        b.Entity<IdeaTag>(e =>
        {
            e.ToTable("IdeaTag");
            e.HasKey(x => new { x.IdeaId, x.TagId });
            e.HasOne<AiIdea>().WithMany().HasForeignKey(x => x.IdeaId);
            e.HasOne<Tag>().WithMany().HasForeignKey(x => x.TagId);
        });

        // --- Material ---
        b.Entity<Material>(e =>
        {
            e.ToTable("Material");
            e.HasKey(x => x.MaterialId);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Category).HasMaxLength(80);
            e.Property(x => x.Recyclability);
            e.Property(x => x.Notes).HasMaxLength(400);
        });

        // --- ProjectMaterial ---
        b.Entity<ProjectMaterial>(e =>
        {
            e.ToTable("ProjectMaterial");
            e.HasKey(x => new { x.ProjectId, x.MaterialId });
            e.Property(x => x.QuantityText).HasMaxLength(80);
            e.HasOne<Project>().WithMany().HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            // Material FK exists in DB; EF will honor it even without nav props
        });

        // --- DropoffSite ---
        b.Entity<DropoffSite>(e =>
        {
            e.ToTable("DropoffSite");
            e.HasKey(x => x.DropoffSiteId);
            e.Property(x => x.Name).HasMaxLength(160).IsRequired();
            e.Property(x => x.AddressLine1).HasMaxLength(160);
            e.Property(x => x.City).HasMaxLength(80);
            e.Property(x => x.Region).HasMaxLength(80);
            e.Property(x => x.PostalCode).HasMaxLength(20);
            e.Property(x => x.CountryCode).HasMaxLength(2);
            e.Property(x => x.AcceptedNotes).HasMaxLength(400);
        });

        // --- UserDropoff ---
        b.Entity<UserDropoff>(e =>
        {
            e.ToTable("UserDropoff");
            e.HasKey(x => x.UserDropoffId);
            e.Property(x => x.QuantityText).HasMaxLength(80);
            e.Property(x => x.DroppedAtUtc).HasColumnType("datetime2(3)");
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId);
            e.HasOne<DropoffSite>().WithMany().HasForeignKey(x => x.DropoffSiteId);
        });

        // --- ItemMaterial ---
        b.Entity<ItemMaterial>(e =>
        {
            e.ToTable("ItemMaterial");
            e.HasKey(x => new { x.ItemId, x.MaterialId });
            e.Property(x => x.PercentShare); // tinyint in DB; nullable
            e.HasOne<Item>().WithMany().HasForeignKey(x => x.ItemId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<Material>().WithMany().HasForeignKey(x => x.MaterialId);
        });

        // --- Feedback ---
        b.Entity<Feedback>(e =>
        {
            e.ToTable("Feedback");
            e.HasKey(x => x.FeedbackId);
            e.Property(x => x.TargetType).HasMaxLength(20).IsRequired();
            e.Property(x => x.Rating);
            e.Property(x => x.Notes).HasMaxLength(800);
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId);
        });

        // --- Comment ---
        b.Entity<Comment>(e =>
        {
            e.ToTable("Comment");
            e.HasKey(x => x.CommentId);
            e.Property(x => x.TargetType).HasMaxLength(20).IsRequired();
            e.Property(x => x.Body).HasMaxLength(1000).IsRequired();
            e.Property(x => x.CreatedAtUtc)
             .HasColumnType("datetime2(3)")
             .HasDefaultValueSql("sysutcdatetime()");
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId);
        });

        b.Entity<UserCredential>(e =>
        {
            e.ToTable("UserCredential");
            e.HasKey(x => x.UserId);
            e.Property(x => x.PasswordHash).IsRequired();
            e.Property(x => x.PasswordSalt).IsRequired();
            e.Property(x => x.CreatedUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasOne(x => x.User).WithOne().HasForeignKey<UserCredential>(x => x.UserId);
        });


    }

    // DbSets
    public DbSet<User> Users => Set<User>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<CoherePrompt> CoherePrompts => Set<CoherePrompt>();
    public DbSet<AiIdea> AiIdeas => Set<AiIdea>();
    public DbSet<ApiRequestLog> ApiRequestLogs => Set<ApiRequestLog>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectStep> ProjectSteps => Set<ProjectStep>();
    public DbSet<ProjectMaterial> ProjectMaterials => Set<ProjectMaterial>();
    public DbSet<SavedIdea> SavedIdeas => Set<SavedIdea>();
    public DbSet<Reaction> Reactions => Set<Reaction>();
    public DbSet<Image> Images => Set<Image>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<IdeaTag> IdeaTags => Set<IdeaTag>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<DropoffSite> DropoffSites => Set<DropoffSite>();
    public DbSet<UserDropoff> UserDropoffs => Set<UserDropoff>();
    public DbSet<ItemMaterial> ItemMaterials => Set<ItemMaterial>();
    public DbSet<Feedback> Feedback => Set<Feedback>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<UserCredential> UserCredentials => Set<UserCredential>();





}
