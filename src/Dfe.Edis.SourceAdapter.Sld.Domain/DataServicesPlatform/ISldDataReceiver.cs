using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Sld.Domain.SubmitLearnerData;

namespace Dfe.Edis.SourceAdapter.Sld.Domain.DataServicesPlatform
{
    public interface ISldDataReceiver
    {
        Task SendLearnerAsync(Learner learner, CancellationToken cancellationToken);
    }
}