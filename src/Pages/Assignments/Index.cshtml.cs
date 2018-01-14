using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Consumer.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Consumer.Pages.Assignments
{
    public class IndexModel : PageModel
    {
        private readonly Data.ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(Data.ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Assignment> Assignment { get;set; }
        public string ReturnUrl { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            Assignment = await _context.Assignment
                .Where(a => a.CreatorId == user.Id)
                .ToListAsync();
            ReturnUrl = Request.GetDisplayUrl();
        }
    }
}
