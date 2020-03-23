using ALE.ETLBox;
using ALE.ETLBox.ConnectionManager;
using ALE.ETLBox.ControlFlow;
using ALE.ETLBox.DataFlow;
using ALE.ETLBox.Helper;
using ALE.ETLBox.Logging;
using ALE.ETLBoxTests.Fixtures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace ALE.ETLBoxTests.DataFlowTests
{
    [Collection("DataFlow")]
    public class CsvDestinationErrorLinkingTests
    {
        public CsvDestinationErrorLinkingTests()
        {

        }

        public class MySimpleRow
        {
            public string Col1 { get; set; }
            public string Col2
            {
                get
                {
                    if (Col1 == null || Col1 == "X")
                        throw new Exception("Error record!");
                    else
                        return "Test" + Col1;
                }
            }
        }

        [Fact]
        public void RedirectSingleRecordWithObject()
        {
            //Arrange
            MemorySource<MySimpleRow> source = new MemorySource<MySimpleRow>();
            source.Data = new List<MySimpleRow>()
            {
                new MySimpleRow() { Col1 = "X" },
                new MySimpleRow() { Col1 = "1" },
                new MySimpleRow() { Col1 = "2" },
                new MySimpleRow() { Col1 = null },
                new MySimpleRow() { Col1 = "3" },

            };
            CsvDestination<MySimpleRow> dest = new CsvDestination<MySimpleRow>("ErrorFile.csv");
            MemoryDestination<ETLBoxError> errorDest = new MemoryDestination<ETLBoxError>();

            //Act
            source.LinkTo(dest);
            dest.LinkErrorTo(errorDest);
            source.Execute();
            dest.Wait();
            errorDest.Wait();

            //Assert
            Assert.Equal(File.ReadAllText("./ErrorFile.csv"),
                 File.ReadAllText("res/CsvDestination/TwoColumnsErrorLinking.csv"));
            Assert.Collection<ETLBoxError>(errorDest.Data,
                d => Assert.True(!string.IsNullOrEmpty(d.RecordAsJson) && !string.IsNullOrEmpty(d.ErrorText)),
                 d => Assert.True(!string.IsNullOrEmpty(d.RecordAsJson) && !string.IsNullOrEmpty(d.ErrorText))
            );
        }


        [Fact]
        public void NoErrorHandling()
        {
            //Arrange
            MemorySource<MySimpleRow> source = new MemorySource<MySimpleRow>();
            source.Data = new List<MySimpleRow>()
            {
                new MySimpleRow() { Col1 = "X" },
                new MySimpleRow() { Col1 = "1" },
                new MySimpleRow() { Col1 = null }
            };
            CsvDestination<MySimpleRow> dest = new CsvDestination<MySimpleRow>("ErrorFileNoError.csv");

            //Act
            //Assert
            Assert.ThrowsAny<Exception>(() =>
           {
               source.LinkTo(dest);
               source.Execute();
               dest.Wait();
           });
        }
    }
}
