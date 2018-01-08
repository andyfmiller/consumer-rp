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

namespace Consumer.Pages.Users
{
    public class CreateModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public InputModel()
            {
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

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

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

            public List<SelectListItem> InstitutionalRoles { get; }
        }

        public IActionResult OnGet()
        {
            Input = new InputModel();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = Input.Email,
                    SourcedId = Input.SourcedId,
                    Email = Input.Email,
                    SendEmail = Input.SendEmail,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    SendNames = Input.SendNames

                };
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    var creator = await _userManager.GetUserAsync(User);
                    user.CreatorId = creator.Id;
                    result = await _userManager.UpdateAsync(user);
                }

                if (result.Succeeded)
                {
                    // LTI - Save the selected institutional roles
                    foreach (var institutionalRole in Input.InstitutionalRoles)
                    {
                        var userRole = Input.UserRoles?.SingleOrDefault(r => r == institutionalRole.Value);
                        if (userRole != null)
                        {
                            await _userManager.AddToRoleAsync(user, institutionalRole.Value);
                        }
                    }

                    return RedirectToPage("./Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }
    }
}