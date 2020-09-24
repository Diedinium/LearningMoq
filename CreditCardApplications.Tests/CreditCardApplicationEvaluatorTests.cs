using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Moq;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xunit;
using Moq.Protected;
using System.ComponentModel.DataAnnotations;

namespace CreditCardApplications.Tests
{
    public class CreditCardApplicationTests
    {
        private Mock<IFrequentFlyerNumberValidator> _frequentFlyerNumberValidator;
        private CreditCardApplicationEvaluator sut;

        public CreditCardApplicationTests()
        {
            _frequentFlyerNumberValidator = new Mock<IFrequentFlyerNumberValidator>(MockBehavior.Strict);
            _frequentFlyerNumberValidator.Setup(x => x.IsValid(It.IsAny<string>()))
                .Returns(true);
            _frequentFlyerNumberValidator.Setup(x => x.ServiceInformation.Licence.LicenceKey)
                .Returns("Valid");
            _frequentFlyerNumberValidator.SetupSet(x => x.ValidationMode = ValidationMode.Quick);

            sut = new CreditCardApplicationEvaluator(_frequentFlyerNumberValidator.Object);
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
            var _application = new CreditCardApplication { GrossAnnualIncome = 100000m };

            CreditCardApplicationDecision decision = sut.Evaluate(_application);

            Assert.Equal(CreditCardApplicationDecision.AutoAccepted, decision);
        }

        [Fact]
        public void CreditCardApplication_AgeReferedWhenUnder20()
        {
            // Arrange
            var _application = new CreditCardApplication { GrossAnnualIncome = 35452m, Age = 20, FrequentFlyerNumber = "test" };

            // Act
            CreditCardApplicationDecision decision = sut.Evaluate(_application);

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
            var _application = new CreditCardApplication { GrossAnnualIncome = 19999m, Age = 25, FrequentFlyerNumber = "test" };

            // Act
            sut.Evaluate(_application);

            // Assert
            //_frequentFlyerNumberValidator.Verify(x => x.IsValid(It.IsAny<string>()), "Frequent flyer number must be validated.");
            _frequentFlyerNumberValidator.Verify(x => x.IsValid(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void CreditCardApplication_NotValidateForHighIncome()
        {
            // Example of a behaviour verification test, checking method was not called.
            // Arrange
            var _application = new CreditCardApplication { GrossAnnualIncome = 100999m, Age = 25, FrequentFlyerNumber = "test" };

            // Act
            sut.Evaluate(_application);

            // Assert
            _frequentFlyerNumberValidator.Verify(x => x.IsValid(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void CreditCardApplication_CheckLicenceKeyForLowIncomeApplications()
        {
            // Example of checking get was used
            // Arrange
            var _application = new CreditCardApplication { GrossAnnualIncome = 98000m, Age = 25, FrequentFlyerNumber = "test" };

            // Act
            sut.Evaluate(_application);


            // Assert
            _frequentFlyerNumberValidator.VerifyGet(x => x.ServiceInformation.Licence.LicenceKey, Times.Once);
        }

        [Fact]
        public void CreditCardApplication_CheckValidationModeSetForLowIncomeApplications()
        {
            // Example of checking get was used
            // Arrange
            var _application = new CreditCardApplication { GrossAnnualIncome = 98000m, Age = 25, FrequentFlyerNumber = "test" };

            // Act
            sut.Evaluate(_application);

            // Assert
            _frequentFlyerNumberValidator.VerifySet(x => x.ValidationMode = It.IsAny<ValidationMode>(), Times.Once);
            _frequentFlyerNumberValidator.Verify(x => x.IsValid(It.IsAny<string>()));
            _frequentFlyerNumberValidator.VerifyGet(x => x.ServiceInformation.Licence.LicenceKey);
            // Used to make sure there are no other calls
            _frequentFlyerNumberValidator.VerifyNoOtherCalls();
        }

        [Fact]
        public void CreditCardApplication_ReferWhenFrequentFlyerValidationError()
        {
            // Arrange
            var _application = new CreditCardApplication { Age = 25 };
            _frequentFlyerNumberValidator.Setup(x => x.IsValid(It.IsAny<string>()))
                .Throws<Exception>();
            _frequentFlyerNumberValidator.Setup(x => x.ServiceInformation.Licence.LicenceKey)
                .Returns("Ok");
            _frequentFlyerNumberValidator.SetupSet(x => x.ValidationMode = ValidationMode.Quick);
            var _sut = new CreditCardApplicationEvaluator(_frequentFlyerNumberValidator.Object);

            // Act
            CreditCardApplicationDecision decision = _sut.Evaluate(_application);

            // Assert
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void CreditCardApplication_IncrementLookupCountOnValidatorLookupPerformedEvent()
        {
            // Example of raising events on a moq object
            // Arrange
            var _application = new CreditCardApplication { Age = 25, FrequentFlyerNumber = "x" };
            _frequentFlyerNumberValidator.Setup(x => x.IsValid(It.IsAny<string>()))
                .Returns(true)
                // Chain on the event being rasied from the setup.
                .Raises(x => x.ValidatorLookupPerformed += null, EventArgs.Empty);
            _frequentFlyerNumberValidator.Setup(x => x.ServiceInformation.Licence.LicenceKey)
                .Returns("Ok");
            _frequentFlyerNumberValidator.SetupSet(x => x.ValidationMode = ValidationMode.Quick);

            var _sut = new CreditCardApplicationEvaluator(_frequentFlyerNumberValidator.Object);

            // Act
            CreditCardApplicationDecision decision = _sut.Evaluate(_application);

            // Manually invoke the event
            //_frequentFlyerNumberValidator.Raise(x => x.ValidatorLookupPerformed += null, EventArgs.Empty);


            // Assert
            Assert.Equal(1, _sut.ValidatorLookupCount);
        }

        [Fact]
        public void CreditCardApplication_ReferInvalidFreqFlyerApplication_SequenceTest()
        {
            // Example of setting up a sequence
            // Arrange
            var _application = new CreditCardApplication { Age = 25, FrequentFlyerNumber = "x" };
            _frequentFlyerNumberValidator.Setup(x => x.ServiceInformation.Licence.LicenceKey)
                .Returns("Ok");
            _frequentFlyerNumberValidator.SetupSet(x => x.ValidationMode = ValidationMode.Quick);
            _frequentFlyerNumberValidator.SetupSequence(x => x.IsValid(It.IsAny<string>()))
                .Throws<Exception>()
                .Returns(true);

            var _sut = new CreditCardApplicationEvaluator(_frequentFlyerNumberValidator.Object);

            // Act/Assert
            CreditCardApplicationDecision decision = _sut.Evaluate(_application);
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);

            CreditCardApplicationDecision decision2 = _sut.Evaluate(_application);
            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision2);
        }

        [Fact]
        public void CreditCardApplication_ReferInvalidFreqFlyerApplication_MultipleCallsSequence()
        {
            // Example of asserting against a list of values by capturing the input values.
            // Arrange
            var _sut = new CreditCardApplicationEvaluator(_frequentFlyerNumberValidator.Object);
            var _application1 = new CreditCardApplication { Age = 25, FrequentFlyerNumber = "aa" };
            var _application2 = new CreditCardApplication { Age = 25, FrequentFlyerNumber = "bb" };
            var _application3 = new CreditCardApplication { Age = 25, FrequentFlyerNumber = "cc" };
            var freqFlyNumsPassed = new List<string>();
            _frequentFlyerNumberValidator.Setup(x => x.IsValid(Capture.In(freqFlyNumsPassed)))
                .Returns(true);
            _frequentFlyerNumberValidator.Setup(x => x.ServiceInformation.Licence.LicenceKey)
                .Returns("Ok");
            _frequentFlyerNumberValidator.SetupSet(x => x.ValidationMode = ValidationMode.Quick);

            // Act
            _sut.Evaluate(_application1);
            _sut.Evaluate(_application2);
            _sut.Evaluate(_application3);

            // Assert
            Assert.Equal(new List<string> { "aa", "bb", "cc" }, freqFlyNumsPassed);
        }

        [Fact]
        public void CreditCardApplication_ReferFraudRisk()
        {
            // Arrange
            var _application = new CreditCardApplication { GrossAnnualIncome = 35452m, Age = 20, FrequentFlyerNumber = "test" };

            // On non interface test mocks, methods must be virtual in order to allow them to be overridden in the test.
            // This is why it is better to use Interfaces where possible.
            Mock<FraudLookup> mockFraudLookup = new Mock<FraudLookup>();
            //mockFraudLookup.Setup(x => x.IsFraudRisk(It.IsAny<CreditCardApplication>()))
            //    .Returns(true);
            // How to do the same with a protected method
            mockFraudLookup
                .Protected()
                .Setup<bool>("CheckApplication", ItExpr.IsAny<CreditCardApplication>())
                .Returns(true);

            var _sut = new CreditCardApplicationEvaluator(_frequentFlyerNumberValidator.Object, mockFraudLookup.Object);

            // Act
            CreditCardApplicationDecision decision = _sut.Evaluate(_application);

            // Assert
            Assert.Equal(CreditCardApplicationDecision.ReferredToHumanFraudRisk, decision);
        }

        [Fact]
        public void CreditCardApplication_LinqToMocks()
        {
            // Arrange
            //_frequentFlyerNumberValidator.Setup(x => x.IsValid(It.IsAny<string>()))
            //    .Returns(true);
            //_frequentFlyerNumberValidator.Setup(x => x.ServiceInformation.Licence.LicenceKey)
            //    .Returns("Valid");
            //_frequentFlyerNumberValidator.SetupSet(x => x.ValidationMode = ValidationMode.Quick);

            IFrequentFlyerNumberValidator frequentFlyerNumberValidator = Mock.Of<IFrequentFlyerNumberValidator>
                (
                    validator => 
                    validator.ServiceInformation.Licence.LicenceKey == "OK" &&
                    validator.IsValid(It.IsAny<string>()) == true &&
                    validator.ValidationMode == ValidationMode.Detailed
                );

            var _sut = new CreditCardApplicationEvaluator(frequentFlyerNumberValidator);
            var _application = new CreditCardApplication { GrossAnnualIncome = 19999m, Age = 25, FrequentFlyerNumber = "test" };
            // Act
            CreditCardApplicationDecision decision = _sut.Evaluate(_application);

            // Assert
            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
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
