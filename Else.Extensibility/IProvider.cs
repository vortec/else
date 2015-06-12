using System.Collections.Generic;

namespace Else.Extensibility
{
    public interface IProvider
    {
        ProviderInterest ExecuteIsInterestedFunc(Query query);
        List<Result> ExecuteQueryFunc(Query query, ITokenSource cancelToken);
    }
}