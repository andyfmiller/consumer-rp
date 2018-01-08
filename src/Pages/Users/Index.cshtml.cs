using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Consumer.Models;
using LtiLibrary.NetCore.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Consumer.Pages.Users
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public class ApplicationUserAndRoles : ApplicationUser
        {
            [Display(Name = "Institutional roles")]
            public string Roles { get; set; }
        }

        public IList<ApplicationUserAndRoles> ApplicationUsers { get; set; }

        public async Task OnGetAsync()
        {
            var creator = await _userManager.GetUserAsync(User);
            var users = await _userManager.Users
                .Where(a => a.CreatorId == creator.Id)
                .OrderBy(a => a.LastName).ThenBy(a => a.FirstName)
                .ToListAsync();

            ApplicationUsers = new List<ApplicationUserAndRoles>();
            foreach (var user in users)
            {
                var roles = new List<string>();

                foreach (var roleName in await _userManager.GetRolesAsync(user))
                {
                    if (roleName.TryParseRole(out var role))
                    {
                        roles.Add(role.ToString());
                    }
                }

                ApplicationUsers.Add(new ApplicationUserAndRoles
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
                });
            }
        }
    }
}
