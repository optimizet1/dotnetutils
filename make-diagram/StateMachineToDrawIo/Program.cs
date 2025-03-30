
using System.Xml;
using StateMachineToDrawIo.Models;

var states = new List<State>
{
    new() { Name = "Draft", X = 80, Y = 80 },
    new() { Name = "Submitted", X = 260, Y = 80 },
    new() { Name = "Processing", X = 440, Y = 80 },
    new() { Name = "Validation", X = 620, Y = 80 },
    new() { Name = "Review", X = 800, Y = 80 },
    new() { Name = "Completed", X = 1000, Y = 80 },
};

var transitions = new List<Transition>
{
    new() { FromStateId = states[0].Id, ToStateId = states[1].Id, Label = "Submit" },
    new() { FromStateId = states[1].Id, ToStateId = states[2].Id, Label = "Auto" },
    new() { FromStateId = states[2].Id, ToStateId = states[3].Id, Label = "Parse" },
    new() { FromStateId = states[3].Id, ToStateId = states[4].Id, Label = "Fail" },
    new() { FromStateId = states[4].Id, ToStateId = states[5].Id, Label = "Resolve" },
};

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, Drawing World!");

var xml = new XmlDocument();

var mxfile = xml.CreateElement("mxfile");
mxfile.SetAttribute("host", "app.diagrams.net");

var diagram = xml.CreateElement("diagram");
diagram.SetAttribute("name", "StateMachine");
mxfile.AppendChild(diagram);

var model = xml.CreateElement("mxGraphModel");
diagram.AppendChild(model);

var root = xml.CreateElement("root");
model.AppendChild(root);

// Root cells
root.AppendChild(CreateCell(xml, "0"));
root.AppendChild(CreateCell(xml, "1", "0"));

// State nodes
foreach (var state in states)
{
    var cell = CreateCell(xml, state.Id, "1", state.Name, true, state.X, state.Y);
    root.AppendChild(cell);
}

// Transitions
foreach (var t in transitions)
{
    var edge = CreateEdge(xml, Guid.NewGuid().ToString("N"), t.FromStateId, t.ToStateId, t.Label);
    root.AppendChild(edge);
}

xml.AppendChild(mxfile);
xml.Save("state_machine.drawio.xml");

Console.WriteLine("Draw.io XML created: state_machine.drawio.xml");

static XmlElement CreateCell(XmlDocument doc, string id, string parent = null, string value = null, bool isVertex = false, int x = 0, int y = 0)
{
    var cell = doc.CreateElement("mxCell");
    cell.SetAttribute("id", id);
    if (parent != null) cell.SetAttribute("parent", parent);
    if (value != null) cell.SetAttribute("value", value);
    if (isVertex)
    {
        cell.SetAttribute("vertex", "1");
        cell.SetAttribute("style", "ellipse;fillColor=#dae8fc;");
        var geometry = doc.CreateElement("mxGeometry");
        geometry.SetAttribute("x", x.ToString());
        geometry.SetAttribute("y", y.ToString());
        geometry.SetAttribute("width", "120");
        geometry.SetAttribute("height", "60");
        geometry.SetAttribute("as", "geometry");
        cell.AppendChild(geometry);
    }
    return cell;
}

static XmlElement CreateEdge(XmlDocument doc, string id, string source, string target, string label)
{
    var edge = doc.CreateElement("mxCell");
    edge.SetAttribute("id", id);
    edge.SetAttribute("value", label);
    edge.SetAttribute("edge", "1");
    edge.SetAttribute("style", "endArrow=block;");
    edge.SetAttribute("parent", "1");
    edge.SetAttribute("source", source);
    edge.SetAttribute("target", target);

    var geometry = doc.CreateElement("mxGeometry");
    geometry.SetAttribute("relative", "1");
    geometry.SetAttribute("as", "geometry");
    edge.AppendChild(geometry);

    return edge;
}
