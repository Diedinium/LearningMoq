using System;

namespace CreditCardApplications
{
    public interface IFrequentFlyerNumberValidator
    {
        IServiceInformation ServiceInformation { get; }
        bool IsValid(string frequentFlyerNumber);
        void IsValid(string frequentFlyerNumber, out bool isValid);
        bool IsValid(ref string frequentFlyterNumber);
        ValidationMode ValidationMode { get; set; }

        event EventHandler ValidatorLookupPerformed;
    }

    public interface ILicenceData
    {
        string LicenceKey { get; }
    }

    public interface IServiceInformation
    {
        ILicenceData Licence { get; }
    }
}