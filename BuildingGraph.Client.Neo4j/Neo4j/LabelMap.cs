using System.Collections.Generic;


namespace BuildingGraph.Client.Neo4j

{    /// <summary>
     /// Mapping of additional labels for know labels.
     /// This is a bit of hack and not the right place for this.
     /// </summary>
    public class LabelMap
    {
        public static Dictionary<string, string[]> DefaultMapping
        {
            get
            {
                var _mapping = new Dictionary<string, string[]>();
                _mapping.Add("Communications", new string[] { "AbstractElement" });
                _mapping.Add("Circuit", new string[] { "AbstractElement" });
                _mapping.Add("Control", new string[] { "AbstractElement" });
                _mapping.Add("Data", new string[] { "AbstractElement" });
                _mapping.Add("DBPanel", new string[] { "AbstractElement", "ElectricalLoadElement" });
                _mapping.Add("Door", new string[] { "AbstractElement" });
                _mapping.Add("Duct", new string[] { "AbstractElement" });
                _mapping.Add("DuctAccessory", new string[] { "AbstractElement" });
                _mapping.Add("DuctTransition", new string[] { "AbstractElement" });
                _mapping.Add("ElectricalLoad", new string[] { "AbstractElement", "ElectricalLoadElement" });              
                _mapping.Add("Equipment", new string[] { "AbstractElement" });
                _mapping.Add("FireAlarm", new string[] { "AbstractElement" });
                _mapping.Add("Floor", new string[] { "AbstractElement" });
                _mapping.Add("Furniture", new string[] { "AbstractElement" });
                _mapping.Add("Level", new string[] { "AbstractElement" });
                _mapping.Add("Lighting", new string[] { "AbstractElement", "ElectricalLoadElement" });
                _mapping.Add("Pipe", new string[] { "AbstractElement" });
                _mapping.Add("PipeTransition", new string[] { "AbstractElement" });
                _mapping.Add("Roof", new string[] { "AbstractElement" });
                _mapping.Add("Safety", new string[] { "AbstractElement" });
                _mapping.Add("Section", new string[] { "AbstractElement" });
                _mapping.Add("Security", new string[] { "AbstractElement" });
                _mapping.Add("Sensor", new string[] { "AbstractElement" });
                _mapping.Add("Space", new string[] { "AbstractElement" });
                _mapping.Add("System", new string[] { "AbstractElement" });
                _mapping.Add("Terminal", new string[] { "AbstractElement" });
                _mapping.Add("Wall", new string[] { "AbstractElement" });
                _mapping.Add("Window", new string[] { "AbstractElement" });
                _mapping.Add("ModelElement", new string[] { "BaseElement" });
                _mapping.Add("ElementType", new string[] { "BaseElement" });
                _mapping.Add("Model", new string[] { "BaseElement", "ContainerContent" });

                return _mapping;

            }
        }
    }
}
