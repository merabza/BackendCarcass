using System.Collections.Generic;
using Moq;
using System.IO;
using System.Text;
using CarcassShared.Models;
using CarcassShared.Tests.Models;
using Moq.Language.Flow;
using Xunit;

namespace CarcassShared.Tests
{
    public class FileLoaderTests
    {
        [Fact]
        public void LoadTest()
        {
            string fakeFileContents = "Hello world";
            FileLoader fileLoader = PrepareFileLoader(fakeFileContents);

            string result = fileLoader.Load("test.txt");

            Assert.Equal(fakeFileContents, result);
        }

        private FileLoader PrepareFileLoader(string fakeFileContents, bool withException = false)
        {
            Mock<IFileManager> mockFileManager = new Mock<IFileManager>();
            byte[] fakeFileBytes = Encoding.UTF8.GetBytes(fakeFileContents);

            MemoryStream fakeMemoryStream = new MemoryStream(fakeFileBytes);

            ISetup<IFileManager, StreamReader> mock =
                mockFileManager.Setup(fileManager => fileManager.StreamReader(It.IsAny<string>()));
            if (withException)
                mock.Throws(new IOException("io error"));
            else
                mock.Returns(() => new StreamReader(fakeMemoryStream));

            return new FileLoader(mockFileManager.Object);
        }

        [Fact]
        public void LoadDeserializeResolveTestWithException()
        {
            string fakeFileContents = @"{
      ""hostUrl"": ""http://*:5011""
    }";
            FileLoader fileLoader = PrepareFileLoader(fakeFileContents, true);

            Kstrl result = fileLoader.LoadDeserializeResolve<Kstrl>("test.txt");
            Kstrl expected = default(Kstrl);
            //Assert.Throws<IOException>(() => fileLoader.LoadDeserializeResolve<Kstrl>("test.txt"));
            Assert.Equal(expected, result);
        }

        [Fact]
        public void LoadDeserializeResolveTest()
        {
            string fakeFileContents = @"{
      ""hostUrl"": ""http://*:5011""
    }";
            FileLoader fileLoader = PrepareFileLoader(fakeFileContents);

            Kstrl result = fileLoader.LoadDeserializeResolve<Kstrl>("test.txt");
            Kstrl expected = new Kstrl { HostUrl = "http://*:5011" };
            Assert.Equal(expected, result);
            //Assert.True(expected.Equals(result));
        }

        [Fact]
        public void LoadDeserializeResolveTestForList()
        {
            string fakeFileContents = @"
[
  {
    ""FirstName"": ""Auto"",
      ""LastName"": ""Administrator"",
      ""UserName"": ""admin"",
      ""Password"": ""Password1"",
      ""Email"": ""admin@example.ge""
    },
    {
      ""FirstName"": ""Elf"",
      ""LastName"": ""Counter"",
      ""UserName"": ""elf"",
      ""Password"": ""Password2"",
      ""Email"": ""elf@example.ge""
    },
    {
      ""FirstName"": ""Troll"",
      ""LastName"": ""Zak"",
      ""UserName"": ""Troll"",
      ""Password"": ""Password3"",
      ""Email"": ""merabza@Example.ge""
    }
    ]";
            FileLoader fileLoader = PrepareFileLoader(fakeFileContents);

            List<TestUser> result = fileLoader.LoadDeserializeResolve<List<TestUser>>("test.txt");
            List<TestUser> expected = new List<TestUser>
            {
                new TestUser
                {
                    FirstName = "Auto", LastName = "Administrator", UserName = "admin", Password = "Password1",
                    Email = "admin@example.ge"
                },
                new TestUser
                {
                    FirstName = "Elf", LastName = "Counter", UserName = "elf", Password = "Password2",
                    Email = "elf@example.ge"
                },
                new TestUser
                {
                    FirstName = "Troll", LastName = "Zak", UserName = "Troll", Password = "Password3",
                    Email = "merabza@Example.ge"
                }
            };
            Assert.Equal(expected, result);
            //Assert.True(expected.Equals(result));
        }
    }
}