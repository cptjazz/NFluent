﻿//  --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ValueBlockTests.cs" company="">
//    Copyright 2014 Cyrille DUPUYDAUBY
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//        http://www.apache.org/licenses/LICENSE-2.0
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace NFluent.Tests
{
    using NFluent.Extensibility;

    using NUnit.Framework;

    [TestFixture]
    public class ValueBlockTests
    {
        [Test]
        public void ShouldWorkForBasicValue()
        {
            var blk = new ValueBlock(2);
            Assert.AreEqual("[2]", blk.GetMessage());

            blk.WithType();
            Assert.AreEqual("[2] of type: [int]", blk.GetMessage());
        }

        [Test]
        public void ShouldWorkForInstance()
        {
            var blk = new InstanceBlock(typeof(string));

            Assert.AreEqual("an instance of type: [string]", blk.GetMessage());
        }

        [Test]
        public void ShouldWorkForEnumeration()
        {
            var list = new []{ "a", "b", "c" };
            var blk = new ValueBlock(list);

            Assert.AreEqual("[\"a\", \"b\", \"c\"]", blk.GetMessage());

            blk.WithEnumerableCount(list.GetLength(0));
            Assert.AreEqual("[\"a\", \"b\", \"c\"] (3 items)", blk.GetMessage());
        }
    }
}