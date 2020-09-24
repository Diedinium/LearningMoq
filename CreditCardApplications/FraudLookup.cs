namespace CreditCardApplications
{
    public class FraudLookup
    {
        public bool IsFraudRisk(CreditCardApplication application)
        {
            return CheckApplication(application);
        }

        protected virtual bool CheckApplication(CreditCardApplication application)
        {
            // Lots more business logic here in a "real" implementation.
            if (application.LastName == "Smith")
            {
                return true;
            }

            return false;
        }
    }
}
