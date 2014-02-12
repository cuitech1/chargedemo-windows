﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InnerFence.ChargeAPI
{
    public class ChargeResponse
    {
        public enum Code
        {
            APPROVED,
            CANCELLED,
            DECLINED,
            ERROR,
        }

        public static class Keys
        {
            public const string AMOUNT = "ifcc_amount";
            public const string CARD_TYPE = "ifcc_cardType";
            public const string CURRENCY = "ifcc_currency";
            public const string ERROR_MESSAGE = "ifcc_errorMessage";
            public const string EXTRA_PARAMS = "ifcc_extraParams";
            public const string REDACTED_CARD_NUMBER = "ifcc_redactedCardNumber";
            public const string RESPONSE_TYPE = "ifcc_responseType";
            public const string TAX_AMOUNT = "ifcc_taxAmount";
            public const string TAX_RATE = "ifcc_taxRate";
            public const string TIP_AMOUNT = "ifcc_tipAmount";
            public const string TRANSACTION_ID = "ifcc_transactionId";
        }

        static class Patterns
        {
            public static readonly Regex AMOUNT = new Regex("^(0|[1-9][0-9]*)[.][0-9][0-9]$");
            public static readonly Regex CARD_TYPE = new Regex("^[A-Za-z ]{0,20}$");
            public static readonly Regex CURRENCY = new Regex("^[A-Z]{3}$");
            public static readonly Regex ERROR_MESSAGE = new Regex("^.*$");
            public static readonly Regex REDACTED_CARD_NUMBER = new Regex("^X*[0-9]{4}$");
            public static readonly Regex RESPONSE_TYPE = new Regex("^[a-z]*$");
            public static readonly Regex TAX_AMOUNT = new Regex("^(0|[1-9][0-9]*)[.][0-9][0-9]$");
            public static readonly Regex TAX_RATE = new Regex("^[0-9]{1,2}([.][0-9]{1,3})?$");
            public static readonly Regex TIP_AMOUNT = new Regex("^(0|[1-9][0-9]*)[.][0-9][0-9]$");
            public static readonly Regex TRANSACTION_ID = new Regex("^.{1,255}");
        }

        public static class Type
        {
            public const string APPROVED = "approved";
            public const string CANCELLED = "cancelled";
            public const string DECLINED = "declined";
            public const string ERROR = "error";
        }

        public String Amount { get; protected set; }
        public String CardType { get; protected set; }
        public String Currency { get; protected set; }
        public virtual String ErrorMessage { get; protected set; }
        public Dictionary<string, string> ExtraParams { get; protected set; }
        public String RedactedCardNumber { get; protected set; }
        public Code ResponseCode { get; protected set; }
        public String ResponseType { get; protected set; }
        public String TaxAmount { get; protected set; }
        public String TaxRate { get; protected set; }
        public String TipAmount { get; protected set; }
        public String TransactionId { get; protected set; }

        protected ChargeResponse() { }

        public ChargeResponse(Uri responseUri)
        {
            if (null == responseUri)
            {
                throw new ArgumentNullException("uri");
            }

            string query = responseUri.Query.TrimStart('?');

            if (String.IsNullOrEmpty(query))
            {
                throw new Exception("Invalid Request [1]");
            }

            Dictionary<string, string> parameters = this.ParseQueryString(query);
            if (parameters.Count == 0)
            {
                throw new Exception("Invalid Request [2]");
            }

            if (parameters.ContainsKey(Keys.AMOUNT))
            {
                this.Amount = parameters[Keys.AMOUNT];
            }
            if (parameters.ContainsKey(Keys.CARD_TYPE))
            {
                this.CardType = parameters[Keys.CARD_TYPE];
            }
            if (parameters.ContainsKey(Keys.CURRENCY))
            {
                this.Currency = parameters[Keys.CURRENCY];
            }
            if (parameters.ContainsKey(Keys.ERROR_MESSAGE))
            {
                this.ErrorMessage = parameters[Keys.ERROR_MESSAGE];
            }
            if (parameters.ContainsKey(Keys.REDACTED_CARD_NUMBER))
            {
                this.RedactedCardNumber = parameters[Keys.REDACTED_CARD_NUMBER];
            }
            if (parameters.ContainsKey(Keys.RESPONSE_TYPE))
            {
                this.ResponseType = parameters[Keys.RESPONSE_TYPE];
            }
            if (parameters.ContainsKey(Keys.TAX_AMOUNT))
            {
                this.TaxAmount = parameters[Keys.TAX_AMOUNT];
            }
            if (parameters.ContainsKey(Keys.TAX_RATE))
            {
                this.TaxRate = parameters[Keys.TAX_RATE];
            }
            if (parameters.ContainsKey(Keys.TIP_AMOUNT))
            {
                this.TipAmount = parameters[Keys.TIP_AMOUNT];
            }
            if (parameters.ContainsKey(Keys.TRANSACTION_ID))
            {
                this.TransactionId = parameters[Keys.TRANSACTION_ID];
            }

            this.ValidateFields();

            if (this.ResponseType == Type.APPROVED)
            {
                this.ResponseCode = Code.APPROVED;
            }
            else if (this.ResponseType == Type.CANCELLED)
            {
                this.ResponseCode = Code.CANCELLED;
            }
            else if (this.ResponseType == Type.DECLINED)
            {
                this.ResponseCode = Code.DECLINED;
            }
            else
            {
                this.ResponseCode = Code.ERROR;
            }

            this.ExtraParams = new Dictionary<string, string>();
            foreach (var parameter in parameters)
            {
                if (!parameter.Key.StartsWith("ifcc_"))
                {
                    this.ExtraParams[parameter.Key] = parameter.Value;
                }
            }
        }

        public Dictionary<string, string> ParseQueryString(string s)
        {
            Dictionary<string, string> data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (s == null)
            {
                return data;
            }

            foreach (string pair in s.Split('&'))
            {
                int index = pair.IndexOf('=');
                if (index == -1)
                {
                    // Just the name, set it as an empty string
                    data[Uri.UnescapeDataString(pair).Replace("+", "%20")] = "";
                }
                else
                {
                    // UnescapeDataString doesn't handle + characters, so replace any + in the unescaped values with an escaped space %20
                    data[Uri.UnescapeDataString(pair.Substring(0, index).Replace("+", "%20"))] = Uri.UnescapeDataString(pair.Substring(index + 1).Replace("+", "%20"));
                }
            }

            return data;
        }

        public void ValidateFields()
        {
            this.ValidateField(Patterns.AMOUNT, this.Amount, Keys.AMOUNT);
            this.ValidateField(Patterns.CARD_TYPE, this.CardType, Keys.CARD_TYPE);
            this.ValidateField(Patterns.CURRENCY, this.Currency, Keys.CURRENCY);
            this.ValidateField(Patterns.ERROR_MESSAGE, this.ErrorMessage, Keys.ERROR_MESSAGE);
            this.ValidateField(Patterns.REDACTED_CARD_NUMBER, this.RedactedCardNumber, Keys.REDACTED_CARD_NUMBER);
            this.ValidateField(Patterns.RESPONSE_TYPE, this.ResponseType, Keys.RESPONSE_TYPE);
            this.ValidateField(Patterns.TAX_AMOUNT, this.TaxAmount, Keys.TAX_AMOUNT);
            this.ValidateField(Patterns.TAX_RATE, this.TaxRate, Keys.TAX_RATE);
            this.ValidateField(Patterns.TIP_AMOUNT, this.TipAmount, Keys.TIP_AMOUNT);
            this.ValidateField(Patterns.TRANSACTION_ID, this.TransactionId, Keys.TRANSACTION_ID);
        }

        public void ValidateField(Regex pattern, string value, string fieldName)
        {
            if (null != value && !pattern.Match(value).Success)
            {
                throw new ArgumentException(fieldName);
            }
        }
    }
}
