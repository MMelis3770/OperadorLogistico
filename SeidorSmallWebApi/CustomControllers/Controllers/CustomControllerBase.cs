using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SeidorSmallWebApi.CustomControllers.Extensions;

namespace SeidorSmallWebApi.CustomControllers.Controllers
{
    public abstract class CustomControllerBase : ControllerBase
    {
        /// <summary>
        /// Add a specific pagination header X-Pagination with the nextLink returned from Service Layer.
        /// </summary>
        [NonAction]
        public virtual void AddPaginationHeader(int skip)
        {
            string callURL = $"{Request.Scheme}://{Request.Host.Value}{Request.Path.Value}{Request.QueryString.Value}";
            string nextLink;
            if(skip != 0) 
            {
                nextLink = callURL.ReplaceSkipValue(skip);
            }
            else
            {
                return;
            }
            var metadata = new
            {
                nextLink
            };

            if (Response != null)
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
        }
    }
}
