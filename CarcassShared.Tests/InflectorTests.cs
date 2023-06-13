using Xunit;

namespace CarcassShared.Tests
{
    public class InflectorTests
    {
        [Theory]
        [InlineData("some title", "Some title")]
        [InlineData("some Title", "Some title")]
        [InlineData("SOMETITLE", "Sometitle")]
        [InlineData("someTitle", "Sometitle")]
        [InlineData("some title goes here", "Some title goes here")]
        [InlineData("some TITLE", "Some title")]
        public void CapitalizeTest(string source, string expectedResult)
        {
            Assert.Equal(expectedResult, source.Capitalize());
        }

        [Theory]
        [InlineData("some title", "Some title")]
        [InlineData("some Title", "Some Title")]
        [InlineData("SOMETITLE", "SOMETITLE")]
        [InlineData("someTitle", "SomeTitle")]
        [InlineData("some title goes here", "Some title goes here")]
        [InlineData("some TITLE", "Some TITLE")]
        public void CapitalizeCamelTest(string source, string expectedResult)
        {
            Assert.Equal(expectedResult, source.CapitalizeCamel());
        }

        [Theory]
        [InlineData("some_title", "some-title")]
        [InlineData("some-title", "some-title")]
        [InlineData("some_title_goes_here", "some-title-goes-here")]
        [InlineData("some_title and_another", "some-title and-another")]
        public void DasherizeTest(string source, string expectedResult)
        {
            Assert.Equal(expectedResult, source.Dasherize());
        }

        [Theory]
        [InlineData("some_title", "Some title")]
        [InlineData("some-title", "Some-title")]
        [InlineData("Some_title", "Some title")]
        [InlineData("someTitle", "Sometitle")]
        [InlineData("someTitle_Another", "Sometitle another")]
        public void HumanizeTest(string source, string expectedResult)
        {
            Assert.Equal(expectedResult, source.Humanize());
        }

        [Theory]
        [InlineData(0, "0th")]
        [InlineData(1, "1st")]
        [InlineData(2, "2nd")]
        [InlineData(3, "3rd")]
        [InlineData(4, "4th")]
        [InlineData(5, "5th")]
        [InlineData(6, "6th")]
        [InlineData(7, "7th")]
        [InlineData(8, "8th")]
        [InlineData(9, "9th")]
        [InlineData(10, "10th")]
        [InlineData(11, "11th")]
        [InlineData(12, "12th")]
        [InlineData(13, "13th")]
        [InlineData(14, "14th")]
        [InlineData(20, "20th")]
        [InlineData(21, "21st")]
        [InlineData(22, "22nd")]
        [InlineData(23, "23rd")]
        [InlineData(24, "24th")]
        [InlineData(100, "100th")]
        [InlineData(101, "101st")]
        [InlineData(102, "102nd")]
        [InlineData(103, "103rd")]
        [InlineData(104, "104th")]
        [InlineData(110, "110th")]
        [InlineData(1000, "1000th")]
        [InlineData(1001, "1001st")]
        public void OrdinalizeNumbersTest(int source, string expectedResult)
        {
            Assert.Equal(expectedResult, source.Ordinalize());
        }

        [Theory]
        [InlineData("0", "0th")]
        [InlineData("1", "1st")]
        [InlineData("2", "2nd")]
        [InlineData("3", "3rd")]
        [InlineData("4", "4th")]
        [InlineData("5", "5th")]
        [InlineData("6", "6th")]
        [InlineData("7", "7th")]
        [InlineData("8", "8th")]
        [InlineData("9", "9th")]
        [InlineData("10", "10th")]
        [InlineData("11", "11th")]
        [InlineData("12", "12th")]
        [InlineData("13", "13th")]
        [InlineData("14", "14th")]
        [InlineData("20", "20th")]
        [InlineData("21", "21st")]
        [InlineData("22", "22nd")]
        [InlineData("23", "23rd")]
        [InlineData("24", "24th")]
        [InlineData("100", "100th")]
        [InlineData("101", "101st")]
        [InlineData("102", "102nd")]
        [InlineData("103", "103rd")]
        [InlineData("104", "104th")]
        [InlineData("110", "110th")]
        [InlineData("1000", "1000th")]
        [InlineData("1001", "1001st")]
        public void OrdinalizeTest(string source, string expectedResult)
        {
            Assert.Equal(expectedResult, source.Ordinalize());
        }

        [Theory]
        [InlineData("customer", "Customer")]
        [InlineData("CUSTOMER", "CUSTOMER")]
        [InlineData("CUStomer", "CUStomer")]
        [InlineData("customer_name", "CustomerName")]
        [InlineData("customer_first_name", "CustomerFirstName")]
        [InlineData("customer_first_name_goes_here", "CustomerFirstNameGoesHere")]
        [InlineData("customer name", "Customer name")]
        public void PascalizeTest(string source, string expectedResult)
        {
            Assert.Equal(expectedResult, source.Pascalize());
        }

        [Theory]
        [InlineData("customer", "customer")]
        [InlineData("CUSTOMER", "cUSTOMER")]
        [InlineData("CUStomer", "cUStomer")]
        [InlineData("customer_name", "customerName")]
        [InlineData("customer_first_name", "customerFirstName")]
        [InlineData("customer_first_name_goes_here", "customerFirstNameGoesHere")]
        [InlineData("customer name", "customer name")]
        public void CamelizeTest(string source, string expectedResult)
        {
            Assert.Equal(expectedResult, source.Camelize());
        }

        [Theory]
        [InlineData("some title", "Some Title")]
        [InlineData("some-title", "Some Title")]
        [InlineData("sometitle", "Sometitle")]
        [InlineData("some-title: The begining", "Some Title: The Begining")]
        [InlineData("some_title:_the_begining", "Some Title: The Begining")]
        [InlineData("some title: The_begining", "Some Title: The Begining")]
        public void TitleizeTest(string source, string expectedResult)
        {
            Assert.Equal(expectedResult, source.Titleize());
        }

        [Theory]
        [InlineData("some title", "some title")]
        [InlineData("some Title", "some Title")]
        [InlineData("SOMETITLE", "sOMETITLE")]
        [InlineData("someTitle", "someTitle")]
        [InlineData("some title goes here", "some title goes here")]
        [InlineData("some TITLE", "some TITLE")]
        public void UnCapitalizeTest(string source, string expectedResult)
        {
            Assert.Equal(expectedResult, source.UnCapitalize());
        }

        [Theory]
        [InlineData("SomeTitle", "some_title")]
        [InlineData("someTitle", "some_title")]
        [InlineData("some title", "some_title")]
        [InlineData("some title that will be underscored", "some_title_that_will_be_underscored")]
        [InlineData("SomeTitleThatWillBeUnderscored", "some_title_that_will_be_underscored")]
        public void UnderscoreTest(string source, string expectedResult)
        {
            Assert.Equal(expectedResult, source.Underscore());
        }
    }
}