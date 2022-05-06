using System.Collections.Generic;

namespace Travel.API.Services
{
    public class PropertyMapping<TSource, TDestinaion> : IPropertyMapping
    {
        public Dictionary<string, PropertyMappingValue> _mappingDictionary { get; set; }

        public PropertyMapping(Dictionary<string, PropertyMappingValue> mappingDictionary)
        {
            _mappingDictionary = mappingDictionary;
        }
    }
}
 
