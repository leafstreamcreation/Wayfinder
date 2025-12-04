using System;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Wayfinder.API.Services;

namespace Wayfinder.API.Filters
{
    /// <summary>
    /// Authentication filter for JWE protected endpoints
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class JweAuthenticationFilter : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            // Check if the action allows anonymous access
            if (actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Count > 0 ||
                actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Count > 0)
            {
                return;
            }

            var authHeader = actionContext.Request.Headers.Authorization;
            
            if (authHeader == null || string.IsNullOrEmpty(authHeader.Scheme) || 
                !authHeader.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase))
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.Unauthorized, 
                    "Authorization header is required");
                return;
            }

            var token = authHeader.Parameter;
            if (string.IsNullOrEmpty(token))
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.Unauthorized, 
                    "Bearer token is required");
                return;
            }

            try
            {
                var jwtService = new JwtService();
                var principal = jwtService.ValidateToken(token);

                if (principal == null)
                {
                    actionContext.Response = actionContext.Request.CreateErrorResponse(
                        HttpStatusCode.Unauthorized, 
                        "Invalid or expired token");
                    return;
                }

                // Set the principal on the current thread
                Thread.CurrentPrincipal = principal;
                
                // Also store in the request properties for easy access
                actionContext.Request.Properties["ClaimsPrincipal"] = principal;
                
                var userId = JwtService.GetUserIdFromPrincipal(principal);
                if (userId.HasValue)
                {
                    actionContext.Request.Properties["UserId"] = userId.Value;
                }
            }
            catch (Exception)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.Unauthorized, 
                    "Token validation failed");
            }
        }

        public override Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            OnAuthorization(actionContext);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Base controller for authenticated endpoints
    /// </summary>
    [JweAuthenticationFilter]
    public abstract class AuthenticatedApiController : ApiController
    {
        /// <summary>
        /// Get the current authenticated user's ID
        /// </summary>
        protected int? CurrentUserId
        {
            get
            {
                if (Request.Properties.TryGetValue("UserId", out object userId))
                {
                    return (int)userId;
                }
                return null;
            }
        }

        /// <summary>
        /// Get the current claims principal
        /// </summary>
        protected ClaimsPrincipal CurrentPrincipal
        {
            get
            {
                if (Request.Properties.TryGetValue("ClaimsPrincipal", out object principal))
                {
                    return (ClaimsPrincipal)principal;
                }
                return Thread.CurrentPrincipal as ClaimsPrincipal;
            }
        }
    }
}
