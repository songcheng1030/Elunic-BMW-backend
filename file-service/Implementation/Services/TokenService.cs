using System;
using System.Text;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;

namespace AIQXFileService.Implementation.Services
{
    public class TokenService
    {
        public TokenService()
        {
        }
        public string generateToken_Ed25519(string privateKey, object data)
        {
            if (string.IsNullOrEmpty(privateKey) || data == null)
            {
                return null;
            }

            // create key object
            var privateKeyBytes = Convert.FromBase64String(privateKey);
            Ed25519PrivateKeyParameters signerKey = new Ed25519PrivateKeyParameters(privateKeyBytes, 0);

            // serialize data to bytes
            string dataStr = JsonConvert.SerializeObject(data);
            var dataBytes = Encoding.UTF8.GetBytes(dataStr);

            // generate signature
            var signer = new Ed25519Signer();
            signer.Init(true, signerKey);
            signer.BlockUpdate(dataBytes, 0, dataBytes.Length);
            byte[] signature = signer.GenerateSignature();

            string token = Convert.ToBase64String(signature);
            return token;
        }
        public bool validateToken_Ed25519(string publicKey, object data, string token)
        {
            // create key object
            var publicKeyBytes = Convert.FromBase64String(publicKey);
            Ed25519PublicKeyParameters validatorKey = new Ed25519PublicKeyParameters(publicKeyBytes, 0);

            // covert token to bytes
            byte[] tokenBytes = Convert.FromBase64String(token);

            // serialize data to bytes
            string dataStr = JsonConvert.SerializeObject(data);
            byte[] dataBytes = Encoding.UTF8.GetBytes(dataStr);

            // signature validation
            var validator = new Ed25519Signer();
            validator.Init(false, validatorKey);
            validator.BlockUpdate(dataBytes, 0, dataBytes.Length);

            bool isValidToken = validator.VerifySignature(tokenBytes);
            return isValidToken;
        }
    }
}