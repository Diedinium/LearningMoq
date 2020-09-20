using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Moq;
using System;
using System.Runtime.CompilerServices;
using Xunit;

namespace CreditCardApplications.Tests
{
    public class CreditCardApplicationTests
    {
        private Mock<IFrequentFlyerNumberValidator> _frequentFlyerNumberValidator;

        public CreditCardApplicationTests()
        {
            _frequentFlyerNumberValidator = new Mock<IFrequentFlyerNumberValidator>(MockBehavior.Strict);
        }

        [Fact]
        public void CreditCardApplication_ThrowsExceptionWhenFreqFlyerIsNull()
        {
            // Act/Assert
            Assert.Throws<ArgumentNullException>(() => {
                var _sut = new CreditCardApplicationEvaluator(null);
            });
        }

        #region CreditCardApplicationEvaulator.Evaluate Tests

        [Fact]
        public void CreditCardApplication_HighIncomeIsAccepted()
        {
            var _sut = new CreditCardApplicationEvaluator(_frequentFlyerNumberValidator.Object);

            var _application = new CreditCardApplication { GrossAnnualIncome = 100000m };

            CreditCardApplicationDecision decision = _sut.Evaluate(_application);

            Assert.Equal(CreditCardApplicationDecision.AutoAccepted, decision);
        }

        [Fact]
        public void CreditCardApplication_AgeReferedWhenUnder20()
        {
            // Arrange
            var _sut = new CreditCardApplicationEvaluator(_frequentFlyerNumberValidator.Object);
            var _application = new CreditCardApplication { GrossAnnualIncome = 35452m, Age = 20, FrequentFlyerNumber = "test" };
            // Argument matching (Always returns specified value so long as any valid string is passed in)
            _frequentFlyerNumberValidator.Setup(x => x.IsValid(It.IsAny<string>()))
                .Returns(true);
            _frequentFlyerNumberValidator.Setup(x => x.ServiceInformation.Licence.LicenceKey)
                .Returns("Valid");
            _frequentFlyerNumberValidator.SetupSet(x => x.ValidationMode = ValidationMode.Quick);

            // Act
            CreditCardApplicationDecision decision = _sut.Evaluate(_application);

            // Assert
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void CreditCardApplication_LowIncomeAutoDeclined()
        {
            // Arrange
            var _sut = new CreditCardApplicationEvaluator(_frequentFlyerNumberValidator.Object);
            var _application = new CreditCardApplication { GrossAnnualIncome = 19999m, Age = 25, FrequentFlyerNumber = "test" };
            // Predicate matching example (Predicate must return true in order for argument matching to return specified value)
            _frequentFlyerNumberValidator.Setup(x => x.IsValid(It.Is<string>(y => y.Contains("test"))))
                .Returns(true);
            _frequentFlyerNumberValidator.Setup(x => x.ServiceInformation.Licence.LicenceKey)
                .Returns("Valid");
            _frequentFlyerNumberValidator.SetupSet(x => x.ValidationMode = ValidationMode.Quick);

            // Act
            CreditCardApplicationDecision decision = _sut.Evaluate(_application);


            // Assert
            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }

        [Fact]
        public void CreditCardApplication_OtherwiseRefersToHuman()
        {
            // Arrange
            var _sut = new CreditCardApplicationEvaluator(_frequentFlyerNumberValidator.Object);
            var _application = new CreditCardApplication { GrossAnnualIncome = 25000m, Age = 25, FrequentFlyerNumber = "b" };
            // Argument range matching
            _frequentFlyerNumberValidator.Setup(x => x.IsValid(It.IsInRange("b", "z", Moq.Range.Inclusive)))
                .Returns(true);
            _frequentFlyerNumberValidator.Setup(x => x.ServiceInformation.Licence.LicenceKey)
                .Returns("Valid");
            _frequentFlyerNumberValidator.SetupSet(x => x.ValidationMode = ValidationMode.Quick);

            // Act
            CreditCardApplicationDecision decision = _sut.Evaluate(_application);

            // Assert
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void CreditCardApplication_RefersWhenInvalidFreqFlyer()
        {
            // Arrange
            _frequentFlyerNumberValidator = new Mock<IFrequentFlyerNumberValidator>();
            var _sut = new CreditCardApplicationEvaluator(_frequentFlyerNumberValidator.Object);
            var _application = new CreditCardApplication { GrossAnnualIncome = 25000m, Age = 25, FrequentFlyerNumber = "test4" };
            // Argument array matching
            //_frequentFlyerNumberValidator.Setup(x => x.IsValid(It.IsIn("test", "test1", "test3")))
            //    .Returns(false);
            // Argument regex matching
            _frequentFlyerNumberValidator.Setup(x => x.IsValid(It.IsRegex("(?i:t)est")))
                .Returns(false);
            
            // Returns default mock objects of Interfaces or abstract classes within a mocked object.
            _frequentFlyerNumberValidator.DefaultValue = DefaultValue.Mock;

            // Act
            CreditCardApplicationDecision decision = _sut.Evaluate(_application);

            // Assert
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void CreditCardApplication_RefersWhenLicenceKeyExpired()
        {
            // Arrange
            var _sut = new CreditCardApplicationEvaluator(_frequentFlyerNumberValidator.Object);
            var _application = new CreditCardApplication { GrossAnnualIncome = 19999m, Age = 25, FrequentFlyerNumber = "test" };
            //var _mockLicenceData = new Mock<ILicenceData>();
            //_mockLicenceData.Setup(x => x.LicenceKey).Returns("EXPIRED");
            //var _mockServiceInfo = new Mock<IServiceInformation>();
            //_mockServiceInfo.Setup(x => x.Licence).Returns(_mockLicenceData.Object);
            _frequentFlyerNumberValidator.Setup(x => x.IsValid(It.Is<string>(y => y.Contains("test"))))
                .Returns(true);
            _frequentFlyerNumberValidator.Setup(x => x.ServiceInformation.Licence.LicenceKey)
                .Returns("EXPIRED");
            //_frequentFlyerNumberValidator.Setup(x => x.ServiceInformation).Returns(_mockServiceInfo.Object);

            // Act
            CreditCardApplicationDecision decision = _sut.Evaluate(_application);


            // Assert
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void CreditCardApplication_UseDetailedLookupForOlderApplications()
        {
            // Arrange
            // Setup tracking on mocked properties (otherwise changes are not rememberd and cannot be checked!)
            // Remember to do before other setups, otherwise it resets all setups - might be better to use specific Setup() if needed after other setups.
            //_frequentFlyerNumberValidator.SetupProperty(x => x.ValidationMode);
            _frequentFlyerNumberValidator.SetupAllProperties();
            var _sut = new CreditCardApplicationEvaluator(_frequentFlyerNumberValidator.Object);
            var _application = new CreditCardApplication { GrossAnnualIncome = 19999m, Age = 30, FrequentFlyerNumber = "test" };
            _frequentFlyerNumberValidator.Setup(x => x.IsValid(It.Is<string>(y => y.Contains("test"))))
                .Returns(true);
            _frequentFlyerNumberValidator.Setup(x => x.ServiceInformation.Licence.LicenceKey)
                .Returns("OK");
            

            // Act
            _sut.Evaluate(_application);


            // Assert
            Assert.Equal(ValidationMode.Detailed, _frequentFlyerNumberValidator.Object.ValidationMode);
        }

        [Fact]
        public void CreditCardApplication_ValidateFrequentFlyerNumberForLowIncomeApplications()
        {
            // Example of a behaviour verification test.
            // Arrange
            var _sut = new CreditCardApplicationEvaluator(_frequentFlyerNumberValidator.Object);
            var _application = new CreditCardApplication { GrossAnnualIncome = 19999m, Age = 25, FrequentFlyerNumber = "test" };
            _frequentFlyerNumberValidator.Setup(x => x.IsValid(It.Is<string>(y => y.Contains("test"))))
                .Returns(true);
            _frequentFlyerNumberValidator.Setup(x => x.ServiceInformation.Licence.LicenceKey)
                .Returns("Valid");
            _frequentFlyerNumberValidator.SetupSet(x => x.ValidationMode = ValidationMode.Quick);

            // Act
            _sut.Evaluate(_application);

            // Assert
            //_frequentFlyerNumberValidator.Verify(x => x.IsValid(It.IsAny<string>()), "Frequent flyer number must be validated.");
            _frequentFlyerNumberValidator.Verify(x => x.IsValid(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void CreditCardApplication_NotValidateForHighIncome()
        {
            // Example of a behaviour verification test, checking method was not called.
            // Arrange
            var _sut = new CreditCardApplicationEvaluator(_frequentFlyerNumberValidator.Object);
            var _application = new CreditCardApplication { GrossAnnualIncome = 100999m, Age = 25, FrequentFlyerNumber = "test" };

            // Act
            _sut.Evaluate(_application);

            // Assert
            _frequentFlyerNumberValidator.Verify(x => x.IsValid(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void CreditCardApplication_CheckLicenceKeyForLowIncomeApplications()
        {
            // Example of checking get was used
            // Arrange
            var _sut = new CreditCardApplicationEvaluator(_frequentFlyerNumberValidator.Object);
            var _application = new CreditCardApplication { GrossAnnualIncome = 98000m, Age = 25, FrequentFlyerNumber = "test" };
            _frequentFlyerNumberValidator.Setup(x => x.IsValid(It.Is<string>(y => y.Contains("test"))))
                .Returns(true);
            _frequentFlyerNumberValidator.Setup(x => x.ServiceInformation.Licence.LicenceKey)
                .Returns("Ok");
            _frequentFlyerNumberValidator.SetupSet(x => x.ValidationMode = ValidationMode.Quick);

            // Act
            _sut.Evaluate(_application);


            // Assert
            _frequentFlyerNumberValidator.VerifyGet(x => x.ServiceInformation.Licence.LicenceKey, Times.Once);
        }

        [Fact]
        public void CreditCardApplication_CheckValidationModeSetForLowIncomeApplications()
        {
            // Example of checking get was used
            // Arrange
            var _sut = new CreditCardApplicationEvaluator(_frequentFlyerNumberValidator.Object);
            var _application = new CreditCardApplication { GrossAnnualIncome = 98000m, Age = 25, FrequentFlyerNumber = "test" };
            _frequentFlyerNumberValidator.Setup(x => x.IsValid(It.Is<string>(y => y.Contains("test"))))
                .Returns(true);
            _frequentFlyerNumberValidator.Setup(x => x.ServiceInformation.Licence.LicenceKey)
                .Returns("Ok");
            _frequentFlyerNumberValidator.SetupSet(x => x.ValidationMode = ValidationMode.Quick);

            // Act
            _sut.Evaluate(_application);


            // Assert
            _frequentFlyerNumberValidator.VerifySet(x => x.ValidationMode = It.IsAny<ValidationMode>(), Times.Once);
            _frequentFlyerNumberValidator.Verify(x => x.IsValid(It.IsAny<string>()));
            _frequentFlyerNumberValidator.VerifyGet(x => x.ServiceInformation.Licence.LicenceKey);
            // Used to make sure there are no other calls
            _frequentFlyerNumberValidator.VerifyNoOtherCalls();
        }

        string GetLicenceKeyExpiryKey()
        {
            return "EXPIRED";
        }

        #endregion

        #region CreditCardApplicationEvaluator.EvaluateUsingOut Tests
        [Fact]
        public void CreditCardApplication_EvauluateUsingOut_DeclinesLowIncomeApplication()
        {
            // Arrange
            var _sut = new CreditCardApplicationEvaluator(_frequentFlyerNumberValidator.Object);
            var _application = new CreditCardApplication { GrossAnnualIncome = 19900m, Age = 25, FrequentFlyerNumber = "test4" };
            // Example of mocking methods that have out return values
            bool isValid = true;
            _frequentFlyerNumberValidator.Setup(x => x.IsValid(It.IsAny<string>(), out isValid));

            // Act
            CreditCardApplicationDecision decision = _sut.EvaluateUsingOut(_application);

            // Assert
            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }
        #endregion

        #region CreditCardApplicationEvaluator.EvaluateUsingRef Tests
        [Fact]
        public void CreditCardApplication_EvauluateUsingRef_DeclinesLowIncomeApplication()
        {
            // Arrange
            var _sut = new CreditCardApplicationEvaluator(_frequentFlyerNumberValidator.Object);
            var _application = new CreditCardApplication { GrossAnnualIncome = 19900m, Age = 25, FrequentFlyerNumber = "test4" };
            string freqFlyNum = "test";
            // Setup for when ref is used in a parameter.
            //_frequentFlyerNumberValidator.Setup(x => x.IsValid(ref freqFlyNum))
            //    .Returns(true);
            // Using any ref matcher instead
            _frequentFlyerNumberValidator.Setup(x => x.IsValid(ref It.Ref<string>.IsAny))
                .Returns(true);

            // Act
            CreditCardApplicationDecision decision = _sut.EvaluateUsingRef(_application, ref freqFlyNum);

            // Assert
            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }
        #endregion
    }
}
