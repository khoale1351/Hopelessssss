using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Travel.Models;

namespace Travel.Data
{
    public partial class TourismDbContext : IdentityDbContext<ApplicationUser>
    {
        public TourismDbContext()
        {
        }

        public TourismDbContext(DbContextOptions<TourismDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Booking> Bookings { get; set; }
        public virtual DbSet<BookingLog> BookingLogs { get; set; }
        public virtual DbSet<CustomerSupport> CustomerSupports { get; set; }
        public virtual DbSet<Destination> Destinations { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<Payment> Payments { get; set; }
        public virtual DbSet<Review> Reviews { get; set; }
        public virtual DbSet<Tour> Tours { get; set; }
        public virtual DbSet<TransactionHistory> TransactionHistories { get; set; }
        public virtual DbSet<UserLog> UserLogs { get; set; }
        public virtual DbSet<Voucher> Vouchers { get; set; }
        public DbSet<ForumPost> ForumPosts { get; set; }
        public DbSet<ForumComment> ForumComments { get; set; }
        public DbSet<ForumCategory> ForumCategories { get; set; }
        public DbSet<ForumPostCategory> ForumPostCategories { get; set; }
        public DbSet<ForumPostLike> ForumPostLikes { get; set; }

        // Loại bỏ DbSet<User> vì bây giờ dùng ApplicationUser

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var connectionString = configuration.GetConnectionString("TourismDB");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Quan trọng để cấu hình Identity

            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(e => e.BookingId).HasName("PK__Bookings__73951ACDC4CC4EB0");

                entity.ToTable(tb =>
                {
                    tb.HasTrigger("trg_ApplyDiscount");
                    tb.HasTrigger("trg_AutoRefundOnCancel");
                    tb.HasTrigger("trg_BlockBannedUsers");
                    tb.HasTrigger("trg_BlockFraudulentBookings");
                    tb.HasTrigger("trg_BlockFraudulentBookingsS");
                    tb.HasTrigger("trg_CheckAvailableSeats");
                    tb.HasTrigger("trg_LockCompletedBookings");
                    tb.HasTrigger("trg_LogBookingChanges");
                    tb.HasTrigger("trg_PreventCancelCompletedBooking");
                    tb.HasTrigger("trg_PreventFreeTourForEmployees");
                    tb.HasTrigger("trg_RefundSeatsOnCancel");
                });

                entity.HasIndex(e => e.BookingDate, "IX_Bookings_BookingDate");
                entity.HasIndex(e => e.Status, "IX_Bookings_Status");
                entity.HasIndex(e => e.TourId, "IX_Bookings_TourID");
                entity.HasIndex(e => e.UserId, "IX_Bookings_UserID");
                entity.HasIndex(e => new { e.UserId, e.Status }, "IX_Bookings_UserID_Status");
                entity.HasIndex(e => new { e.UserId, e.TourId, e.StartDate }, "UQ_User_Tour_StartDate").IsUnique();

                entity.Property(e => e.BookingId).HasColumnName("BookingID");
                entity.Property(e => e.BookingDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.DiscountAmountApplied)
                    .HasDefaultValueSql("(NULL)")
                    .HasColumnType("decimal(18, 2)");
                entity.Property(e => e.DiscountPercentageApplied)
                    .HasDefaultValueSql("(NULL)")
                    .HasColumnType("decimal(5, 2)");
                entity.Property(e => e.PaymentStatus)
                    .HasMaxLength(50)
                    .HasDefaultValue("Pending");
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.TourId).HasColumnName("TourID");
                entity.Property(e => e.UserId).HasColumnName("UserID");
                entity.Property(e => e.VoucherID).HasColumnName("VoucherID");

                entity.HasOne(d => d.Tour).WithMany(p => p.Bookings)
                    .HasForeignKey(d => d.TourId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK__Bookings__TourID__6E01572D");

                entity.HasOne(d => d.User).WithMany(p => p.Bookings)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Bookings__UserID__6D0D32F4");

                entity.HasOne(d => d.Voucher).WithMany(p => p.Bookings)
                    .HasForeignKey(d => d.VoucherID)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK__Bookings__Vouche__6EF57B66");
            });

            modelBuilder.Entity<BookingLog>(entity =>
            {
                entity.HasKey(e => e.LogId).HasName("PK__BookingL__5E5499A828EB7457");
                entity.HasIndex(e => e.BookingId, "IX_BookingLogs_BookingID");
                entity.Property(e => e.LogId).HasColumnName("LogID");
                entity.Property(e => e.BookingId).HasColumnName("BookingID");
                entity.Property(e => e.ChangedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.NewStatus).HasMaxLength(50);
                entity.Property(e => e.OldStatus).HasMaxLength(50);
                entity.HasOne(d => d.Booking).WithMany(p => p.BookingLogs)
                    .HasForeignKey(d => d.BookingId)
                    .HasConstraintName("FK_BookingLogs_Bookings");
                entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.BookingLogs)
                    .HasForeignKey(d => d.ChangedBy)
                    .HasConstraintName("FK_BookingLogs_ChangedBy");
            });

            modelBuilder.Entity<CustomerSupport>(entity =>
            {
                entity.HasKey(e => e.TicketId).HasName("PK__Customer__712CC627EE2C71A0");
                entity.ToTable("CustomerSupport");
                entity.HasIndex(e => e.IssueType, "IX_CustomerSupport_IssueType");
                entity.HasIndex(e => e.UserId, "IX_CustomerSupport_UserID");
                entity.Property(e => e.TicketId).HasColumnName("TicketID");
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.IssueType).HasMaxLength(255);
                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .HasDefaultValue("Open");
                entity.Property(e => e.UserId).HasColumnName("UserID");
                entity.HasOne(d => d.User).WithMany(p => p.CustomerSupports)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__CustomerS__UserI__03F0984C");
            });

            modelBuilder.Entity<Destination>(entity =>
            {
                entity.HasKey(e => e.DestinationId).HasName("PK__Destinat__DB5FE4ACA0DC73B8");
                entity.HasIndex(e => new { e.Country, e.City }, "IX_Destinations_CountryCity");
                entity.HasIndex(e => e.IsPopular, "IX_Destinations_IsPopular");
                entity.Property(e => e.DestinationId).HasColumnName("DestinationID");
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.ImageUrl)
                    .HasMaxLength(255)
                    .HasColumnName("ImageURL");
                entity.Property(e => e.IsPopular).HasDefaultValue(false);
                entity.Property(e => e.Latitude).HasColumnType("decimal(9, 6)");
                entity.Property(e => e.Longitude).HasColumnType("decimal(9, 6)");
                entity.Property(e => e.Name).HasMaxLength(255);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E32A0DF464B");
                entity.HasIndex(e => e.UserId, "IX_Notifications_UserID");
                entity.Property(e => e.NotificationId).HasColumnName("NotificationID");
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.UserId).HasColumnName("UserID");
                entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Notificat__UserI__0D7A0286");
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A58DF206702");
                entity.ToTable(tb =>
                {
                    tb.HasTrigger("trg_CheckPaymentAmount");
                    tb.HasTrigger("trg_SyncPaymentAndBookingStatus");
                });
                entity.HasIndex(e => e.BookingId, "IX_Payments_BookingID");
                entity.HasIndex(e => e.PaymentStatus, "IX_Payments_PaymentStatus");
                entity.Property(e => e.PaymentId).HasColumnName("PaymentID");
                entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.BookingId).HasColumnName("BookingID");
                entity.Property(e => e.PaymentMethod).HasMaxLength(100);
                entity.Property(e => e.PaymentStatus)
                    .HasMaxLength(50)
                    .HasDefaultValue("Pending");
                entity.Property(e => e.TransactionDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.UserId).HasColumnName("UserID");
                entity.HasOne(d => d.Booking).WithMany(p => p.Payments)
                    .HasForeignKey(d => d.BookingId)
                    .HasConstraintName("FK__Payments__Bookin__778AC167");
                entity.HasOne(d => d.User).WithMany(p => p.Payments)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Payments__UserID__76969D2E");
            });

            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.ReviewId).HasName("PK__Reviews__74BC79AED75AD75A");
                entity.ToTable(tb =>
                {
                    tb.HasTrigger("trg_LockReviewAfter7Days");
                    tb.HasTrigger("trg_OnlyReviewTourIfAttended");
                });
                entity.HasIndex(e => e.TourId, "IX_Reviews_TourID");
                entity.HasIndex(e => e.UserId, "IX_Reviews_UserID");
                entity.Property(e => e.ReviewId).HasColumnName("ReviewID");
                entity.Property(e => e.ReviewDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.TourId).HasColumnName("TourID");
                entity.Property(e => e.UserId).HasColumnName("UserID");
                entity.HasOne(d => d.Tour).WithMany(p => p.Reviews)
                    .HasForeignKey(d => d.TourId)
                    .HasConstraintName("FK__Reviews__TourID__7D439ABD");
                entity.HasOne(d => d.User).WithMany(p => p.Reviews)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Reviews__UserID__7C4F7684");
            });

            modelBuilder.Entity<Tour>(entity =>
            {
                entity.HasKey(e => e.TourId).HasName("PK__Tours__604CEA10DA15408F");
                entity.HasIndex(e => e.DestinationId, "IX_Tours_DestinationID");
                entity.HasIndex(e => new { e.StartDate, e.EndDate }, "IX_Tours_StartDate_EndDate");
                entity.HasIndex(e => e.TourGuideId, "IX_Tours_TourGuideID");
                entity.HasIndex(e => e.TourStatus, "IX_Tours_TourStatus");
                entity.Property(e => e.TourId).HasColumnName("TourID");
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.DestinationId).HasColumnName("DestinationID");
                entity.Property(e => e.EndDate).HasColumnType("datetime");
                entity.Property(e => e.ImageUrl).HasColumnName("ImageURL");
                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.StartDate).HasColumnType("datetime");
                entity.Property(e => e.TourGuideId).HasColumnName("TourGuideID");
                entity.Property(e => e.TourName).HasMaxLength(255);
                entity.Property(e => e.TourStatus).HasMaxLength(50);
                entity.Property(e => e.TourType).HasMaxLength(50);
                entity.HasOne(d => d.Destination).WithMany(p => p.Tours)
                    .HasForeignKey(d => d.DestinationId)
                    .HasConstraintName("FK__Tours__Destinati__4D94879B");
                entity.HasOne(d => d.TourGuide).WithMany(p => p.Tours)
                    .HasForeignKey(d => d.TourGuideId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK__Tours__TourGuide__4E88ABD4");
            });

            modelBuilder.Entity<TransactionHistory>(entity =>
            {
                entity.HasKey(e => e.TransactionId).HasName("PK__Transact__55433A4B80B692E4");
                entity.ToTable("TransactionHistory");
                entity.HasIndex(e => e.UserId, "IX_TransactionHistory_UserID");
                entity.Property(e => e.TransactionId).HasColumnName("TransactionID");
                entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.TransactionDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.TransactionType).HasMaxLength(50);
                entity.Property(e => e.UserId).HasColumnName("UserID");
                entity.HasOne(d => d.User).WithMany(p => p.TransactionHistories)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Transacti__UserI__09A971A2");
            });

            modelBuilder.Entity<UserLog>(entity =>
            {
                entity.HasKey(e => e.LogId).HasName("PK__UserLogs__5E5499A8CDC0D66B");
                entity.HasIndex(e => e.UserId, "IX_UserLogs_UserID");
                entity.Property(e => e.LogId).HasColumnName("LogID");
                entity.Property(e => e.ChangedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.NewEmail).HasMaxLength(255);
                entity.Property(e => e.NewPhone).HasMaxLength(20);
                entity.Property(e => e.OldEmail).HasMaxLength(255);
                entity.Property(e => e.OldPhone).HasMaxLength(20);
                entity.Property(e => e.UserId).HasColumnName("UserID");
                entity.HasOne(d => d.User).WithMany(p => p.UserLogs)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_UserLogs_Users");
            });

            modelBuilder.Entity<Voucher>(entity =>
            {
                entity.HasKey(e => e.VoucherId).HasName("PK__Vouchers__3AEE79C18ECF40F9");
                entity.ToTable(tb =>
                {
                    tb.HasTrigger("trg_CheckVoucherExpiry");
                    tb.HasTrigger("trg_Check_UsageCount");
                    tb.HasTrigger("trg_NotifyNewPromotion");
                });
                entity.HasIndex(e => new { e.ExpiryDate, e.IsActive }, "IX_Vouchers_ExpiryDate_IsActive");
                entity.HasIndex(e => e.Code, "UQ__Vouchers__A25C5AA7FDCA5D15").IsUnique();
                entity.Property(e => e.VoucherId).HasColumnName("VoucherID");
                entity.Property(e => e.Code).HasMaxLength(50);
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Description).HasMaxLength(255);
                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.DiscountPercentage).HasColumnType("decimal(5, 2)");
                entity.Property(e => e.ExpiryDate).HasColumnType("datetime");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.MaxDiscountAmount).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.MinimumBookingValue)
                    .HasDefaultValue(0m)
                    .HasColumnType("decimal(18, 2)");
                entity.Property(e => e.UsageCount).HasDefaultValue(0);
            });

            // Configure Forum relationships
            modelBuilder.Entity<ForumComment>(entity =>
            {
                entity.HasOne(c => c.Post)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(c => c.PostId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(c => c.ParentComment)
                    .WithMany(p => p.Replies)
                    .HasForeignKey(c => c.ParentCommentId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(c => c.User)
                    .WithMany()
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<ForumPost>(entity =>
            {
                entity.HasOne(p => p.User)
                    .WithMany()
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<ForumCategory>(entity =>
            {
                // Không cần định nghĩa lại mối quan hệ nhiều-nhiều ở đây
            });

            modelBuilder.Entity<ForumPostCategory>(entity =>
            {
                entity.HasKey(pc => new { pc.PostId, pc.CategoryId });

                entity.HasOne(pc => pc.Post)
                    .WithMany(p => p.Categories)
                    .HasForeignKey(pc => pc.PostId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(pc => pc.Category)
                    .WithMany(c => c.Posts)
                    .HasForeignKey(pc => pc.CategoryId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<ForumPostLike>(entity =>
            {
                entity.HasOne(l => l.Post)
                    .WithMany(p => p.Likes)
                    .HasForeignKey(l => l.PostId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(l => l.User)
                    .WithMany()
                    .HasForeignKey(l => l.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(l => new { l.UserId, l.PostId }).IsUnique();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
