using System.Threading.Tasks;
using Consumer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Consumer.Pages
{
    public class IndexModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync(string username)
        {
            await _signInManager.PasswordSignInAsync(username, "Password1!", false, lockoutOnFailure: false);

            return RedirectToPage("./Index");
        }

    }
}
