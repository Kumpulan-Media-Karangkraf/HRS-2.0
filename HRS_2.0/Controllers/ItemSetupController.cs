using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HRS_2.Models;
using HRS_2.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CreditControl.Controllers
{
    public class ItemSetupController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<ItemSetupController> _logger;

        public ItemSetupController(DatabaseContext context, ILogger<ItemSetupController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: ItemSetups
        public async Task<IActionResult> Index()
        {
              return _context.ItemSetup != null ? 
                          View(await _context.ItemSetup.ToListAsync()) :
                          Problem("Entity set 'AppDbContext.itemsetups'  is null.");
        }


        public IActionResult Create()
        {
            // Initialize an empty ItemSetup object
            var itemSetup = new ItemSetup();
            return View(itemSetup);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DocNo,Name,Item")] ItemSetup itemSetup)
        {
            if (ModelState.IsValid)
            {
                bool conflictDetected = false;

                do
                {
                    // Determine the prefix based on the selected Item
                    string prefix = itemSetup.Item switch
                    {
                        "Salesperson" => "SP",
                        "Status Case" => "SC",
                        "Status Payment" => "STP",
                        "Doc Type" => "DT",
                        "Company" => "CO",
                        "Reason" => "RSN",
                        _ => "ITEM"
                    };

                    // Check if the DocNo already exists
                    var existingItem = await _context.ItemSetup
                        .FirstOrDefaultAsync(i => i.DocNo == itemSetup.DocNo);

                    if (existingItem != null)
                    {
                        // Generate a new DocNo
                        var lastDocNo = await _context.ItemSetup
                            .Where(i => i.DocNo.StartsWith(prefix))
                            .OrderByDescending(i => i.DocNo)
                            .FirstOrDefaultAsync();

                        var nextDocNo = $"{prefix}-001"; // Default value
                        if (lastDocNo != null)
                        {
                            // Extract numeric part of last DocNo and increment
                            var lastNumericPart = int.Parse(lastDocNo.DocNo.Substring(prefix.Length + 1));
                            var nextNumericPart = lastNumericPart + 1;
                            nextDocNo = $"{prefix}-{nextNumericPart:D6}"; // Format as PREFIX-000002
                        }

                        itemSetup.DocNo = nextDocNo;
                        conflictDetected = true;
                    }
                    else
                    {
                        conflictDetected = false;
                    }

                } while (conflictDetected);

                // Save the item setup
                _context.Add(itemSetup);
                await _context.SaveChangesAsync();

             
                return RedirectToAction(nameof(Index));
            }

            return View(itemSetup);
        }


        [HttpPost]
        public async Task<IActionResult> Edit([FromForm] ItemSetup model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid data submitted." });
                }

                var existingItem = await _context.ItemSetup.FindAsync(model.DocNo);
                if (existingItem == null)
                {
                    return Json(new { success = false, message = "Item not found." });
                }

                // Update only the properties that can be changed
                existingItem.Name = model.Name;
                existingItem.Item = model.Item;

                // Track changes and save
                _context.ItemSetup.Update(existingItem);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Item updated successfully." });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error while updating item setup");
                return Json(new { success = false, message = "The record was modified by another user. Please refresh and try again." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating item setup");
                return Json(new { success = false, message = "An error occurred while updating the item." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string docNo)
        {
            try
            {
                if (string.IsNullOrEmpty(docNo))
                {
                    _logger.LogWarning("Delete attempt with null or empty docNo");
                    return Json(new { success = false, message = "Invalid document number provided." });
                }

                // Log the incoming docNo and its length
                _logger.LogInformation($"Attempting to delete item with DocNo: '{docNo}', Length: {docNo.Length}");

                // Log all DocNo values in the database for comparison
                var allDocNos = await _context.ItemSetup.Select(i => i.DocNo).ToListAsync();
                _logger.LogInformation($"Available DocNos in database: {string.Join(", ", allDocNos)}");

                // Try to find the item and log the SQL query
                var item = await _context.ItemSetup
                    .AsTracking()
                    .Where(i => i.DocNo == docNo)
                    .FirstOrDefaultAsync();

                if (item == null)
                {
                    // Try to find with trimmed value
                    item = await _context.ItemSetup
                        .AsTracking()
                        .Where(i => i.DocNo == docNo.Trim())
                        .FirstOrDefaultAsync();

                    if (item == null)
                    {
                        // Try case-insensitive comparison
                        item = await _context.ItemSetup
                            .AsTracking()
                            .Where(i => i.DocNo.ToLower() == docNo.Trim().ToLower())
                            .FirstOrDefaultAsync();
                    }
                }

                if (item == null)
                {
                    _logger.LogWarning($"Item with DocNo '{docNo}' not found. " +
                        $"Tried exact match, trimmed match, and case-insensitive match.");
                    return Json(new { success = false, message = $"Item with document number '{docNo}' not found." });
                }

                // Log the found item details
                _logger.LogInformation($"Found item: DocNo='{item.DocNo}', Name='{item.Name}', Item='{item.Item}'");

                // Remove the item
                _context.ItemSetup.Remove(item);

                // Save changes
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Successfully deleted item with DocNo '{docNo}'");
                return Json(new { success = true, message = "Item deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting item setup with DocNo '{docNo}'");
                return Json(new { success = false, message = "An error occurred while deleting the item." });
            }
        }

        private bool ItemSetupExists(string id)
        {
          return (_context.ItemSetup?.Any(e => e.DocNo == id)).GetValueOrDefault();
        }
    }
}
