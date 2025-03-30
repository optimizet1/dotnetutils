namespace StateMachineToDrawIo.Models
{
    public class Transition
    {
        public string FromStateId { get; set; }
        public string ToStateId { get; set; }
        public string Label { get; set; }
    }
}
