using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using NehasBeed.Models;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

using PdfSharp.Fonts;

namespace NehasBeed.Services
{
    public class WindowsFontResolver : IFontResolver
    {
        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            string fontName = "Arial";
            if (isBold && isItalic) fontName = "ArialBoldItalic";
            else if (isBold) fontName = "ArialBold";
            else if (isItalic) fontName = "ArialItalic";
            return new FontResolverInfo(fontName);
        }

        public byte[]? GetFont(string faceName)
        {
            try
            {
                string path = faceName switch
                {
                    "ArialBold" => @"C:\Windows\Fonts\arialbd.ttf",
                    "ArialItalic" => @"C:\Windows\Fonts\ariali.ttf",
                    "ArialBoldItalic" => @"C:\Windows\Fonts\arialbi.ttf",
                    _ => @"C:\Windows\Fonts\arial.ttf"
                };

                if (System.IO.File.Exists(path))
                {
                    return System.IO.File.ReadAllBytes(path);
                }
            }
            catch
            {
            }
            return null;
        }
    }

    public class EmailService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        static EmailService()
        {
            try
            {
                GlobalFontSettings.FontResolver = new WindowsFontResolver();
            }
            catch
            {
                // Already registered
            }
        }

        public EmailService(IWebHostEnvironment env, IConfiguration config)
        {
            _env = env;
            _config = config;
        }

        // Generate standard PDF invoice using PDFsharp 6.x
        public byte[] GenerateInvoicePdf(Order order)
        {
            try
            {
                var document = new PdfDocument();
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                
                var fontTitle = new XFont("Arial", 20, XFontStyleEx.Bold);
                var fontHeading = new XFont("Arial", 12, XFontStyleEx.Bold);
                var fontRegular = new XFont("Arial", 10, XFontStyleEx.Regular);
                var fontBold = new XFont("Arial", 10, XFontStyleEx.Bold);

                // Draw Store details
                gfx.DrawString("NEHAS BEED", fontTitle, XBrushes.Black, new XRect(40, 40, page.Width.Point - 80, 30), XStringFormats.TopLeft);
                gfx.DrawString("Handcrafted Luxury Bags & Accessories", fontRegular, XBrushes.DarkGray, new XRect(40, 70, page.Width.Point - 80, 20), XStringFormats.TopLeft);

                // Invoice metadata
                gfx.DrawString($"INVOICE: {order.InvoiceNumber}", fontHeading, XBrushes.Black, new XRect(40, 110, page.Width.Point - 80, 20), XStringFormats.TopLeft);
                gfx.DrawString($"Date: {order.CreatedAt.ToLocalTime().ToString("dd MMM yyyy, HH:mm")}", fontRegular, XBrushes.Black, new XRect(40, 130, page.Width.Point - 80, 20), XStringFormats.TopLeft);
                gfx.DrawString($"Payment Method: {order.PaymentMethod}", fontRegular, XBrushes.Black, new XRect(40, 150, page.Width.Point - 80, 20), XStringFormats.TopLeft);

                // Shipping details
                gfx.DrawString("SHIP TO:", fontHeading, XBrushes.Black, new XRect(40, 190, page.Width.Point - 80, 20), XStringFormats.TopLeft);
                gfx.DrawString(order.CustomerName, fontRegular, XBrushes.Black, new XRect(40, 210, page.Width.Point - 80, 20), XStringFormats.TopLeft);
                gfx.DrawString(order.CustomerEmail, fontRegular, XBrushes.Black, new XRect(40, 230, page.Width.Point - 80, 20), XStringFormats.TopLeft);
                gfx.DrawString(order.ShippingAddress, fontRegular, XBrushes.Black, new XRect(40, 250, page.Width.Point - 80, 20), XStringFormats.TopLeft);
                gfx.DrawString($"{order.City}, {order.Postcode}, {order.Country}", fontRegular, XBrushes.Black, new XRect(40, 270, page.Width.Point - 80, 20), XStringFormats.TopLeft);

                // Table items header
                int y = 320;
                gfx.DrawString("Product", fontHeading, XBrushes.Black, new XRect(40, y, 200, 20), XStringFormats.TopLeft);
                gfx.DrawString("Color", fontHeading, XBrushes.Black, new XRect(260, y, 100, 20), XStringFormats.TopLeft);
                gfx.DrawString("Qty", fontHeading, XBrushes.Black, new XRect(380, y, 50, 20), XStringFormats.TopLeft);
                gfx.DrawString("Unit Price", fontHeading, XBrushes.Black, new XRect(440, y, 80, 20), XStringFormats.TopLeft);
                gfx.DrawString("Total", fontHeading, XBrushes.Black, new XRect(510, y, 80, 20), XStringFormats.TopLeft);

                gfx.DrawLine(XPens.Black, 40, y + 20, 560, y + 20);
                y += 30;

                // Items list
                foreach (var item in order.OrderItems)
                {
                    gfx.DrawString(item.ProductName, fontRegular, XBrushes.Black, new XRect(40, y, 200, 20), XStringFormats.TopLeft);
                    gfx.DrawString(item.SelectedColor ?? "", fontRegular, XBrushes.Black, new XRect(260, y, 100, 20), XStringFormats.TopLeft);
                    gfx.DrawString(item.Quantity.ToString(), fontRegular, XBrushes.Black, new XRect(380, y, 50, 20), XStringFormats.TopLeft);
                    gfx.DrawString($"£{item.UnitPrice.ToString("F2")}", fontRegular, XBrushes.Black, new XRect(440, y, 80, 20), XStringFormats.TopLeft);
                    gfx.DrawString($"£{(item.UnitPrice * item.Quantity).ToString("F2")}", fontRegular, XBrushes.Black, new XRect(510, y, 80, 20), XStringFormats.TopLeft);
                    y += 20;
                }

                gfx.DrawLine(XPens.LightGray, 40, y + 5, 560, y + 5);
                y += 20;

                // Totals
                gfx.DrawString($"Subtotal: £{order.Subtotal.ToString("F2")}", fontRegular, XBrushes.Black, new XRect(40, y, 520, 20), XStringFormats.TopRight);
                y += 18;
                gfx.DrawString($"Shipping Cost: £{order.ShippingCost.ToString("F2")}", fontRegular, XBrushes.Black, new XRect(40, y, 520, 20), XStringFormats.TopRight);
                y += 18;
                gfx.DrawString($"Total Amount: £{order.TotalAmount.ToString("F2")}", fontHeading, XBrushes.Black, new XRect(40, y, 520, 20), XStringFormats.TopRight);

                // Footer note
                y += 50;
                gfx.DrawString("Thank you for shopping at NEHAS BEED! ✦", fontHeading, XBrushes.DarkGray, new XRect(40, y, page.Width.Point - 80, 20), XStringFormats.Center);

                using (var ms = new MemoryStream())
                {
                    document.Save(ms);
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                // Fallback: write error details to fallback text in byte array
                Console.WriteLine($"Error generating PDF invoice: {ex.Message}");
                return Encoding.UTF8.GetBytes($"[PDF GENERATION FALLBACK] Invoice: {order.InvoiceNumber}\nAmount: £{order.TotalAmount}");
            }
        }

        // Mock send helper - saves to Sent Emails directory and prints log output
        private void SaveMockEmail(string toEmail, string subject, string body, string invoiceNum, string actionName, byte[]? attachmentPdf = null)
        {
            try
            {
                var directory = Path.Combine(_env.WebRootPath, "sent_emails");
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Save Email body HTML
                var emailPath = Path.Combine(directory, $"{invoiceNum}_{actionName}_email.html");
                File.WriteAllText(emailPath, body, Encoding.UTF8);

                // Save PDF attachment if present
                if (attachmentPdf != null)
                {
                    var pdfPath = Path.Combine(directory, $"{invoiceNum}_{actionName}_invoice.pdf");
                    File.WriteAllBytes(pdfPath, attachmentPdf);
                }

                Console.WriteLine($"[EMAIL MOCK] Email successfully simulated for {toEmail}. Subject: {subject}. Saved to {emailPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EMAIL ERROR] Failed to save mock email: {ex.Message}");
            }
        }

        // Real SMTP dispatch helper
        private async Task SendActualEmailAsync(string toEmail, string subject, string body, byte[]? pdfBytes = null, string? pdfName = null)
        {
            try
            {
                var server = _config["SmtpSettings:Server"];
                var portStr = _config["SmtpSettings:Port"];
                var senderEmail = _config["SmtpSettings:SenderEmail"];
                var password = _config["SmtpSettings:Password"];
                var senderName = _config["SmtpSettings:SenderName"] ?? "NEHAS BEED";
                
                if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(server))
                {
                    Console.WriteLine("[SMTP] SmtpSettings:Password or SenderEmail is empty in appsettings.json. Real email dispatch skipped.");
                    return;
                }

                int port = 587;
                if (!int.TryParse(portStr, out port)) port = 587;

                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(senderEmail, senderName);
                    mail.To.Add(toEmail);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;

                    if (pdfBytes != null && !string.IsNullOrEmpty(pdfName))
                    {
                        mail.Attachments.Add(new Attachment(new MemoryStream(pdfBytes), pdfName, "application/pdf"));
                    }

                    using (var smtp = new SmtpClient(server, port))
                    {
                        smtp.Credentials = new NetworkCredential(senderEmail, password);
                        smtp.EnableSsl = _config.GetValue<bool>("SmtpSettings:EnableSsl", true);
                        await smtp.SendMailAsync(mail);
                    }
                }
                Console.WriteLine($"[SMTP] Real email successfully sent to {toEmail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SMTP ERROR] Failed to send real email to {toEmail}: {ex.Message}");
            }
        }

        // Send Email: Order Placed
        public async Task SendOrderPlacedEmailAsync(Order order)
        {
            var pdfBytes = GenerateInvoicePdf(order);
            var subject = $"Order Placed: Nehas Beed Order #{order.InvoiceNumber}";
            
            var itemsHtml = new StringBuilder();
            foreach (var item in order.OrderItems)
            {
                itemsHtml.Append($@"
                    <tr>
                        <td style='padding: 10px; border-bottom: 1px solid #eee;'>{item.ProductName} ({item.SelectedColor})</td>
                        <td style='padding: 10px; border-bottom: 1px solid #eee; text-align: center;'>{item.Quantity}</td>
                        <td style='padding: 10px; border-bottom: 1px solid #eee; text-align: right;'>£{item.UnitPrice.ToString("F2")}</td>
                        <td style='padding: 10px; border-bottom: 1px solid #eee; text-align: right;'>£{(item.UnitPrice * item.Quantity).ToString("F2")}</td>
                    </tr>
                ");
            }

            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 24px; border: 1px solid #eae5e0; border-radius: 12px; background: #fff;'>
                    <h2 style='color: #111; font-style: italic; font-weight: normal; border-bottom: 1px solid #eae5e0; padding-bottom: 12px;'>NEHAS BEED ✦</h2>
                    <p>Dear {order.CustomerName},</p>
                    <p>Thank you for placing your order with us! Your order <strong>{order.InvoiceNumber}</strong> has been successfully received and is currently in <strong>Pending</strong> status.</p>
                    
                    <h3 style='margin-top: 24px;'>Order Details</h3>
                    <table style='width: 100%; border-collapse: collapse;'>
                        <thead>
                            <tr style='background: #f7f6f2;'>
                                <th style='padding: 10px; text-align: left;'>Product</th>
                                <th style='padding: 10px; text-align: center;'>Qty</th>
                                <th style='padding: 10px; text-align: right;'>Price</th>
                                <th style='padding: 10px; text-align: right;'>Total</th>
                            </tr>
                        </thead>
                        <tbody>
                            {itemsHtml}
                        </tbody>
                    </table>

                    <div style='margin-top: 16px; text-align: right; line-height: 1.6;'>
                        <p>Subtotal: <strong>£{order.Subtotal.ToString("F2")}</strong></p>
                        <p>Shipping Cost: <strong>£{order.ShippingCost.ToString("F2")}</strong></p>
                        <p style='font-size: 1.15rem; font-weight: bold;'>Total Amount: £{order.TotalAmount.ToString("F2")}</p>
                    </div>

                    <p style='margin-top: 24px;'>Please find your invoice PDF attached to this email.</p>
                    <p>We will notify you as soon as your order status changes.</p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 32px 0;' />
                    <p style='font-size: 0.8rem; color: #888; text-align: center;'>NEHAS BEED Handcrafted Luxury · 40+ Hours of Craftsmanship per Bag</p>
                </div>
            ";

            SaveMockEmail(order.CustomerEmail, subject, body, order.InvoiceNumber, "placed", pdfBytes);
            await SendActualEmailAsync(order.CustomerEmail, subject, body, pdfBytes, $"Invoice_{order.InvoiceNumber}.pdf");
        }

        // Send Email: Order Confirmed
        public async Task SendOrderConfirmedEmailAsync(Order order, string siteBaseUrl)
        {
            var pdfBytes = GenerateInvoicePdf(order);
            var subject = $"Order Confirmed: Nehas Beed Order #{order.InvoiceNumber}";
            
            // Build Cancel order link with 24 hours warning
            var cancelLink = $"{siteBaseUrl}/Home/CancelOrder?orderId={order.Id}";

            var itemsHtml = new StringBuilder();
            foreach (var item in order.OrderItems)
            {
                itemsHtml.Append($@"
                    <tr>
                        <td style='padding: 10px; border-bottom: 1px solid #eee;'>{item.ProductName} ({item.SelectedColor})</td>
                        <td style='padding: 10px; border-bottom: 1px solid #eee; text-align: center;'>{item.Quantity}</td>
                        <td style='padding: 10px; border-bottom: 1px solid #eee; text-align: right;'>£{(item.UnitPrice * item.Quantity).ToString("F2")}</td>
                    </tr>
                ");
            }

            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 24px; border: 1px solid #eae5e0; border-radius: 12px; background: #fff;'>
                    <h2 style='color: #27ae60; font-style: italic; font-weight: normal; border-bottom: 1px solid #eae5e0; padding-bottom: 12px;'>Order Confirmed! ✦</h2>
                    <p>Dear {order.CustomerName},</p>
                    <p>We are pleased to inform you that your order <strong>{order.InvoiceNumber}</strong> has been officially **Confirmed** by NEHAS BEED. Our craftsmen are now preparing it for shipment.</p>
                    
                    <h3 style='margin-top: 24px;'>Confirmed Order Summary</h3>
                    <table style='width: 100%; border-collapse: collapse;'>
                        <thead>
                            <tr style='background: #f7f6f2;'>
                                <th style='padding: 10px; text-align: left;'>Product</th>
                                <th style='padding: 10px; text-align: center;'>Qty</th>
                                <th style='padding: 10px; text-align: right;'>Total</th>
                            </tr>
                        </thead>
                        <tbody>
                            {itemsHtml}
                        </tbody>
                    </table>

                    <p style='font-size: 1.1rem; font-weight: bold; text-align: right; margin-top: 16px;'>Total Paid: £{order.TotalAmount.ToString("F2")}</p>
                    
                    <div style='margin-top: 32px; padding: 20px; border-radius: 8px; border: 1px solid #ebccd1; background-color: #f2dede; color: #a94442;'>
                        <h4 style='margin-top: 0; margin-bottom: 8px;'>Need to cancel your order?</h4>
                        <p style='font-size: 0.88rem; line-height: 1.5; margin-bottom: 16px;'>
                            Please note that you can cancel this order within <strong>24 hours</strong> of receiving this confirmation. After 24 hours, orders enter the packaging phase and cannot be cancelled.
                        </p>
                        <a href='{cancelLink}' style='display: inline-block; padding: 10px 20px; background: #d9534f; color: #fff; text-decoration: none; border-radius: 6px; font-size: 0.85rem; font-weight: bold;'>Cancel My Order</a>
                    </div>

                    <p style='margin-top: 24px;'>Please find your confirmed invoice PDF attached to this email.</p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 32px 0;' />
                    <p style='font-size: 0.8rem; color: #888; text-align: center;'>NEHAS BEED Handcrafted Luxury · 40+ Hours of Craftsmanship per Bag</p>
                </div>
            ";

            SaveMockEmail(order.CustomerEmail, subject, body, order.InvoiceNumber, "confirmed", pdfBytes);
            await SendActualEmailAsync(order.CustomerEmail, subject, body, pdfBytes, $"Invoice_{order.InvoiceNumber}_Confirmed.pdf");
        }

        // Send Email: Order Dispatched
        public async Task SendOrderDispatchedEmailAsync(Order order)
        {
            var subject = $"Order Dispatched: Nehas Beed Order #{order.InvoiceNumber}";
            
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 24px; border: 1px solid #eae5e0; border-radius: 12px; background: #fff;'>
                    <h2 style='color: #3498db; font-style: italic; font-weight: normal; border-bottom: 1px solid #eae5e0; padding-bottom: 12px;'>On Its Way! 🚚</h2>
                    <p>Dear {order.CustomerName},</p>
                    <p>Great news! Your order <strong>{order.InvoiceNumber}</strong> has been handed over to our premium courier partner and is now on its way to you.</p>
                    <p><strong>Shipping Address:</strong><br />
                    {order.CustomerName}<br />
                    {order.ShippingAddress}<br />
                    {order.City}, {order.Postcode}, {order.Country}</p>
                    
                    <p style='margin-top: 24px; font-weight: bold;'>Expected Delivery Time: 2 - 4 business days.</p>
                    <p>We hope you love your handcrafted masterpiece!</p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 32px 0;' />
                    <p style='font-size: 0.8rem; color: #888; text-align: center;'>NEHAS BEED Handcrafted Luxury · 40+ Hours of Craftsmanship per Bag</p>
                </div>
            ";

            SaveMockEmail(order.CustomerEmail, subject, body, order.InvoiceNumber, "dispatched", null);
            await SendActualEmailAsync(order.CustomerEmail, subject, body, null, null);
        }
    }
}
