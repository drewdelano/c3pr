using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace C3PR.Api.Models
{
    public class IAmATeaPot : StatusCodeResult
    {
        public IAmATeaPot() : base(418)
        {
        }
    }
}