using System;

namespace Shuttle.Recall
{
    public class DefaultConcurrenyExceptionSpecification : IConcurrenyExceptionSpecification
    {
        public bool IsSatisfiedBy(Exception exception)
        {
            return false;
        }
    }
}