using Engie.PCC.Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engie.PCC.Api.Notifiers
{
    public interface ILoadResultNotifier
    {
        Task NotifyLoadResultAsync(PayLoad payLoad, List<PowerplantResult> results);
    }
}