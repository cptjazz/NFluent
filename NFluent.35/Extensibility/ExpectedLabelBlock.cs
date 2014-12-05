#region File header

// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="ExpectedLabelBlock.cs" company="">
// //   Copyright 2014 Cyrille Dupuydauy, Thomas PIERRAIN
// //   Licensed under the Apache License, Version 2.0 (the "License");
// //   you may not use this file except in compliance with the License.
// //   You may obtain a copy of the License at
// //       http://www.apache.org/licenses/LICENSE-2.0
// //   Unless required by applicable law or agreed to in writing, software
// //   distributed under the License is distributed on an "AS IS" BASIS,
// //   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// //   See the License for the specific language governing permissions and
// //   limitations under the License.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------
#endregion

namespace NFluent.Extensibility
{
    /// <summary>
    /// The expected label block.
    /// </summary>
    internal class ExpectedLabelBlock : GenericLabelBlock
    {
        protected override string Adjective()
        {
            return "expected";
        }
   }

    /// <summary>
    /// The expected label block.
    /// </summary>
    internal class GivenLabelBlock : GenericLabelBlock
    {
        protected override string Adjective()
        {
            return "given";
        }
    }
}