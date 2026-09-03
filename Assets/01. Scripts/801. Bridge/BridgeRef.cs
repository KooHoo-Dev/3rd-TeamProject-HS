using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WeGotOne
{
    public class BridgeRef
    {
        private readonly ReadOnlyDictionary<int, int> minimumRequirements;

        public int BridgeIndex { get; }
        public int RequiredValue { get; }
        public IReadOnlyDictionary<int, int> MinimumRequirements => minimumRequirements;

        public BridgeRef(int bridgeIndex, int requiredValue, Dictionary<int, int> minimumRequirements)
        {
            BridgeIndex = bridgeIndex;
            RequiredValue = requiredValue;
            this.minimumRequirements = new ReadOnlyDictionary<int, int>(
                new Dictionary<int, int>(minimumRequirements));
        }
    }
}
