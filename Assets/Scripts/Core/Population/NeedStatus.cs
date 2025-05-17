namespace Glade.Core.Population
{
    public class NeedStatus
    {
        public NeedDefinition def;
        public float buffer; // days of stock on hand
        public float timer;  // accum dt until one day passes
    }
}
