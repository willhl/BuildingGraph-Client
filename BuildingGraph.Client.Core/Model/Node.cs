using System;
using System.Collections.Generic;
using System.Linq;

namespace BuildingGraph.Client.Model
{
    public class Node
    {
        ICollection<string> _labels = new List<string>();

        public Node()
        {
        }

        public Node(string Label) : this()
        {
            _labels.Add(Label);
        }

        public Node(string Label, string Id) : this(Label)
        {
            this.Id = Id;
        }

        public Node(ICollection<string> Labels, string Id)
        {
            foreach (var lb in Labels) _labels.Add(lb);
            this.Id = Id;
        }

        public Node(ICollection<string> Labels)
        {
            foreach (var lb in Labels) _labels.Add(lb);
        }


        public string Id { get; set; }


        public string Label
        {
            get
            {
                return Labels.First();
            }
        }

        public virtual ICollection<string> Labels
        {
            get
            {
                if (_labels.Count == 0 && !_labels.Contains(GetType().Name)) _labels.Add(GetType().Name);
                return _labels;
            }
        }

        public string Name { get; set; }
        public Dictionary<string, object> ExtendedProperties { get; } = new Dictionary<string, object>();


        public virtual Dictionary<string, object> GetAllProperties()
        {
            var allProps = new Dictionary<string, object>();

            foreach (var kvp in ExtendedProperties)
            {
                allProps.Add(kvp.Key, kvp.Value);
            }

            if (!allProps.ContainsKey("Name")) allProps.Add("Name", Name);

            return allProps;

        }

    }
}
