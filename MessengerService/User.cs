using System.ServiceModel;


namespace MessengerService
{
    public class User
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public OperationContext UserOperationContext { get; set; }
    }
}
