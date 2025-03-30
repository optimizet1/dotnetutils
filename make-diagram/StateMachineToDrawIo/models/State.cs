namespace StateMachineToDrawIo.Models
{
    public class State
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
