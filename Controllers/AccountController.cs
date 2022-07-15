using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Host.SystemWeb;
using ZirconiumX.Models;

namespace ZirconiumX.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        private ZirconiumEntities db = new ZirconiumEntities();

        // VIEWS
        private const string ERROR_VIEW = "Error";
        private const string LOCKOUT_VIEW = "Lockout";
        private const string BUSINESSVIEWS_VIEW = "BusinessViews";
        private const string CONFIRMEMAIL_VIEW = "ConfirmEmail";
        private const string FORGOTPASSWORDCONFIRMATION_VIEW = "ForgotPasswordConfirmation";
        private const string EXTERNALLOGINCONFIRMATION_VIEW = "ExternalLoginConfirmation";
        private const string EXTERNALLOGINFAILURE_VIEW = "ExternalLoginFailure";

        // ACTIONS
        private const string BUSINESSVIEWS_ACTION = "BusinessViewAction";
        private const string SENDCODE_ACTION = "SendCode";
        private const string RESETPASSWORDCONFIRMATION_ACTION = "ResetPasswordConfirmation";
        private const string LOGIN_ACTION = "Login";
        private const string VERIFYCODE_ACTION = "VerifyCode";

        private static BusinessRegisterViewModel businessRegisterViewModel =
            new BusinessRegisterViewModel();

        public AccountController()
        { }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: Account
        public ActionResult Index()
        {
            return View();
        }


        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    // return RedirectToLocal(returnUrl);
                    return RedirectToAction(BUSINESSVIEWS_ACTION);
                case SignInStatus.LockedOut:
                    return View(LOCKOUT_VIEW);
                case SignInStatus.RequiresVerification:
                    return RedirectToAction(SENDCODE_ACTION, new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View(ERROR_VIEW);
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View(ERROR_VIEW);
            }
            return RedirectToAction(VERIFYCODE_ACTION, new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        [Route("Account/BusinessViews/{id:int}")]
        public ActionResult BusinessViews(int? id)
        {
            if (id.HasValue)
            {
                var found = db.BusinessRegistrations.Single(br => br.ID == id.Value);
                if (found != null)
                {
                    ViewBag.MyModel = found;
                }
            }

            return View(BUSINESSVIEWS_VIEW);
        }

        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View(ERROR_VIEW);
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View(LOCKOUT_VIEW);
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterForBus()
        {
            var model = businessRegisterViewModel;

            var user = new ApplicationUser { UserName = model.BName, Email = model.BEmail };
            var result = await UserManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                // TODO: DATABASE CODE
                db.BusinessRegistrations.Add(MapViewModelToBR(model));

                //db.Entry(model).State = System.Data.Entity.EntityState.Added;
                try
                {
                    db.SaveChanges();

                }
                catch (System.Data.Entity.Validation.DbEntityValidationException /*ex*/)
                {
                    ModelState.AddModelError("", "Could not save changes to the Server!.");
                }

                return RedirectToAction("Index", "Home"); // success
            }
            AddErrors(result);


            // If we got this far, something failed, redisplay form
            return RedirectToAction("RegisterBusinessDetails"); // failure
        }

        public ActionResult RegisterBusinessDetails()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterBusinessDetails(BusinessDetailsViewModel model)
        {
            if (ModelState.IsValid)
            {
                businessRegisterViewModel.BName = model.BName;
                businessRegisterViewModel.BAddress = model.BAddress;
                businessRegisterViewModel.Category = model.Category;
                businessRegisterViewModel.BType = model.BType;
                businessRegisterViewModel.CSize = model.CSize;
                businessRegisterViewModel.RNumber = model.RNumber;
                businessRegisterViewModel.TNumber = model.TNumber;
                businessRegisterViewModel.YEstablishment = model.YEstablishment;
                businessRegisterViewModel.BDescription = model.BDescription;

                return RedirectToAction("RegisterManagementTeam");
            }

            ModelState.AddModelError("", "All information must be filled!");
            return View(model);
        }

        public ActionResult RegisterManagementTeam()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterManagementTeam(ManagementTeamViewModel model)
        {
            if (ModelState.IsValid)
            {
                businessRegisterViewModel.SName1 = model.SName;
                businessRegisterViewModel.SPosition1 = model.SPosition;
                businessRegisterViewModel.SEmail1 = model.SEmail;
                businessRegisterViewModel.SPhoneNo1 = model.SPhoneNo;

                return RedirectToAction("RegisterBusLoginDetails");
            }

            ModelState.AddModelError("", "All information must be filled!");
            return View(model);
        }

        public ActionResult RegisterBusLoginDetails()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterBusLoginDetails(BusinessLoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                businessRegisterViewModel.BEmail = model.BEmail;
                businessRegisterViewModel.Password = model.Password;
                businessRegisterViewModel.ConfirmPassword = model.ConfirmPassword;

                //return RedirectToAction("RegisterForBus");
                {
                    var brvm = businessRegisterViewModel;

                    var user = new ApplicationUser { UserName = brvm.BName, Email = brvm.BEmail };
                    var result = await UserManager.CreateAsync(user, brvm.Password);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                        // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                        // Send an email with this link
                        // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                        // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                        // TODO: DATABASE CODE
                        db.BusinessRegistrations.Add(MapViewModelToBR(brvm));

                        //db.Entry(model).State = System.Data.Entity.EntityState.Added;
                        try
                        {
                            db.SaveChanges();

                        }
                        catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                        {
              

                            // Retrieve the error messages as a list of strings.
                            var errorMessages = ex.EntityValidationErrors
                                    .SelectMany(x => x.ValidationErrors)
                                    .Select(x => x.ErrorMessage);

                            // Join the list to a single string.
                            var fullErrorMessage = string.Join("; ", errorMessages);

                            // Combine the original exception message with the new one.
                            var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);
                            ModelState.AddModelError("", exceptionMessage);
                            return View(model);

                        }

                        return RedirectToAction("Index", "Home"); // success
                    }
                    AddErrors(result);

                    // If we got this far, something failed, redisplay form
                    return RedirectToAction("RegisterBusinessDetails"); // failure
                }
            }

            ModelState.AddModelError("", "All information must be filled!!");
            return View(model); // failure
        }

        // GET: /Account/RegisterForCust
        [AllowAnonymous]
        public ActionResult RegisterForCust()
        {
            return View();
        }

        // POST: /Account/RegisterForCust
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterForCust(UserRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.CName, Email = model.CEmail };
                var result = await UserManager.CreateAsync(user, model.CPassword);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    // TODO: DATABASE CODE

                    db.UserRegistrations.Add(MapViewModelToUR(model));
                    // db.Entry(model).State = System.Data.Entity.EntityState.Added;

                    try
                    {
                        db.SaveChanges();

                    }
                    catch (System.Data.Entity.Validation.DbEntityValidationException /*ex*/)
                    {
                        ModelState.AddModelError("", "Could not save changes to the Server!.");
                    }

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        private UserRegisterViewModel MapURToViewModel(UserRegistration ur)
        {
            UserRegisterViewModel urvm = new UserRegisterViewModel
            {
                CName = ur.name,
                CAddress = ur.address,
                CPassword = ur.password,
                CEmail = ur.email,
                ConfirmPassword = ur.confirm_password,
                Username = ur.username,
                CPhoneNo = ur.phoneNo
            };

            return urvm;
        }

        private UserRegistration MapViewModelToUR(UserRegisterViewModel urvm)
        {
            UserRegistration ur = new UserRegistration
            {
                address = urvm.CAddress,
                password = urvm.CPassword,
                confirm_password = urvm.ConfirmPassword,
                email = urvm.CEmail,
                name = urvm.CName,
                phoneNo = urvm.CPhoneNo,
                username = urvm.Username
            };

            return ur;
        }

        private BusinessRegisterViewModel MapBRToViewModel(BusinessRegistration br)
        {
            BusinessRegisterViewModel brvm = new BusinessRegisterViewModel
            {
                BAddress = br.BAddress,
                BDescription = br.BDescription,
                BEmail = br.BEmail,
                BName = br.BName,
                BType = br.BType,
                Category = br.Category,
                ConfirmPassword = br.ConfirmPassword,
                CSize = (br.CSize ?? default(int)).ToString(),
                Password = br.Password,
                RNumber = br.RNumber,
                SEmail1 = br.SEmail1,
                SPhoneNo1 = br.SPhoneNo1,
                YEstablishment = br.YEstablishment,
                TNumber = (br.TNumber ?? default(int)),
                SPosition1 = br.SPosition1,
                SName1 = br.SName,

            };

            return brvm;
        }

        private BusinessRegistration MapViewModelToBR(BusinessRegisterViewModel brvm)
        {
            BusinessRegistration br = new BusinessRegistration
            {
                BAddress = brvm.BAddress,
                BDescription = brvm.BDescription,
                BEmail = brvm.BEmail,
                BName = brvm.BName,
                BType = brvm.BType,
                Category = brvm.Category,
                ConfirmPassword = brvm.ConfirmPassword,
                Password = brvm.Password,
                CSize = Int16.Parse(brvm.CSize),
                RNumber = brvm.RNumber,
                SEmail1 = brvm.SEmail1,
                SName = brvm.SName1,
                SPhoneNo1 = brvm.SPhoneNo1,
                SPosition1 = brvm.SPosition1,
                YEstablishment = brvm.YEstablishment,
                TNumber = brvm.TNumber
            };
            return br;

        }

        #endregion

    }

}