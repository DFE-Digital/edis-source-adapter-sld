using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Dfe.Edis.SourceAdapter.Sld.Application
{
    public interface IChangeProcessor
    {
        Task CheckForUpdatedProvidersAsync(CancellationToken cancellationToken);
    }

    public class ChangeProcessor : IChangeProcessor
    {
        public ChangeProcessor(
            ILogger<ChangeProcessor> logger)
        {
            
        }
        
        public async Task CheckForUpdatedProvidersAsync(CancellationToken cancellationToken)
        {
            // TODO: implement this
        }
    }
}