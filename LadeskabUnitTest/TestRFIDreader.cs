﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LadeskabLogik;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace LadeskabUnitTest
{
    [TestFixture()]
    class TestRFIDreader
    {
        private IRfidReader _uut;
        private RfidSensedEventArgs _recivedEventArgs;

        [SetUp]
        public void Setup()
        {
            _recivedEventArgs = null;
            _uut = new RfidReader();

            _uut.RfidSensedEvents += (o, args) => { _recivedEventArgs = args; };
        }

        [Test]
        public void test()
        {
            _uut.SetRfidReaderStatus(true);
            Assert.That(_recivedEventArgs.RfidSensed, Is.True);
        }

        [Test]
        public void test2()
        {
            _uut.SetRfidReaderStatus(false);
            Assert.That(_recivedEventArgs.RfidSensed, Is.False);
        }
    }
}
