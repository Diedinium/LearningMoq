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

            // Act
            CreditCardApplicationDecision decision = _sut.Evaluate(_application);

            // Assert
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void CreditCardApplication_RefersWhenInvalidFreqFlyer()
        {
            // Arrange
            var _sut = new CreditCardApplicationEvaluator(_frequentFlyerNumberValidator.Object);
            var _application = new CreditCardApplication { GrossAnnualIncome = 25000m, Age = 25, FrequentFlyerNumber = "test4" };
            // Argument array matching
            //_frequentFlyerNumberValidator.Setup(x => x.IsValid(It.IsIn("test", "test1", "test3")))
            //    .Returns(false);
            // Argument regex matching
            _frequentFlyerNumberValidator.Setup(x => x.IsValid(It.IsRegex("(?i:t)est")))
                .Returns(false);

            // Act
            CreditCardApplicationDecision decision = _sut.Evaluate(_application);

            // Assert
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
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
