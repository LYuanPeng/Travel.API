using System.Collections.Generic;

namespace Travel.API.Services
{
    public class PropertyMappingValue
    {
        public IEnumerable<string> DestinationProperty { get; private set; }

        public PropertyMappingValue(IEnumerable<string> destinationProperty)
        {
            DestinationProperty = destinationProperty;
        }
    }
}
