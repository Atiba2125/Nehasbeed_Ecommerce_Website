using Microsoft.AspNetCore.Mvc;
using NehasBeed.Data;
using NehasBeed.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
namespace NehasBeed.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly NehasBeed.Services.EmailService _emailService;

        public HomeController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, NehasBeed.Services.EmailService emailService)
        {
            _db = db;
            _userManager = userManager;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            var featuredProducts = await _db.Products.OrderBy(p => p.Id).Take(8).ToListAsync();
            return View(featuredProducts);
        }

        public async Task<IActionResult> Shop(string? cat, string? search)
        {
            var query = _db.Products.AsQueryable();
            if (!string.IsNullOrEmpty(cat) && cat != "all")
                query = query.Where(p => p.Category == cat);
            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()) || p.Category.ToLower().Contains(search.ToLower()));

            var products = await query.ToListAsync();
            ViewBag.Cat    = cat;
            ViewBag.Search = search;
            return View(products);
        }

        public async Task<IActionResult> Product(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();
            var related = await _db.Products
                .Where(p => p.Category == product.Category && p.Id != product.Id)
                .Take(4).ToListAsync();
            ViewBag.Related = related;
            return View(product);
        }

        [Authorize]
        public IActionResult Cart()
        {
            if (User.IsInRole("Admin"))
            {
                TempData["AdminWarning"] = "Admins cannot buy products, add items to cart, or checkout.";
                return RedirectToAction("Dashboard", "Admin");
            }
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            if (User.IsInRole("Admin"))
            {
                TempData["AdminWarning"] = "Admins cannot buy products, add items to cart, or checkout.";
                return RedirectToAction("Dashboard", "Admin");
            }

            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    ViewBag.UserEmail = user.Email;
                    ViewBag.UserFullName = user.FullName;
                }
            }
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public async Task<IActionResult> NewArrivals()
        {
            // Retrieve products marked with "New" badge or sorted by ID descending
            var newProducts = await _db.Products
                .OrderByDescending(p => p.Id)
                .Take(8)
                .ToListAsync();
            return View(newProducts);
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitContact(string name, string email, string subject, string message)
        {
            TempData["ContactSuccess"] = "Thank you for reaching out! Your message has been received.";
            return RedirectToAction("Contact");
        }

        public IActionResult FAQ()
        {
            return View();
        }

        // POST: Place Order — saves to database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(
            string customerName, string customerEmail, string customerPhone,
            string shippingAddress, string city, string postcode, string country,
            string paymentMethod,
            List<int> productIds, List<int> quantities, List<string> colors)
        {
            if (User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Generate invoice number
            var invoiceNum = "NB-" + (await _db.Orders.CountAsync() + 10001).ToString();

            var order = new Order
            {
                InvoiceNumber   = invoiceNum,
                CustomerName    = customerName,
                CustomerEmail   = customerEmail,
                CustomerPhone   = customerPhone,
                ShippingAddress = shippingAddress,
                City            = city,
                Postcode        = postcode,
                Country         = country,
                PaymentMethod   = paymentMethod,
                Status          = "Pending",
                UserId          = _userManager.GetUserId(User),
                CreatedAt       = DateTime.UtcNow,
                UpdatedAt       = DateTime.UtcNow
            };

            decimal subtotal = 0;
            for (int i = 0; i < productIds.Count; i++)
            {
                var prod = await _db.Products.FindAsync(productIds[i]);
                if (prod == null) continue;
                int qty = i < quantities.Count ? quantities[i] : 1;
                string col = i < colors.Count ? colors[i] : "";
                var item = new OrderItem
                {
                    ProductId     = prod.Id,
                    ProductName   = prod.Name,
                    UnitPrice     = prod.Price,
                    Quantity      = qty,
                    SelectedColor = col,
                    ProductImage  = prod.Image
                };
                order.OrderItems.Add(item);
                subtotal += prod.Price * qty;

                // Decrement stock
                prod.StockQuantity = Math.Max(0, prod.StockQuantity - qty);
                if (prod.StockQuantity == 0)
                {
                    prod.InStock = false;
                    // Add admin and customer notification for sold-out product with image
                    _db.AdminNotifications.Add(new AdminNotification
                    {
                        Message   = $"💔 The popular \"{prod.Name}\" has just sold out! Restocking in a few days. ✦",
                        Type      = "warning",
                        ImageUrl  = prod.Image,
                        IsActive  = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            order.Subtotal     = subtotal;
            order.ShippingCost = subtotal >= 150 ? 0 : 8m; // £8 standard delivery
            order.TotalAmount  = order.Subtotal + order.ShippingCost;

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            // Send order placed email with PDF attachment
            _ = Task.Run(() => _emailService.SendOrderPlacedEmailAsync(order));

            TempData["OrderPlaced"]   = true;
            TempData["InvoiceNumber"] = invoiceNum;
            return RedirectToAction("OrderSuccess");
        }

        public IActionResult OrderSuccess()
        {
            if (TempData["OrderPlaced"] == null) return RedirectToAction("Index");
            ViewBag.InvoiceNumber = TempData["InvoiceNumber"];
            return View();
        }

        // GET: CancelOrder (Customer cancellation link from email)
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var order = await _db.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                ViewBag.ErrorTitle = "Order Not Found";
                ViewBag.ErrorMessage = "The order cancellation link is invalid or the order does not exist.";
                return View("CancelResult");
            }

            if (order.Status != "Confirmed")
            {
                if (order.Status == "Cancelled")
                {
                    ViewBag.ErrorTitle = "Already Cancelled";
                    ViewBag.ErrorMessage = $"Your order {order.InvoiceNumber} has already been cancelled.";
                    return View("CancelResult");
                }
                else if (order.Status == "Dispatched")
                {
                    ViewBag.ErrorTitle = "Order Already Dispatched";
                    ViewBag.ErrorMessage = $"Great news! Your order {order.InvoiceNumber} has already been dispatched and is on its way to you. Therefore, it can no longer be cancelled.";
                    return View("CancelResult");
                }
                else if (order.Status == "Delivered")
                {
                    ViewBag.ErrorTitle = "Order Already Delivered";
                    ViewBag.ErrorMessage = $"Your order {order.InvoiceNumber} has already been delivered and cannot be cancelled. If you need assistance with returns, please review our return policy.";
                    return View("CancelResult");
                }
                
                ViewBag.ErrorTitle = "Cancellation Not Allowed";
                ViewBag.ErrorMessage = $"Your order {order.InvoiceNumber} cannot be cancelled at this stage (Current Status: {order.Status}).";
                return View("CancelResult");
            }

            // Check 24-hour limit from ConfirmedAt
            if (order.ConfirmedAt.HasValue)
            {
                var timePassed = DateTime.UtcNow - order.ConfirmedAt.Value;
                if (timePassed > TimeSpan.FromHours(24))
                {
                    ViewBag.ErrorTitle = "Cancellation Window Expired";
                    ViewBag.ErrorMessage = $"Sorry, the 24-hour cancellation period for Order {order.InvoiceNumber} has expired. This order is now being processed for shipment and cannot be cancelled.";
                    return View("CancelResult");
                }
            }

            // Cancel the order
            order.Status = "Cancelled";
            order.UpdatedAt = DateTime.UtcNow;

            // Restock items
            foreach (var item in order.OrderItems)
            {
                if (item.Product != null)
                {
                    item.Product.StockQuantity += item.Quantity;
                    item.Product.InStock = true;
                }
            }

            await _db.SaveChangesAsync();

            ViewBag.SuccessTitle = "Order Cancelled Successfully ✦";
            ViewBag.SuccessMessage = $"Your order {order.InvoiceNumber} has been successfully cancelled. The stock has been returned to our inventory, and your refund is being processed.";
            return View("CancelResult");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View();
    }
}
