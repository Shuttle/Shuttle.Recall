using System;

namespace Shuttle.Recall
{
    public interface IConcurrenyExceptionSpecification
    {
        bool IsSatisfiedBy(Exception exception);
    }
}