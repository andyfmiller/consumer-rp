using System.Linq;
using System.Threading.Tasks;
using Consumer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Consumer.Pages.Assignments
{
    public class EditModel : PageModel
    {
        private readonly Data.ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EditModel(Data.ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Assignment Assignment { get; set; }

        [BindProperty]
        public string ReturnUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id, string returnUrl)
        {
            if (id == null)
            {
                return NotFound();
            }

            Assignment = await _context.Assignment
                .SingleOrDefaultAsync(m => m.Id == id);

            if (Assignment == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (Assignment.CreatorId != user.Id)
            {
                return Unauthorized();
            }

            ViewData["CourseId"] = new SelectList(_context.Course, "Id", "Name");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Assignment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AssignmentExists(Assignment.Id))
                {
                    return NotFound();
                }
                throw;
            }

            if (string.IsNullOrEmpty(ReturnUrl))
            {
                return RedirectToPage("./Index");
            }

            return Redirect(ReturnUrl);
        }

        private bool AssignmentExists(int id)
        {
            return _context.Assignment.Any(e => e.Id == id);
        }
    }
}
