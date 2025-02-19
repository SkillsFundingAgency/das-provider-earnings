using Microsoft.EntityFrameworkCore;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.ReadModel;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess
{
    public class ApprenticeshipEarningsDataContext : DbContext
    {
        public ApprenticeshipEarningsDataContext(DbContextOptions<ApprenticeshipEarningsDataContext> options) : base(options)
        {
        }

        public virtual DbSet<Earning> Earning { get; set; } = null!;

        public virtual DbSet<ApprenticeshipModel> Apprenticeships { get; set; }
        public virtual DbSet<EpisodeModel> Episodes { get; set; }
        public virtual DbSet<EpisodePriceModel> EpisodePrices { get; set; }
        public virtual DbSet<EarningsProfileModel> EarningsProfiles { get; set; }
        public virtual DbSet<InstalmentModel> Instalments { get; set; }

        public virtual DbSet<EarningsProfileHistoryModel> EarningsProfileHistories { get; set; }
        public virtual DbSet<InstalmentHistoryModel> InstalmentHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Apprenticeship
            modelBuilder.Entity<ApprenticeshipModel>()
                .HasMany(x => x.Episodes)
                .WithOne()
                .HasForeignKey(fk => fk.ApprenticeshipKey);
            modelBuilder.Entity<ApprenticeshipModel>()
                .HasKey(a => new { a.Key });

            // Episode
            modelBuilder.Entity<EpisodeModel>()
                .HasKey(a => new { a.Key });
            modelBuilder.Entity<EpisodeModel>()
                .HasOne(a => a.EarningsProfile).WithOne().HasForeignKey<EarningsProfileModel>(x => x.EpisodeKey);
            modelBuilder.Entity<EpisodeModel>()
                .HasMany(a => a.EarningsProfileHistory)
                .WithOne()
                .HasForeignKey(x => x.EpisodeKey);
            modelBuilder.Entity<EpisodeModel>()
                .Property(p => p.FundingType)
                .HasConversion(
                    v => v.ToString(),
                    v => (FundingType)Enum.Parse(typeof(FundingType), v));
            modelBuilder.Entity<EpisodeModel>()
                .HasMany(a => a.Prices)
                .WithOne()  
                .HasForeignKey(x => x.EpisodeKey);

            // EpisodePrice
            modelBuilder.Entity<EpisodePriceModel>()
                .HasKey(x => x.Key);

            // EarningsProfile
            modelBuilder.Entity<EarningsProfileModel>()
                .HasKey(x => x.EarningsProfileId);

            modelBuilder.Entity<EarningsProfileModel>()
                .HasMany(x => x.Instalments)
                .WithOne()
                .HasForeignKey(fk => fk.EarningsProfileId);

            modelBuilder.Entity<EarningsProfileModel>()
                .HasMany(x => x.AdditionalPayments)
                .WithOne()
                .HasForeignKey(fk => fk.EarningsProfileId);

            // Instalment
            modelBuilder.Entity<InstalmentModel>()
                .HasKey(x => x.Key);

            // EarningsProfileHistory
            modelBuilder.Entity<EarningsProfileHistoryModel>()
                .HasKey(x => x.EarningsProfileId);

            modelBuilder.Entity<EarningsProfileHistoryModel>()
                .HasMany(x => x.Instalments)
                .WithOne()
                .HasForeignKey(fk => fk.EarningsProfileId);

            // Instalment
            modelBuilder.Entity<InstalmentHistoryModel>()
                .HasKey(x => x.Key);

            // AdditionalPayment
            modelBuilder.Entity<AdditionalPaymentModel>()
                .HasKey(x => x.Key);

            modelBuilder.Entity<AdditionalPaymentHistoryModel>()
                .HasKey(x => x.Key);

            base.OnModelCreating(modelBuilder);
        }
    }
}
