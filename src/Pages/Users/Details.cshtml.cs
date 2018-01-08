using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Consumer.Models;
using LtiLibrary.NetCore.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Consumer.Pages.Users
{
    public class DetailsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public DetailsModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public class ApplicationUserAndRoles : ApplicationUser
        {
            [Display(Name = "Institutional roles")]
            public string Roles { get; set; }
        }

        public ApplicationUserAndRoles ApplicationUser { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await _userManager.FindByIdAsync(id);

            if (applicationUser == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (applicationUser.CreatorId != user.Id)
            {
                return Unauthorized();
            }

            var roles = new List<string>();

            foreach (var roleName in await _userManager.GetRolesAsync(applicationUser))
            {
                if (roleName.TryParseRole(out var role))
                {
                    roles.Add(role.ToString());
                }
            }

            ApplicationUser = new ApplicationUserAndRoles
            {
                Id = user.Id,
                UserName = user.UserName,
                SourcedId = user.SourcedId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                SendNames = user.SendNames,
                Email = user.Email,
                SendEmail = user.SendEmail,
                Roles = string.Join(", ", roles)
            };

            return Page();
        }
    }
}
