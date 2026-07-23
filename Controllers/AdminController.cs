using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NehasBeed.Data;
using NehasBeed.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace NehasBeed.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly NehasBeed.Services.EmailService _emailService;
        private readonly IWebHostEnvironment _env;

        public AdminController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, NehasBeed.Services.EmailService emailService, IWebHostEnvironment env)
        {
            _db = db;
            _userManager = userManager;
            _emailService = emailService;
            _env = env;
        }

        // ─────────────────────────────────────────
        //  DASHBOARD
        // ─────────────────────────────────────────
        public async Task<IActionResult> Dashboard()
        {
            var allOrders   = await _db.Orders.ToListAsync();
            var usersInRole = await _userManager.GetUsersInRoleAsync("User");
            var products    = await _db.Products.ToListAsync();
            var notifications = await _db.AdminNotifications
                .Where(n => n.IsActive)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            var stats = new
            {
                PendingOrders   = allOrders.Count(o => o.Status == "Pending"),
                LowStock        = products.Count(p => !p.InStock || p.StockQuantity == 0),
                TotalRevenue    = allOrders.Where(o => o.Status == "Confirmed" || o.Status == "Dispatched").Sum(o => o.TotalAmount),
                TotalOrders     = allOrders.Count,
                TotalProducts   = products.Count,
                TotalCustomers  = usersInRole.Count,
                RecentOrders    = allOrders.OrderByDescending(o => o.CreatedAt).Take(5).ToList(),
                TopProducts     = products.Take(4).ToList(),
                Notifications   = notifications
            };
            ViewBag.Stats = stats;
            return View();
        }

        // ─────────────────────────────────────────
        //  PRODUCTS CRUD
        // ─────────────────────────────────────────
        public async Task<IActionResult> Products(int page = 1, string search = "")
        {
            int pageSize = 10;
            var query = _db.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search) || p.Category.Contains(search));

            int total = await query.CountAsync();
            var products = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.CurrentPage   = page;
            ViewBag.TotalPages    = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.TotalProducts = total;
            ViewBag.Search = search;
            return View(products);
        }

        // GET: Product Details
        public async Task<IActionResult> ProductDetails(int id)
        {
            var p = await _db.Products.FindAsync(id);
            if (p == null) return NotFound();
            return View(p);
        }

        // GET: Create Product
        public IActionResult CreateProduct() => View(new Product());

        // POST: Create Product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product model, IFormFile? imageFile)
        {
            ModelState.Remove("Colors");
            ModelState.Remove("Images");

            if (!ModelState.IsValid) return View(model);

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
                using var stream = new FileStream(savePath, FileMode.Create);
                await imageFile.CopyToAsync(stream);
                model.Image  = "images/" + fileName;
                model.Images = "[\"images/" + fileName + "\"]";
            }
            else
            {
                model.Image  = "images/bag-1.png";
                model.Images = "[\"images/bag-1.png\"]";
            }

            model.InStock   = model.StockQuantity > 0;
            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = DateTime.UtcNow;
            _db.Products.Add(model);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Product added successfully!";
            return RedirectToAction("Products");
        }

        // GET: Edit Product
        public async Task<IActionResult> EditProduct(int id)
        {
            var p = await _db.Products.FindAsync(id);
            if (p == null) return NotFound();
            return View(p);
        }

        // POST: Edit Product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(Product model, IFormFile? imageFile)
        {
            ModelState.Remove("Colors");
            ModelState.Remove("Images");

            if (!ModelState.IsValid) return View(model);

            var existing = await _db.Products.FindAsync(model.Id);
            if (existing == null) return NotFound();

            existing.Name          = model.Name;
            existing.Category      = model.Category;
            existing.Price         = model.Price;
            existing.OriginalPrice = model.OriginalPrice;
            existing.Description   = model.Description;
            existing.Badge         = model.Badge;
            existing.StockQuantity = model.StockQuantity;
            existing.InStock       = model.StockQuantity > 0;
            existing.Rating        = model.Rating;
            existing.Reviews       = model.Reviews;
            existing.UpdatedAt     = DateTime.UtcNow;

            if (existing.StockQuantity > 0)
            {
                var staleNotifs = await _db.AdminNotifications
                    .Where(n => n.IsActive && n.Type == "warning" && n.Message.Contains(existing.Name))
                    .ToListAsync();
                foreach (var sn in staleNotifs)
                {
                    sn.IsActive = false;
                }
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
                using var stream = new FileStream(savePath, FileMode.Create);
                await imageFile.CopyToAsync(stream);
                existing.Image  = "images/" + fileName;
                existing.Images = "[\"images/" + fileName + "\"]";
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Product updated successfully!";
            return RedirectToAction("Products");
        }

        // POST: Delete Product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var p = await _db.Products.FindAsync(id);
            if (p != null) { _db.Products.Remove(p); await _db.SaveChangesAsync(); }
            TempData["Success"] = "Product deleted.";
            return RedirectToAction("Products");
        }

        // ─────────────────────────────────────────
        //  ORDERS (No Reject — Confirm & Dispatch only)
        // ─────────────────────────────────────────
        public async Task<IActionResult> Orders(string search = "")
        {
            var query = _db.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.Product).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(o => o.InvoiceNumber.Contains(search) ||
                                         o.CustomerName.Contains(search) ||
                                         o.CustomerEmail.Contains(search) ||
                                         o.Status.Contains(search));

            var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
            ViewBag.Search = search;
            return View(orders);
        }

        // POST: Update Order Status (Confirm / Dispatch only — no Reject)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            if (status != "Confirmed" && status != "Dispatched") return RedirectToAction("Orders");

            var order = await _db.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order != null)
            {
                order.Status    = status;
                order.UpdatedAt = DateTime.UtcNow;

                if (status == "Confirmed")
                {
                    order.ConfirmedAt = DateTime.UtcNow;
                }

                await _db.SaveChangesAsync();

                // Send email notification with attachment
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                if (status == "Confirmed")
                {
                    _ = Task.Run(() => _emailService.SendOrderConfirmedEmailAsync(order, baseUrl));
                }
                else if (status == "Dispatched")
                {
                    _ = Task.Run(() => _emailService.SendOrderDispatchedEmailAsync(order));
                }

                TempData["Success"] = $"Order {order.InvoiceNumber} marked as {status} and notification email generated.";
            }
            return RedirectToAction("Orders");
        }

        // GET: Cancellations (view user-cancelled orders)
        public async Task<IActionResult> Cancellations(string search = "")
        {
            var query = _db.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.Status == "Cancelled")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(o => o.InvoiceNumber.Contains(search) ||
                                         o.CustomerName.Contains(search) ||
                                         o.CustomerEmail.Contains(search));
            }

            var list = await query.OrderByDescending(o => o.UpdatedAt).ToListAsync();
            ViewBag.Search = search;
            return View(list);
        }

        // GET: Invoice (printable)
        public async Task<IActionResult> Invoice(int id)
        {
            var order = await _db.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();
            return View(order);
        }

        // ─────────────────────────────────────────
        //  NOTIFICATIONS CRUD
        // ─────────────────────────────────────────
        public async Task<IActionResult> Notifications()
        {
            var list = await _db.AdminNotifications
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNotification(string message, string type = "info", string? imageUrl = null)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                _db.AdminNotifications.Add(new AdminNotification
                {
                    Message   = message.Trim(),
                    Type      = type,
                    ImageUrl  = imageUrl?.Trim(),
                    IsActive  = true,
                    CreatedAt = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
                TempData["Success"] = "Notification published!";
            }
            return RedirectToAction("Notifications");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleNotification(int id)
        {
            var n = await _db.AdminNotifications.FindAsync(id);
            if (n != null) { n.IsActive = !n.IsActive; await _db.SaveChangesAsync(); }
            return RedirectToAction("Notifications");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var n = await _db.AdminNotifications.FindAsync(id);
            if (n != null) { _db.AdminNotifications.Remove(n); await _db.SaveChangesAsync(); }
            return RedirectToAction("Notifications");
        }

        // API: fetch active notifications for the public site (includes DB notifications and dynamic New Arrivals)
        [AllowAnonymous]
        public async Task<IActionResult> ActiveNotifications()
        {
            // Fetch explicit active admin notifications
            var dbList = await _db.AdminNotifications
                .Where(n => n.IsActive)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new { Id = n.Id, Message = n.Message, Type = n.Type, ImageUrl = n.ImageUrl })
                .ToListAsync<object>();

            // Fetch latest 5 in-stock products to show as New Arrivals
            var newProducts = await _db.Products
                .Where(p => p.StockQuantity > 0)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToListAsync();

            foreach (var p in newProducts)
            {
                dbList.Add(new {
                    Id = -p.Id, // Negative ID to distinguish from DB ones
                    Message = $"✨ New Arrival: Check out the stunning \"{p.Name}\" for only £{p.Price.ToString("F2")}! ✦",
                    Type = "success",
                    ImageUrl = p.Image
                });
            }

            return Json(dbList);
        }

        // ─────────────────────────────────────────
        //  USERS CRUD
        // ─────────────────────────────────────────
        public async Task<IActionResult> Users(string search = "")
        {
            var regularUsers = await _userManager.GetUsersInRoleAsync("User");
            var userList = regularUsers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                userList = userList.Where(u => (u.FullName != null && u.FullName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                                               (u.Email   != null && u.Email.Contains(search, StringComparison.OrdinalIgnoreCase)));

            ViewBag.Search = search;
            return View(userList.ToList());
        }

        public IActionResult CreateUser() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(string fullName, string email, string password, string role = "User")
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Email and password are required.");
                return View();
            }

            var user = new ApplicationUser
            {
                UserName       = email,
                Email          = email,
                FullName       = fullName,
                EmailConfirmed = true,
                CreatedAt      = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                TempData["Success"] = $"User {email} created successfully!";
                return RedirectToAction("Users");
            }

            foreach (var err in result.Errors)
                ModelState.AddModelError("", err.Description);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
                TempData["Success"] = "User removed.";
            }
            return RedirectToAction("Users");
        }

        // GET: EmailLogs (to inspect local html emails)
        public IActionResult EmailLogs()
        {
            var directory = Path.Combine(_env.WebRootPath, "sent_emails");
            var list = new List<string>();
            if (Directory.Exists(directory))
            {
                list = Directory.GetFiles(directory, "*.html")
                    .Select(Path.GetFileName)
                    .OrderByDescending(f => f)
                    .Select(x => x!)
                    .ToList();
            }
            return View(list);
        }

        // GET: ViewEmailLog
        public IActionResult ViewEmailLog(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
            {
                return BadRequest();
            }
            var path = Path.Combine(_env.WebRootPath, "sent_emails", fileName);
            if (!System.IO.File.Exists(path)) return NotFound();
            var html = System.IO.File.ReadAllText(path);
            return Content(html, "text/html");
        }
    }
}
