using System;
using System.IO;
using NeoModules.Core;
using NeoModules.Core.KeyPair;
using NeoModules.Core.NVM;
using NeoModules.NEP6.Transactions;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Helper = NeoModules.Core.KeyPair.Helper;

namespace NeoModules.NEP6.Helpers
{
    public static class Utils
    {
        public static readonly UInt256 NeoToken = UInt256.Parse("c56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b");
        public static readonly UInt256 GasToken = UInt256.Parse("602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7");

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static byte[] TranscodeSignatureToConcat(byte[] derSignature, int outputLength)
        {
            if (derSignature.Length < 8 || derSignature[0] != 48) throw new Exception("Invalid ECDSA signature format");

            int offset;
            if (derSignature[1] > 0)
                offset = 2;
            else if (derSignature[1] == 0x81)
                offset = 3;
            else
                throw new Exception("Invalid ECDSA signature format");

            var rLength = derSignature[offset + 1];

            int i = rLength;
            while (i > 0
                   && derSignature[offset + 2 + rLength - i] == 0)
                i--;

            var sLength = derSignature[offset + 2 + rLength + 1];

            int j = sLength;
            while (j > 0
                   && derSignature[offset + 2 + rLength + 2 + sLength - j] == 0)
                j--;

            var rawLen = Math.Max(i, j);
            rawLen = Math.Max(rawLen, outputLength / 2);

            if ((derSignature[offset - 1] & 0xff) != derSignature.Length - offset
                || (derSignature[offset - 1] & 0xff) != 2 + rLength + 2 + sLength
                || derSignature[offset] != 2
                || derSignature[offset + 2 + rLength] != 2)
                throw new Exception("Invalid ECDSA signature format");

            var concatSignature = new byte[2 * rawLen];

            Array.Copy(derSignature, offset + 2 + rLength - i, concatSignature, rawLen - i, i);
            Array.Copy(derSignature, offset + 2 + rLength + 2 + sLength - j, concatSignature, 2 * rawLen - j, j);

            return concatSignature;
        }

        internal static byte[] Sign(byte[] message, byte[] prikey)
        {
            var signer = SignerUtilities.GetSigner("SHA256withECDSA");
            var curve = NistNamedCurves.GetByName("P-256");
            var dom = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);
            ECKeyParameters privateKeyParameters = new ECPrivateKeyParameters(new BigInteger(1, prikey), dom);

            signer.Init(true, privateKeyParameters);
            signer.BlockUpdate(message, 0, message.Length);
            var sig = signer.GenerateSignature();

            return TranscodeSignatureToConcat(sig, 64);
        }

        public static uint ToTimestamp(this DateTime time)
        {
            return (uint)(time.ToUniversalTime() - UnixEpoch).TotalSeconds;
        }

        public static byte[] GenerateScript(string scriptHash, string operation, object[] args, bool addNonce = false)
        {
            return GenerateScript(UInt160.Parse(scriptHash), operation, args, addNonce);
        }

        public static byte[]
            GenerateScript(UInt160 script, string operation, object[] args, bool addNonce = false) 
            //todo maintain nonce for sending the same txhash in case of tx failure/mempool
        {
            using (var sb = new ScriptBuilder())
            {
                if (args != null)
                    sb.EmitAppCall(script, operation, args);
                else
                    sb.EmitAppCall(script, operation);

                if (addNonce)
                {
                    var timestamp = DateTime.UtcNow.ToTimestamp();
                    var nonce = BitConverter.GetBytes(timestamp);

                    sb.Emit(OpCode.RET);
                    sb.EmitPush(nonce);
                }

                return sb.ToArray();
            }
        }

        public static byte[] GetHashData(Transaction tx)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                tx.SerializeUnsigned(writer);
                writer.Flush();
                return ms.ToArray();
            }
        }

        public static byte[] Sign(KeyPair key, Transaction tx, bool signed = true)
        {
            var txData = tx.SerializeTransaction();

            var signature = Sign(txData, key.PrivateKey);
            if (signed)
            {
                var invocationScript = ("40" + signature.ToHexString()).HexToBytes();
                var verificationScript = Helper.CreateSignatureRedeemScript(key.PublicKey);
                tx.Witnesses = new[]
                {
                    new Witness
                    {
                        InvocationScript = invocationScript,
                        VerificationScript = verificationScript
                    }
                };
                return tx.ToArray();
            }

            return signature;
        }
    }

    public class WalletException : Exception
    {
        public WalletException(string msg) : base(msg)
        {
        }
    }
}