using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Consumer.Models;
using LtiLibrary.NetCore.Extensions;
using LtiLibrary.NetCore.Lis.v1;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Consumer.Pages.Users
{
    public class EditModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public EditModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;

            InstitutionalRoles = new List<SelectListItem>();
            foreach (Enum role in Enum.GetValues(typeof(InstitutionalRole)))
            {
                InstitutionalRoles.Add(new SelectListItem
                {
                    Value = role.GetUrn(),
                    Text = role.ToString()
                });
            }
        }

        public string UserName { get; set; }

        public List<SelectListItem> InstitutionalRoles { get; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public string Id { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Display(Name = "First name")]
            public string FirstName { get; set; }

            [Display(Name = "Last name")]
            public string LastName { get; set; }

            [Display(Name = "Send my email address when I launch an LTI Tool")]
            public bool SendEmail { get; set; }

            [Display(Name = "Send my names when I launch an LTI Tool")]
            public bool SendNames { get; set; }

            [Display(Name = "SIS ID")]
            public string SourcedId { get; set; }

            [Display(Name = "Institutional roles (send with my context role when I launch an LTI Tool)")]
            public List<string> UserRoles { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var creator = await _userManager.GetUserAsync(User);
            if (user.CreatorId != creator.Id)
            {
                return Unauthorized();
            }

            UserName = user.UserName;

            Input = new InputModel
            {
                Id = id,
                Email = user.Email,

                // LTI - Set the current names and privacy settings
                FirstName = user.FirstName,
                LastName = user.LastName,
                SendEmail = user.SendEmail,
                SendNames = user.SendNames,
                SourcedId = user.SourcedId
            };

            // LTI - Select the current institutional roles
            foreach (var roleUrn in await _userManager.GetRolesAsync(user))
            {
                var userRole = InstitutionalRoles.SingleOrDefault(r => r.Value == roleUrn);
                if (userRole != default(SelectListItem))
                {
                    userRole.Selected = true;
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (!string.IsNullOrEmpty(Input.SourcedId))
            {
                var sourcedIdInUse = _userManager.Users
                    .Where(u => !string.IsNullOrEmpty(u.SourcedId))
                    .Where(u => u.Id != Input.Id)
                    .Any(u => u.SourcedId == Input.SourcedId);

                if (sourcedIdInUse)
                {
                    ModelState.AddModelError("Input.SourcedId", "SIS ID must be unique.");
                    return Page();
                }
            }

            var user = await _userManager.FindByIdAsync(Input.Id);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{Input.Id}'.");
            }

            if (Input.Email != user.Email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, Input.Email);
                if (!setEmailResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting email for user with ID '{user.Id}'.");
                }
            }

            // LTI - Save names and privacy settings
            if (await TryUpdateModelAsync(user, "Input", u => u.FirstName, u => u.LastName, u => u.SendEmail, u => u.SendNames, u => u.SourcedId))
            {
                try
                {
                    await _userManager.UpdateAsync(user);
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", $"Unable to set LTI data for user with ID '{user.Id}'.");
                }
            }

            // LTI - Save the selected institutional roles
            foreach (var institutionalRole in InstitutionalRoles)
            {
                var userRole = Input.UserRoles?.SingleOrDefault(r => r == institutionalRole.Value);
                if (userRole != null)
                {
                    await _userManager.AddToRoleAsync(user, institutionalRole.Value);
                }
                else
                {
                    await _userManager.RemoveFromRoleAsync(user, institutionalRole.Value);
                }
            }

            return RedirectToPage("./Index");
        }
    }
}
