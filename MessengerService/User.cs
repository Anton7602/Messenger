using System;
using System.ServiceModel;


namespace MessengerService
{
    public class User : EventArgs
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public OperationContext UserOperationContext { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
