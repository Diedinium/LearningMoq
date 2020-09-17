using Moq;
using Xunit;

namespace CreditCardApplications.Tests
{
    public class CreditCardApplicationTests
    {
        private Mock<IFrequentFlyerNumberValidator> _frequentFlyerNumberValidator;

        public CreditCardApplicationTests()
        {
            _frequentFlyerNumberValidator = new Mock<IFrequentFlyerNumberValidator>();
        }

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
            var _application = new CreditCardApplication { GrossAnnualIncome = 35452m, Age = 20 };
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
            var _application = new CreditCardApplication { GrossAnnualIncome = 19999m, Age = 25 };
            _frequentFlyerNumberValidator.Setup(x => x.IsValid(It.IsAny<string>()))
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
            var _application = new CreditCardApplication { GrossAnnualIncome = 25000m, Age = 25 };
            _frequentFlyerNumberValidator.Setup(x => x.IsValid(It.IsAny<string>()))
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
            var _application = new CreditCardApplication { GrossAnnualIncome = 25000m, Age = 25 };
            _frequentFlyerNumberValidator.Setup(x => x.IsValid(It.IsAny<string>()))
                .Returns(false);

            // Act
            CreditCardApplicationDecision decision = _sut.Evaluate(_application);

            // Assert
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }
    }
}
