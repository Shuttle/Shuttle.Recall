using System;

namespace Shuttle.Recall;

public class DefaultConcurrencyExceptionSpecification : IConcurrencyExceptionSpecification
{
    public bool IsSatisfiedBy(Exception exception)
    {
        return false;
    }
}