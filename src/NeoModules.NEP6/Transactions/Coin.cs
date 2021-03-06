﻿using System;
using System.Collections.Generic;
using System.Text;
using NeoModules.Core.KeyPair;

namespace NeoModules.NEP6.Transactions
{
    public class Coin : IEquatable<Coin>
    {
        public CoinReference Reference;
        public TransactionOutput Output;

        private string _address = null;
        public string Address
        {
            get
            {
                if (_address == null)
                {
                    _address = Output.ScriptHash.ToAddress();
                }
                return _address;
            }
        }
        
        public bool Equals(Coin other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;
            return Reference.Equals(other.Reference);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Coin);
        }

        public override int GetHashCode()
        {
            return Reference.GetHashCode();
        }
    }
}
