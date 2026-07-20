using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NehasBeed.Models;

namespace NehasBeed.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<AdminNotification> AdminNotifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Order relationships
            builder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Computed column — ignore LineTotal
            builder.Entity<OrderItem>()
                .Ignore(oi => oi.LineTotal);

            // Seed Products
            builder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Mosaic Tote", Category = "Beaded bags", Price = 185.00m, OriginalPrice = 220.00m, Description = "A stunning handcrafted beaded tote bag featuring an intricate mosaic pattern. Each bead is meticulously placed by skilled artisans using premium Japanese Miyuki seed beads.", Colors = "[\"Multicolor\",\"Earth Tones\",\"Blue Ocean\"]", Badge = "Bestseller", Image = "images/bag-1.png", Images = "[\"images/bag-1.png\",\"images/bag-2.png\"]", InStock = true, Rating = 5.0m, Reviews = 124, CreatedAt = new DateTime(2026, 1, 1), UpdatedAt = new DateTime(2026, 1, 1) },
                new Product { Id = 2, Name = "Terra Bucket", Category = "Beaded bags", Price = 165.00m, OriginalPrice = null, Description = "A sophisticated bucket bag in warm terracotta tones. Handcrafted with thousands of premium seed beads creating a rich, textured surface.", Colors = "[\"Terracotta\",\"Rust\",\"Sand\"]", Badge = "New", Image = "images/bag-2.png", Images = "[\"images/bag-2.png\",\"images/bag-1.png\"]", InStock = true, Rating = 4.8m, Reviews = 54, CreatedAt = new DateTime(2026, 1, 2), UpdatedAt = new DateTime(2026, 1, 2) },
                new Product { Id = 3, Name = "Noir Clutch", Category = "Beaded bags", Price = 145.00m, OriginalPrice = 175.00m, Description = "An elegant evening clutch in classic noir. The deep black beads create a sophisticated pattern perfect for formal occasions.", Colors = "[\"Black\",\"Midnight\"]", Badge = "Sale", Image = "images/bag-3.png", Images = "[\"images/bag-3.png\"]", InStock = true, Rating = 4.9m, Reviews = 87, CreatedAt = new DateTime(2026, 1, 3), UpdatedAt = new DateTime(2026, 1, 3) },
                new Product { Id = 4, Name = "Pearl Mini", Category = "Beaded bags", Price = 210.00m, OriginalPrice = null, Description = "A luxurious mini bag adorned with pearlescent beads. The delicate sheen and feminine silhouette make this a truly special piece.", Colors = "[\"Pearl White\",\"Champagne\",\"Rose\"]", Badge = "Limited", Image = "images/bag-4.png", Images = "[\"images/bag-4.png\",\"images/bag-5.png\"]", InStock = true, Rating = 5.0m, Reviews = 67, CreatedAt = new DateTime(2026, 1, 4), UpdatedAt = new DateTime(2026, 1, 4) },
                new Product { Id = 5, Name = "Garden Pouch", Category = "Beaded bags", Price = 125.00m, OriginalPrice = null, Description = "A charming pouch featuring a delicate floral beaded design. Perfect for carrying your essentials with artistic flair.", Colors = "[\"Floral Mix\",\"Pink Garden\",\"Sage Green\"]", Badge = null, Image = "images/bag-5.png", Images = "[\"images/bag-5.png\",\"images/bag-1.png\"]", InStock = true, Rating = 4.7m, Reviews = 43, CreatedAt = new DateTime(2026, 1, 5), UpdatedAt = new DateTime(2026, 1, 5) },
                new Product { Id = 6, Name = "Sovereign Chain", Category = "Chain", Price = 85.00m, OriginalPrice = 105.00m, Description = "A bold statement chain necklace featuring intricate gold-tone beadwork. Each link is handcrafted for a luxurious drape and finish.", Colors = "[\"Gold\",\"Antique Gold\",\"Silver\"]", Badge = "Bestseller", Image = "images/bag-3.png", Images = "[\"images/bag-3.png\"]", InStock = true, Rating = 4.9m, Reviews = 98, CreatedAt = new DateTime(2026, 1, 6), UpdatedAt = new DateTime(2026, 1, 6) },
                new Product { Id = 7, Name = "Onyx Layers", Category = "Necklaces", Price = 95.00m, OriginalPrice = null, Description = "A multi-strand layered necklace featuring deep onyx seed beads. The layered effect creates a rich, textured look perfect for any occasion.", Colors = "[\"Onyx Black\",\"Charcoal\"]", Badge = "New", Image = "images/bag-1.png", Images = "[\"images/bag-1.png\"]", InStock = true, Rating = 5.0m, Reviews = 76, CreatedAt = new DateTime(2026, 1, 7), UpdatedAt = new DateTime(2026, 1, 7) },
                new Product { Id = 8, Name = "Crystal Drops", Category = "Earings", Price = 55.00m, OriginalPrice = null, Description = "Delicate crystal bead earrings that catch the light beautifully. Lightweight yet statement-making, these earrings are perfect for day to evening wear.", Colors = "[\"Crystal Clear\",\"Rose Gold\",\"Sapphire\"]", Badge = null, Image = "images/bag-4.png", Images = "[\"images/bag-4.png\"]", InStock = true, Rating = 4.8m, Reviews = 112, CreatedAt = new DateTime(2026, 1, 8), UpdatedAt = new DateTime(2026, 1, 8) }
            );

            // Seed sample Orders
            builder.Entity<Order>().HasData(
                new Order { Id = 1, InvoiceNumber = "NB-10001", CustomerName = "Sophia Loren", CustomerEmail = "sophia@example.com", CustomerPhone = "+44 7700 900001", ShippingAddress = "123 Luxury Lane", City = "London", Postcode = "SW1A 1AA", Country = "United Kingdom", Subtotal = 185.00m, ShippingCost = 0m, TotalAmount = 185.00m, PaymentMethod = "Card", Status = "Confirmed", CreatedAt = new DateTime(2026, 7, 15, 10, 0, 0), UpdatedAt = new DateTime(2026, 7, 15, 10, 0, 0) },
                new Order { Id = 2, InvoiceNumber = "NB-10002", CustomerName = "Grace Kelly", CustomerEmail = "grace@example.com", CustomerPhone = "+377 99 99 00 00", ShippingAddress = "Palace Avenue", City = "Monaco", Postcode = "98000", Country = "Monaco", Subtotal = 210.00m, ShippingCost = 12.00m, TotalAmount = 222.00m, PaymentMethod = "PayPal", Status = "Dispatched", CreatedAt = new DateTime(2026, 7, 14, 9, 0, 0), UpdatedAt = new DateTime(2026, 7, 14, 9, 0, 0) },
                new Order { Id = 3, InvoiceNumber = "NB-10003", CustomerName = "Zara Ahmed", CustomerEmail = "zara@example.com", CustomerPhone = "+971 50 000 0000", ShippingAddress = "Sheikh Zayed Road", City = "Dubai", Postcode = "00000", Country = "UAE", Subtotal = 120.00m, ShippingCost = 0m, TotalAmount = 120.00m, PaymentMethod = "Card", Status = "Pending", CreatedAt = new DateTime(2026, 7, 17, 8, 0, 0), UpdatedAt = new DateTime(2026, 7, 17, 8, 0, 0) },
                new Order { Id = 4, InvoiceNumber = "NB-10004", CustomerName = "Maria Rossi", CustomerEmail = "maria@example.com", CustomerPhone = "+39 02 0000000", ShippingAddress = "Via della Moda 5", City = "Milan", Postcode = "20121", Country = "Italy", Subtotal = 145.00m, ShippingCost = 0m, TotalAmount = 145.00m, PaymentMethod = "Card", Status = "Rejected", CreatedAt = new DateTime(2026, 7, 13, 14, 0, 0), UpdatedAt = new DateTime(2026, 7, 13, 14, 0, 0) },
                new Order { Id = 5, InvoiceNumber = "NB-10005", CustomerName = "Lena Böhm", CustomerEmail = "lena@example.com", CustomerPhone = "+49 30 00000000", ShippingAddress = "Kurfürstendamm 10", City = "Berlin", Postcode = "10719", Country = "Germany", Subtotal = 165.00m, ShippingCost = 12.00m, TotalAmount = 177.00m, PaymentMethod = "Card", Status = "Confirmed", CreatedAt = new DateTime(2026, 7, 12, 11, 0, 0), UpdatedAt = new DateTime(2026, 7, 12, 11, 0, 0) }
            );

            // Seed OrderItems
            builder.Entity<OrderItem>().HasData(
                new OrderItem { Id = 1, OrderId = 1, ProductId = 1, ProductName = "Mosaic Tote", UnitPrice = 185.00m, Quantity = 1, SelectedColor = "Multicolor", ProductImage = "images/bag-1.png" },
                new OrderItem { Id = 2, OrderId = 2, ProductId = 4, ProductName = "Pearl Mini", UnitPrice = 210.00m, Quantity = 1, SelectedColor = "Pearl White", ProductImage = "images/bag-4.png" },
                new OrderItem { Id = 3, OrderId = 3, ProductId = 6, ProductName = "Sovereign Chain", UnitPrice = 85.00m, Quantity = 1, SelectedColor = "Gold", ProductImage = "images/bag-3.png" },
                new OrderItem { Id = 4, OrderId = 3, ProductId = 7, ProductName = "Onyx Layers", UnitPrice = 35.00m, Quantity = 1, SelectedColor = "Onyx Black", ProductImage = "images/bag-1.png" },
                new OrderItem { Id = 5, OrderId = 4, ProductId = 3, ProductName = "Noir Clutch", UnitPrice = 145.00m, Quantity = 1, SelectedColor = "Black", ProductImage = "images/bag-3.png" },
                new OrderItem { Id = 6, OrderId = 5, ProductId = 2, ProductName = "Terra Bucket", UnitPrice = 165.00m, Quantity = 1, SelectedColor = "Terracotta", ProductImage = "images/bag-2.png" }
            );
        }
    }
}
