//------------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Security.Cryptography;
using Xunit;

namespace Microsoft.IdentityModel.Tokens.Tests
{
    public class RsaSecurityKeyTests
    {
        [Fact]
        public void Constructor()
        {
            // testing constructor that takes rsa parameters
            RsaSecurityKeyConstructor(KeyingMaterial.RsaParameters_1024, ExpectedException.NoExceptionExpected);
            RsaSecurityKeyConstructor(KeyingMaterial.RsaParameters_1024_Public, ExpectedException.NoExceptionExpected);

            // missing modulus or exponent
            RsaSecurityKeyConstructor(KeyingMaterial.RsaParameters_2048_MissingExponent, ExpectedException.ArgumentException("IDX10700"));
            RsaSecurityKeyConstructor(KeyingMaterial.RsaParameters_2048_MissingModulus, ExpectedException.ArgumentException("IDX10700"));

            // testing constructor that takes Rsa instance
            RsaSecurityKeyConstructorWithRsa(null, ExpectedException.ArgumentNullException("rsa"));
#if NET451
            RSA rsaCsp_2048 = new RSACryptoServiceProvider();
            rsaCsp_2048.ImportParameters(KeyingMaterial.RsaParameters_2048);
            RSA rsaCsp_2048_Public = new RSACryptoServiceProvider();
            rsaCsp_2048_Public.ImportParameters(KeyingMaterial.RsaParameters_2048_Public);

            RsaSecurityKeyConstructorWithRsa(rsaCsp_2048, ExpectedException.NoExceptionExpected);
            RsaSecurityKeyConstructorWithRsa(rsaCsp_2048_Public, ExpectedException.NoExceptionExpected);
#endif
        }

        private void RsaSecurityKeyConstructor(RSAParameters parameters, ExpectedException ee)
        {
            try
            {
                var rsaSecurityKey = new RsaSecurityKey(parameters);
                ee.ProcessNoException();
            }
            catch (Exception exception)
            {
                ee.ProcessException(exception);
            }
        }

        private void RsaSecurityKeyConstructorWithRsa(RSA rsa, ExpectedException ee)
        {
            try
            {
                var rsaSecurityKey = new RsaSecurityKey(rsa);
                ee.ProcessNoException();
            }
            catch (Exception exception)
            {
                ee.ProcessException(exception);
            }
        }

#pragma warning disable CS3016 // Arrays as attribute arguments is not CLS-compliant
        [Theory, MemberData("HasPrivateKeyTheoryData")]
#pragma warning restore CS3016 // Arrays as attribute arguments is not CLS-compliant
        public void HasPrivateKey(string testId, AsymmetricSecurityKey key, bool expected)
        {
            if (expected)
                Assert.True(key.HasPrivateKey, testId);
            else
                Assert.False(key.HasPrivateKey, testId);
        }

        public static TheoryData<string, SecurityKey, bool> HasPrivateKeyTheoryData()
        {
            var theoryData = new TheoryData<string, SecurityKey, bool>();
#if NET451
            theoryData.Add(
                "KeyingMaterial.RsaSecurityKeyWithCspProvider_2048",
                KeyingMaterial.RsaSecurityKeyWithCspProvider_2048,
                true
            );

            theoryData.Add(
                "KeyingMaterial.RsaSecurityKeyWithCspProvider_2048_Public",
                KeyingMaterial.RsaSecurityKeyWithCspProvider_2048_Public,
                false
            );
#endif

#if NETCOREAPP1_0
            theoryData.Add(
                "KeyingMaterial.RsaSecurityKeyWithCngProvider_2048",
                KeyingMaterial.RsaSecurityKeyWithCngProvider_2048,
                true
            );

            theoryData.Add(
                "KeyingMaterial.RsaSecurityKeyWithCngProvider_2048_Public",
                KeyingMaterial.RsaSecurityKeyWithCngProvider_2048_Public,
                false
            );
#endif
            theoryData.Add(
                "KeyingMaterial.RsaSecurityKey_2048",
                KeyingMaterial.RsaSecurityKey_2048,
                true
            );

            theoryData.Add(
                "KeyingMaterial.RsaSecurityKey_2048_Public",
                KeyingMaterial.RsaSecurityKey_2048_Public,
                false
            );

            return theoryData;
        }

        [Fact]
        public void KeySize()
        {
#if NET451
            Assert.True(KeyingMaterial.RsaSecurityKeyWithCspProvider_2048.KeySize == 2048, string.Format(CultureInfo.InvariantCulture, "Keysize '{0}' != 2048", KeyingMaterial.RsaSecurityKeyWithCspProvider_2048.KeySize));
            Assert.True(KeyingMaterial.RsaSecurityKeyWithCspProvider_2048_Public.KeySize == 2048, string.Format(CultureInfo.InvariantCulture, "Keysize '{0}' != 2048", KeyingMaterial.RsaSecurityKeyWithCspProvider_2048.KeySize));
#endif
            Assert.True(KeyingMaterial.RsaSecurityKey_2048.KeySize == 2048, string.Format(CultureInfo.InvariantCulture, "Keysize '{0}' != 2048", KeyingMaterial.RsaSecurityKey_2048.KeySize));
            Assert.True(KeyingMaterial.RsaSecurityKey_4096.KeySize == 4096, string.Format(CultureInfo.InvariantCulture, "Keysize '{0}' != 4096", KeyingMaterial.RsaSecurityKey_4096.KeySize));
        }

#pragma warning disable CS3016 // Arrays as attribute arguments is not CLS-compliant
        [Theory, MemberData("IsSupportedAlgDataSet")]
#pragma warning restore CS3016 // Arrays as attribute arguments is not CLS-compliant
        public void IsSupportedAlgorithm(RsaSecurityKey key, string alg, bool isPrivateKey, bool expectedResult)
        {
            if (key.CryptoProviderFactory.IsSupportedAlgorithm(alg, key) != expectedResult)
                Assert.True(false, string.Format("{0} failed with alg: {1}. ExpectedResult: {2}", key, alg, expectedResult));
       }

        public static TheoryData<RsaSecurityKey, string, bool, bool> IsSupportedAlgDataSet
        {
            get
            {
                var dataset = new TheoryData<RsaSecurityKey, string, bool, bool>();
#if NET451
                dataset.Add(KeyingMaterial.RsaSecurityKeyWithCspProvider_2048, SecurityAlgorithms.RsaSha256Signature, KeyingMaterial.RsaSecurityKeyWithCspProvider_2048.HasPrivateKey, true);
                dataset.Add(KeyingMaterial.RsaSecurityKeyWithCspProvider_2048_Public, SecurityAlgorithms.RsaSha256, KeyingMaterial.RsaSecurityKeyWithCspProvider_2048_Public.HasPrivateKey, true);
                dataset.Add(KeyingMaterial.RsaSecurityKeyWithCspProvider_2048, SecurityAlgorithms.EcdsaSha256, KeyingMaterial.RsaSecurityKeyWithCspProvider_2048.HasPrivateKey, false);
#endif
                dataset.Add(KeyingMaterial.RsaSecurityKey_2048, SecurityAlgorithms.RsaSha256Signature, KeyingMaterial.RsaSecurityKey_2048.HasPrivateKey, true);
                RsaSecurityKey testKey = new RsaSecurityKey(KeyingMaterial.RsaParameters1);
                testKey.CryptoProviderFactory = new CustomCryptoProviderFactory(new string[] { SecurityAlgorithms.EcdsaSha256 });
                dataset.Add(testKey, SecurityAlgorithms.EcdsaSha256, testKey.HasPrivateKey, true);
                return dataset;
            }
        }
    }
}
