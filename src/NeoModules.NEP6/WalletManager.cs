using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using NeoModules.Core;
using NeoModules.Core.KeyPair;
using NeoModules.JsonRpc.Client;
using NeoModules.NEP6.Helpers;
using NeoModules.NEP6.Models;
using NeoModules.NEP6.TransactionManagers;
using NeoModules.Rest.Interfaces;
using NeoModules.Rest.Services;
using ECCurve = NeoModules.Core.KeyPair.Cryptography.ECC.ECCurve;
using Helper = NeoModules.Core.KeyPair.Helper;
using NeoECPoint = NeoModules.Core.KeyPair.Cryptography.ECC.ECPoint;

namespace NeoModules.NEP6
{
    /// <summary>
    ///     Class responsible for basic wallet management.
    /// </summary>
    public class WalletManager
    {
        private readonly Wallet _wallet;
        private IClient _client;
        private INeoscanService _restService;

        /// <summary>
        ///     Online Wallet Manager construtor
        /// </summary>
        /// <param name="wallet"></param>
        /// <param name="restService"></param>
        /// <param name="client"></param>
        public WalletManager(INeoscanService restService, IClient client, Wallet wallet = null)
        {
            _restService = restService;
            _client = client;

            _wallet = wallet ?? new Wallet();
            if (_wallet.Accounts.Any())
                foreach (var walletAccount in _wallet.Accounts)
                    walletAccount.TransactionManager =
                        new AccountSignerTransactionManager(_client, _restService, walletAccount);
        }

        /// <summary>
        ///     Offline Wallet Manager construtor
        /// </summary>
        /// <param name="wallet"></param>
        public WalletManager(Wallet wallet)
        {
            _wallet = wallet;
            _client = null;
            _restService = null;
        }

        /// <summary>
        ///     Offline Wallet Manager construtor with empty wallet
        /// </summary>
        /// <param name="walletLabel"></param>
        public WalletManager(string walletLabel)
        {
            _wallet = new Wallet(walletLabel);
        }

        /// <summary>
        ///     Adds or Updates an Account to the Accounts List on the associated Wallet.
        /// </summary>
        /// <param name="account">Account to be added/updated</param>
        public void AddAccount(Account account)
        {
            lock (_wallet.Accounts)
            {
                var oldAccount = _wallet.Accounts.FirstOrDefault(a => a.Address == account.Address);
                if (oldAccount != null) // update account
                {
                    oldAccount.IsDefault = account.IsDefault;
                    oldAccount.Nep2Key = account.Nep2Key;
                    oldAccount.IsLock = account.IsLock;
                    oldAccount.Label = account.Label;
                    oldAccount.Extra = account.Extra;
                    if (account.Contract == null)
                    {
                        account.Contract = oldAccount.Contract;
                    }
                    else
                    {
                        var oldContract = oldAccount.Contract;
                        if (oldContract != null)
                        {
                            var contract = account.Contract;
                            contract.Parameters = oldContract.Parameters;
                            contract.Deployed = oldContract.Deployed;
                        }
                    }

                    oldAccount.TransactionManager =
                        new AccountSignerTransactionManager(_client, _restService, oldAccount);
                    account.TransactionManager = oldAccount.TransactionManager;
                }
                else // add new account to list
                {
                    _wallet.Accounts.Add(account);
                    account.TransactionManager = new AccountSignerTransactionManager(_client, _restService, account);
                }
            }
        }

        /// <summary>
        ///     Returns the first Account on the Wallet that has IsDefault set to true. Else, returns the first one with that has
        ///     an encrypted key.
        /// </summary>
        /// <returns></returns>
        public Account GetDefaultAccount()
        {
            if (_wallet.Accounts.Count == 0) throw new WalletException($"No accounts available in this Wallet");
            var defaultAccount = _wallet.Accounts.FirstOrDefault(a => a.IsDefault);
            if (defaultAccount != null) return defaultAccount;
            defaultAccount = _wallet.Accounts.FirstOrDefault(a => !string.IsNullOrEmpty(a.Nep2Key));
            return defaultAccount ?? _wallet.Accounts[0];
        }

        /// <summary>
        ///     Change the Default account on the current open wallet
        /// </summary>
        /// <param name="account"></param>
        public void ChangeDefaultAccount(Account account)
        {
            if (_wallet.Accounts.Count == 0) throw new WalletException($"No accounts available in this Wallet");
            if (account == null) throw new WalletException($"AddressScriptHash cannot be null");

            var newDefaultAccounAddress = account.Address.ToAddress();

            if (_wallet.Accounts.All(acc => acc.Address.ToAddress() != newDefaultAccounAddress))
                throw new WalletException($"No account with the specified address found");
            foreach (var walletAccount in _wallet.Accounts) walletAccount.IsDefault = false;

            var newDefaultAccount = _wallet.Accounts.SingleOrDefault(acc => acc.Address.ToAddress() == newDefaultAccounAddress);
            if (newDefaultAccount != null) newDefaultAccount.IsDefault = true;
        }

        public void ChangeDefaultAccount(int index)
        {
            var account = _wallet.Accounts[index];
            ChangeDefaultAccount(account);
        }

        public void ChangeDefaultAccount(string address)
        {
            var account = _wallet.Accounts.SingleOrDefault(acc => acc.Address.ToAddress() == address);
            ChangeDefaultAccount(account);
        }

        /// <summary>
        ///     Decrypts and add the account to the Wallet Account List, using WIF.
        /// </summary>
        /// <param name="wif"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public Account ImportAccount(string wif, string label)
        {
            var key = new KeyPair(Wallet.GetPrivateKeyFromWif(wif));
            var contract = new Contract
            {
                Script = Helper.CreateSignatureRedeemScript(key.PublicKey),
                Parameters = new List<Parameter>
                {
                    new Parameter("signature", ParameterType.Signature)
                },
                Deployed = false
            };
            var account = new Account(contract.ScriptHash, key)
            {
                Contract = contract,
                Label = label
            };
            AddAccount(account);
            return account;
        }

        /// <summary>
        ///     Decrypts and add the account to the Wallet Account List, using WIF.
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public Account ImportAccount(byte[] privateKey, string label)
        {
            var key = new KeyPair(privateKey);
            var contract = new Contract
            {
                Script = Helper.CreateSignatureRedeemScript(key.PublicKey),
                Parameters = new List<Parameter>
                {
                    new Parameter("signature", ParameterType.Signature)
                },
                Deployed = false
            };
            var account = new Account(contract.ScriptHash, key)
            {
                Contract = contract,
                Label = label
            };
            AddAccount(account);
            return account;
        }

        /// <summary>
        ///     Decrypts and add the account to the Wallet Account List, using NEP2.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="encryptedPrivateKey"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<Account> ImportAccount(string encryptedPrivateKey, string password, string label)
        {
            var privateKey = await Nep2.Decrypt(encryptedPrivateKey, password, _wallet.Scrypt);
            var key = new KeyPair(privateKey);
            Array.Clear(privateKey, 0, privateKey.Length);

            var contract = new Contract
            {
                Script = Helper.CreateSignatureRedeemScript(key.PublicKey),
                Parameters = new List<Parameter>
                {
                    new Parameter("signature", ParameterType.Signature)
                },
                Deployed = false
            };

            var account = new Account(contract.ScriptHash, key)
            {
                Nep2Key = encryptedPrivateKey,
                Label = label,
                Contract = contract,
                IsDefault = false
            };

            AddAccount(account);
            return account;
        }

        /// <summary>
        ///     Creates an Account and returns it.
        /// </summary>
        /// <returns></returns>
        public Account CreateAccount(string label)
        {
            var privateKey = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(privateKey);
            }

            var key = new KeyPair(privateKey);
            Array.Clear(privateKey, 0, privateKey.Length);
            var contract = new Contract
            {
                Script = Helper.CreateSignatureRedeemScript(key.PublicKey),
                Parameters = new List<Parameter>
                {
                    new Parameter("signature", ParameterType.Signature)
                },
                Deployed = false
            };
            var account = new Account(contract.ScriptHash, key)
            {
                Contract = contract,
                Label = label
            };
            AddAccount(account);
            return account;
        }

        /// <summary>
        ///     Creates an Account, ecrypts it using NEP2 and returns it.
        /// </summary>
        /// <returns></returns>
        public async Task<Account> CreateAccount(string label, string password)
        {
            var privateKey = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(privateKey);
            }

            var key = new KeyPair(privateKey);
            Array.Clear(privateKey, 0, privateKey.Length);
            var contract = new Contract
            {
                Script = Helper.CreateSignatureRedeemScript(key.PublicKey),
                Parameters = new List<Parameter>
                {
                    new Parameter("signature", ParameterType.Signature)
                },
                Deployed = false
            };
            var encryptedKey = await Nep2.Encrypt(key.Export(), password);
            var account = new Account(contract.ScriptHash, key)
            {
                Nep2Key = encryptedKey,
                Contract = contract,
                Label = label
            };
            AddAccount(account);
            return account;
        }

        /// <summary>
        /// Create a multi-sig account from a list of public keys.
        /// 
        /// </summary>
        /// <param name="signingThreshold">Minimum number of signatures required for verification. Must be larger than 0 and less than number of keys provided.</param>
        /// <param name="pubKeys">List of public keys to form the account. 2-16 keys allowed. Order is important.</param>
        /// <param name="label"></param>
        public Account CreateMultiSigAccount(int signingThreshold, string[] pubKeys, string label)
        {
            if (signingThreshold >= pubKeys.Length) throw new ArgumentOutOfRangeException($"{nameof(signingThreshold)} must be larger than 0 and less than number of keys provided");

            var publicKeys = new List<NeoECPoint>();
            foreach (var pubKey in pubKeys)
            {
                publicKeys.Add(NeoECPoint.Parse(pubKey, ECCurve.Secp256r1));
            }

            var verificationScript = Helper.CreateMultiSigRedeemScript(signingThreshold, publicKeys.ToArray());
            var contract = new Contract
            {
                Script = verificationScript,
                Parameters = Enumerable.Repeat(new Parameter("signature", ParameterType.Signature), signingThreshold).ToList(),
                Deployed = false
            };

            var account = new Account(verificationScript.ToScriptHash(), label, false, false, null, contract, null);
            AddAccount(account);
            return account;
        }

        /// <summary>
        ///     Deletes the Account from the wallet, passing an address as parameter. Returns True if deleted, or false if not.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public bool DeleteAccount(string address)
        {
            var scriptHash = address.ToScriptHash();
            var account = _wallet.Accounts.FirstOrDefault(p => p.Address == scriptHash);
            return DeleteAccount(account);
        }

        /// <summary>
        ///     Deletes the Account from the wallet, passing an Account object as parameter. Returns True if deleted, or false if
        ///     not.
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public bool DeleteAccount(Account account)
        {
            if (account == null) return false;
            lock (_wallet.Accounts)
            {
                bool deleted;
                deleted = _wallet.Accounts.Remove(account);
                return deleted;
            }
        }

        /// <summary>
        ///     Returns an Account that corresponds with the address provided.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public Account GetAccount(string address)
        {
            return GetAccount(address.ToScriptHash());
        }

        /// <summary>
        ///     Returns an Account that corresponds with the scriptHash provided.
        /// </summary>
        /// <param name="scriptHash"></param>
        /// <returns></returns>
        public Account GetAccount(UInt160 scriptHash)
        {
            lock (_wallet.Accounts)
            {
                return _wallet.Accounts.FirstOrDefault(p => p.Address == scriptHash);
            }
        }

        /// <summary>
        ///     Gets the KeyPair object using the encrypted key and password pair.
        /// </summary>
        /// <param name="nep2Key"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<KeyPair> GetKey(string nep2Key, string password)
        {
            if (nep2Key == null) return null;
            var decryptedKey = await Nep2.Decrypt(nep2Key, password, _wallet.Scrypt);
            var privKey = new KeyPair(decryptedKey);
            return privKey;
        }

        /// <summary>
        ///     Verifies if the provided encrypted key and password are correct.
        /// </summary>
        /// <param name="nep2Key"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<bool> VerifyPassword(string nep2Key, string password)
        {
            try
            {
                await Nep2.Decrypt(nep2Key, password, _wallet.Scrypt);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        ///     Load wallet from file path
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>Wallet object</returns>
        public static Wallet LoadFromFile(string filePath)
        {
            using (var file = File.OpenText(filePath))
            {
                var json = file.ReadToEnd();
                return Wallet.FromJson(json);
            }
        }

        //TODO: this client and rest stuff must be refractored
        public void ChangeApiEndPoints(IClient client, INeoscanService restService)
        {
            _client = client;
            _restService = restService;
            ChangeAccountsClient();
        }

        public void ChangeApiEndPoints(IClient client, string restUrl)
        {
            _client = client;
            _restService = new NeoScanRestService(restUrl);
            ChangeAccountsClient();
        }

        private void ChangeAccountsClient()
        {
            foreach (var walletAccount in _wallet.Accounts)
                walletAccount.TransactionManager =
                    new AccountSignerTransactionManager(_client, _restService, walletAccount);
        }
    }
}