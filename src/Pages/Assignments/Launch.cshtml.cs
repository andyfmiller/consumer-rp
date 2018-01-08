using System;
using System.Threading.Tasks;
using Consumer.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Consumer.Pages.Assignments
{
    public class LaunchModel : PageModel
    {
        private readonly Data.ApplicationDbContext _context;

        public LaunchModel(Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public Assignment Assignment { get; set; }
        public int? CourseId { get; set; }
        public string ReturnUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(int? courseId, int? assignmentId, string returnUrl)
        {
            if (assignmentId == null)
            {
                return NotFound();
            }

            Assignment = await _context.Assignment
                .SingleOrDefaultAsync(m => m.Id == assignmentId);
            CourseId = courseId;
            if (!string.IsNullOrEmpty(returnUrl) && Uri.TryCreate(new Uri(Request.GetDisplayUrl()), returnUrl, out var returnUri))
            {
                ReturnUrl = returnUri.AbsoluteUri;
            }

            return Page();
        }
    }
}