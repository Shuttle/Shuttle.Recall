using System;

namespace Shuttle.Recall;

public interface IConcurrencyExceptionSpecification
{
    bool IsSatisfiedBy(Exception exception);
}