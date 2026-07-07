using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Minimart_Api.DTOS.Products;
using Minimart_Api.Models;

namespace Minimart_Api.Data
{
    public class MinimartDBContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public MinimartDBContext(DbContextOptions<MinimartDBContext> options) : base(options) { }
        
        // Only modern Identity system models
        public virtual DbSet<Addresses> Addresses { get; set; }
        public virtual DbSet<Cart> Cart { get; set; }
        public virtual DbSet<CartItem> CartItems { get; set; }
        public virtual DbSet<SavedItems> SavedItems { get; set; }

        // Merchant system models
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<SubCategory> SubCategories { get; set; }
        public virtual DbSet<SubSubCategory> SubSubCategories { get; set; }
        public virtual DbSet<Merchants> Merchants { get; set; }

        public virtual DbSet<Counties> Counties { get; set; }
        public virtual DbSet<DeliveryStations> DeliveryStations { get; set; }
        public virtual DbSet<Features> Features { get; set; }
        public virtual DbSet<OrderItem> OrderItems { get; set; }
        public virtual DbSet<OrderProduct> OrderProducts { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderStatus> OrderStatuses { get; set; }
        public virtual DbSet<OrderTracking> OrderTracking{ get; set; }
        public virtual DbSet<PaymentDetails> PaymentDetails { get; set; }
        public virtual DbSet<PaymentMethods> PaymentMethods { get; set; }
        public virtual DbSet<MerchantPaymentMethod> MerchantPaymentMethods { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Reviews> Reviews { get; set; }
        public virtual DbSet<Towns> Towns { get; set; }

        // Authentication related models
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
        public virtual DbSet<UserLoginAttempt> UserLoginAttempts { get; set; }

        public DbSet<MpesaTransaction> MpesaTransactions { get; set; }

        // Payout system models
        public virtual DbSet<Payout> Payouts { get; set; }
        public virtual DbSet<PayoutTransaction> PayoutTransactions { get; set; }

        public virtual DbSet<SlugRedirect> SlugRedirects { get; set; } // ADD THIS LINE

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Call base method first for Identity tables
            base.OnModelCreating(modelBuilder);

            // Configure authentication models
            ConfigureAuthenticationEntities(modelBuilder);

            // Products configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(p => p.ProductId);

                // Configure category relationships with Guid IDs
                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.SubCategory)
                    .WithMany(sc => sc.Products)
                    .HasForeignKey(p => p.SubCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.SubSubCategory)
                    .WithMany(ssc => ssc.Products)
                    .HasForeignKey(p => p.SubSubCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Merchant)
                    .WithMany(m => m.Products)
                    .HasForeignKey(p => p.MerchantID)
                    .OnDelete(DeleteBehavior.Restrict);

                // Configure audit fields
                entity.Property(p => p.CreatedOn).HasColumnType("timestamp with time zone");
                entity.Property(p => p.UpdatedOn).HasColumnType("timestamp with time zone");
                entity.Property(p => p.DeletedOn).HasColumnType("timestamp with time zone");

                // Composite: category filter is always the leading column
                entity.HasIndex(p => new { p.CategoryId, p.IsDeleted, p.IsActive, p.Status });

                // Facet GROUP BY columns
                entity.HasIndex(p => new { p.CategoryId, p.Brand });
                entity.HasIndex(p => new { p.CategoryId, p.SubCategoryId });

                // Sort columns paired with CategoryId
                entity.HasIndex(p => new { p.CategoryId, p.Price });
                entity.HasIndex(p => new { p.CategoryId, p.Discount });
                entity.HasIndex(p => new { p.CategoryId, p.CreatedOn });

                // In-stock filter
                entity.HasIndex(p => new { p.CategoryId, p.StockQuantity });

                // Configure ImageUrls as text array
                entity.Property(p => p.ImageUrls).HasColumnType("text[]");
            });

            // Features configuration
            modelBuilder.Entity<Features>(entity =>
            {
                entity.ToTable("Features");
                entity.HasKey(f => f.FeatureID);

                // Configure category system relationships
                entity.HasOne(f => f.Category)
                    .WithMany()
                    .HasForeignKey(f => f.CategoryID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.SubCategory)
                    .WithMany()
                    .HasForeignKey(f => f.SubCategoryID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.SubSubCategory)
                    .WithMany()
                    .HasForeignKey(f => f.SubSubCategoryID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Category system configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(c => c.CategoryId);

                entity.HasOne(c => c.Merchant)
                    .WithMany(m => m.Categories)
                    .HasForeignKey(c => c.MerchantID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.Parent)
                    .WithMany(c => c.Children)
                    .HasForeignKey(c => c.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SubCategory>(entity =>
            {
                entity.ToTable("SubCategories");
                entity.HasKey(sc => sc.SubCategoryId);

                entity.HasOne(sc => sc.Category)
                    .WithMany(c => c.SubCategories)
                    .HasForeignKey(sc => sc.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SubSubCategory>(entity =>
            {
                entity.ToTable("SubSubCategories");
                entity.HasKey(ssc => ssc.SubSubCategoryId);

                entity.HasOne(ssc => ssc.SubCategory)
                    .WithMany(sc => sc.SubSubCategories)
                    .HasForeignKey(ssc => ssc.SubCategoryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure other important entities
            ConfigureOrderEntities(modelBuilder);
            ConfigureUserEntities(modelBuilder);
            ConfigureAdditionalEntities(modelBuilder);
        }

        private void ConfigureAuthenticationEntities(ModelBuilder modelBuilder)
        {
            // Configure RefreshToken entity
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("RefreshTokens");
                entity.HasKey(rt => rt.Id);

                entity.Property(rt => rt.Token).IsRequired().HasMaxLength(500);
                entity.Property(rt => rt.JwtId).IsRequired().HasMaxLength(200);
                entity.Property(rt => rt.ApplicationUserId).IsRequired().HasMaxLength(450);

                entity.Property(rt => rt.CreationDate).HasColumnType("timestamp with time zone");
                entity.Property(rt => rt.ExpiryDate).HasColumnType("timestamp with time zone");

                // Configure relationship with ApplicationUser
                entity.HasOne(rt => rt.User)
                    .WithMany()
                    .HasForeignKey(rt => rt.ApplicationUserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Add indexes for performance
                entity.HasIndex(rt => rt.Token).IsUnique();
                entity.HasIndex(rt => rt.JwtId);
                entity.HasIndex(rt => rt.ApplicationUserId);
                entity.HasIndex(rt => rt.ExpiryDate);
            });

            // Configure UserLoginAttempt entity
            modelBuilder.Entity<UserLoginAttempt>(entity =>
            {
                entity.ToTable("UserLoginAttempts");
                entity.HasKey(ula => ula.Id);

                entity.Property(ula => ula.Email).IsRequired().HasMaxLength(255);
                entity.Property(ula => ula.IpAddress).IsRequired().HasMaxLength(45);
                entity.Property(ula => ula.UserAgent).HasMaxLength(500);
                entity.Property(ula => ula.FailureReason).HasMaxLength(255);

                entity.Property(ula => ula.AttemptDate).HasColumnType("timestamp with time zone");

                // Add indexes for performance and security queries
                entity.HasIndex(ula => ula.Email);
                entity.HasIndex(ula => ula.IpAddress);
                entity.HasIndex(ula => ula.AttemptDate);
                entity.HasIndex(ula => new { ula.Email, ula.AttemptDate });
                entity.HasIndex(ula => new { ula.IpAddress, ula.AttemptDate });
            });
        }

        private void ConfigureAdditionalEntities(ModelBuilder modelBuilder)
        {
            // Configure SlugRedirect entity (SEO)
            modelBuilder.Entity<SlugRedirect>(entity =>
            {
                entity.ToTable("SlugRedirects");
                entity.HasKey(sr => sr.RedirectId); // FIXED: Changed from Id to RedirectId
                
                entity.Property(sr => sr.CreatedAt)
                    .HasColumnType("timestamp with time zone");
                
                entity.Property(sr => sr.IsActive)
                    .HasDefaultValue(true);
                
                entity.HasOne(sr => sr.Product)
                    .WithMany()
                    .HasForeignKey(sr => sr.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(sr => sr.OldSlug);
                entity.HasIndex(sr => sr.NewSlug);
                entity.HasIndex(sr => sr.ProductId);
                entity.HasIndex(sr => sr.IsActive);
            });

            // Configure CartItem entity
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.ToTable("CartItems");
                entity.HasKey(ci => ci.CartItemId);
                
                // Configure relationship with Product
                entity.HasOne(ci => ci.Product)
                    .WithMany(p => p.CartItems)
                    .HasForeignKey(ci => ci.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                // Add indexes
                entity.HasIndex(ci => ci.CartId);
                entity.HasIndex(ci => ci.ProductId);
            });

            // Configure Counties entity
            modelBuilder.Entity<Counties>(entity =>
            {
                entity.ToTable("Counties");
                entity.HasKey(c => c.CountyId);
                
                entity.Property(c => c.CreatedOn).HasColumnType("timestamp with time zone");
                
                // Configure relationship with Towns
                entity.HasMany(c => c.Towns)
                    .WithOne(t => t.County)
                    .HasForeignKey(t => t.CountyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Towns entity
            modelBuilder.Entity<Towns>(entity =>
            {
                entity.ToTable("Towns");
                entity.HasKey(t => t.TownId);
                
                entity.Property(t => t.CreatedOn).HasColumnType("timestamp with time zone");
                
                // Configure relationship with DeliveryStations
                entity.HasMany(t => t.DeliveryStations)
                    .WithOne(ds => ds.Town)
                    .HasForeignKey(ds => ds.TownId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                // Add index
                entity.HasIndex(t => t.CountyId);
            });

            // Configure DeliveryStations entity
            modelBuilder.Entity<DeliveryStations>(entity =>
            {
                entity.ToTable("DeliveryStations");
                entity.HasKey(ds => ds.DeliveryStationId);
                
                entity.Property(ds => ds.CreatedOn).HasColumnType("timestamp with time zone");
                
                // Add indexes
                entity.HasIndex(ds => ds.TownId);
            });

            // Configure PaymentMethods entity
            modelBuilder.Entity<PaymentMethods>(entity =>
            {
                entity.ToTable("PaymentMethods");
                entity.HasKey(pm => pm.PaymentMethodID);
                
                // Configure relationship with PaymentDetails
                entity.HasMany(pm => pm.PaymentDetails)
                    .WithOne(pd => pd.PaymentMethod)
                    .HasForeignKey(pd => pd.PaymentMethodID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure MerchantPaymentMethod entity
            modelBuilder.Entity<MerchantPaymentMethod>(entity =>
            {
                entity.ToTable("MerchantPaymentMethods");
                entity.HasKey(mpm => mpm.Id);
                
                // Configure relationship with Merchant
                entity.HasOne(mpm => mpm.Merchant)
                    .WithMany(m => m.MerchantPaymentMethods)
                    .HasForeignKey(mpm => mpm.MerchantId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                // Configure relationship with PaymentMethod
                entity.HasOne(mpm => mpm.PaymentMethod)
                    .WithMany(pm => pm.MerchantPaymentMethods)
                    .HasForeignKey(mpm => mpm.PaymentMethodId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Add unique constraint - one payment method per merchant
                entity.HasIndex(mpm => new { mpm.MerchantId, mpm.PaymentMethodId })
                    .IsUnique()
                    .HasDatabaseName("UQ_MerchantPaymentMethods_MerchantId_PaymentMethodId");
                
                // Add indexes for performance
                entity.HasIndex(mpm => mpm.MerchantId)
                    .HasDatabaseName("IX_MerchantPaymentMethods_MerchantId");
                entity.HasIndex(mpm => mpm.PaymentMethodId)
                    .HasDatabaseName("IX_MerchantPaymentMethods_PaymentMethodId");
                entity.HasIndex(mpm => mpm.IsEnabled)
                    .HasDatabaseName("IX_MerchantPaymentMethods_IsEnabled");
                
                // Configure timestamp fields
                entity.Property(mpm => mpm.CreatedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("NOW()");
                    
                entity.Property(mpm => mpm.UpdatedAt)
                    .HasColumnType("timestamp with time zone");
                    
                // Configure Configuration field
                entity.Property(mpm => mpm.Configuration)
                    .HasMaxLength(500);
            });

            // Configure PaymentDetails entity
            modelBuilder.Entity<PaymentDetails>(entity =>
            {
                entity.ToTable("PaymentDetails");
                entity.HasKey(pd => pd.PaymentID);
                
                // Add indexes for performance
                entity.HasIndex(pd => pd.TrxReference).IsUnique();
                entity.HasIndex(pd => pd.PaymentReference);
                entity.HasIndex(pd => pd.PaymentMethodID);
            });

            // Configure MpesaTransaction entity
            modelBuilder.Entity<MpesaTransaction>(entity =>
            {
                entity.ToTable("MpesaTransactions");
                entity.HasKey(mt => mt.Id);
                
                entity.Property(mt => mt.CreatedAt).HasColumnType("timestamp with time zone");
                
                // Add useful indexes for Mpesa transactions
                entity.HasIndex(mt => mt.TransID);
                entity.HasIndex(mt => mt.BillRefNumber);
                entity.HasIndex(mt => mt.MSISDN);
                entity.HasIndex(mt => mt.BusinessShortCode);
            });

            // Configure OrderItem entity (if still used)
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("OrderItems");
                entity.HasKey(oi => oi.OrderItemId);
                
                // Configure relationship with Product
                entity.HasOne(oi => oi.Product)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(oi => oi.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                // Add indexes
                entity.HasIndex(oi => oi.ProductId);
            });

            // Configure Payout entities
            ConfigurePayoutEntities(modelBuilder);
        }

        private void ConfigureOrderEntities(ModelBuilder modelBuilder)
        {
            // Order configuration with ApplicationUserId for Identity system
            modelBuilder.Entity<Models.Order>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(o => o.OrderID);
                
                // Configure ApplicationUserId as string for Identity system
                entity.Property(o => o.ApplicationUserId)
                    .HasColumnType("text");

                // Configure Status as string
                entity.Property(o => o.Status)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("varchar(50)")
                    .HasDefaultValue("Pending");

                // Add index for better query performance
                entity.HasIndex(o => o.ApplicationUserId);
                entity.HasIndex(o => o.OrderDate);
                entity.HasIndex(o => o.Status);
                entity.HasIndex(o => o.StatusEnum);

                // Configure relationships
                entity.HasMany(o => o.OrderProducts)
                    .WithOne(op => op.Order)
                    .HasForeignKey(op => op.OrderID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(o => o.PaymentDetails)
                    .WithOne()
                    .HasForeignKey<Models.Order>(o => o.PaymentID)
                    .OnDelete(DeleteBehavior.Cascade);

                    
                // Configure relationship with ApplicationUser
                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(o => o.ApplicationUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // OrderStatus configuration
            modelBuilder.Entity<OrderStatus>(entity =>
            {
                entity.ToTable("OrderStatuses");
                entity.HasKey(os => os.StatusID);
            });

            modelBuilder.Entity<OrderProduct>(entity =>
            {
                entity.HasKey(op => op.OrderProductID);

                entity.HasOne(op => op.Product)
                    .WithMany(p => p.OrderProducts)
                    .HasForeignKey(op => op.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(op => op.Merchant)
                    .WithMany()
                    .HasForeignKey(op => op.MerchantID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(op => op.Order)
                    .WithMany(o => o.OrderProducts)
                    .HasForeignKey(op => op.OrderID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OrderTracking>(entity =>
            {
                entity.ToTable("OrderTracking");
                entity.HasKey(ot => ot.TrackingID);

                // Configure status as strings
                entity.Property(ot => ot.CurrentStatus)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("varchar(50)")
                    .HasDefaultValue("Processing");

                entity.Property(ot => ot.PreviousStatus)
                    .HasMaxLength(50)
                    .HasColumnType("varchar(50)");

                entity.HasOne(ot => ot.Order)
                    .WithMany(o => o.OrderTrackings)
                    .HasForeignKey(ot => ot.OrderID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ot => ot.Product)
                    .WithMany(p => p.OrderTrackings)
                    .HasForeignKey(ot => ot.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureUserEntities(ModelBuilder modelBuilder)
        {
            // Configure Application User - let Identity handle the primary key configuration
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                // Only configure custom properties, let Identity handle Id, UserName, etc.
                entity.Property(u => u.CreatedAt).HasColumnType("timestamp with time zone");
                entity.Property(u => u.LastLogin).HasColumnType("timestamp with time zone");
                entity.Property(u => u.PasswordChangesOn).HasColumnType("timestamp with time zone");
                entity.Property(u => u.LastPasswordReset).HasColumnType("timestamp with time zone");
                entity.Property(u => u.TemporaryPasswordExpiry).HasColumnType("timestamp with time zone");
                entity.Property(u => u.LastLoginDate).HasColumnType("timestamp with time zone");
                
                // Configure navigation properties to related entities
                entity.HasMany(u => u.Addresses)
                    .WithOne(a => a.User)
                    .HasForeignKey(a => a.ApplicationUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(u => u.Carts)
                    .WithOne()
                    .HasForeignKey("ApplicationUserId")
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(u => u.Reviews)
                    .WithOne(r => r.User)
                    .HasForeignKey(r => r.ApplicationUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Merchants entity
            modelBuilder.Entity<Merchants>(entity =>
            {
                entity.ToTable("Merchants");
                entity.HasKey(m => m.MerchantID);
                
                // Ensure ApplicationUserId is properly configured to reference ApplicationUser.Id
                entity.Property(m => m.ApplicationUserId)
                    .HasColumnType("text"); // Match ApplicationUser.Id type
                
                // Configure Documents as PostgreSQL text array
                entity.Property(m => m.Documents)
                    .HasColumnType("text[]");
                
                // Add index for performance
                entity.HasIndex(m => m.ApplicationUserId)
                    .IsUnique(); // One merchant per user
                    
                // Configure relationship with ApplicationUser
                entity.HasOne(m => m.User)
                    .WithOne(u => u.Merchant)
                    .HasForeignKey<Merchants>(m => m.ApplicationUserId)
                    .IsRequired(false);
            });

            // Configure Addresses entity
            modelBuilder.Entity<Addresses>(entity =>
            {
                entity.ToTable("Addresses");
                entity.HasKey(a => a.AddressID);
                
                // Add index for performance
                entity.HasIndex(a => a.ApplicationUserId);
            });

            // Configure Cart entity
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.ToTable("Cart");
                entity.HasKey(c => c.CartId);
                
                // Configure relationship with CartItems
                entity.HasMany(c => c.CartItems)
                    .WithOne(ci => ci.Cart)
                    .HasForeignKey(ci => ci.CartId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                // Add index for performance
                entity.HasIndex(c => c.ApplicationUserId);
            });

            // Configure Reviews entity
            modelBuilder.Entity<Reviews>(entity =>
            {
                entity.ToTable("Reviews");
                entity.HasKey(r => r.ReviewId);
                
                // Configure relationship with Product
                entity.HasOne(r => r.Product)
                    .WithMany(p => p.Reviews)
                    .HasForeignKey(r => r.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                // Add indexes for performance
                entity.HasIndex(r => r.ApplicationUserId);
                entity.HasIndex(r => r.ProductId);
            });

            // Configure SavedItems entity
            modelBuilder.Entity<SavedItems>(entity =>
            {
                entity.ToTable("SavedItems");
                entity.HasKey(s => s.Id);
                
                // Configure relationship with Product
                entity.HasOne(s => s.Product)
                    .WithMany()
                    .HasForeignKey(s => s.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                // Add index for performance
                entity.HasIndex(s => s.ApplicationUserId);
                entity.HasIndex(s => s.ProductId);
            });
        }

        private decimal CalculateMedian(List<decimal> values)
        {
            if (!values.Any()) return 0;
            
            var sorted = values.OrderBy(x => x).ToList();
            int count = sorted.Count;
            
            if (count % 2 == 0)
            {
                return (sorted[count / 2 - 1] + sorted[count / 2]) / 2;
            }
            else
            {
                return sorted[count / 2];
            }
        }

        private void ConfigurePayoutEntities(ModelBuilder modelBuilder)
        {
            // Configure Payout entity
            modelBuilder.Entity<Payout>(entity =>
            {
                entity.ToTable("Payouts");
                entity.HasKey(p => p.PayoutId);

                // Configure relationships
                entity.HasOne(p => p.Merchant)
                    .WithMany()
                    .HasForeignKey(p => p.MerchantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.PaymentMethod)
                    .WithMany()
                    .HasForeignKey(p => p.PaymentMethodId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(p => p.PayoutTransactions)
                    .WithOne(pt => pt.Payout)
                    .HasForeignKey(pt => pt.PayoutId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Configure indexes
                entity.HasIndex(p => p.MerchantId);
                entity.HasIndex(p => p.Status);
                entity.HasIndex(p => p.PeriodStartDate);
                entity.HasIndex(p => p.PeriodEndDate);
                entity.HasIndex(p => p.CreatedDate);
                entity.HasIndex(p => new { p.MerchantId, p.Status });

                // Configure decimal precision
                entity.Property(p => p.GrossAmount).HasColumnType("decimal(18,2)");
                entity.Property(p => p.CommissionAmount).HasColumnType("decimal(18,2)");
                entity.Property(p => p.NetAmount).HasColumnType("decimal(18,2)");
                entity.Property(p => p.CommissionRate).HasColumnType("decimal(5,4)");

                // Configure timestamp fields
                entity.Property(p => p.PeriodStartDate).HasColumnType("timestamp with time zone");
                entity.Property(p => p.PeriodEndDate).HasColumnType("timestamp with time zone");
                entity.Property(p => p.CreatedDate).HasColumnType("timestamp with time zone");
                entity.Property(p => p.ScheduledDate).HasColumnType("timestamp with time zone");
                entity.Property(p => p.CompletedDate).HasColumnType("timestamp with time zone");
                entity.Property(p => p.UpdatedDate).HasColumnType("timestamp with time zone");
            });

            // Configure PayoutTransaction entity
            modelBuilder.Entity<PayoutTransaction>(entity =>
            {
                entity.ToTable("PayoutTransactions");
                entity.HasKey(pt => pt.PayoutTransactionId);

                // Configure relationships
                entity.HasOne(pt => pt.Payout)
                    .WithMany(p => p.PayoutTransactions)
                    .HasForeignKey(pt => pt.PayoutId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pt => pt.Order)
                    .WithMany()
                    .HasForeignKey(pt => pt.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Configure indexes
                entity.HasIndex(pt => pt.PayoutId);
                entity.HasIndex(pt => pt.OrderId);
                entity.HasIndex(pt => pt.OrderCompletedDate);
                entity.HasIndex(pt => pt.CreatedDate);

                // Configure decimal precision
                entity.Property(pt => pt.OrderAmount).HasColumnType("decimal(18,2)");
                entity.Property(pt => pt.CommissionAmount).HasColumnType("decimal(18,2)");
                entity.Property(pt => pt.NetAmount).HasColumnType("decimal(18,2)");
                entity.Property(pt => pt.CommissionRate).HasColumnType("decimal(5,4)");

                // Configure timestamp fields
                entity.Property(pt => pt.OrderCompletedDate).HasColumnType("timestamp with time zone");
                entity.Property(pt => pt.CreatedDate).HasColumnType("timestamp with time zone");
            });
        }
    }
}