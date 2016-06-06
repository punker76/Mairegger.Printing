// Copyright 2016 Michael Mairegger
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Mairegger.Printing.Test.Content
{
    using System.Drawing.Printing;
    using System.IO;
    using System.Linq;
    using System.Printing;
    using System.Threading;
    using System.Windows.Controls;
    using Mairegger.Printing.PrintProcessor;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class PrintProcessorCollectionTests
    {
        [Test]
        public void Ctor()
        {
            Mock<Printing.PrintProcessor.PrintProcessor>[] m1 =
            {
                new Mock<Printing.PrintProcessor.PrintProcessor>(),
                new Mock<Printing.PrintProcessor.PrintProcessor>(),
                new Mock<Printing.PrintProcessor.PrintProcessor>(),
                new Mock<Printing.PrintProcessor.PrintProcessor>()
            };
            PrintProcessorCollection pp = new PrintProcessorCollection(m1.Select(i => i.Object), "FileName");
            CollectionAssert.AreEqual(m1.Select(i => i.Object), pp);

            Assert.That(pp.FileName, Is.EqualTo("FileName"));
        }

        [Test]
        public void Ctor_Null_ThrowsArgumentNullException()
        {
            Assert.That(() => new PrintProcessorCollection(null, "FileName"), Throws.ArgumentNullException);
            Assert.That(() => new PrintProcessorCollection(null), Throws.ArgumentNullException);
        }

        [Test]
        public void Ctor_SingleElement()
        {
            var p = new Mock<Printing.PrintProcessor.PrintProcessor>();
            PrintProcessorCollection pp = new PrintProcessorCollection(p.Object);

            Assert.That(pp.FileName, Is.EqualTo(p.Object.FileName));

            CollectionAssert.Contains(pp, p.Object);

            Assert.That(pp.Count, Is.EqualTo(1));
        }

        [Test]
        public void FileName_Default_IsStringEmpty()
        {
            var ppcoll = new PrintProcessorCollection(Enumerable.Empty<Printing.PrintProcessor.PrintProcessor>());
            Assert.That(ppcoll.FileName, Is.Empty);
        }

        [Test]
        public void FileName_InvalidCharacters_GetsRemoved()
        {
            var ppcoll = new PrintProcessorCollection(Enumerable.Empty<Printing.PrintProcessor.PrintProcessor>());
            var formattableString = $"Hello{Path.GetInvalidFileNameChars()[0]}Hello{Path.GetInvalidFileNameChars()[1]}";

            Assert.That(ppcoll.FileName, Is.Empty);

            ppcoll.FileName = formattableString;
            CollectionAssert.DoesNotContain(ppcoll.FileName, Path.GetInvalidFileNameChars());
        }

        [Test]
        public void FileName_Null_IsStringEmpty()
        {
            var ppcoll = new PrintProcessorCollection(Enumerable.Empty<Printing.PrintProcessor.PrintProcessor>());

            ppcoll.FileName = null;
            Assert.That(ppcoll.FileName, Is.Empty);
        }

        [Test]
        public void PreviewDocument()
        {
            var printProcessor = new PrintProcessorCollection(Enumerable.Empty<Printing.PrintProcessor.PrintProcessor>());
            var windowProvider = new Mock<IWindowProvider>();

            printProcessor.PreviewDocument(windowProvider.Object);

            windowProvider.Verify(i => i.Show(It.IsNotNull<string>(), It.IsNotNull<DocumentViewer>()), Times.Never);
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void PreviewDocument1()
        {
            var printDialog = new Mock<IPrintDialog>();
            var testPrintProcessor = new TestPrintProcessor();
            testPrintProcessor.PrintDialog = printDialog.Object;
            var printProcessor = new PrintProcessorCollection(testPrintProcessor);
            var windowProvider = new Mock<IWindowProvider>();
            printProcessor.PreviewDocument(windowProvider.Object);

            windowProvider.Verify(i => i.Show(It.IsNotNull<string>(), It.IsNotNull<DocumentViewer>()), Times.Once);
        }

        [Test]
        public void PrintDocument_NoPrintProcessor_DoesNotPrint()
        {
            var ppcoll = new PrintProcessorCollection(Enumerable.Empty<Printing.PrintProcessor.PrintProcessor>());
            Assert.That(ppcoll.PrintDocument(), Is.False);
            Assert.That(ppcoll.PrintDocument(string.Empty), Is.False);
        }

        [Test]
        public void PrintDoucment_CloseDialog_ReturnsFalse()
        {
            var printDialog = new Mock<IPrintDialog>();
            printDialog.Setup(i => i.ShowDialog()).Returns(false);

            var testPrintProcessor = new TestPrintProcessor();
            testPrintProcessor.PrintDialog = printDialog.Object;
            var printProcessor = new PrintProcessorCollection(testPrintProcessor);

            Assert.That(printProcessor.PrintDocument(), Is.False);
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void PrintDoucment_Direct_ReturnsTrue()
        {
            var printDialog = new Mock<IPrintDialog>();
            printDialog.Setup(i => i.ShowDialog()).Returns(true);

            var testPrintProcessor = new TestPrintProcessor();
            var printProcessor = new PrintProcessorCollection(testPrintProcessor);

            testPrintProcessor.PrintDialog = printDialog.Object;

            Assert.That(printProcessor.PrintDocument(), Is.True);
            Assert.That(printProcessor.PrintDocument(PrinterSettings.InstalledPrinters[0], new LocalPrintServer()), Is.True);
            Assert.That(printProcessor.PrintDocument(PrinterSettings.InstalledPrinters[0]), Is.True);
        }
    }
}