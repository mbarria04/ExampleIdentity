// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using CapaData.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using PracticaIdentity.Services;
using PracticaIdentity.Models;

namespace PracticaIdentity.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly ActiveDirectoryService _activeAD;
        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger, ActiveDirectoryService activeAD)
        {
            _signInManager = signInManager;
            _logger = logger;
            _activeAD = activeAD;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            //[Required]
            //[EmailAddress]
            //public string Email { get; set; }

            [Required]       
            public string UserName { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        // Sin Active Directory
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(Input.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");

                    var AD = new DatosValidacionAD
                    {
                        UserID = Input.UserName,
                        Password = Input.Password
                    };

                    if (_activeAD.EsValido(AD, out string aderror))


                        return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            return Page();
        }


        //public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        //{
        //    returnUrl ??= Url.Content("~/");

        //    if (ModelState.IsValid)
        //    {
        //        using (var client = new HttpClient())
        //        {
        //            var requestData = new
        //            {
        //                usuario = Input.Email,
        //                password = Input.Password
        //            };

        //            var json = System.Text.Json.JsonSerializer.Serialize(requestData);
        //            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        //            var response = await client.PostAsync("https://api.midominio.com/auth/login", content);

        //            if (response.IsSuccessStatusCode)
        //            {
        //                var result = await response.Content.ReadAsStringAsync();
        //                bool isLoggedIn = bool.Parse(result); // Si el API devuelve "true" o "false"

        //                if (isLoggedIn)
        //                {
        //                    _logger.LogInformation("Usuario autenticado vía API.");
        //                    return LocalRedirect(returnUrl);
        //                }
        //                else
        //                {
        //                    ModelState.AddModelError(string.Empty, "Credenciales inválidas.");
        //                }
        //            }
        //            else
        //            {
        //                ModelState.AddModelError(string.Empty, "Error al conectar con el servicio de autenticación.");
        //            }
        //        }
        //    }

        //    return Page();
        //}


        // este metodo es con active directory 
        //public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        //{
        //    returnUrl ??= Url.Content("~/");


        //    var adData = new DatosValidacionAD
        //    {
        //        UserID = Input.UserName,       // o username según tu AD
        //        Password = Input.Password
        //    };



        //    if (!_activeAD.EsValido(adData, out string adError))
        //    {
        //        ModelState.AddModelError(string.Empty, adError);
        //        return Page();
        //    }


        //    var user = await _signInManager.UserManager.FindByNameAsync(Input.UserName);

        //    if (user == null)
        //    {
        //        ModelState.AddModelError(string.Empty, "Usuario no registrado en el sistema.");
        //        return Page();
        //    }


        //    await _signInManager.SignInAsync(user, Input.RememberMe);

        //    _logger.LogInformation("Usuario autenticado vía Active Directory.");

        //    return LocalRedirect(returnUrl);
        //}





    }
}
